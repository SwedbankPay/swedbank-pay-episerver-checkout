using System;
using System.Configuration;

namespace SwedbankPay.Episerver.Checkout.Configuration
{
	public class ConnectionElement : ConfigurationElement
	{
		[ConfigurationProperty("apiUrl", IsRequired = true)]
		public Uri ApiUrl
		{
			get => (Uri)this["apiUrl"];
			set => this["apiUrl"] = value;
		}

		[ConfigurationProperty("merchantId", IsRequired = true)]
		public string MerchantId
		{
			get => (string)this["merchantId"];
			set => this["merchantId"] = value;
		}

		[ConfigurationProperty("token", IsRequired = true)]
		public string Token
		{
			get => (string)this["token"];
			set => this["token"] = value;
		}
	}
}
