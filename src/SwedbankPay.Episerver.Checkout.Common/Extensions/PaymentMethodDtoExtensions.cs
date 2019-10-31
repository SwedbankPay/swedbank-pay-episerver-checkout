using System;
using System.Linq;
using EPiServer.Globalization;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Newtonsoft.Json;

namespace SwedbankPay.Episerver.Checkout.Common.Extensions
{
    public static class PaymentMethodDtoExtensions
    {
        public static ConnectionConfiguration GetConnectionConfiguration(this PaymentMethodDto paymentMethodDto, MarketId marketId)
        {
            var configuration = JsonConvert.DeserializeObject<ConnectionConfiguration>(paymentMethodDto.GetParameter($"{marketId.Value}_{Constants.SwedbankPaySerializedMarketOptions}", string.Empty));

            if (configuration == null)
            {
                throw new Exception($"PaymentMethod {paymentMethodDto.PaymentMethod.FirstOrDefault()?.SystemKeyword} is not configured for market {marketId} and language {ContentLanguage.PreferredCulture.Name}");
            }

            return new ConnectionConfiguration
            {
                ApiUrl = configuration.ApiUrl,
                MerchantId = configuration.MerchantId,
                Token = configuration.Token
            };
        }
    }
}
