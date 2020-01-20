using EPiServer.Commerce.Order;
using EPiServer.Logging;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

using SwedbankPay.Episerver.Checkout.Common;

using System;

namespace SwedbankPay.Episerver.Checkout.OrderManagement.Steps
{
    public class AuthorizePaymentStep : AuthorizePaymentStepBase
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(AuthorizePaymentStep));
        
        public AuthorizePaymentStep(IPayment payment, IMarket market, SwedbankPayClientFactory swedbankPayClientFactory) : base(payment, market, swedbankPayClientFactory)
        {
        }

        public override bool ProcessAuthorization(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            var orderId = orderGroup.Properties[Constants.SwedbankPayCheckoutOrderIdCartField]?.ToString();
            if (!string.IsNullOrEmpty(orderId))
            {
                try
                {
                    var result = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrder.Get(new Uri(orderId, UriKind.Relative)));
                    if (result != null)
                    {
                        return true;
                    }

                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Authorize completed");
                }
                catch (Exception ex)
                {
                    payment.Status = PaymentStatus.Failed.ToString();
                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {ex.Message}");
                    Logger.Error(ex.Message, ex);

                    return false;
                }

                return true;
            }

            return false;
        }
    }
}