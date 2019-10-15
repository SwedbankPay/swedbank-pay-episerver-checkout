using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace SwedbankPay.Checkout.Episerver.OrderManagement.Steps
{
    public abstract class AuthorizePaymentStepBase : PaymentStep
    {
        public AuthorizePaymentStepBase(IPayment payment, IMarket market, SwedbankPayOrderServiceFactory swedbankPayOrderServiceFactory) : base(payment, market, swedbankPayOrderServiceFactory)
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
