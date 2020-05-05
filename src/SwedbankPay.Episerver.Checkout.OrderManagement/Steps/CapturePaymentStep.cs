using EPiServer.Commerce.Order;
using EPiServer.Logging;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Sdk;

using System;

using TransactionType = Mediachase.Commerce.Orders.TransactionType;

namespace SwedbankPay.Episerver.Checkout.OrderManagement.Steps
{
    public class CapturePaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CapturePaymentStep));
        private readonly IMarket _market;
        private readonly IRequestFactory _requestFactory;

        public CapturePaymentStep(IPayment payment, IMarket market, SwedbankPayClientFactory swedbankPayClientFactory, IRequestFactory requestFactory) : base(payment, market, swedbankPayClientFactory)
        {
            _market = market;
            _requestFactory = requestFactory;
        }

        public override PaymentStepResult Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment)
        {
            var paymentStepResult = new PaymentStepResult();

            if (payment.TransactionType == TransactionType.Capture.ToString())
            {
                var orderId = orderGroup.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
                if (!string.IsNullOrEmpty(orderId))
                {
                    long? remainingCaptureAmount = null;
                    try
                    {
                        if (shipment == null)
                        {
                            throw new InvalidOperationException("Can't find correct shipment");
                        }

                        var paymentOrder = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrders.Get(new Uri(orderId, UriKind.Relative)));
                        if (paymentOrder.Operations.Capture == null)
                        {
                            remainingCaptureAmount = paymentOrder.PaymentOrderResponse.RemainingCaptureAmount?.Value;
                            if (!remainingCaptureAmount.HasValue || remainingCaptureAmount.Value == 0)
                            {
                                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Capture is not possible on this order {orderId}, capture already performed");
                                paymentStepResult.Status = true;
                                return paymentStepResult;
                            }

                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Capture is not possible on this order {orderId}");
                            return paymentStepResult;
                        }

                        var captureRequest = _requestFactory.GetCaptureRequest(payment, _market, shipment);
                        var captureResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Capture(captureRequest));

                        if (captureResponse.Capture.Transaction.Type == Sdk.TransactionType.Capture && captureResponse.Capture.Transaction.State.Equals(State.Completed))
                        {
                            payment.ProviderTransactionID = captureResponse.Capture.Transaction.Number;
                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Order captured at SwedbankPay, Transaction number: {captureResponse.Capture.Transaction.Number}");
                            paymentStepResult.Status = true;
                        }

                        return paymentStepResult;
                    }
                    catch (Exception ex)
                    {
                        payment.Status = PaymentStatus.Failed.ToString();
                        paymentStepResult.Message = ex.Message;
                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {ex.Message}, remaining capture amount: {remainingCaptureAmount}");
                        Logger.Error(ex.Message, ex);
                    }
                }

                return paymentStepResult;
            }

            if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, shipment);
            }

            return paymentStepResult;
        }
    }
}
