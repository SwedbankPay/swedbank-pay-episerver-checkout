using SwedbankPay.Sdk.Consumers;

namespace SwedbankPay.Episerver.Checkout.Common
{
	public class AddressDetailsDto
    {
	    public string Email { get; }

	    public string Msisdn { get; }

	    public AddressDto ShippingAddress { get; }

		public AddressDetailsDto(ShippingDetails shippingDetails)
		{
			Email = shippingDetails.Email.ToString();
			Msisdn = shippingDetails.Msisdn.ToString();
			ShippingAddress = new AddressDto(shippingDetails.ShippingAddress);
		}

		public AddressDetailsDto(BillingDetails billingDetails)
		{
			Email = billingDetails.Email.ToString();
			Msisdn = billingDetails.Msisdn.ToString();
			ShippingAddress = new AddressDto(billingDetails.BillingAddress);
		}
	}
}
