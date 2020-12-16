using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SwedbankPay.Episerver.Checkout.Configuration
{
	public class ShippingAddressRestrictedToCountryElement : ConfigurationElement
	{
		[ConfigurationProperty("country", IsRequired = true)]
		public string Country
		{
			get => (string)this["country"];
			set => this["country"] = value;
		}
	}

	public class ShippingAddressRestrictedToCountriesElementCollection : ConfigurationElementCollection
	{
		public List<string> All => this.Cast<ShippingAddressRestrictedToCountryElement>().Select(x => x.Country).ToList();

		/// <summary>
		/// Adds an element to the collection of market elements
		/// </summary>
		/// <param name="element"></param>
		public void Add(ShippingAddressRestrictedToCountryElement element)
		{
			BaseAdd(element);
		}

		/// <summary>
		/// Adds elements to the collection of market elements
		/// </summary>
		/// <param name="countries"></param>
		public void Add(IList<string> countries)
		{
			if (countries == null || !countries.Any())
			{
				return;
			}

			foreach (var country in countries)
			{
				BaseAdd(CreateNewElement(country));
			}
		}

		/// <summary>
		/// Removes an element to the collection of market elements.</summary>
		/// <param name="element"></param>
		public void Remove(ShippingAddressRestrictedToCountryElement element)
		{
			BaseRemove(element.Country);
		}

		/// <summary>Removes all members of the collection.</summary>
		public void Clear()
		{
			BaseClear();
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ShippingAddressRestrictedToCountryElement();
		}

		protected override ConfigurationElement CreateNewElement(string country)
		{
			return new ShippingAddressRestrictedToCountryElement { Country = country };
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ShippingAddressRestrictedToCountryElement)element).Country;
		}
	}
}
