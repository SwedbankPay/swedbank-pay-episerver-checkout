using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SwedbankPay.Episerver.Checkout.Configuration
{
	public class HostUrlsElement : ConfigurationElement
	{
		[ConfigurationProperty("url", IsRequired = true)]
		public Uri Url
		{
			get => (Uri)this["url"];
			set => this["url"] = value;
		}
	}

	public class HostUrlsElementCollection : ConfigurationElementCollection
	{
		public List<Uri> All => this.Cast<HostUrlsElement>().Select(x => x.Url).ToList();

		/// <summary>Adds an element to the collection of parameter elements.</summary>
		/// <param name="element">The <see cref="T:System.Runtime.Serialization.Configuration.ParameterElement" /> element to add to the collection.</param>
		public void Add(HostUrlsElement element)
		{
			BaseAdd(element);
		}

		public void Add(List<Uri> hostUrls)
		{
			if (hostUrls == null || !hostUrls.Any())
			{
				return;
			}

			foreach (var url in hostUrls)
			{
				BaseAdd(CreateNewElement(url));
			}
		}

		/// <summary>Removes all members of the collection.</summary>
		public void Clear()
		{
			BaseClear();
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new HostUrlsElement();
		}

		protected ConfigurationElement CreateNewElement(Uri uri)
		{
			return new HostUrlsElement { Url = uri };
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((HostUrlsElement)element).Url;
		}
	}
}
