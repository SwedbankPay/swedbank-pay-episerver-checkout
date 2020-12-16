using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.Configuration;

using System.Configuration;
using System.Linq;
using System.Web.Configuration;

namespace SwedbankPay.Episerver.Checkout
{
	public class WebConfigCheckoutConfigurationLoader : ICheckoutConfigurationLoader
	{
		public CheckoutConfiguration GetConfiguration(MarketId marketId)
		{
			var market = SwedbankPaySection.Instance.Markets.All.FirstOrDefault(x => x.MarketId == marketId.Value);
			return market != null ? CreateCheckoutConfiguration(marketId, market) : new CheckoutConfiguration();
		}

		public CheckoutConfiguration GetConfiguration(MarketId marketId, string languageId)
		{
			var market = SwedbankPaySection.Instance.Markets.All.FirstOrDefault(x => x.MarketId == marketId.Value && x.Language == languageId);
			return market != null ? CreateCheckoutConfiguration(marketId, market) : new CheckoutConfiguration();
		}

		public CheckoutConfiguration GetConfiguration(PaymentMethodDto paymentMethodDto, MarketId marketId)
		{
			var languageId = paymentMethodDto.PaymentMethod.FirstOrDefault()?.LanguageId;
			return GetConfiguration(marketId, languageId);
		}

		public void SetConfiguration(CheckoutConfiguration configuration, PaymentMethodDto paymentMethod, string currentMarket)
		{
			var languageId = paymentMethod.PaymentMethod.FirstOrDefault()?.LanguageId;
			var key = configuration.MarketId + languageId;

			// get the config file for this application
			var config = WebConfigurationManager.OpenWebConfiguration("~");
			var swedbankPaySection = (SwedbankPaySection)config.GetSection("SwedbankPay");

			var containsKey = swedbankPaySection.Markets.ContainsKey(key);
			if (containsKey)
			{
				// set the new values
				swedbankPaySection.Markets.Remove(key);
			}

			// set the new values
			var marketElement = CreateMarketElement(configuration, languageId);
			swedbankPaySection.Markets.Add(marketElement);

			// save and refresh the config file
			config.Save(ConfigurationSaveMode.Minimal);
			ConfigurationManager.RefreshSection("SwedbankPay");
		}


		private static MarketElement CreateMarketElement(CheckoutConfiguration checkoutConfiguration, string language)
		{
			return new MarketElement
			{
				Language = language,
				MarketId = checkoutConfiguration.MarketId,
				Connection = new ConnectionElement
				{
					Token = checkoutConfiguration.Token,
					ApiUrl = checkoutConfiguration.ApiUrl,
					MerchantId = checkoutConfiguration.MerchantId
				},
				Checkout = new CheckoutElement
				{
					LogoUrl = checkoutConfiguration.LogoUrl,
					UseAnonymousCheckout = checkoutConfiguration.UseAnonymousCheckout,
					CompleteUrl = checkoutConfiguration.CompleteUrl,
					HostUrls = new HostUrlsElementCollection
					{
						checkoutConfiguration.HostUrls
					},
					CancelUrl = checkoutConfiguration.CancelUrl,
					TermsOfServiceUrl = checkoutConfiguration.TermsOfServiceUrl,
					PaymentUrl = checkoutConfiguration.PaymentUrl,
					CallbackUrl = checkoutConfiguration.CallbackUrl,
					ShippingAddressRestrictedToCountries = new ShippingAddressRestrictedToCountriesElementCollection
					{
						checkoutConfiguration.ShippingAddressRestrictedToCountries
					}
				}
			};
		}

		private static CheckoutConfiguration CreateCheckoutConfiguration(MarketId marketId, MarketElement market)
		{
			return new CheckoutConfiguration
			{
				MarketId = marketId.Value,
				ApiUrl = market.Connection.ApiUrl,
				MerchantId = market.Connection.MerchantId,
				Token = market.Connection.Token,
				CallbackUrl = market.Checkout.CallbackUrl,
				CancelUrl = market.Checkout.CancelUrl,
				CompleteUrl = market.Checkout.CompleteUrl,
				LogoUrl = market.Checkout.LogoUrl,
				PaymentUrl = market.Checkout.PaymentUrl,
				TermsOfServiceUrl = market.Checkout.TermsOfServiceUrl,
				UseAnonymousCheckout = market.Checkout.UseAnonymousCheckout,
				HostUrls = market.Checkout.HostUrls?.All,
				ShippingAddressRestrictedToCountries = market.Checkout.ShippingAddressRestrictedToCountries?.All
			};
		}
	}
}