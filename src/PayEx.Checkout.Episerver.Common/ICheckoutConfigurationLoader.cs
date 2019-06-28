namespace PayEx.Checkout.Episerver.Common
{
    using Mediachase.Commerce;
    using Mediachase.Commerce.Orders.Dto;

    public interface ICheckoutConfigurationLoader
    {
        CheckoutConfiguration GetConfiguration(MarketId marketId);

        CheckoutConfiguration GetConfiguration(MarketId marketId, string languageId);

        void SetConfiguration(CheckoutConfiguration configuration, PaymentMethodDto paymentMethod, string currentMarket);
    }
}
