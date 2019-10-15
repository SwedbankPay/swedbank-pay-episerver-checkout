using System;
using SwedbankPay.Checkout.Episerver.Common;
using SwedbankPay.Client;

namespace SwedbankPay.Checkout.Episerver.Extensions
{
    public static class SwedBankPayConfigurationExtensions
    {
        public static SwedbankPayOptions ToSwedbankPayConfiguration(this CheckoutConfiguration checkoutConfiguration, string merchantName = "Authority")
        {
            return new SwedbankPayOptions
            {
                Token = checkoutConfiguration.Token,
                MerchantId = checkoutConfiguration.MerchantId,
                ApiBaseUrl = !string.IsNullOrWhiteSpace(checkoutConfiguration.ApiUrl) ? new Uri(checkoutConfiguration.ApiUrl) : null,
                CallBackUrl = !string.IsNullOrWhiteSpace(checkoutConfiguration.CallbackUrl) ? new Uri(checkoutConfiguration.CallbackUrl) : null,
                CancelPageUrl = !string.IsNullOrWhiteSpace(checkoutConfiguration.CallbackUrl) ? new Uri(checkoutConfiguration.CancelUrl) : null,
                CompletePageUrl = !string.IsNullOrWhiteSpace(checkoutConfiguration.CompleteUrl) ? new Uri(checkoutConfiguration.CompleteUrl) : null,
                MerchantName = merchantName
            };
        }
    }
}
