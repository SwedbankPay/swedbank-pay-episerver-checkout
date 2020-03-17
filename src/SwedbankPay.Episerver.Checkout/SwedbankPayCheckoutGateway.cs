using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.OrderManagement.Steps;
using SwedbankPay.Episerver.Checkout.Steps;

using System;
using System.Linq;

namespace SwedbankPay.Episerver.Checkout
{
    public class SwedbankPayCheckoutGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(SwedbankPayCheckoutGateway));

        private IOrderForm _orderForm;    
        public IOrderGroup OrderGroup { get; set; }
        
        internal Injected<SwedbankPayClientFactory> InjectedSwedbankPayClientFactory { get; set; }
        private SwedbankPayClientFactory SwedbankPayClientFactory => InjectedSwedbankPayClientFactory.Service;
        
        internal Injected<IMarketService> InjectedMarketService { get; set; }
        private IMarketService MarketService => InjectedMarketService.Service;

        internal Injected<IRequestFactory> InjectedRequestFactory { get; set; }
        private IRequestFactory RequestFactory => InjectedRequestFactory.Service;

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
                Logger.Debug("SwedbankPay checkout gateway. Processing Payment ....");

                if (_orderForm == null)
                {
                    _orderForm = OrderGroup.Forms.FirstOrDefault(form => form.Payments.Contains(payment));
                }

                var market = MarketService.GetMarket(OrderGroup.MarketId);
                var authorizePaymentStep = new AuthorizePaymentStep(payment, market, SwedbankPayClientFactory);
                var capturePaymentStep = new CapturePaymentStep(payment, market, SwedbankPayClientFactory, RequestFactory);
                var creditPaymentStep = new CreditPaymentStep(payment, market, SwedbankPayClientFactory, RequestFactory);
                var cancelPaymentStep = new CancelPaymentStep(payment, market, SwedbankPayClientFactory, RequestFactory);

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