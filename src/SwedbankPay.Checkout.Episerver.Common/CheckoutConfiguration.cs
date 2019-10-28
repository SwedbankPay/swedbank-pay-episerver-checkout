﻿using System.Collections.Generic;

namespace SwedbankPay.Checkout.Episerver.Common
{
    public class CheckoutConfiguration : ConnectionConfiguration
    {
        public List<string> HostUrls { get; set; }
        public string CompleteUrl { get; set; }
        public string CancelUrl { get; set; }
        public string CallbackUrl { get; set; }
        public string TermsOfServiceUrl { get; set; }
        public string PaymentUrl { get; set; }
        public string LogoUrl { get; set; }
        public bool UseAnonymousCheckout { get; set; }
    }
}