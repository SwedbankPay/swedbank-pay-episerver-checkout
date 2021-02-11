using EPiServer.Business.Commerce.Exception;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using EPiServer.Web;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;

using SwedbankPay.Episerver.Checkout.Common.Helpers;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.PaymentOrders;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using SwedbankPay.Sdk.Consumers;
using Currency = Mediachase.Commerce.Currency;

namespace SwedbankPay.Episerver.Checkout.Common
{
    [ServiceConfiguration(typeof(IRequestFactory))]
    public class RequestFactory : IRequestFactory
    {
        private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IShippingCalculator _shippingCalculator;
        private readonly SwedbankPayTaxCalculator _swedbankPayTaxCalculator;
        private readonly IReturnLineItemCalculator _returnLineItemCalculator;

        public RequestFactory(
            ICheckoutConfigurationLoader checkoutConfigurationLoader,
            IOrderGroupCalculator orderGroupCalculator,
            IShippingCalculator shippingCalculator,
            SwedbankPayTaxCalculator swedbankPayTaxCalculator, IReturnLineItemCalculator returnLineItemCalculator)
        {
            _checkoutConfigurationLoader = checkoutConfigurationLoader ??
                                           throw new ArgumentNullException(nameof(checkoutConfigurationLoader));
            _orderGroupCalculator = orderGroupCalculator;
            _shippingCalculator = shippingCalculator;
            _swedbankPayTaxCalculator = swedbankPayTaxCalculator;
            _returnLineItemCalculator = returnLineItemCalculator;
        }

        public virtual PaymentOrderRequest GetPaymentOrderRequest(
            IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string description, string consumerProfileRef = null)
        {
            if (orderGroup == null)
                throw new ArgumentNullException(nameof(orderGroup));
            if (market == null)
                throw new ArgumentNullException(nameof(market));

            var marketCountry = CountryCodeHelper.GetTwoLetterCountryCode(market.Countries.FirstOrDefault());
            if (string.IsNullOrWhiteSpace(marketCountry))
                throw new ConfigurationException($"Please select a country in Commerce Manager for market {orderGroup.MarketId}");


            List<OrderItem> orderItems = new List<OrderItem>();
            foreach (var orderGroupForm in orderGroup.Forms)
            {
                foreach (var shipment in orderGroupForm.Shipments)
                {
                    orderItems.AddRange(GetOrderItems(market, orderGroup.Currency, shipment.ShippingAddress, shipment.LineItems));
                    orderItems.Add(GetShippingOrderItem(shipment, market));
                }
            }

            return CreatePaymentOrderRequest(orderGroup, market, consumerProfileRef, orderItems, description);
        }

        public virtual ConsumerRequest GetConsumerResourceRequest(Language language,
            IList<CountryCode> shippingAddressRestrictedToCountryCodes, EmailAddress email = null,
            Msisdn msisdn = null, NationalIdentifier nationalIdentifier = null)
        {
	        var consumerResourceRequest = new ConsumerRequest(language);
            if(shippingAddressRestrictedToCountryCodes != null && shippingAddressRestrictedToCountryCodes.Any())
			{
				foreach (var shippingAddressRestrictedToCountryCode in shippingAddressRestrictedToCountryCodes)
				{
					consumerResourceRequest.ShippingAddressRestrictedToCountryCodes.Add(shippingAddressRestrictedToCountryCode);
				}
            }
            return consumerResourceRequest;
        }

        public virtual PaymentOrderReversalRequest GetReversalRequest(IPayment payment, IEnumerable<ILineItem> lineItems, IMarket market, IShipment shipment, string description = "Reversing payment.")
        {
            var lineItemsList = lineItems.ToList();
            var orderItems = GetOrderItems(market, shipment.ParentOrderGroup.Currency, shipment.ShippingAddress, lineItemsList).ToList();

            var totalAmountIncludingVatAsDecimal = GetTotalAmountIncludingVatAsDecimal(orderItems);

            var additionalCostsForReversal = payment.Amount - totalAmountIncludingVatAsDecimal;

            if (additionalCostsForReversal < 0)
            {
                var aggregatedOrderItem = new OrderItem("Returns", "Returns", OrderItemType.Other, "NOTAPPLICABLE", 1, //TODO Vat is not included in this special case with "negative" costs 
                    "pcs", new Amount(payment.Amount), 0, amount: new Amount(payment.Amount),
                    new Amount(0));

                var orderReversalRequest = new PaymentOrderReversalRequest(new Amount(payment.Amount), new Amount(0), description, DateTime.Now.Ticks.ToString());
                orderReversalRequest.Transaction.OrderItems.Add(aggregatedOrderItem);
                return orderReversalRequest;
            }

            var shippingTaxPercentage = _swedbankPayTaxCalculator.GetTaxPercentage(lineItemsList.FirstOrDefault(), market, shipment.ShippingAddress,
                TaxType.ShippingTax);

            var shippingVatAmount = additionalCostsForReversal * (shippingTaxPercentage / 100) / (1 + shippingTaxPercentage / 100);

            orderItems.Add(new OrderItem("Shipping", "Shipping", OrderItemType.ShippingFee, "NOTAPPLICABLE", 1, "pcs",
                new Amount(additionalCostsForReversal), (int)(shippingTaxPercentage * 100), amount:new Amount(additionalCostsForReversal),
                new Amount(shippingVatAmount)));

            var totalVatAmountAsDecimal = GetTotalVatAmountAsDecimal(orderItems);
            var paymentOrderReversalRequest = new PaymentOrderReversalRequest(new Amount(GetTotalAmountIncludingVatAsDecimal(orderItems)), new Amount(totalVatAmountAsDecimal), description, DateTime.Now.Ticks.ToString());
            foreach (var orderItem in orderItems)
            {
	            paymentOrderReversalRequest.Transaction.OrderItems.Add(orderItem);
            }

            return paymentOrderReversalRequest;
        }

        public virtual PaymentOrderCaptureRequest GetCaptureRequest(IPayment payment, IMarket market, IShipment shipment, bool addShipmentInOrderItem = true, string description = "Capturing payment.")
        {
            List<OrderItem> orderItems = new List<OrderItem>
            {
                new OrderItem("Capture", "Capture", OrderItemType.Other, "Capture", 1, "pcs",
                    new Amount(payment.Amount), 0, new Amount(payment.Amount), new Amount(0))
            };
            var paymentOrderCaptureRequest = new PaymentOrderCaptureRequest(new Amount(GetTotalAmountIncludingVatAsDecimal(orderItems)), new Amount(GetTotalVatAmountAsDecimal(orderItems)), description, DateTime.Now.Ticks.ToString());

            foreach (var orderItem in orderItems)
            {
	            paymentOrderCaptureRequest.Transaction.OrderItems.Add(orderItem);
            }

            return paymentOrderCaptureRequest;
        }

        public virtual PaymentOrderCancelRequest GetCancelRequest(string description = "Cancelling purchase order.")
        {
            return new PaymentOrderCancelRequest(DateTime.Now.Ticks.ToString(), description);
        }

        public virtual PaymentOrderAbortRequest GetAbortRequest(string abortReason)
        {
            return new PaymentOrderAbortRequest(abortReason);
        }

        public virtual PaymentOrderUpdateRequest GetUpdateRequest(IOrderGroup orderGroup, IMarket market)
        {
            var totals = _orderGroupCalculator.GetOrderGroupTotals(orderGroup);
            var updateRequest = new PaymentOrderUpdateRequest(new Amount(totals.Total.Amount), new Amount(totals.TaxTotal));

            foreach (var orderGroupForm in orderGroup.Forms)
            {
	            foreach (var shipment in orderGroupForm.Shipments)
	            {
		            var orderItems = GetOrderItems(market, orderGroup.Currency, shipment.ShippingAddress, shipment.LineItems);
		            foreach (var orderItem in orderItems)
		            {
			            updateRequest.PaymentOrder.OrderItems.Add(orderItem);

		            }
		            updateRequest.PaymentOrder.OrderItems.Add(GetShippingOrderItem(shipment, market));
	            }
            }

            return updateRequest;
        }

        private IEnumerable<OrderItem> GetOrderItems(IMarket market, Currency currency, IOrderAddress shippingAddress, IEnumerable<ILineItem> lineItems)
        {
            return lineItems.Select(item =>
            {
                var unitPrice = item.PlacedPrice;
                var extendedPrice = item.ReturnQuantity > 0 ? _returnLineItemCalculator.GetExtendedPrice(item as IReturnLineItem, currency) : item.GetExtendedPrice(currency);

                var salesTax = _swedbankPayTaxCalculator.GetSalesTax(item, market, shippingAddress, extendedPrice);
                var vatPercent = (int)_swedbankPayTaxCalculator.GetTaxPercentage(item, market, shippingAddress, TaxType.SalesTax) * 100;
                var amount = market.PricesIncludeTax ? extendedPrice : extendedPrice + salesTax;

                return new OrderItem(item.LineItemId.ToString(), item.DisplayName, OrderItemType.Product, "FASHION",
                    item.ReturnQuantity > 0 ? item.ReturnQuantity : item.Quantity, "PCS", new Amount(unitPrice), vatPercent, new Amount(amount),
                    new Amount(salesTax));
            });
        }

        private decimal GetTotalAmountIncludingVatAsDecimal(IEnumerable<OrderItem> orderItems)
        {
            return orderItems.Sum(x => x.Amount);
        }
        private decimal GetTotalVatAmountAsDecimal(IEnumerable<OrderItem> orderItems)
        {
            return orderItems.Sum(x => x.VatAmount);
        }
    
        private PaymentOrderRequest CreatePaymentOrderRequest(IOrderGroup orderGroup, IMarket market, string consumerProfileRef, List<OrderItem> orderItems, string description)
        {
            var configuration = _checkoutConfigurationLoader.GetConfiguration(market.MarketId);
            var currencyCode = orderGroup.Currency.CurrencyCode;

            var payer = !string.IsNullOrEmpty(consumerProfileRef)
                ? new Payer
                {
                    ConsumerProfileRef = consumerProfileRef
                }
                : null;

            var payeeReference = DateTime.Now.Ticks.ToString();

            var paymentOrderRequest = new PaymentOrderRequest(Operation.Purchase, new Sdk.Currency(currencyCode), new Amount(GetTotalAmountIncludingVatAsDecimal(orderItems)), new Amount(GetTotalVatAmountAsDecimal(orderItems)), description,
	            HttpContext.Current.Request.UserAgent, new Language(CultureInfo.CreateSpecificCulture(ContentLanguage.PreferredCulture.Name).TextInfo.CultureName),
	            false, GetMerchantUrls(orderGroup, market, payeeReference), new PayeeInfo(configuration.MerchantId,
		            payeeReference));

            paymentOrderRequest.PaymentOrder.Payer = payer;
            foreach (var orderItem in orderItems)
            {
	            paymentOrderRequest.PaymentOrder.OrderItems.Add(orderItem);
            }

            return paymentOrderRequest;
        }

        private OrderItem GetShippingOrderItem(IShipment shipment, IMarket market)
        {
            var currency = shipment.ParentOrderGroup.Currency;
            var shippingVatAmount = _shippingCalculator.GetShippingTax(shipment, market, currency).Round();

            var shippingAmount = _shippingCalculator.GetDiscountedShippingAmount(shipment, market, currency);

            var amount = market.PricesIncludeTax ? shippingAmount : shippingAmount + shippingVatAmount;

            var vatPercent = shippingAmount > 0
                ? market.PricesIncludeTax
                    ? (int)((shippingVatAmount / (shippingAmount - shippingVatAmount)).Round() * 10000)
                    : (int)((shippingVatAmount / (shippingAmount)).Round() * 10000)
                : 0;

            return new OrderItem("SHIPPING", "SHIPPINGFEE", OrderItemType.ShippingFee, "NOTAPPLICABLE", 1, "PCS",
                new Amount(shippingAmount.Amount), vatPercent, new Amount(amount.Amount),
                new Amount(shippingVatAmount.Amount));
        }

        private Urls GetMerchantUrls(IOrderGroup orderGroup, IMarket market, string payeeReference)
        {
            var checkoutConfiguration = _checkoutConfigurationLoader.GetConfiguration(market.MarketId);
            
            Uri ToFullSiteUrl(Func<CheckoutConfiguration, Uri> fieldSelector)
            {
                if (fieldSelector(checkoutConfiguration) == null)
                    return null;
                var url = fieldSelector(checkoutConfiguration).OriginalString
                    .Replace("{orderGroupId}", orderGroup.OrderLink.OrderGroupId.ToString())
                    .Replace("{payeeReference}", payeeReference);

                if (string.IsNullOrWhiteSpace(url))
                {
	                return null;
                }

                var uriBuilder = new UriBuilder(url);
                return !uriBuilder.Uri.IsAbsoluteUri ? new Uri(SiteDefinition.Current.SiteUrl, uriBuilder.Uri.PathAndQuery) : uriBuilder.Uri;
            }

            var cancelUrl = checkoutConfiguration.PaymentUrl != null ? ToFullSiteUrl(c => c.CancelUrl) : null;

            return new Urls(checkoutConfiguration.HostUrls, ToFullSiteUrl(c => c.CompleteUrl),
				checkoutConfiguration.TermsOfServiceUrl)
            {
                CallbackUrl = ToFullSiteUrl(c => c.CallbackUrl),
                CancelUrl = cancelUrl,
                LogoUrl = checkoutConfiguration.LogoUrl,
                PaymentUrl = ToFullSiteUrl(c => c.PaymentUrl)
            };
        }
    }
}