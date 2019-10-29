namespace SwedbankPay.Checkout.Episerver.Common
{
    using EPiServer;
    using EPiServer.Business.Commerce.Exception;
    using EPiServer.Commerce.Catalog.ContentTypes;
    using EPiServer.Commerce.Order;
    using EPiServer.Commerce.Order.Calculator;
    using EPiServer.ServiceLocation;
    using EPiServer.Web;

    using Mediachase.Commerce;
    using Mediachase.Commerce.Catalog;
    using Mediachase.Commerce.Catalog.Managers;
    using Mediachase.Commerce.Orders.Dto;

    using SwedbankPay.Checkout.Episerver.Common.Extensions;
    using SwedbankPay.Checkout.Episerver.Common.Helpers;
    using SwedbankPay.Sdk;
    using SwedbankPay.Sdk.Consumers;
    using SwedbankPay.Sdk.PaymentOrders;
    using SwedbankPay.Sdk.Transactions;

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    [ServiceConfiguration(typeof(IRequestFactory))]
    public class RequestFactory : IRequestFactory
    {
        private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;
        private readonly IContentRepository _contentRepository;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IShippingCalculator _shippingCalculator;
        private readonly ITaxCalculator _taxCalculator;
        private readonly ReferenceConverter _referenceConverter;

        //private ICollection<OrderItem> _orderItems;

        public RequestFactory(
             ICheckoutConfigurationLoader checkoutConfigurationLoader,
             IContentRepository contentRepository,
             IOrderGroupCalculator orderGroupCalculator,
             IShippingCalculator shippingCalculator,
             ITaxCalculator taxCalculator,
             ReferenceConverter referenceConverter)
        {
            _checkoutConfigurationLoader = checkoutConfigurationLoader;
            _contentRepository = contentRepository;
            _orderGroupCalculator = orderGroupCalculator;
            _shippingCalculator = shippingCalculator;
            _taxCalculator = taxCalculator;
            _referenceConverter = referenceConverter;
        }

        public virtual PaymentOrderRequestContainer GetPaymentOrderRequestContainer(
          IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string consumerProfileRef = null)
        {
            if (orderGroup == null)
            {
                throw new ArgumentNullException(nameof(orderGroup));
            }
            if (market == null)
            {
                throw new ArgumentNullException(nameof(market));
            }

            var marketCountry = CountryCodeHelper.GetTwoLetterCountryCode(market.Countries.FirstOrDefault());
            if (string.IsNullOrWhiteSpace(marketCountry))
            {
                throw new ConfigurationException($"Please select a country in Commerce Manager for market {orderGroup.MarketId}");
            }

            var firstShipment = orderGroup.GetFirstShipment();
            var orderItems = GetOrderItems(market, orderGroup.GetFirstShipment());
            orderItems.Add(GetShippingOrderItem(firstShipment, market));

            return CreatePaymentOrderRequestContainer(orderGroup, market, consumerProfileRef, orderItems);

        }

        public virtual ConsumersRequest GetConsumerResourceRequest(IMarket market, string email = null, string mobilePhone = null, string ssn = null)
        {
            var twoLetterIsoRegionName = new RegionInfo(market.DefaultLanguage.TextInfo.CultureName).TwoLetterISORegionName;

            var initiateConsumerSessionRequestObject = new ConsumersRequest
            {
                Email = email,
                Msisdn = mobilePhone,
                ConsumerCountryCode = twoLetterIsoRegionName
            };

            if (!string.IsNullOrWhiteSpace(ssn))
            {
                initiateConsumerSessionRequestObject.NationalIdentifier = new NationalIdentifier
                {
                    CountryCode = twoLetterIsoRegionName,
                    SocialSecurityNumber = ssn
                };
            }

            return initiateConsumerSessionRequestObject;
        }

        public virtual TransactionRequest GetTransactionRequest(IPayment payment, IMarket market, IShipment shipment, string description) // TODO is this the right way to do it?
        {
            var currency = shipment.ParentOrderGroup.Currency;
            var vatAmount = _shippingCalculator.GetSalesTax(shipment, market, currency);
            vatAmount += _shippingCalculator.GetShippingTax(shipment, market, currency);
            var amount = AmountHelper.GetAmount(payment.Amount);

            var transactionRequest = new TransactionRequest
            {
                Amount = amount,
                Description = description,
                VatAmount = AmountHelper.GetAmount(vatAmount),
                PayeeReference = DateTime.Now.Ticks.ToString(),
                OrderItems = GetOrderItems(market, shipment)
            };
            transactionRequest.OrderItems.Add(GetShippingOrderItem(shipment, market));
            return transactionRequest;
        }

        private List<OrderItem> GetOrderItems(IMarket market, IShipment shipment)
        {
            return shipment.LineItems.Select(item =>
            {
                var unitPrice = AmountHelper.GetAmount(item.PlacedPrice);
                var currency = shipment.ParentOrderGroup.Currency;
                var amount = AmountHelper.GetAmount(item.GetExtendedPrice(currency));
                var vatAmount = AmountHelper.GetAmount(item.GetSalesTax(market, currency, shipment.ShippingAddress));
                
                return new OrderItem
                {
                    Reference = item.LineItemId.ToString(),
                    Amount = market.PricesIncludeTax ? amount : amount + vatAmount,
                    Class = "FASHION", //TODO Get Value from interface 
                    Description = "",
                    DiscountDescription = "",
                    DiscountPrice = AmountHelper.GetAmount(item.GetDiscountedPrice(currency)),
                    ImageUrl = "", //TODO Get correct value
                    ItemUrl = "", //TODO Get correct value
                    Name = item.DisplayName,
                    Quantity = (int)(item.Quantity),
                    QuantityUnit = "PCS", //TODO Get Value from interface
                    Type = "PRODUCT", //TODO Get Value from interface
                    UnitPrice = unitPrice,
                    VatAmount = vatAmount,
                    VatPercent = (int)((double)vatAmount / amount * 10000) //TODO Get correct value
                };
            }).ToList();
        }

        private PaymentOrderRequestContainer CreatePaymentOrderRequestContainer(IOrderGroup orderGroup, IMarket market, string consumerProfileRef, IEnumerable<OrderItem> orderItems)
        {
            var configuration = _checkoutConfigurationLoader.GetConfiguration(market.MarketId).ToSwedbankPayConfiguration();
            var totals = _orderGroupCalculator.GetOrderGroupTotals(orderGroup);
            var vatAmount = _taxCalculator.GetSalesTax(orderGroup.GetAllLineItems(), market, orderGroup.GetFirstShipment().ShippingAddress, orderGroup.Currency);
            var paymentOrderRequestObject = new PaymentOrderRequestContainer();
            if (configuration != null && totals != null)
            {
                paymentOrderRequestObject.Paymentorder = new PaymentOrderRequest
                {
                    Amount = AmountHelper.GetAmount(totals.Total),
                    VatAmount = AmountHelper.GetAmount(totals.TaxTotal),
                    Currency = orderGroup.Currency.CurrencyCode,
                    Description = "Description",
                    Language = market.DefaultLanguage.TextInfo.CultureName,
                    UserAgent = HttpContext.Current.Request.UserAgent,
                    Urls = GetMerchantUrls(orderGroup, market),
                    PayeeInfo = new PayeeInfo
                    {
                        PayeeId = configuration.MerchantId,
                        PayeeReference = DateTime.Now.Ticks.ToString()
                    },
                    OrderItems = orderItems.ToList()
                };

                if (!string.IsNullOrWhiteSpace(consumerProfileRef))
                {
                    paymentOrderRequestObject.Paymentorder.Payer = new Payer
                    {
                        ConsumerProfileRef = consumerProfileRef
                    };
                }

            }
            return paymentOrderRequestObject;
        }

        private OrderItem GetShippingOrderItem(IShipment shipment, IMarket market)
        {
            var currency = shipment.ParentOrderGroup.Currency;
            var shippingVatAmount = AmountHelper.GetAmount(_shippingCalculator.GetShippingTax(shipment, market, currency));
            var shippingAmount = AmountHelper.GetAmount(_shippingCalculator.GetShippingCost(shipment, market, currency));

            return new OrderItem
            {
                Type = "SHIPPING_FEE",
                Reference = "SHIPPING",
                Quantity = 1,
                DiscountPrice = shippingAmount,
                DiscountDescription = "",
                Name = "SHIPPINGFEE",
                VatAmount = shippingVatAmount,
                ItemUrl = "",
                ImageUrl = "",
                Description = "Shipping fee",
                Amount = market.PricesIncludeTax ? shippingAmount : shippingAmount + shippingVatAmount,
                Class = "NOTAPPLICABLE",
                UnitPrice = shippingAmount,
                QuantityUnit = "PCS",
                VatPercent = (int)((double)shippingVatAmount / shippingAmount * 10000)
            };
        }
        private Urls GetMerchantUrls(IOrderGroup orderGroup, IMarket market)
        {
            CheckoutConfiguration checkoutConfiguration = _checkoutConfigurationLoader.GetConfiguration(market.MarketId, market.DefaultLanguage.Name);

            string ToFullSiteUrl(Func<CheckoutConfiguration, string> fieldSelector)
            {
                if (string.IsNullOrWhiteSpace(fieldSelector(checkoutConfiguration)))
                {
                    return null;
                }

                var url = fieldSelector(checkoutConfiguration)?.ToString().Replace("{orderGroupId}", orderGroup.OrderLink.OrderGroupId.ToString());
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return uri.ToString();
                }

                return new Uri(SiteDefinition.Current.SiteUrl, url).ToString();
            }

            return new Urls
            {
                TermsOfServiceUrl = checkoutConfiguration.TermsOfServiceUrl,
                CallbackUrl = ToFullSiteUrl(c => c.CallbackUrl),
                PaymentUrl = ToFullSiteUrl(c => c.PaymentUrl),
                CancelUrl = string.IsNullOrWhiteSpace(checkoutConfiguration.PaymentUrl) ? ToFullSiteUrl(c => c.CancelUrl) : null,
                CompleteUrl = ToFullSiteUrl(c => c.CompleteUrl),
                LogoUrl = checkoutConfiguration.LogoUrl,
                HostUrls = checkoutConfiguration.HostUrls
            };
        }

    }
}
