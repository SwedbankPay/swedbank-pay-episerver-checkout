using EPiServer.Commerce.Order;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.Common.Extensions;
using SwedbankPay.Sdk;

using System.Linq;
using System.Threading.Tasks;

namespace SwedbankPay.Episerver.Checkout.OrderManagement.Steps
{
    public abstract class PaymentStep
    {
        protected PaymentStep Successor;

        public PaymentStep(IPayment payment, IMarket market, ISwedbankPayClientFactory swedbankPayClientFactory)
        {
            MarketId = market.MarketId;
            PaymentMethod = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);

            if (PaymentMethod != null)
                SwedbankPayClient = swedbankPayClientFactory.Create(PaymentMethod, market.MarketId);
        }

        protected PaymentMethodDto PaymentMethod { get; set; }
        public ISwedbankPayClient SwedbankPayClient { get; }
        public MarketId MarketId { get; }


        public void SetSuccessor(PaymentStep successor)
        {
            Successor = successor;
        }

        public abstract Task<PaymentStepResult> Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup,
            IShipment shipment);

        public void AddNoteAndSaveChanges(IOrderGroup orderGroup, string transactionType, string noteMessage)
        {
            var noteTitle = $"{PaymentMethod.PaymentMethod.FirstOrDefault()?.Name} {transactionType.ToLower()}";
            orderGroup.AddNote(noteTitle, $"Payment {transactionType.ToLower()}: {noteMessage}");
        }
    }
}