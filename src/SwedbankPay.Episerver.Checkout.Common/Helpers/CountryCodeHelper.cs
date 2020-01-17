using EPiServer.Personalization.Providers.MaxMind;
using EPiServer.ServiceLocation;

using ISO3166;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SwedbankPay.Episerver.Checkout.Common.Helpers
{
    public static class CountryCodeHelper
    {
        private static Injected<GeolocationProvider> GeoLocationProvider;
        private static Injected<ICountryRegionProvider> CountryRegionProvider;

        public static string GetTwoLetterCountryCode(string countryCode)
        {
            return Country.List.FirstOrDefault(x => x.ThreeLetterCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))?.TwoLetterCode
                ?? Country.List.FirstOrDefault(x => x.TwoLetterCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))?.TwoLetterCode;
        }

        public static string GetThreeLetterCountryCode(string countryCode)
        {
            return Country.List.FirstOrDefault(x => x.TwoLetterCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))?.ThreeLetterCode
                ?? Country.List.FirstOrDefault(x => x.ThreeLetterCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))?.ThreeLetterCode;
        }

        internal static IEnumerable<Country> GetCountryCodes()
        {
            return Country.List;
        }

        internal static IEnumerable<string> GetTwoLetterCountryCodes(IEnumerable<string> threeLetterCodes)
        {
            var newList = new List<string>();
            foreach (var item in threeLetterCodes)
            {
                newList.Add(GetTwoLetterCountryCode(item));
            }
            return newList;
        }

        internal static string GetContinentByCountry(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                return string.Empty;
            }
            var continents = GeoLocationProvider.Service.GetContinentCodes();

            if (countryCode.Length == 3)
            {
                countryCode = GetTwoLetterCountryCode(countryCode);
            }

            foreach (var continent in continents)
            {
                if (GeoLocationProvider.Service.GetCountryCodes(continent).Any(x => x.Equals(countryCode)))
                {
                    return continent;
                }
            }
            return string.Empty;
        }

        internal static string GetStateName(string twoLetterCountryCode, string stateCode)
        {
            return CountryRegionProvider.Service.GetStateName(twoLetterCountryCode, stateCode);
        }
        internal static string GetStateCode(string twoLetterCountryCode, string stateName)
        {
            return CountryRegionProvider.Service.GetStateCode(twoLetterCountryCode, stateName);
        }
    }
}
