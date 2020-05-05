using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.OrderManagement.Steps;
using SwedbankPay.Episerver.Checkout.Steps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwedbankPay.Episerver.Checkout
{
    public class SwedbankPayCheckoutGateway : ISplitPaymentPlugin, ISplitPaymentGateway, IPaymentPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(SwedbankPayCheckoutGateway));

        private IOrderForm _orderForm;
        private IShipment _shipment;

        public IOrderGroup OrderGroup { get; set; }

        internal Injected<SwedbankPayClientFactory> InjectedSwedbankPayClientFactory { get; set; }
        private SwedbankPayClientFactory SwedbankPayClientFactory => InjectedSwedbankPayClientFactory.Service;

        internal Injected<IMarketService> InjectedMarketService { get; set; }
        private IMarketService MarketService => InjectedMarketService.Service;

        internal Injected<IRequestFactory> InjectedRequestFactory { get; set; }
        private IRequestFactory RequestFactory => InjectedRequestFactory.Service;


        /// <summary>
        /// Returns the configuration data associated with a plugin.
        /// Sets the configuration plugin data. This data typically includes
        /// information like plugin URL, account info and so on.
        /// </summary>
        public virtual IDictionary<string, string> Settings { get; set; }

        /// <summary>
        /// Processes the payment. Can be used for both positive and negative transactions.
        /// </summary>
        /// <param name="orderGroup">The order group.</param>
        /// <param name="payment">The payment.</param>
        /// <returns>The payment processing result.</returns>
        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            OrderGroup = orderGroup;
            _orderForm = orderGroup.GetFirstForm();

            var message = string.Empty;
            return ProcessPayment(payment, ref message)
                ? PaymentProcessingResult.CreateSuccessfulResult(message)
                : PaymentProcessingResult.CreateUnsuccessfulResult(message);
        }

        /// <summary>
        /// Processes a payment. Can be used for both positive and negative transactions.
        /// </summary>
        /// <param name="payment">The payment to be processed.</param>
        /// <param name="message">The message passed back, most often an error if transaction failed.</param>
        /// <returns><c>True</c> if process successfully; otherwise <c>False</c>.</returns>
        public bool ProcessPayment(Payment payment, ref string message)
        {
            return ProcessPayment(payment as IPayment, ref message);
        }

        public bool ProcessPayment(IPayment payment, ref string message)
        {
            if (_orderForm == null)
            {
                _orderForm = OrderGroup.GetFirstForm();
            }

            _shipment = _orderForm.Shipments.FirstOrDefault();

            var result = ProcessPayment(payment, _shipment);
            message = result.Message;

            return result.Status;
        }


        /// <summary>Process payment associated with shipment.</summary>
        /// <param name="payment">The payment</param>
        /// <param name="shipment">The shipment</param>
        /// <param name="message">The message</param>
        /// <returns>True if process successful, otherwise false</returns>
        public bool ProcessPayment(Payment payment, Shipment shipment, ref string message)
        {
            var result = ProcessPayment(payment as IPayment, shipment);
            message = result.Message;
            return result.Status;
        }

        /// <summary>Process payment associated with shipment.</summary>
        /// <param name="orderGroup">The order group.</param>
        /// <param name="payment">The payment.</param>
        /// <param name="shipment">The shipment.</param>
        /// <returns><c>True</c> if process successful, otherwise <c>False</c>.</returns>
        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment, IShipment shipment)
        {
            OrderGroup = orderGroup;
            _orderForm = orderGroup.GetFirstForm();
            var result = ProcessPayment(payment, shipment);
            var message = result.Message;
            return result.Status
                ? PaymentProcessingResult.CreateSuccessfulResult(message)
                : PaymentProcessingResult.CreateUnsuccessfulResult(message);
        }

        public PaymentStepResult ProcessPayment(IPayment payment, IShipment shipment)
        {
            var paymentStepResult = new PaymentStepResult();
            _shipment = shipment;

            if (_orderForm == null)
            {
                _orderForm = (payment as Payment)?.Parent ?? OrderGroup?.Forms.FirstOrDefault(form => form.Payments.Contains(payment));
            }
            if (OrderGroup == null)
            {
                OrderGroup = (_orderForm as OrderForm)?.Parent;
            }

            if (OrderGroup == null)
            {
                paymentStepResult.Message = "OrderGroup is null";
                throw new Exception(paymentStepResult.Message);
            }

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

                return authorizePaymentStep.Process(payment, _orderForm, OrderGroup, _shipment);
            }
            catch (Exception ex)
            {
                Logger.Error("Process checkout failed with error: " + ex.Message, ex);
                paymentStepResult.Message = ex.Message;
                throw;
            }
        }
    }
}