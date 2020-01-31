using EPiServer.Globalization;
using EPiServer.ServiceLocation;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

using Newtonsoft.Json;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.Extensions;

using System;
using System.Linq;

namespace SwedbankPay.Episerver.Checkout
{
    [ServiceConfiguration(typeof(ICheckoutConfigurationLoader))]
    public class DefaultCheckoutConfigurationLoader : ICheckoutConfigurationLoader
    {
        public CheckoutConfiguration GetConfiguration(MarketId marketId)
        {
            return GetConfiguration(marketId, ContentLanguage.PreferredCulture.Name);
        }

        public CheckoutConfiguration GetConfiguration(MarketId marketId, string languageId)
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.SwedbankPayCheckoutSystemKeyword, languageId, returnInactive: true);
            if (paymentMethod == null)
            {
                throw new Exception($"PaymentMethod {Constants.SwedbankPayCheckoutSystemKeyword} is not configured for market {marketId} and language {ContentLanguage.PreferredCulture.Name}");
            }
            return GetConfiguration(paymentMethod, marketId);
        }
        public CheckoutConfiguration GetConfiguration(PaymentMethodDto paymentMethodDto, MarketId marketId)
        {
            var languageId = paymentMethodDto.PaymentMethod.FirstOrDefault()?.LanguageId;
            var parameter = paymentMethodDto.GetParameter($"{marketId.Value}_{languageId}_{Constants.SwedbankPaySerializedMarketOptions}", string.Empty);

            var configuration = JsonConvert.DeserializeObject<CheckoutConfiguration>(parameter);

            return configuration ?? new CheckoutConfiguration();
        }

        public void SetConfiguration(CheckoutConfiguration configuration, PaymentMethodDto paymentMethod, string currentMarket)
        {
            var serialized = JsonConvert.SerializeObject(configuration);
            var languageId = paymentMethod.PaymentMethod.First().LanguageId;
            paymentMethod.SetParameter($"{currentMarket}_{languageId}_{Constants.SwedbankPaySerializedMarketOptions}", serialized);
        }
    }
}
