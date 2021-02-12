using SwedbankPay.Episerver.Checkout.Common.Helpers;

namespace SwedbankPay.Episerver.Checkout.Common
{
	public class AddressDto
    {
	    public AddressDto(Sdk.Consumers.Address address)
	    {
		    Addressee = address.Addressee;
		    City = address.City;
		    CoAddress = address.CoAddress;
		    CountryCode = CountryCodeHelper.GetThreeLetterCountryCode(address.CountryCode);
		    StreetAddress = address.StreetAddress;
		    ZipCode = address.ZipCode;
	    }

	    public string Addressee { get; }
	    public string City { get; }
	    public string CoAddress { get; }
	    public string CountryCode { get; }
	    public string StreetAddress { get; }
	    public string ZipCode { get; }
    }
}
