namespace PayEx.Checkout.Episerver.OrderManagement.Steps
{
    using EPiServer.Commerce.Order;
    using EPiServer.Logging;

    using Mediachase.Commerce;
    using Mediachase.Commerce.Markets;
    using Mediachase.Commerce.Orders;

    using PayEx.Checkout.Episerver.Common;
    using PayEx.Checkout.Episerver.Common.Helpers;

    using SwedbankPay.Client.Models.Common;
    using SwedbankPay.Client.Models.Request.Transaction;

    using System;
    using System.Linq;
    using System.Net;

    public class CapturePaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CapturePaymentStep));
        private IMarket _market;

        public CapturePaymentStep(IPayment payment, MarketId marketId, SwedbankPayOrderServiceFactory swedbankPayOrderServiceFactory, IMarketService marketService) : base(payment, marketId, swedbankPayOrderServiceFactory)
        {
            _market = marketService.GetMarket(marketId);
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message)
        {
            if (payment.TransactionType == TransactionType.Capture.ToString())
            {
                var amount = AmountHelper.GetAmount((decimal) payment.Amount);
                var orderId = orderGroup.Properties[Constants.PayExOrderIdField]?.ToString();
                if (!string.IsNullOrEmpty(orderId))
                {
                    try
                    {
                        if (shipment == null)
                        {
                            throw new InvalidOperationException("Can't find correct shipment");
                        }

                        var orderItems = Enumerable.ToList<OrderItem>(shipment.LineItems.Select(l => FromLineItem(l, orderGroup.Currency)));
                        orderItems.Add(new OrderItem
                        {
                            Amount = AmountHelper.GetAmount(shipment.GetShippingCost(_market, orderGroup.Currency)),
                            Description = "Shipping cost"
                        });

                        var transaction = new TransactionRequest
                        {
                            Description = "Capturing the authorized payment",
                            Amount = amount,
                            VatAmount = 0,
                            PayeeReference = "",
                            OrderItems = orderItems
                        };

                        var transactionResponse = SwedbankPayOrderService.Capture(new TransactionRequestContainer(transaction), orderId).Result;
                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, transactionResponse == null
                                ? $"Capture is not possible on this order {orderId}"
                                : $"Captured {payment.Amount}, id {transactionResponse.Id}");
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
                return true;
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
                Name = item.DisplayName,
                Type = "",
                Class = "",
                ItemUrl = "",
                ImageUrl = "",
                Description = "",
                DiscountDescription = "",
                Quantity = (int)item.Quantity,
                QuantityUnit = "",
                UnitPrice = AmountHelper.GetAmount(item.PlacedPrice),
                DiscountPrice = AmountHelper.GetAmount(item.GetDiscountedPrice(currency)),
                VatPercent = 0,
                Amount = AmountHelper.GetAmount(item.GetExtendedPrice(currency)),
                VatAmount = 0
            };

            return itemDescription;
        }
    }
}
