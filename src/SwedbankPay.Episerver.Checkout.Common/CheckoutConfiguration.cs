using System;
using System.Collections.Generic;

namespace SwedbankPay.Episerver.Checkout.Common
{
    public class CheckoutConfiguration : ConnectionConfiguration
    {
        public List<Uri> HostUrls { get; set; }
        public Uri CompleteUrl { get; set; }
        public Uri CancelUrl { get; set; }
        public Uri CallbackUrl { get; set; }
        public Uri TermsOfServiceUrl { get; set; }
        public Uri PaymentUrl { get; set; }
        public Uri LogoUrl { get; set; }
        public bool UseAnonymousCheckout { get; set; }
        public List<string> ShippingAddressRestrictedToCountries { get; set; }
    }
}
