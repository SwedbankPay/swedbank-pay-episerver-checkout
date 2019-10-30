using System;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using SwedbankPay.Checkout.Episerver.Common;
using SwedbankPay.Checkout.Episerver.Common.Helpers;
using SwedbankPay.Sdk.PaymentOrders;
using SwedbankPay.Sdk.Transactions;

namespace SwedbankPay.Checkout.Episerver.OrderManagement.Steps
{
    public class CapturePaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CapturePaymentStep));
        private readonly IMarket _market;
        private readonly IRequestFactory _requestFactory;

        public CapturePaymentStep(IPayment payment, IMarket market, SwedbankPayOrderServiceFactory swedbankPayOrderServiceFactory, IMarketService marketService, IRequestFactory requestFactory) : base(payment, market, swedbankPayOrderServiceFactory)
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

                        var captureTransactionRequest = _requestFactory.GetTransactionRequest(payment, _market, shipment, description:"Capturing the authorized payment");
                        
                        var transactionResponse = AsyncHelper.RunSync(() =>
                            SwedbankPayOrderService.Capture(new TransactionRequestContainer(captureTransactionRequest), orderId));
                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, transactionResponse == null
                                ? $"Capture is not possible on this order {orderId}"
                                : $"Captured {payment.Amount}, id {transactionResponse.Id}");
                        return true;
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

        private OrderItem FromLineItem(ILineItem item, Currency currency)
        {
            var itemDescription = new OrderItem
            {
                Reference = item.Code,
                Amount = AmountHelper.GetAmount(item.GetExtendedPrice(currency)),
                Class = "FASHION", //TODO Get Value from interface 
                Description = "",
                DiscountDescription = "",
                DiscountPrice = AmountHelper.GetAmount(item.GetDiscountedPrice(currency)),
                ImageUrl = "",
                ItemUrl = "",
                Name = item.DisplayName,
                Quantity = (int)(item.Quantity),
                QuantityUnit = "PCS", //TODO Get Value from interface
                Type = "PRODUCT", //TODO Get Value from interface
                UnitPrice = AmountHelper.GetAmount(item.PlacedPrice),
                VatAmount = 0,
                VatPercent = 0
            };

            return itemDescription;
        }
    }
}
