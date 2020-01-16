using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace SwedbankPay.Episerver.Checkout.OrderManagement.Steps
{
    public abstract class AuthorizePaymentStepBase : PaymentStep
    {
        public AuthorizePaymentStepBase(IPayment payment, IMarket market, SwedbankPayClientFactory swedbankPayClientFactory) : base(payment, market, swedbankPayClientFactory)
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
