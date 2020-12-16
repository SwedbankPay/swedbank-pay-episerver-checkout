using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SwedbankPay.Episerver.Checkout.Configuration
{
	[ConfigurationCollection(typeof(MarketElement), AddItemName = "market", CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class MarketElementCollection : ConfigurationElementCollection
	{
		public List<MarketElement> All => this.Cast<MarketElement>().ToList();

		public MarketElementCollection()
		{
			AddElementName = "market";
		}

		/// <summary>
		/// Adds an element to the collection of market elements
		/// </summary>
		/// <param name="element"></param>
		public void Add(MarketElement element)
		{
			BaseAdd(element);
		}

		/// <summary>
		/// Removes an element to the collection of market elements.</summary>
		/// <param name="key"></param>
		public void Remove(string key)
		{
			BaseRemove(key);
		}

		/// <summary>
		/// Removes an element to the collection of market elements.</summary>
		/// <param name="element"></param>
		public void Remove(MarketElement element)
		{
			BaseRemove(element.Key);
		}

		/// <summary>Removes all members of the collection.</summary>
		public void Clear()
		{
			BaseClear();
		}

		public bool ContainsKey(string key)
		{
			var keys = new List<object>(BaseGetAllKeys());
			return keys.Contains(key);
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new MarketElement();
		}

		protected override string ElementName => "market";

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((MarketElement)element).Key;
		}

		public new MarketElement this[string key] => (MarketElement)BaseGet(key);

		/// <summary>Gets or sets the element in the collection at the specified position.</summary>
		/// <param name="index">The position of the element in the collection to get or set.</param>
		/// <returns>A <see cref="T:System.Runtime.Serialization.Configuration.ParameterElement" /> from the collection.</returns>
		public MarketElement this[int index] => (MarketElement)BaseGet(index);
	}
}
