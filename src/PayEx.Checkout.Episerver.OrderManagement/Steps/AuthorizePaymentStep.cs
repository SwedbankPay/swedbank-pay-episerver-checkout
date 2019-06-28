namespace PayEx.Checkout.Episerver.OrderManagement.Steps
{
    using EPiServer.Commerce.Order;
    using EPiServer.Logging;
    using Mediachase.Commerce;
    using Mediachase.Commerce.Orders;
    using PayEx.Checkout.Episerver.Common;
    using System;
    using System.Net;
    using PayEx.Net.Api.Exceptions;

    public class AuthorizePaymentStep : AuthorizePaymentStepBase
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(AuthorizePaymentStep));
        
        public AuthorizePaymentStep(IPayment payment, MarketId marketId, PayExOrderServiceFactory payExOrderServiceFactory) : base(payment, marketId, payExOrderServiceFactory)
        {
        }

        public override bool ProcessAuthorization(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            var orderId = orderGroup.Properties[Constants.PayExCheckoutOrderIdCartField]?.ToString();
            try
            {
                var result = PayExOrderService.GetOrder(orderId);

                if (result != null)
                {
                    return true;
                }

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Authorize completed");
            }
            catch (Exception ex) when (ex is PayExException || ex is WebException)
            {
                var exceptionMessage = GetExceptionMessage(ex);

                payment.Status = PaymentStatus.Failed.ToString();
                message = exceptionMessage;
                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {exceptionMessage}");
                Logger.Error(exceptionMessage, ex);

                return false;
            }

            return true;
        }
    }
}