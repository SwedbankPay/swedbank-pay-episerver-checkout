using System;

namespace SwedbankPay.Episerver.Checkout.Common
{
    public class ConnectionConfiguration
    {
        public string MarketId { get; set; }
        public string Token { get; set; }
        public Uri ApiUrl { get; set; }
        public string MerchantId { get; set; }
    }
}
