using EPiServer.Commerce.Order;
using EPiServer.Logging;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.OrderManagement.Extensions;
using SwedbankPay.Sdk;

using System;
using System.Linq;
using TransactionType = Mediachase.Commerce.Orders.TransactionType;

namespace SwedbankPay.Episerver.Checkout.OrderManagement.Steps
{
    public class CancelPaymentStep : PaymentStep
    {
        private readonly IMarket _market;
        private readonly IRequestFactory _requestFactory;
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CancelPaymentStep));

        public CancelPaymentStep(IPayment payment, IMarket market, SwedbankPayClientFactory swedbankPayClientFactory, IRequestFactory requestFactory) : base(payment, market, swedbankPayClientFactory)
        {
            _market = market;
            _requestFactory = requestFactory;
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message)
        {
            if (payment.TransactionType == TransactionType.Void.ToString())
            {
                try
                {
                    var orderId = orderGroup.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
                    var previousPayment = orderForm.Payments.FirstOrDefault(x => x.IsSwedbankPayPayment());
                    
                    //If payed by swish, do a reversal
                    if (previousPayment != null && previousPayment.TransactionType == TransactionType.Sale.ToString() && !string.IsNullOrWhiteSpace(orderId))
                    {
                        var paymentOrder = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrder.Get(new Uri(orderId, UriKind.Relative)));
                        if (paymentOrder.Operations.Reversal == null)
                        {
                            message = "Reversal is not a valid operation";
                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"{message}");
                        }
                        else
                        {
                            var reversalRequest = _requestFactory.GetReversalRequest(payment, _market, shipment, description: "Cancelling purchase order");

                            var reversalResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Reversal(reversalRequest));
                            if (reversalResponse.Reversal.Transaction.Type == Sdk.TransactionType.Reversal)
                            {
                                payment.Status = PaymentStatus.Processed.ToString();
                                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Refunded {payment.Amount}");
                                return true;
                            }
                            else
                            {
                                message = "Error when executing reversal";
                                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {message}");
                            }
                        }
                    }

                    else if (!string.IsNullOrWhiteSpace(orderId))
                    {
                        var paymentOrder = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrder.Get(new Uri(orderId, UriKind.Relative)));
                        if (paymentOrder.Operations.Cancel == null)
                        {
                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Cancel is not possible on this order {orderId}");
                            return false;
                        }

                        var cancelRequest = _requestFactory.GetCancelRequest();
                        var cancelResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Cancel(cancelRequest));
                        if (cancelResponse.Cancellation.Transaction.Type == Sdk.TransactionType.Cancellation && cancelResponse.Cancellation.Transaction.State.Equals(State.Completed))
                        {
                            payment.Status = PaymentStatus.Processed.ToString();
                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Order cancelled at SwedbankPay");
                            return true;
                        }
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