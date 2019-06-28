namespace PayEx.Checkout.Episerver
{
    using EPiServer.Globalization;
    using EPiServer.ServiceLocation;
    using Mediachase.Commerce;
    using Mediachase.Commerce.Orders.Dto;
    using Mediachase.Commerce.Orders.Managers;
    using Newtonsoft.Json;
    using PayEx.Checkout.Episerver.Common;
    using PayEx.Checkout.Episerver.Extensions;
    using System;

    [ServiceConfiguration(typeof(ICheckoutConfigurationLoader))]
    public class DefaultCheckoutConfigurationLoader : ICheckoutConfigurationLoader
    {
        public CheckoutConfiguration GetConfiguration(MarketId marketId)
        {
            return GetConfiguration(marketId, ContentLanguage.PreferredCulture.Name);
        }

        public CheckoutConfiguration GetConfiguration(MarketId marketId, string languageId)
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.PayExCheckoutSystemKeyword, languageId, returnInactive: true);
            if (paymentMethod == null)
            {
                throw new Exception($"PaymentMethod {Constants.PayExCheckoutSystemKeyword} is not configured for market {marketId} and language {ContentLanguage.PreferredCulture.Name}");
            }
            return GetPayexCheckoutConfiguration(paymentMethod, marketId);
        }

        public void SetConfiguration(CheckoutConfiguration configuration, PaymentMethodDto paymentMethod, string currentMarket)
        {
            var serialized = JsonConvert.SerializeObject(configuration);
            paymentMethod.SetParameter($"{currentMarket}_{Constants.PayExSerializedMarketOptions}", serialized);
        }


        private static CheckoutConfiguration GetPayexCheckoutConfiguration(PaymentMethodDto paymentMethodDto, MarketId marketId)
        {
            var parameter = paymentMethodDto.GetParameter($"{marketId.Value}_{Constants.PayExSerializedMarketOptions}", string.Empty);

            var configuration = JsonConvert.DeserializeObject<CheckoutConfiguration>(parameter);

            return configuration ?? new CheckoutConfiguration();
        }
    }
}
