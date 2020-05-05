using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
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
            _requestFactory = requestFactory;
            _market = market;
        }

        public override PaymentStepResult Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment)
        {
            var paymentStepResult = new PaymentStepResult();

            if (payment.TransactionType == TransactionType.Void.ToString())
            {
                try
                {
                    var orderId = orderGroup.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
                    var previousPayment = orderForm.Payments.FirstOrDefault(x => x.IsSwedbankPayPayment());
                    
                    //If payed by swish, do a reversal
                    if (previousPayment != null && previousPayment.TransactionType == TransactionType.Sale.ToString() && !string.IsNullOrWhiteSpace(orderId))
                    {
                        var paymentOrder = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrders.Get(new Uri(orderId, UriKind.Relative)));
                        if (paymentOrder.Operations.Reverse == null)
                        {
                            paymentStepResult.Message = "Reversal is not a valid operation";

                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"{paymentStepResult.Message}");
                        }
                        else
                        {
                            var reversalRequest = _requestFactory.GetReversalRequest(payment, orderForm.GetAllLineItems(), _market, shipment, description: "Cancelling purchase order");
                            var reversalResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Reverse(reversalRequest));
                            if (reversalResponse.Reversal.Transaction.Type == Sdk.TransactionType.Reversal)
                            {
                                payment.Status = PaymentStatus.Processed.ToString();
                                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Refunded {payment.Amount}");
                                paymentStepResult.Status = true;
                                return paymentStepResult;
                            }
                            else
                            {
                                paymentStepResult.Message = "Error when executing reversal";
                                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {paymentStepResult.Message}");
                            }
                        }
                    }

                    else if (!string.IsNullOrWhiteSpace(orderId))
                    {
                        var paymentOrder = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrders.Get(new Uri(orderId, UriKind.Relative)));
                        if (paymentOrder.Operations.Cancel == null)
                        {
                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Cancel is not possible on this order {orderId}");
                            return paymentStepResult;
                        }

                        var cancelRequest = _requestFactory.GetCancelRequest();
                        var cancelResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Cancel(cancelRequest));
                        if (cancelResponse.Cancellation.Transaction.Type == Sdk.TransactionType.Cancellation && cancelResponse.Cancellation.Transaction.State.Equals(State.Completed))
                        {
                            payment.Status = PaymentStatus.Processed.ToString();
                            payment.TransactionID = cancelResponse.Cancellation.Transaction.Number;
                            payment.ProviderTransactionID = cancelResponse.Cancellation.Transaction.Id.ToString();
                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Order cancelled at SwedbankPay");
                            return paymentStepResult;
                        }
                    }

                    return paymentStepResult;
                }
                catch (Exception ex)
                {
                    payment.Status = PaymentStatus.Failed.ToString();
                    paymentStepResult.Message = ex.Message;
                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {ex.Message}");
                    Logger.Error(ex.Message, ex);
                }
            }

            if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, shipment);
            }

            return paymentStepResult;
        }
    }
}