using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer.Business.Commerce.Exception;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using SwedbankPay.Episerver.Checkout.Common.Helpers;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.Consumers;
using SwedbankPay.Sdk.PaymentOrders;

namespace SwedbankPay.Episerver.Checkout.Common
{
    [ServiceConfiguration(typeof(IRequestFactory))]
    public class RequestFactory : IRequestFactory
    {
        private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IShippingCalculator _shippingCalculator;

        public RequestFactory(
            ICheckoutConfigurationLoader checkoutConfigurationLoader,
            IOrderGroupCalculator orderGroupCalculator,
            IShippingCalculator shippingCalculator)
        {
            _checkoutConfigurationLoader = checkoutConfigurationLoader ??
                                           throw new ArgumentNullException(nameof(checkoutConfigurationLoader));
            _orderGroupCalculator = orderGroupCalculator;
            _shippingCalculator = shippingCalculator;
        }

        public virtual PaymentOrderRequest GetPaymentOrderRequest(
            IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string description, string consumerProfileRef = null)
        {
            if (orderGroup == null) throw new ArgumentNullException(nameof(orderGroup));
            if (market == null) throw new ArgumentNullException(nameof(market));

            var marketCountry = CountryCodeHelper.GetTwoLetterCountryCode(market.Countries.FirstOrDefault());
            if (string.IsNullOrWhiteSpace(marketCountry))
                throw new ConfigurationException($"Please select a country in Commerce Manager for market {orderGroup.MarketId}");

            var firstShipment = orderGroup.GetFirstShipment();
            var orderItems = GetOrderItems(market, orderGroup.GetFirstShipment()).ToList();
            orderItems.Add(GetShippingOrderItem(firstShipment, market));

            return CreatePaymentOrderRequest(orderGroup, market, consumerProfileRef, orderItems, description);
        }

        public virtual ConsumersRequest GetConsumerResourceRequest(Language language,
            IEnumerable<RegionInfo> shippingAddressRestrictedToCountryCodes, EmailAddress email = null,
            Msisdn msisdn = null, NationalIdentifier nationalIdentifier = null)
        {
            return new ConsumersRequest(language, shippingAddressRestrictedToCountryCodes, Operation.Initiate);
        }

        public virtual ReversalRequest GetReversalRequest(IPayment payment, IMarket market, IShipment shipment, bool addShipmentInOrderItem = true, string description = "Reversing payment.")
        {
            var currency = shipment.ParentOrderGroup.Currency;
            var vatAmount = _shippingCalculator.GetSalesTax(shipment, market, currency);
            
            var orderItems = GetOrderItems(market, shipment).ToList();
            if (addShipmentInOrderItem)
            {
                vatAmount += _shippingCalculator.GetShippingTax(shipment, market, currency);
                orderItems.Add(GetShippingOrderItem(shipment, market));
            }

            return new ReversalRequest(Amount.FromDecimal(payment.Amount), Amount.FromDecimal(vatAmount), orderItems, description, DateTime.Now.Ticks.ToString());
        }

        public virtual CaptureRequest GetCaptureRequest(IPayment payment, IMarket market, IShipment shipment, bool addShipmentInOrderItem = true, string description = "Capturing payment.")
        {
            var currency = shipment.ParentOrderGroup.Currency;
            var vatAmount = _shippingCalculator.GetSalesTax(shipment, market, currency);
            
            var orderItems = GetOrderItems(market, shipment).ToList();
            if (addShipmentInOrderItem)
            {
                vatAmount += _shippingCalculator.GetShippingTax(shipment, market, currency);
                orderItems.Add(GetShippingOrderItem(shipment, market));
            }

            return new CaptureRequest(Amount.FromDecimal(payment.Amount), Amount.FromDecimal(vatAmount.Amount), orderItems, description, DateTime.Now.Ticks.ToString());
        }

        public virtual CancelRequest GetCancelRequest(string description = "Cancelling purchase order.")
        {
            return new CancelRequest(DateTime.Now.Ticks.ToString(), description);
        }

        public virtual AbortRequest GetAbortRequest()
        {
            return new AbortRequest();
        }

        public virtual UpdateRequest GetUpdateRequest(IOrderGroup orderGroup)
        {
            var totals = _orderGroupCalculator.GetOrderGroupTotals(orderGroup);
            return new UpdateRequest(Amount.FromDecimal(totals.Total.Amount), Amount.FromDecimal(totals.TaxTotal));
        }

        private IEnumerable<OrderItem> GetOrderItems(IMarket market, IShipment shipment)
        {
            return shipment.LineItems.Select(item =>
            {
                var unitPrice = item.PlacedPrice;
                var currency = shipment.ParentOrderGroup.Currency;
                var extendedPrice = item.GetExtendedPrice(currency);
                var salesTax = item.GetSalesTax(market, currency, shipment.ShippingAddress);
                var vatPercent = (int) (salesTax / extendedPrice * 10000);

                var amount = market.PricesIncludeTax ? extendedPrice : extendedPrice + salesTax;

                return new OrderItem(item.LineItemId.ToString(), item.DisplayName, OrderItemType.Product, "FASHION",
                    item.Quantity, "PCS", Amount.FromDecimal(unitPrice), vatPercent, Amount.FromDecimal(amount),
                    Amount.FromDecimal(salesTax));
            });
        }

        private PaymentOrderRequest CreatePaymentOrderRequest(IOrderGroup orderGroup, IMarket market, string consumerProfileRef, List<OrderItem> orderItems, string description)
        {
            var configuration = _checkoutConfigurationLoader.GetConfiguration(market.MarketId);
            var currencyCode = orderGroup.Currency.CurrencyCode;
            var totals = _orderGroupCalculator.GetOrderGroupTotals(orderGroup);

            var payer = !string.IsNullOrEmpty(consumerProfileRef)
                ? new Payer
                {
                    ConsumerProfileRef = consumerProfileRef
                }
                : null;

            return new PaymentOrderRequest(Operation.Purchase, new CurrencyCode(currencyCode),
                Amount.FromDecimal(totals.Total), Amount.FromDecimal(totals.TaxTotal), description,
                HttpContext.Current.Request.UserAgent, CultureInfo.CreateSpecificCulture(ContentLanguage.PreferredCulture.Name),
                false,
                GetMerchantUrls(orderGroup, market), new PayeeInfo(new Guid(configuration.MerchantId),
                    DateTime.Now.Ticks.ToString()), orderItems: orderItems, payer: payer);
        }

        private OrderItem GetShippingOrderItem(IShipment shipment, IMarket market)
        {
            var currency = shipment.ParentOrderGroup.Currency;
            var shippingVatAmount = _shippingCalculator.GetShippingTax(shipment, market, currency);
            var shippingAmount = _shippingCalculator.GetShippingCost(shipment, market, currency);

            var amount = market.PricesIncludeTax ? shippingAmount : shippingAmount + shippingVatAmount;

            var vatPercent = (int) (shippingVatAmount.Amount / shippingAmount.Amount * 10000);

            return new OrderItem("SHIPPING", "SHIPPINGFEE", OrderItemType.ShippingFee, "NOTAPPLICABLE", 1, "PCS",
                Amount.FromDecimal(shippingAmount.Amount), vatPercent, Amount.FromDecimal(amount.Amount),
                Amount.FromDecimal(shippingVatAmount.Amount));
        }

        private Urls GetMerchantUrls(IOrderGroup orderGroup, IMarket market)
        {
            var checkoutConfiguration = _checkoutConfigurationLoader.GetConfiguration(market.MarketId);

            Uri ToFullSiteUrl(Func<CheckoutConfiguration, Uri> fieldSelector)
            {
                if (fieldSelector(checkoutConfiguration) == null) return null;

                var uriBuilder = new UriBuilder(fieldSelector(checkoutConfiguration));
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["orderGroupId"] = orderGroup.OrderLink.OrderGroupId.ToString();

                uriBuilder.Query = query.ToString();
                if (!uriBuilder.Uri.IsAbsoluteUri)
                    return new Uri(SiteDefinition.Current.SiteUrl, uriBuilder.Uri.PathAndQuery);

                return uriBuilder.Uri;
            }

            var cancelUrl = checkoutConfiguration.PaymentUrl != null ? ToFullSiteUrl(c => c.CancelUrl) : null;

            return new Urls(checkoutConfiguration.HostUrls, ToFullSiteUrl(c => c.CompleteUrl),
                checkoutConfiguration.TermsOfServiceUrl, cancelUrl, ToFullSiteUrl(c => c.PaymentUrl),
                ToFullSiteUrl(c => c.CallbackUrl), checkoutConfiguration.LogoUrl);
        }
    }
}