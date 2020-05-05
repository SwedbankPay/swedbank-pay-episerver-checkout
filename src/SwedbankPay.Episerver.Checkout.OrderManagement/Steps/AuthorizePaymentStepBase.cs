using EPiServer.Commerce.Order;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

using SwedbankPay.Episerver.Checkout.Common;

namespace SwedbankPay.Episerver.Checkout.OrderManagement.Steps
{
    public abstract class AuthorizePaymentStepBase : PaymentStep
    {
        protected AuthorizePaymentStepBase(IPayment payment, IMarket market, ISwedbankPayClientFactory swedbankPayClientFactory) : base(payment, market, swedbankPayClientFactory)
        {
        }

        public override PaymentStepResult Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment)
        {
            if (payment.TransactionType != TransactionType.Authorization.ToString())
            {
                var paymentStepResult = new PaymentStepResult();
                if (Successor != null)
                {
                    paymentStepResult = Successor.Process(payment, orderForm, orderGroup, shipment);
                    paymentStepResult.Status = true;
                    paymentStepResult.Status = Successor != null && paymentStepResult.Status;
                }

                return paymentStepResult;
            }

            return ProcessAuthorization(payment, orderGroup);
        }

        public abstract PaymentStepResult ProcessAuthorization(IPayment payment, IOrderGroup orderGroup);
    }
}
