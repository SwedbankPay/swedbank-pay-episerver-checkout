using System.Configuration;

namespace SwedbankPay.Episerver.Checkout.Configuration
{
	public class MarketElement : ConfigurationElement
	{
		[ConfigurationProperty("key")]
		public string Key => MarketId + Language;

		[ConfigurationProperty("id", IsRequired = true)]
		public string MarketId
		{
			get => (string)this["id"];
			set => this["id"] = value;
		}

		[ConfigurationProperty("language", IsRequired = true)]
		public string Language
		{
			get => (string)this["language"];
			set => this["language"] = value;
		}

		[ConfigurationProperty("connection", IsRequired = true)]
		public ConnectionElement Connection
		{
			get => (ConnectionElement)this["connection"];
			set => this["connection"] = value;
		}

		[ConfigurationProperty("checkout", IsRequired = true)]
		public CheckoutElement Checkout
		{
			get => (CheckoutElement)this["checkout"];
			set => this["checkout"] = value;
		}
	}
}
