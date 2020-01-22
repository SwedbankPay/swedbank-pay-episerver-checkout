using System;
using System.Globalization;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.Common.Helpers;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.Consumers;
using SwedbankPay.Sdk.PaymentOrders;

namespace SwedbankPay.Episerver.Checkout
{
    [ServiceConfiguration(typeof(ISwedbankPayCheckoutService))]
    public class SwedbankPayCheckoutService : ISwedbankPayCheckoutService
    {
        private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;
        private readonly ICurrentMarket _currentMarket;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(SwedbankPayCheckoutService));
        private readonly IMarketService _marketService;
        private readonly IOrderRepository _orderRepository;
        private readonly IRequestFactory _requestFactory;
        private readonly ISwedbankPayClientFactory _swedbankPayClientFactory;
        private PaymentMethodDto _paymentMethodDto;

        public SwedbankPayCheckoutService(
            ICurrentMarket currentMarket,
            ICheckoutConfigurationLoader checkoutConfigurationLoader,
            IMarketService marketService,
            IOrderRepository orderRepository,
            ISwedbankPayClientFactory swedbankPayClientFactory,
            IRequestFactory requestFactory)
        {
            _currentMarket = currentMarket;
            _checkoutConfigurationLoader = checkoutConfigurationLoader;
            _marketService = marketService;
            _orderRepository = orderRepository;
            _swedbankPayClientFactory = swedbankPayClientFactory;
            _requestFactory = requestFactory;
        }

        public PaymentMethodDto PaymentMethodDto => _paymentMethodDto ?? (_paymentMethodDto =
                                                        PaymentManager.GetPaymentMethodBySystemName(
                                                            Constants.SwedbankPayCheckoutSystemKeyword,
                                                            ContentLanguage.PreferredCulture.Name, true));

        public virtual PaymentOrder CreateOrUpdatePaymentOrder(IOrderGroup orderGroup, string description,
            string consumerProfileRef = null)
        {
            var orderId = orderGroup.Properties[Constants.SwedbankPayCheckoutOrderIdCartField]?.ToString();
            //return string.IsNullOrWhiteSpace(orderId) ? CreateOrder(orderGroup, userAgent, consumerProfileRef) :  UpdateOrder(orderId, orderGroup, userAgent);
            return
                CreatePaymentOrder(orderGroup, description, consumerProfileRef); //TODO Change to UpdateOrder when SwedbankPay Api supports updating of orderitems
        }

        public virtual Consumer InitiateConsumerSession(string email = null, string mobilePhone = null,
            string ssn = null)
        {
            var market = _currentMarket.GetCurrentMarket();
            var swedbankPayClient = _swedbankPayClientFactory.Create(market);

            var config = _checkoutConfigurationLoader.GetConfiguration(market.MarketId);

            try
            {
                var initiateConsumerSessionRequest =
                    _requestFactory.GetConsumerResourceRequest(new Language(market.DefaultLanguage.TextInfo.CultureName),
                        config.ShippingAddressRestrictedToCountries.Select(x =>
                            new RegionInfo(CountryCodeHelper.GetTwoLetterCountryCode(x))),
                        string.IsNullOrEmpty(email) ? null : new EmailAddress(email),
                        string.IsNullOrEmpty(mobilePhone) ? null : new Msisdn(mobilePhone),
                        string.IsNullOrEmpty(ssn)
                            ? null
                            : new NationalIdentifier(new RegionInfo(market.DefaultLanguage.TextInfo.CultureName), ssn));

                return AsyncHelper.RunSync(() =>
                    swedbankPayClient.Consumers.InitiateSession(initiateConsumerSessionRequest));
            }

            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw;
            }
        }

        public PaymentOrder GetPaymentOrder(IOrderGroup orderGroup, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None)
        {
            var swedbankPayCheckoutOrderId = orderGroup.Properties[Constants.SwedbankPayCheckoutOrderIdCartField]?.ToString();
            if (!string.IsNullOrWhiteSpace(swedbankPayCheckoutOrderId))
            {
                var market = _marketService.GetMarket(orderGroup.MarketId);
                var uri = new Uri(swedbankPayCheckoutOrderId, UriKind.Relative);
                return GetPaymentOrder(uri, market, paymentOrderExpand);
            }

            return null;
        }

        public virtual PaymentOrder GetPaymentOrder(Uri id, IMarket market,
            PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None)
        {
            var config = LoadCheckoutConfiguration(market);
            var swedbankPayClient = _swedbankPayClientFactory.Create(config);

            try
            {
                return AsyncHelper.RunSync(() => swedbankPayClient.PaymentOrder.Get(id, paymentOrderExpand));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw;
            }
        }


        public void CancelOrder(IOrderGroup orderGroup)
        {
            var market = _marketService.GetMarket(orderGroup.MarketId);
            var swedbankPayClient = _swedbankPayClientFactory.Create(market);

            var orderId = orderGroup.Properties[Constants.SwedbankPayCheckoutOrderIdCartField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId))
                try
                {
                    var cancelRequest = _requestFactory.GetCancelRequest();
                    var paymentOrder = AsyncHelper.RunSync(() => swedbankPayClient.PaymentOrder.Get(new Uri(orderId)));

                    if (paymentOrder.Operations.Cancel != null)
                    {
                        var cancelResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Cancel(cancelRequest));
                        if (cancelResponse.Cancellation.Transaction.Type == TransactionType.Cancellation &&
                            cancelResponse.Cancellation.Transaction.State.Equals(State.Completed))
                        {
                            orderGroup.Properties[Constants.SwedbankPayCheckoutOrderIdCartField] = null;
                            _orderRepository.Save(orderGroup);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                    throw;
                }
        }

        public void Complete(IPurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null) throw new ArgumentNullException(nameof(purchaseOrder));
            var orderForm = purchaseOrder.GetFirstForm();
            var payment = orderForm?.Payments.FirstOrDefault(x =>
                x.PaymentMethodName.Equals(Constants.SwedbankPayCheckoutSystemKeyword));
            if (payment == null) return;
        }

        public CheckoutConfiguration LoadCheckoutConfiguration(IMarket market)
        {
            return _checkoutConfigurationLoader.GetConfiguration(market.MarketId, market.DefaultLanguage.Name);
        }

        private PaymentOrder CreatePaymentOrder(IOrderGroup orderGroup, string description, string consumerProfileRef = null)
        {
            var market = _currentMarket.GetCurrentMarket();
            var swedbankPayClient = _swedbankPayClientFactory.Create(market);

            try
            {
                var paymentOrderRequest = _requestFactory.GetPaymentOrderRequest(orderGroup, market, PaymentMethodDto, description, consumerProfileRef);
                var paymentOrder = AsyncHelper.RunSync(() => swedbankPayClient.PaymentOrder.Create(paymentOrderRequest));

                orderGroup.Properties[Constants.SwedbankPayCheckoutOrderIdCartField] = paymentOrder.PaymentOrderResponse.Id;
                _orderRepository.Save(orderGroup);
                return paymentOrder;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw;
            }
        }

        private PaymentOrder UpdatePaymentOrder(Uri orderId, IOrderGroup orderGroup, string userAgent)
        {
            var market = _marketService.GetMarket(orderGroup.MarketId);
            var swedbankPayClient = _swedbankPayClientFactory.Create(PaymentMethodDto, orderGroup.MarketId);
            var total = orderGroup.GetTotal();

            var updateOrderRequest = _requestFactory.GetUpdateRequest(orderGroup);
            var paymentOrder = AsyncHelper.RunSync(() => swedbankPayClient.PaymentOrder.Get(orderId));

            if (paymentOrder?.Operations?.Update != null)
            {
                var response = AsyncHelper.RunSync(() => paymentOrder.Operations.Update(updateOrderRequest));
            }

            orderGroup.Properties[Constants.SwedbankPayCheckoutOrderIdCartField] =
                paymentOrder?.PaymentOrderResponse.Id;
            _orderRepository.Save(orderGroup);

            return paymentOrder;
        }
    }
}