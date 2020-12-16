using System.Configuration;

namespace SwedbankPay.Episerver.Checkout.Configuration
{
	public class SwedbankPaySection : ConfigurationSection
	{
		private static readonly object SyncObject = new object();
		private static SwedbankPaySection _swedbankPay;

		[ConfigurationProperty("markets", IsRequired = false)]
		public MarketElementCollection Markets => (MarketElementCollection)base["markets"];


		/// <summary>
		/// Gets the instance of the <see cref="T:Srk.Web.Erkpn.Commerce.Shared.Payments.SwedbankPay.Configuration.SwedbankPaySection" /> section
		/// </summary>
		/// <remarks>Returns null if the section isn't found</remarks>
		public static SwedbankPaySection Instance
		{
			get
			{
				EnsureCurrentConfig();
				return _swedbankPay;
			}
		}

		private static void EnsureCurrentConfig()
		{
			if (_swedbankPay != null)
			{
				return;
			}

			lock (SyncObject)
			{
				if (_swedbankPay != null)
				{
					return;
				}

				_swedbankPay = ConfigurationManager.GetSection("SwedbankPay") as SwedbankPaySection ?? new SwedbankPaySection();
			}
		}
	}
}
