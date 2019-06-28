namespace PayEx.Checkout.Episerver.OrderManagement.Steps
{
    using EPiServer.Commerce.Order;
    using Mediachase.Commerce;
    using Mediachase.Commerce.Orders;

    public abstract class AuthorizePaymentStepBase : PaymentStep
    {
        public AuthorizePaymentStepBase(IPayment payment, MarketId marketId, PayExOrderServiceFactory payExOrderServiceFactory) : base(payment, marketId, payExOrderServiceFactory)
        {
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message)
        {
            if (payment.TransactionType != TransactionType.Authorization.ToString())
            {
                return Successor != null && Successor.Process(payment, orderForm, orderGroup, shipment, ref message);
            }

            return ProcessAuthorization(payment, orderGroup, ref message);
        }

        public abstract bool ProcessAuthorization(IPayment payment, IOrderGroup orderGroup, ref string message);
    }
}
