using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;

namespace SwedbankPay.Checkout.Episerver.Common
{
    public interface ICheckoutConfigurationLoader
    {
        CheckoutConfiguration GetConfiguration(MarketId marketId);

        CheckoutConfiguration GetConfiguration(MarketId marketId, string languageId);

        void SetConfiguration(CheckoutConfiguration configuration, PaymentMethodDto paymentMethod, string currentMarket);
    }
}
