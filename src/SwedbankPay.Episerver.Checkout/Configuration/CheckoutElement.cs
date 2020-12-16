using System;
using System.Configuration;

namespace SwedbankPay.Episerver.Checkout.Configuration
{
	public class CheckoutElement : ConfigurationElement
	{
		[ConfigurationProperty("hostUrls", IsRequired = false)]
		public HostUrlsElementCollection HostUrls
		{
			get => (HostUrlsElementCollection)this["hostUrls"];
			set => this["hostUrls"] = value;
		}

		[ConfigurationProperty("completeUrl", IsRequired = false)]
		public Uri CompleteUrl
		{
			get => (Uri)base["completeUrl"];
			set => base["completeUrl"] = value;
		}

		[ConfigurationProperty("cancelUrl", IsRequired = false)]
		public Uri CancelUrl
		{
			get => (Uri)this["cancelUrl"];
			set => this["cancelUrl"] = value;
		}

		[ConfigurationProperty("callbackUrl", IsRequired = false)]
		public Uri CallbackUrl
		{
			get => (Uri)this["callbackUrl"];
			set => this["callbackUrl"] = value;
		}

		[ConfigurationProperty("termsOfServiceUrl", IsRequired = false)]
		public Uri TermsOfServiceUrl
		{
			get => (Uri)this["termsOfServiceUrl"];
			set => this["termsOfServiceUrl"] = value;
		}

		[ConfigurationProperty("paymentUrl", IsRequired = false)]
		public Uri PaymentUrl
		{
			get => (Uri)this["paymentUrl"];
			set => this["paymentUrl"] = value;
		}

		[ConfigurationProperty("logoUrl", IsRequired = false)]
		public Uri LogoUrl
		{
			get => (Uri)this["logoUrl"];
			set => this["logoUrl"] = value;
		}


		[ConfigurationProperty("shippingAddressRestrictedToCountries", IsRequired = false)]
		public ShippingAddressRestrictedToCountriesElementCollection ShippingAddressRestrictedToCountries
		{
			get => (ShippingAddressRestrictedToCountriesElementCollection)this["shippingAddressRestrictedToCountries"];
			set => this["shippingAddressRestrictedToCountries"] = value;
		}

		[ConfigurationProperty("useAnonymousCheckout", IsRequired = false)]
		public bool UseAnonymousCheckout
		{
			get => (bool)this["useAnonymousCheckout"];
			set => this["useAnonymousCheckout"] = value;
		}
	}
}
