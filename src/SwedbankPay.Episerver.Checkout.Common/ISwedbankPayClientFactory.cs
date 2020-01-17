using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Microsoft.Extensions.Logging;
using SwedbankPay.Sdk;

namespace SwedbankPay.Episerver.Checkout.Common
{
    public interface ISwedbankPayClientFactory
    {
        ISwedbankPayClient Create(IMarket market, ILogger logger = null);
        ISwedbankPayClient Create(PaymentMethodDto paymentMethodDto, MarketId marketMarketId);
        ISwedbankPayClient Create(ConnectionConfiguration connectionConfiguration);
    }
}