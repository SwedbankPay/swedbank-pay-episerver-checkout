using EPiServer.Commerce.Order;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

using SwedbankPay.Episerver.Checkout.Common;

using System.Threading.Tasks;

namespace SwedbankPay.Episerver.Checkout.OrderManagement.Steps
{
    public abstract class AuthorizePaymentStepBase : PaymentStep
    {
        protected AuthorizePaymentStepBase(IPayment payment, IMarket market, ISwedbankPayClientFactory swedbankPayClientFactory) : base(payment, market, swedbankPayClientFactory)
        {
        }

        public override async Task<PaymentStepResult> Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment)
        {
            if (payment.TransactionType != TransactionType.Authorization.ToString())
            {
                var paymentStepResult = new PaymentStepResult();
                if (Successor != null)
                {
                    paymentStepResult = await Successor.Process(payment, orderForm, orderGroup, shipment).ConfigureAwait(false);
                    paymentStepResult.Status = paymentStepResult.Status;
                    paymentStepResult.Status = Successor != null && paymentStepResult.Status;
                }

                return paymentStepResult;
            }

            return await ProcessAuthorization(payment, orderGroup).ConfigureAwait(false);
        }

        public abstract Task<PaymentStepResult> ProcessAuthorization(IPayment payment, IOrderGroup orderGroup);
    }
}
