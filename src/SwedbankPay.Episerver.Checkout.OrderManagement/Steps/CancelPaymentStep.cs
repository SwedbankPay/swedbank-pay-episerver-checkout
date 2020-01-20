using EPiServer.Commerce.Order;
using EPiServer.Logging;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Sdk;

using System;

namespace SwedbankPay.Episerver.Checkout.OrderManagement.Steps
{
    public class CancelPaymentStep : PaymentStep
    {
        private readonly IRequestFactory _requestFactory;
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CancelPaymentStep));

        public CancelPaymentStep(IPayment payment, IMarket market, SwedbankPayClientFactory swedbankPayClientFactory, IRequestFactory requestFactory) : base(payment, market, swedbankPayClientFactory)
        {
            _requestFactory = requestFactory;
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message)
        {
            if (payment.TransactionType == TransactionType.Void.ToString())
            {
                try
                {
                    var orderId = orderGroup.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
                    if (string.IsNullOrEmpty(orderId)) return false;
                    
                    var paymentOrder = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrder.Get(new Uri(orderId)));
                    if (paymentOrder.Operations.Cancel == null)
                    {
                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Cancel is not possible on this order {orderId}");
                        return false;
                    }

                    var cancelRequest = _requestFactory.GetCancelRequest();
                    var cancelResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Cancel(cancelRequest));
                    if (cancelResponse.Cancellation.Transaction.Type == "Cancel" && cancelResponse.Cancellation.Transaction.State.Equals(State.Completed))
                    {
                        payment.Status = PaymentStatus.Processed.ToString();
                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Order cancelled at SwedbankPay");
                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    payment.Status = PaymentStatus.Failed.ToString();
                    message = ex.Message;
                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {ex.Message}");
                    Logger.Error(ex.Message, ex);
                    return false;
                }
            }

            if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, shipment, ref message);
            }

            return false;
        }
    }
}