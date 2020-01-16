using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.Common.Helpers;
using SwedbankPay.Episerver.Checkout.OrderManagement.Extensions;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.PaymentOrders;
using SwedbankPay.Sdk.Payments;

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
                    //var previousPayment = orderForm.Payments.FirstOrDefault(x => x.IsSwedbankPayPayment());
                    //if (previousPayment != null && previousPayment.TransactionType == TransactionType.Sale.ToString())
                    //{
                    //    var cancelRequest = _requestFactory.GetCancelRequest();
                    //    var paymentOrder = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrder.Get(new Uri(orderId)));
                    //    paymentOrder.Operations.Cancel
                    //    var cancelResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Cancel(cancelRequest));
                        
                    //    if (reversalResponseObject == null)
                    //    {
                    //        payment.Status = PaymentStatus.Failed.ToString();
                    //        message = "Reversal is not a valid operation";
                    //        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {message}");
                    //        Logger.Error($"Reversal is not a valid operation for {orderId}");
                    //        return false;
                    //    }

                    //    payment.Status = PaymentStatus.Processed.ToString();

                    //    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Refunded {payment.Amount}");
                    //}

                    if (string.IsNullOrEmpty(orderId)) return false;
                    var cancelRequest = _requestFactory.GetCancelRequest();
                    var paymentOrder = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrder.Get(new Uri(orderId)));
                    if (paymentOrder.Operations.Cancel != null)
                    {
                        var cancelResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Cancel(cancelRequest));
                        if (cancelResponse.Cancellation.Transaction.Type == "Cancel" && cancelResponse.Cancellation.Transaction.State.Equals(State.Completed))
                        {
                            payment.Status = PaymentStatus.Processed.ToString();
                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Order cancelled at SwedbankPay");
                            return true;
                        }
                    }
                        
                    else
                    {
                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Cancel is not possible on this order {orderId}");
                        return false;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    var exceptionMessage = GetExceptionMessage(ex);

                    payment.Status = PaymentStatus.Failed.ToString();
                    message = exceptionMessage;
                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {exceptionMessage}");
                    Logger.Error(exceptionMessage, ex);
                    return false;
                }
            }
            else if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, shipment, ref message);
            }
            return false;
        }
    }
}