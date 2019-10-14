namespace PayEx.Checkout.Episerver
{
    using EPiServer.Commerce.Order;
    using EPiServer.Globalization;
    using EPiServer.Logging;
    using EPiServer.ServiceLocation;
    using Mediachase.Commerce;
    using Mediachase.Commerce.Markets;
    using Mediachase.Commerce.Orders.Dto;
    using Mediachase.Commerce.Orders.Managers;
    using PayEx.Checkout.Episerver.Common;
    using PayEx.Checkout.Episerver.Extensions;
    using PayEx.Checkout.Episerver.OrderManagement;
    using SwedbankPay.Client;
    using SwedbankPay.Client.Models;
    using SwedbankPay.Client.Models.Response;
    using System;
    using System.Linq;

    [ServiceConfiguration(typeof(IPayExCheckoutService))]
    public class PayExCheckoutService : IPayExCheckoutService
    {
        private readonly ICurrentMarket _currentMarket;
        private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;
        private readonly IMarketService _marketService;
        private readonly IOrderRepository _orderRepository;
        private readonly SwedbankPayOrderServiceFactory _swedbankPayOrderServiceFactory;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(PayExCheckoutService));
        private PaymentMethodDto _paymentMethodDto;
        private SwedbankPayOptions _checkoutConfiguration;
        private readonly IRequestFactory _requestFactory;

        public PaymentMethodDto PaymentMethodDto => _paymentMethodDto ?? (_paymentMethodDto = PaymentManager.GetPaymentMethodBySystemName(Constants.PayExCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name, returnInactive: true));

        public PayExCheckoutService(
            ICurrentMarket currentMarket,
            ICheckoutConfigurationLoader checkoutConfigurationLoader,
            IMarketService marketService,
            IOrderRepository orderRepository,
            SwedbankPayOrderServiceFactory swedbankPayOrderServiceFactory, IRequestFactory requestFactory)
        {
            _currentMarket = currentMarket;
            _checkoutConfigurationLoader = checkoutConfigurationLoader;
            _marketService = marketService;
            _orderRepository = orderRepository;
            _swedbankPayOrderServiceFactory = swedbankPayOrderServiceFactory;
            _requestFactory = requestFactory;
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

            var initiateConsumerSessionRequestObject = _requestFactory.Create(market, email, mobilePhone, ssn); 

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
                //var paymentOrderRequestContainer = GetCheckoutOrderData(orderGroup, market, PaymentMethodDto, consumerProfileRef);
                var paymentOrderRequestContainer = _requestFactory.Create(orderGroup, market, PaymentMethodDto, consumerProfileRef);
                
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

            var paymentOrderRequestContainer = _requestFactory.Create(orderGroup, market, PaymentMethodDto);
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

        public CheckoutConfiguration LoadCheckoutConfiguration(IMarket market)
        {
            return _checkoutConfigurationLoader.GetConfiguration(market.MarketId, market.DefaultLanguage.Name);
        }

        public SwedbankPayOptions GetConfiguration(IMarket market)
        {
            return _checkoutConfigurationLoader.GetConfiguration(market.MarketId, market.DefaultLanguage.Name).ToSwedbankPayConfiguration();
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
