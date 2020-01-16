using System;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.Common.Helpers;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.PaymentOrders;

namespace SwedbankPay.Episerver.Checkout.OrderManagement.Steps
{
    public class CapturePaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CapturePaymentStep));
        private readonly IMarket _market;
        private readonly IRequestFactory _requestFactory;

        public CapturePaymentStep(IPayment payment, IMarket market, SwedbankPayClientFactory swedbankPayClientFactory, IMarketService marketService, IRequestFactory requestFactory) : base(payment, market, swedbankPayClientFactory)
        {
            _market = market;
            _requestFactory = requestFactory;
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message)
        {
            if (payment.TransactionType == TransactionType.Capture.ToString())
            {
                var amount = AmountHelper.GetAmount((decimal) payment.Amount);
                var orderId = orderGroup.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
                if (!string.IsNullOrEmpty(orderId))
                {
                    try
                    {
                        if (shipment == null)
                        {
                            throw new InvalidOperationException("Can't find correct shipment");
                        }
                        
                        var captureRequest = _requestFactory.GetCaptureRequest(payment, _market, shipment, addShipmentInOrderItem: true);
                        var paymentOrder = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrder.Get(new Uri(orderId)));
                        
                        if (paymentOrder.Operations.Capture != null)
                        {
                            var captureResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Capture(captureRequest));

                            if (captureResponse.Capture.Transaction.Type == "Capture" && captureResponse.Capture.Transaction.State.Equals(State.Completed))
                            {
                                payment.Status = PaymentStatus.Processed.ToString();
                                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Order captured at SwedbankPay");
                                return true;
                            }
                        }
                        else
                        {
                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Capture is not possible on this order {orderId}");
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
                return false;

            }
            else if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, shipment, ref message);
            }

            return false;
        }
    }
}
