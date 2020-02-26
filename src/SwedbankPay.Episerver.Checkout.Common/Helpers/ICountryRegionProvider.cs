namespace SwedbankPay.Episerver.Checkout.Common.Helpers
{
    internal interface ICountryRegionProvider
    {
        string GetStateName(string twoLetterCountryCode, string stateCode);
        string GetStateCode(string twoLetterCountryCode, string stateName);
    }
}
