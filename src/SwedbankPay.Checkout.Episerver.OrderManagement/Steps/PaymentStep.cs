using System;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using SwedbankPay.Checkout.Episerver.Common.Extensions;

namespace SwedbankPay.Checkout.Episerver.OrderManagement.Steps
{
    public abstract class PaymentStep
    {
        protected PaymentStep Successor;
        protected PaymentMethodDto PaymentMethod { get; set; }
        public ISwedbankPayOrderService SwedbankPayOrderService { get; }
        public MarketId MarketId { get; }

        public PaymentStep(IPayment payment, IMarket market, SwedbankPayOrderServiceFactory swedbankPayOrderServiceFactory)
        {
            MarketId = market.MarketId;
            PaymentMethod = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
            
            if(PaymentMethod != null)
            {
                SwedbankPayOrderService = swedbankPayOrderServiceFactory.Create(market);
            }
        }

        
        public void SetSuccessor(PaymentStep successor)
        {
            Successor = successor;
        }

        public abstract bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message);

        public void AddNoteAndSaveChanges(IOrderGroup orderGroup, string transactionType, string noteMessage)
        {
            var noteTitle = $"{PaymentMethod.PaymentMethod.FirstOrDefault()?.Name} {transactionType.ToLower()}";

            orderGroup.AddNote(noteTitle, $"Payment {transactionType.ToLower()}: {noteMessage}");
        }

        protected string GetExceptionMessage(Exception ex)
        {
            return string.Empty; //TODO swed
            //var exceptionMessage = string.Empty;
            //switch (ex)
            //{
            //    case PayExException apiException:

            //        exceptionMessage =
            //            $"{apiException.Error.Instance}" +
            //            $"{apiException.ErrorCode} " +
            //            $"{string.Join(", ", apiException.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}";
            //        break;
            //    case WebException webException:
            //        exceptionMessage = webException.Message;
            //        break;
            //}
            //return exceptionMessage;
        }
    }
}
