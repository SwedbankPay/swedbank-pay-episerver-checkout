namespace PayEx.Checkout.Episerver.OrderManagement.Steps
{
    using EPiServer.Commerce.Order;
    using EPiServer.Logging;
    using Mediachase.Commerce;
    using Mediachase.Commerce.Markets;
    using Mediachase.Commerce.Orders;
    using PayEx.Checkout.Episerver.Common;
    using PayEx.Checkout.Episerver.Common.Helpers;
    using PayEx.Net.Api.Exceptions;
    using PayEx.Net.Api.Models;
    using System;
    using System.Linq;
    using System.Net;

    public class CapturePaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CapturePaymentStep));
        private IMarket _market;

        public CapturePaymentStep(IPayment payment, MarketId marketId, PayExOrderServiceFactory payExOrderServiceFactory, IMarketService marketService) : base(payment, marketId, payExOrderServiceFactory)
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

                        var itemDescriptions = Enumerable.ToList<ItemDescription>(shipment.LineItems.Select(l => FromLineItem(l, orderGroup.Currency)));
                        itemDescriptions.Add(new ItemDescription
                        {
                            Amount = AmountHelper.GetAmount(shipment.GetShippingCost(_market, orderGroup.Currency)),
                            Description = "Shipping cost"
                        });

                        var vatSummaries = shipment.LineItems.Select(l => TaxFromLineItem(l, orderGroup.Currency, shipment)).ToList();
                        vatSummaries.Add(new VatSummary
                        {
                            Amount = AmountHelper.GetAmount(orderGroup.GetShippingTotal()),
                            VatAmount =AmountHelper.GetAmount(orderGroup.GetShippingTotal() - orderGroup.GetShippingSubTotal()),
                            VatPercent = 0,  //TODO: PayEx Get correct tax value
                        });

                        var transaction = new Transaction
                        {
                            Amount = amount,
                            ItemDescriptions = itemDescriptions,
                            Description = "Capturing the authorized payment",
                            VatSummary = vatSummaries,
                        };

                        var checkoutConfiguration = PayExOrderService.Capture(new PaymentOrderTransactionObject { Transaction = transaction }, orderId);
                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType,
                            checkoutConfiguration == null
                                ? $"Capture is not possible on this order {orderId}"
                                : $"Captured {payment.Amount}, id {checkoutConfiguration.Capture.Id}");
                    }
                    catch (Exception ex) when (ex is PayExException || ex is WebException)
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

        private VatSummary TaxFromLineItem(ILineItem item, Currency currency, IShipment shipment)
        {
            var vatSummary = new VatSummary
            {
                Amount = AmountHelper.GetAmount(item.GetExtendedPrice(currency).Amount),
                VatAmount = AmountHelper.GetAmount(item.GetSalesTax(_market, currency, shipment.ShippingAddress).Amount),
                VatPercent = AmountHelper.GetAmount(0) //TODO: PayEx Get correct tax value
            };

            return vatSummary;
        }

        private ItemDescription FromLineItem(ILineItem item, Currency currency)
        {
            var itemDescription = new ItemDescription
            {
                Description = item.DisplayName,
                Amount = AmountHelper.GetAmount(item.GetExtendedPrice(currency).Amount)
            };

            return itemDescription;
        }
    }
}
