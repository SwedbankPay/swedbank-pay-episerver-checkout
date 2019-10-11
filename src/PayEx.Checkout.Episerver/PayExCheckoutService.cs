using System.Collections.Generic;

namespace PayEx.Checkout.Episerver
{
    using EPiServer.Business.Commerce.Exception;
    using EPiServer.Commerce.Order;
    using EPiServer.Globalization;
    using EPiServer.Logging;
    using EPiServer.ServiceLocation;
    using EPiServer.Web;

    using Mediachase.Commerce;
    using Mediachase.Commerce.Markets;
    using Mediachase.Commerce.Orders.Dto;
    using Mediachase.Commerce.Orders.Managers;

    using PayEx.Checkout.Episerver.Common;
    using PayEx.Checkout.Episerver.Common.Helpers;
    using PayEx.Checkout.Episerver.Helpers;
    using PayEx.Checkout.Episerver.OrderManagement;

    using SwedbankPay.Client;
    using SwedbankPay.Client.Models;
    using SwedbankPay.Client.Models.Common;
    using SwedbankPay.Client.Models.Request;
    using SwedbankPay.Client.Models.Response;

    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;

    [ServiceConfiguration(typeof(IPayExCheckoutService))]
    public class PayExCheckoutService : IPayExCheckoutService
    {
        private readonly ICurrentMarket _currentMarket;
        private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;
        private readonly IMarketService _marketService;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IOrderRepository _orderRepository;
        private readonly SwedbankPayOrderServiceFactory _swedbankPayOrderServiceFactory;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(PayExCheckoutService));
        private PaymentMethodDto _paymentMethodDto;
        private SwedbankPayOptions _checkoutConfiguration;
        private readonly IShippingCalculator _shippingCalculator;

        public PaymentMethodDto PaymentMethodDto => _paymentMethodDto ?? (_paymentMethodDto = PaymentManager.GetPaymentMethodBySystemName(Constants.PayExCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name, returnInactive: true));

        public PayExCheckoutService(
            ICurrentMarket currentMarket,
            ICheckoutConfigurationLoader checkoutConfigurationLoader,
            IMarketService marketService,
            IOrderGroupCalculator orderGroupCalculator,
            IOrderRepository orderRepository,
            SwedbankPayOrderServiceFactory swedbankPayOrderServiceFactory, IShippingCalculator shippingCalculator)
        {
            _currentMarket = currentMarket;
            _checkoutConfigurationLoader = checkoutConfigurationLoader;
            _marketService = marketService;
            _orderGroupCalculator = orderGroupCalculator;
            _orderRepository = orderRepository;
            _swedbankPayOrderServiceFactory = swedbankPayOrderServiceFactory;
            _shippingCalculator = shippingCalculator;
        }

        public SwedbankPayOptions GetCheckoutConfiguration(IMarket market)
        {
            return _checkoutConfiguration ?? (_checkoutConfiguration = GetConfiguration(market));
        }

        public virtual PaymentOrderResponseContainer CreateOrUpdateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null)
        {
            var orderId = orderGroup.Properties[Constants.PayExCheckoutOrderIdCartField]?.ToString();
            //return string.IsNullOrWhiteSpace(orderId) ? CreateOrder(orderGroup, userAgent, consumerProfileRef) :  UpdateOrder(orderId, orderGroup, userAgent);
            return CreateOrder(orderGroup, userAgent, consumerProfileRef); //TODO Change to UpdateOrder when PayEx Api supports updating of orderitems
        }

        public virtual ConsumerResourceResponse InitiateConsumerSession(string email = null, string mobilePhone = null, string ssn = null)
        {

            var swedbankPayClient = GetSwedbankPayClient();
            var market = _marketService.GetMarket(_currentMarket.GetCurrentMarket().MarketId);

            var twoLetterIsoRegionName = new RegionInfo(market.DefaultLanguage.TextInfo.CultureName).TwoLetterISORegionName;

            var initiateConsumerSessionRequestObject = new ConsumerResourceRequest
            {
                Email = email,
                Msisdn = mobilePhone,
                ConsumerCountryCode = twoLetterIsoRegionName
            };

            if (!string.IsNullOrWhiteSpace(ssn))
            {
                initiateConsumerSessionRequestObject.NationalIdentifier = new NationalIdentifier
                {
                    CountryCode = twoLetterIsoRegionName,
                    SocialSecurityNumber = ssn
                };
            }

            try
            {
                var initiateConsumerSessionResponseObject = AsyncHelper.RunSync(() => swedbankPayClient.Consumer.InitiateSession(initiateConsumerSessionRequestObject));
                return initiateConsumerSessionResponseObject;
            }
            //TODO swed
            //catch (PayExException ex)
            //{
            //	_logger.Error($"{ex.ErrorCode} - {ex.Error.Title}... {ex.Error.Detail}::: {string.Join(", ", ex.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}");
            //	throw;
            //}
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw;
            }
        }


        public virtual ShippingDetails GetShippingDetails(string uri)
        {
            var swedbankPayClient = GetSwedbankPayClient();
            return AsyncHelper.RunSync(() => swedbankPayClient.Consumer.GetShippingDetails(uri));
        }

        public virtual PaymentOrderResponseContainer CreateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null)
        {
            var swedbankPayClient = GetSwedbankPayClient(orderGroup);
            var market = _marketService.GetMarket(orderGroup.MarketId);

            try
            {
                var paymentOrderRequestContainer = GetCheckoutOrderData(orderGroup, market, PaymentMethodDto, consumerProfileRef);
                var paymentOrder = AsyncHelper.RunSync(() => swedbankPayClient.PaymentOrders.CreatePaymentOrder(paymentOrderRequestContainer));

                orderGroup.Properties[Constants.PayExCheckoutOrderIdCartField] = paymentOrder.PaymentOrder.Id;

                _orderRepository.Save(orderGroup);
                return paymentOrder;
            }
            //TODO swed
            //catch (PayExException ex)
            //{
            //	_logger.Error($"{ex.ErrorCode} - {ex.Error.Title}... {ex.Error.Detail}::: {string.Join(", ", ex.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}");
            //	throw;
            //}
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw;
            }
        }



        public virtual PaymentOrderResponseContainer UpdateOrder(string orderId, IOrderGroup orderGroup, string userAgent)
        {
            var market = _marketService.GetMarket(orderGroup.MarketId);
            var swedbankPayClient = GetSwedbankPayClient(orderGroup);

            var paymentOrderRequestContainer = GetCheckoutOrderData(orderGroup, market, PaymentMethodDto);
            var paymentOrderResponseObject = AsyncHelper.RunSync(() => swedbankPayClient.PaymentOrders.UpdatePaymentOrder(orderId, paymentOrderRequestContainer));

            orderGroup.Properties[Constants.PayExCheckoutOrderIdCartField] = paymentOrderResponseObject.PaymentOrder.Id;
            _orderRepository.Save(orderGroup);

            return paymentOrderResponseObject;
        }

        public virtual PaymentOrderResponseContainer GetOrder(string id, IMarket market, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None)
        {
            var swedbankPayOptions = GetConfiguration(market);
            var swedbankPayClient = new SwedbankPayClient(swedbankPayOptions);

            try
            {
                var paymentOrderResponseContainer = AsyncHelper.RunSync(() => swedbankPayClient.PaymentOrders.GetPaymentOrder(id, paymentOrderExpand));
                return paymentOrderResponseContainer;
            }
            //TODO swed
            //catch (PayExException ex)
            //{
            //	_logger.Error($"{ex.ErrorCode} - {ex.Error.Title}... {ex.Error.Detail}::: {string.Join(", ", ex.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}");
            //	throw;
            //}
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw;
            }
        }


        public void CancelOrder(IOrderGroup orderGroup)
        {
            var market = _marketService.GetMarket(orderGroup.MarketId);
            var swedbankPayOrderService = _swedbankPayOrderServiceFactory.Create(market);

            var orderId = orderGroup.Properties[Constants.PayExCheckoutOrderIdCartField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                try
                {
                    var cancelResponseObject = AsyncHelper.RunSync(() => swedbankPayOrderService.CancelOrder(orderId));
                    if (cancelResponseObject != null)
                    {
                        orderGroup.Properties[Constants.PayExCheckoutOrderIdCartField] = null;
                        _orderRepository.Save(orderGroup);
                    }

                }
                //catch (PayExException ex)
                //{
                //	_logger.Error($"{ex.ErrorCode} - {ex.Error.Title}... {ex.Error.Detail}::: {string.Join(", ", ex.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}");
                //	throw;
                //}
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                    throw;
                }
            }
        }

        public void Complete(IPurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                throw new ArgumentNullException(nameof(purchaseOrder));
            }
            var orderForm = purchaseOrder.GetFirstForm();
            var payment = orderForm?.Payments.FirstOrDefault(x => x.PaymentMethodName.Equals(Constants.PayExCheckoutSystemKeyword));
            if (payment == null)
            {
                return;
            }
        }

        //public async Task<PaymentOrderResponseContainer> GetPayment(string id, IOrderGroup cart, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None)
        //{
        //	var swedbankPayClient = GetSwedbankPayClient(cart);
        //	try
        //	{
        //		var paymentResponseContainer = await swedbankPayClient.PaymentOrders.GetPaymentOrder(id, paymentOrderExpand);
        //		return paymentResponseContainer;
        //	}
        //	//catch (PayExException ex)
        //	//{
        //	//	_logger.Error($"{ex.ErrorCode} - {ex.Error.Title}... {ex.Error.Detail}::: {string.Join(", ", ex.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}");
        //	//	throw;
        //	//}
        //	catch (Exception ex)
        //	{
        //		_logger.Error(ex.Message, ex);
        //		throw;
        //	}
        //}



        protected virtual PaymentOrderRequestContainer GetCheckoutOrderData(
          IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string consumerProfileRef = null)
        {
            var totals = _orderGroupCalculator.GetOrderGroupTotals(orderGroup);

            var marketCountry = CountryCodeHelper.GetTwoLetterCountryCode(market.Countries.FirstOrDefault());
            if (string.IsNullOrWhiteSpace(marketCountry))
            {
                throw new ConfigurationException($"Please select a country in Commerce Manager for market {orderGroup.MarketId}");
            }
            var configuration = GetConfiguration(market);
            
            var orderItems = orderGroup.GetAllLineItems().Select(item =>
            {
                var unitPrice = AmountHelper.GetAmount(item.PlacedPrice);
                var amount = AmountHelper.GetAmount(item.GetExtendedPrice(orderGroup.Currency));
                var vatAmount = AmountHelper.GetAmount(item.GetSalesTax(market, orderGroup.Currency, orderGroup.GetFirstShipment().ShippingAddress));                            
                
                return new OrderItem
                {
                    Reference = item.LineItemId.ToString(),
                    Amount = market.PricesIncludeTax ? amount : amount + vatAmount,
                    Class = "FASHION", //TODO Get Value from interface 
                    Description = "",
                    DiscountDescription = "",
                    DiscountPrice = AmountHelper.GetAmount(item.GetDiscountedPrice(orderGroup.Currency)),
                    ImageUrl = "", //TODO Get correct value
                    ItemUrl = "", //TODO Get correct value
                    Name = item.DisplayName,
                    Quantity = (int)(item.Quantity),
                    QuantityUnit = "PCS", //TODO Get Value from interface
                    Type = "PRODUCT", //TODO Get Value from interface
                    UnitPrice = unitPrice,
                    VatAmount = vatAmount, //TODO Get correct value
                    VatPercent = (int)((double) vatAmount / amount * 10000) //TODO Get correct value
                };
            }).ToList();

            var shippingVatAmount = AmountHelper.GetAmount(_shippingCalculator.GetShippingTax(orderGroup.GetFirstShipment(), market, orderGroup.Currency));
            var shippingAmount = AmountHelper.GetAmount(orderGroup.GetShippingTotal());
            orderItems.Add(new OrderItem
            {
                Type = "SHIPPING_FEE",
                Reference = "SHIPPING",
                Quantity = 1,
                DiscountPrice = shippingAmount,
                DiscountDescription = "",
                Name = "SHIPPINGFEE",
                VatAmount = shippingVatAmount,
                ItemUrl = "",
                ImageUrl = "",
                Description = "Shipping fee",
                Amount = market.PricesIncludeTax ? shippingAmount : shippingAmount + shippingVatAmount,
                Class = "NOTAPPLICABLE",
                UnitPrice = shippingAmount,
                QuantityUnit = "PCS",
                VatPercent = (int)((double)shippingVatAmount / shippingAmount * 10000)
            });

            var paymentOrderRequestObject = new PaymentOrderRequestContainer
            {
                Paymentorder = new PaymentOrderRequest
                {
                    Amount = AmountHelper.GetAmount(totals.Total),
                    VatAmount = AmountHelper.GetAmount(totals.TaxTotal),
                    Currency = market.DefaultCurrency.CurrencyCode,
                    Description = "Description",
                    Language = market.DefaultLanguage.TextInfo.CultureName,
                    UserAgent = HttpContext.Current.Request.UserAgent,
                    Urls = GetMerchantUrls(orderGroup),
                    PayeeInfo = new PayeeInfo
                    {
                        PayeeId = configuration.MerchantId,
                        PayeeReference = DateTime.Now.Ticks.ToString()
                    },
                    OrderItems = orderItems.ToList()
                }
            };

            if (!string.IsNullOrWhiteSpace(consumerProfileRef))
            {
                paymentOrderRequestObject.Paymentorder.Payer = new Payer
                {
                    ConsumerProfileRef = consumerProfileRef
                };
            }

            return paymentOrderRequestObject;
        }

        protected virtual Urls GetMerchantUrls(IOrderGroup orderGroup)
        {
            if (PaymentMethodDto == null) return null;
            var market = _marketService.GetMarket(orderGroup.MarketId);
            CheckoutConfiguration checkoutConfiguration = LoadCheckoutConfiguration(market);

            string ToFullSiteUrl(Func<CheckoutConfiguration, string> fieldSelector)
            {
                if (string.IsNullOrWhiteSpace(fieldSelector(checkoutConfiguration)))
                {
                    return null;
                }

                var url = fieldSelector(checkoutConfiguration)?.ToString().Replace("{orderGroupId}", orderGroup.OrderLink.OrderGroupId.ToString());
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return uri.ToString();
                }

                return new Uri(SiteDefinition.Current.SiteUrl, url).ToString();
            }

            return new Urls
            {
                TermsOfServiceUrl = checkoutConfiguration.TermsOfServiceUrl,
                CallbackUrl = ToFullSiteUrl(c => c.CallbackUrl),
                PaymentUrl = ToFullSiteUrl(c => c.PaymentUrl),
                CancelUrl = string.IsNullOrWhiteSpace(checkoutConfiguration.PaymentUrl) ? ToFullSiteUrl(c => c.CancelUrl) : null,
                CompleteUrl = ToFullSiteUrl(c => c.CompleteUrl),
                LogoUrl = checkoutConfiguration.LogoUrl,
                HostUrls = checkoutConfiguration.HostUrls
            };
        }


        public CheckoutConfiguration LoadCheckoutConfiguration(IMarket market)
        {
            return _checkoutConfigurationLoader.GetConfiguration(market.MarketId, market.DefaultLanguage.Name);
        }

        public SwedbankPayOptions GetConfiguration(IMarket market)
        {
            var checkoutConfiguration = _checkoutConfigurationLoader.GetConfiguration(market.MarketId, market.DefaultLanguage.Name);
            return SwedbankPayOptions(checkoutConfiguration);
        }

        private static SwedbankPayOptions SwedbankPayOptions(CheckoutConfiguration checkoutConfiguration)
        {
            return new SwedbankPayOptions
            {
                Token = checkoutConfiguration.Token,
                MerchantId = checkoutConfiguration.MerchantId,
                ApiBaseUrl = !string.IsNullOrWhiteSpace(checkoutConfiguration.ApiUrl) ? new Uri(checkoutConfiguration.ApiUrl) : null,
                CallBackUrl = !string.IsNullOrWhiteSpace(checkoutConfiguration.CallbackUrl) ? new Uri(checkoutConfiguration.CallbackUrl) : null,
                CancelPageUrl = !string.IsNullOrWhiteSpace(checkoutConfiguration.CallbackUrl) ? new Uri(checkoutConfiguration.CancelUrl) : null,
                CompletePageUrl = !string.IsNullOrWhiteSpace(checkoutConfiguration.CompleteUrl) ? new Uri(checkoutConfiguration.CompleteUrl) : null,
                MerchantName = "Authority"
            };
        }

        private SwedbankPayOptions GetConfiguration(MarketId marketId)
        {
            var checkoutConfiguration = _checkoutConfigurationLoader.GetConfiguration(marketId);
            return SwedbankPayOptions(checkoutConfiguration);
        }

        private SwedbankPayClient GetSwedbankPayClient()
        {
            var market = _marketService.GetMarket(_currentMarket.GetCurrentMarket().MarketId);
            var swedbankPayOptions = GetConfiguration(market);
            var swedbankPayClient = new SwedbankPayClient(swedbankPayOptions);
            return swedbankPayClient;
        }

        private SwedbankPayClient GetSwedbankPayClient(IOrderGroup orderGroup)
        {
            var market = _marketService.GetMarket(orderGroup.MarketId);
            var swedbankPayOrderService = _swedbankPayOrderServiceFactory.Create(market);

            var swedbankPayOptions = GetConfiguration(market);
            var swedbankPayClient = new SwedbankPayClient(swedbankPayOptions);
            return swedbankPayClient;
        }
    }
}
