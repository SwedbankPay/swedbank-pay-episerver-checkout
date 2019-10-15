//namespace PayEx.Checkout
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using EPiServer.Commerce.Order;
//    using EPiServer.Logging;
//    using Mediachase.Commerce.Markets;

//    public abstract class PayexService
//    {
//        private readonly IMarketService _marketService;
//        private readonly IOrderRepository _orderRepository;
//        private readonly IOrderGroupCalculator _orderGroupCalculator;
//        private readonly IPaymentProcessor _paymentProcessor;
//        private readonly ILogger _logger = LogManager.GetLogger(typeof(PayexService));
        
//        protected PayexService(
//            IMarketService marketService,
//            IOrderRepository orderRepository,
//            IOrderGroupCalculator orderGroupCalculator,
//            IPaymentProcessor paymentProcessor)
//        {
//            _marketService = marketService;
//            _orderRepository = orderRepository;
//            _orderGroupCalculator = orderGroupCalculator;
//            _paymentProcessor = paymentProcessor;
//        }


//        public List<OrderLine> GetOrderLines(ICart cart, OrderGroupTotals orderGroupTotals, bool sendProductAndImageUrlField)
//        {
//            var shipment = cart.GetFirstShipment();
//            var market = _marketService.GetMarket(cart.MarketId);
//            var currentCountry = shipment.ShippingAddress?.CountryCode ?? market.Countries.FirstOrDefault();

//            var includedTaxesOnLineItems = !CountryCodeHelper.GetContinentByCountry(currentCountry).Equals("NA", StringComparison.InvariantCultureIgnoreCase);
//            return GetOrderLines(cart, orderGroupTotals, includedTaxesOnLineItems, sendProductAndImageUrlField);
//        }

//        public List<OrderLine> GetOrderLines(ICart cart, OrderGroupTotals orderGroupTotals, bool includeTaxOnLineItems, bool sendProductAndImageUrl)
//        {
//            return includeTaxOnLineItems ? GetOrderLinesWithTax(cart, orderGroupTotals, sendProductAndImageUrl) : GetOrderLinesWithoutTax(cart, orderGroupTotals, sendProductAndImageUrl);
//        }

//        private List<OrderLine> GetOrderLinesWithoutTax(ICart cart, OrderGroupTotals orderGroupTotals, bool sendProductAndImageUrl)
//        {
//            var shipment = cart.GetFirstShipment();
//            var orderLines = new List<OrderLine>();

//            // Line items
//            foreach (var lineItem in cart.GetAllLineItems())
//            {
//                var orderLine = lineItem.GetOrderLine(sendProductAndImageUrl);
//                orderLines.Add(orderLine);
//            }

//            // Shipment
//            if (shipment != null && orderGroupTotals.ShippingTotal.Amount > 0)
//            {
//                var shipmentOrderLine = shipment.GetOrderLine(cart, orderGroupTotals, false);
//                orderLines.Add(shipmentOrderLine);
//            }

//            // Sales tax
//            orderLines.Add(new PatchedOrderLine()
//            {
//                Type = "sales_tax",
//                Name = "Sales Tax",
//                Quantity = 1,
//                TotalAmount = AmountHelper.GetAmount(orderGroupTotals.TaxTotal),
//                UnitPrice = AmountHelper.GetAmount(orderGroupTotals.TaxTotal),
//                TotalTaxAmount = 0,
//                TaxRate = 0
//            });

//            // Order level discounts
//            var orderDiscount = cart.GetOrderDiscountTotal();
//            var entryLevelDiscount = cart.GetAllLineItems().Sum(x => x.GetEntryDiscount());
//            var totalDiscount = orderDiscount.Amount + entryLevelDiscount;
//            if (totalDiscount > 0)
//            {
//                orderLines.Add(new PatchedOrderLine()
//                {
//                    Type = "discount",
//                    Name = "Discount",
//                    Quantity = 1,
//                    TotalAmount = -AmountHelper.GetAmount(totalDiscount),
//                    UnitPrice = -AmountHelper.GetAmount(totalDiscount),
//                    TotalTaxAmount = 0,
//                    TaxRate = 0
//                });
//            }
//            return orderLines;
//        }

//        private List<OrderLine> GetOrderLinesWithTax(ICart cart, OrderGroupTotals orderGroupTotals, bool sendProductAndImageUrl)
//        {
//            var shipment = cart.GetFirstShipment();
//            var orderLines = new List<OrderLine>();
//            var market = _marketService.GetMarket(cart.MarketId);

//            // Line items
//            foreach (var lineItem in cart.GetAllLineItems())
//            {
//                var orderLine = lineItem.GetOrderLineWithTax(market, cart.GetFirstShipment(), cart.Currency, sendProductAndImageUrl);
//                orderLines.Add(orderLine);
//            }

//            // Shipment
//            if (shipment != null && orderGroupTotals.ShippingTotal.Amount > 0)
//            {
//                var shipmentOrderLine = shipment.GetOrderLine(cart, orderGroupTotals, true);
//                orderLines.Add(shipmentOrderLine);
//            }

//            // Without tax
//            var orderLevelDiscount = AmountHelper.GetAmount(cart.GetOrderDiscountTotal());
//            if (orderLevelDiscount > 0)
//            {
//                // Order level discounts with tax
//                var totalOrderAmountWithoutDiscount = orderLines.Where(x => x.TotalAmount.HasValue).Sum(x => x.TotalAmount.Value);
//                var totalOrderAmountWithDiscount = AmountHelper.GetAmount(orderGroupTotals.Total.Amount);
//                var orderLevelDiscountIncludingTax = totalOrderAmountWithoutDiscount - totalOrderAmountWithDiscount;

//                // Tax
//                var discountTax = (orderLevelDiscountIncludingTax - orderLevelDiscount);

//                orderLines.Add(new PatchedOrderLine()
//                {
//                    Type = "discount",
//                    Name = "Discount",
//                    Quantity = 1,
//                    TotalAmount = orderLevelDiscountIncludingTax * -1,
//                    UnitPrice = orderLevelDiscountIncludingTax * -1,
//                    TotalTaxAmount = discountTax * -1,
//                    TaxRate = AmountHelper.GetAmount(((decimal)discountTax) / orderLevelDiscount * 100)
//                });
//            }
//            return orderLines;
//        }

//    }
//}
