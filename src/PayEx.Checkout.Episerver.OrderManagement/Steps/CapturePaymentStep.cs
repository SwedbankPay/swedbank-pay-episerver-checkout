using EPiServer.Commerce.Order.Internal;

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

        public CapturePaymentStep(IPayment payment, IMarket market, SwedbankPayOrderServiceFactory swedbankPayOrderServiceFactory, IMarketService marketService) : base(payment, market, swedbankPayOrderServiceFactory)
        {
            _market = market;
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
                            Type = "SHIPPING_FEE",
                            Reference = "SHIPPING",
                            Quantity = 1,
                            DiscountPrice = AmountHelper.GetAmount(orderGroup.GetShippingTotal().Amount),
                            DiscountDescription = "",
                            Name = "SHIPPINGFEE",
                            VatAmount = 0, //TODO Get correct value
                            ItemUrl = "", //TODO Get correct value
                            ImageUrl = "", //TODO Get correct value
                            Description = "Shipping fee",
                            Amount = AmountHelper.GetAmount(orderGroup.GetShippingTotal().Amount),
                            Class = "NOTAPPLICABLE",
                            UnitPrice = AmountHelper.GetAmount(orderGroup.GetShippingTotal().Amount),
                            QuantityUnit = "PCS",
                            VatPercent = 0 //TODO Get correct value
                        });

                        var transaction = new TransactionRequest
                        {
                            Description = "Capturing the authorized payment",
                            Amount = amount,
                            VatAmount = 0,
                            PayeeReference = "",
                            
                            OrderItems = orderItems
                        };

                        var transactionResponse = AsyncHelper.RunSync(() =>
                            SwedbankPayOrderService.Capture(new TransactionRequestContainer(transaction), orderId));
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
