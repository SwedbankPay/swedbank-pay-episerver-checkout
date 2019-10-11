namespace PayEx.Checkout.Episerver
{
    using EPiServer.Commerce.Order;
    using EPiServer.Logging;
    using EPiServer.ServiceLocation;
    using Mediachase.Commerce.Markets;
    using Mediachase.Commerce.Orders;
    using Mediachase.Commerce.Plugins.Payment;
    using PayEx.Checkout.Episerver.OrderManagement;
    using PayEx.Checkout.Episerver.OrderManagement.Steps;
    using System;
    using System.Linq;

    public class PayExCheckoutGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(PayExCheckoutGateway));

        private IOrderForm _orderForm;    
        public IOrderGroup OrderGroup { get; set; }


        internal Injected<SwedbankPayOrderServiceFactory> InjectedPayExOrderServiceFactory { get; set; }
        private SwedbankPayOrderServiceFactory SwedbankPayOrderServiceFactory => InjectedPayExOrderServiceFactory.Service;

        internal Injected<IMarketService> InjectedMarketService { get; set; }
        private IMarketService MarketService => InjectedMarketService.Service;


        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            OrderGroup = orderGroup;
            _orderForm = orderGroup.GetFirstForm();

            var message = string.Empty;
            return ProcessPayment(payment, ref message)
                ? PaymentProcessingResult.CreateSuccessfulResult(message)
                : PaymentProcessingResult.CreateUnsuccessfulResult(message);
        }

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            OrderGroup = payment.Parent.Parent;
            _orderForm = payment.Parent;
            return ProcessPayment(payment, ref message);
        }

        public bool ProcessPayment(IPayment payment, ref string message)
        {
            try
            {
                Logger.Debug("PayEx checkout gateway. Processing Payment ....");

                if (_orderForm == null)
                {
                    _orderForm = OrderGroup.Forms.FirstOrDefault(form => form.Payments.Contains(payment));
                }

                var market = MarketService.GetMarket(OrderGroup.MarketId);
                var authorizePaymentStep = new AuthorizePaymentStep(payment, market, SwedbankPayOrderServiceFactory);
                var capturePaymentStep = new CapturePaymentStep(payment, market, SwedbankPayOrderServiceFactory, MarketService);
                var creditPaymentStep = new CreditPaymentStep(payment, market, SwedbankPayOrderServiceFactory);
                var cancelPaymentStep = new CancelPaymentStep(payment, market, SwedbankPayOrderServiceFactory);

                authorizePaymentStep.SetSuccessor(capturePaymentStep);
                capturePaymentStep.SetSuccessor(creditPaymentStep); 
                creditPaymentStep.SetSuccessor(cancelPaymentStep);

                return authorizePaymentStep.Process(payment, _orderForm, OrderGroup, OrderGroup.GetFirstShipment(), ref message);
            }
            catch (Exception ex)
            {
                Logger.Error("Process checkout failed with error: " + ex.Message, ex);
                message = ex.Message;
                throw;
            }
        }
    }
}
