using System;
using System.Collections.Generic;
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
using SwedbankPay.Episerver.Checkout.OrderManagement;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.Consumers;
using SwedbankPay.Sdk.PaymentOrders;

namespace SwedbankPay.Episerver.Checkout
{
    [ServiceConfiguration(typeof(ISwedbankPayCheckoutService))]
    public class SwedbankSwedbankPayCheckoutService : ISwedbankPayCheckoutService
    {
        private readonly ICurrentMarket _currentMarket;
        private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;
        private readonly IMarketService _marketService;
        private readonly IOrderRepository _orderRepository;
        private readonly SwedbankPayClientFactory _swedbankPayClientFactory;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(SwedbankSwedbankPayCheckoutService));
        private PaymentMethodDto _paymentMethodDto;
        private readonly IRequestFactory _requestFactory;
        
        public PaymentMethodDto PaymentMethodDto => _paymentMethodDto ?? (_paymentMethodDto = PaymentManager.GetPaymentMethodBySystemName(Constants.SwedbankPayCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name, returnInactive: true));

        public SwedbankSwedbankPayCheckoutService(
            ICurrentMarket currentMarket,
            ICheckoutConfigurationLoader checkoutConfigurationLoader,
            IMarketService marketService,
            IOrderRepository orderRepository,
            SwedbankPayClientFactory swedbankPayClientFactory, IRequestFactory requestFactory)
        {
            _currentMarket = currentMarket;
            _checkoutConfigurationLoader = checkoutConfigurationLoader;
            _marketService = marketService;
            _orderRepository = orderRepository;
            _requestFactory = requestFactory;
        }
        
        public virtual PaymentOrder CreateOrUpdateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null)
        {
            var orderId = orderGroup.Properties[Constants.SwedbankPayCheckoutOrderIdCartField]?.ToString();
            //return string.IsNullOrWhiteSpace(orderId) ? CreateOrder(orderGroup, userAgent, consumerProfileRef) :  UpdateOrder(orderId, orderGroup, userAgent);
            return CreateOrder(orderGroup, userAgent, consumerProfileRef); //TODO Change to UpdateOrder when SwedbankPay Api supports updating of orderitems
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
                    _requestFactory.GetConsumerResourceRequest(market.DefaultLanguage, config.ShippingAddressRestrictedToCountries.Select(x => new RegionInfo(x)),
                        string.IsNullOrEmpty(email) ? null : new EmailAddress(email), string.IsNullOrEmpty(mobilePhone) ? null : new Msisdn(mobilePhone),
                        string.IsNullOrEmpty(ssn) ? null : new NationalIdentifier(new RegionInfo(market.DefaultLanguage.Name), ssn));
                
                return AsyncHelper.RunSync(() => swedbankPayClient.Consumers.InitiateSession(initiateConsumerSessionRequest));
                
            }
            
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw;
            }
        }


        public virtual ShippingDetails GetShippingDetails(Uri uri)
        {
            var market = _currentMarket.GetCurrentMarket();
            var swedbankPayClient = _swedbankPayClientFactory.Create(market);

            return AsyncHelper.RunSync(() => swedbankPayClient.Consumers.GetShippingDetails(uri));
        }

        public virtual PaymentOrder CreateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null)
        {
            var market = _currentMarket.GetCurrentMarket();
            var swedbankPayClient = _swedbankPayClientFactory.Create(market);

            try
            {
                
                var paymentOrderRequest = _requestFactory.GetPaymentOrderRequest(orderGroup, market, PaymentMethodDto, consumerProfileRef);
                
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
        

        public virtual PaymentOrder UpdateOrder(Uri orderId, IOrderGroup orderGroup, string userAgent)
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
            
            orderGroup.Properties[Constants.SwedbankPayCheckoutOrderIdCartField] = paymentOrder?.PaymentOrderResponse.Id;
            _orderRepository.Save(orderGroup);

            return paymentOrder;
        }

        public virtual PaymentOrder GetOrder(Uri id, IMarket market, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None)
        {
            var config = LoadCheckoutConfiguration(market);
            var swedbankPayClient = _swedbankPayClientFactory.Create(config);

            try
            {
                
                return AsyncHelper.RunSync(() => swedbankPayClient.PaymentOrder.Get(id, paymentOrderExpand));
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
            var swedbankPayClient = _swedbankPayClientFactory.Create(market);

            var orderId = orderGroup.Properties[Constants.SwedbankPayCheckoutOrderIdCartField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                try
                {
                    var cancelRequest = _requestFactory.GetCancelRequest();
                        
                    var paymentOrder = AsyncHelper.RunSync(() => swedbankPayClient.PaymentOrder.Get(new Uri(orderId)));

                    if (paymentOrder.Operations.Cancel != null)
                    {
                        var cancelResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Cancel(cancelRequest));
                        if (cancelResponse.Cancellation.Transaction.Type == "Cancel" && cancelResponse.Cancellation.Transaction.State.Equals(State.Completed))
                        {
                            orderGroup.Properties[Constants.SwedbankPayCheckoutOrderIdCartField] = null;
                            _orderRepository.Save(orderGroup);
                        }
                        
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
            var payment = orderForm?.Payments.FirstOrDefault(x => x.PaymentMethodName.Equals(Constants.SwedbankPayCheckoutSystemKeyword));
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

        public CheckoutConfiguration LoadCheckoutConfiguration(IMarket market)
        {
            return _checkoutConfigurationLoader.GetConfiguration(market.MarketId, market.DefaultLanguage.Name);
        }
        
        //private SwedbankPayClient GetSwedbankPayClient()
        //{
        //    var market = _marketService.GetMarket(_currentMarket.GetCurrentMarket().MarketId);
        //    var swedbankPayOptions = GetConfiguration(market);
        //    var swedbankPayClient = new SwedbankPayClient(swedbankPayOptions);
        //    return swedbankPayClient;
        //}

        //private SwedbankPayClient GetSwedbankPayClient(IOrderGroup orderGroup)
        //{
        //    var market = _marketService.GetMarket(orderGroup.MarketId);
        //    var swedbankPayOrderService = _swedbankPayClientFactory.Create(market);

        //    var swedbankPayOptions = GetConfiguration(market);
        //    var swedbankPayClient = new SwedbankPayClient(swedbankPayOptions);
        //    return swedbankPayClient;
        //}
    }
}
