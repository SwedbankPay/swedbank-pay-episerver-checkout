using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using SwedbankPay.Sdk.Consumers;
using SwedbankPay.Sdk.PaymentOrders;
using SwedbankPay.Sdk.Transactions;

namespace SwedbankPay.Episerver.Checkout.Common
{
    public interface IRequestFactory
    {
        PaymentOrderRequestContainer GetPaymentOrderRequestContainer(IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string consumerProfileRef = null);
        ConsumersRequest GetConsumerResourceRequest(IMarket market, string email, string mobilePhone, string ssn);
        TransactionRequest GetTransactionRequest(IPayment payment, IMarket market, IShipment shipment, string description, bool addShipmentInOrderItem = true);
    }
}
