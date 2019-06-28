namespace PayEx.Checkout.Episerver.OrderManagement.Steps
{
    using EPiServer.Commerce.Order;
    using Mediachase.Commerce;
    using Mediachase.Commerce.Orders.Dto;
    using Mediachase.Commerce.Orders.Managers;
    using PayEx.Checkout.Episerver.Common.Extensions;
    using PayEx.Net.Api.Exceptions;
    using System;
    using System.Linq;
    using System.Net;

    public abstract class PaymentStep
    {
        protected PaymentStep Successor;
        protected PaymentMethodDto PaymentMethod { get; set; }
        public IPayExOrderService PayExOrderService { get; }
        public MarketId MarketId { get; }

        public PaymentStep(IPayment payment, MarketId marketId, PayExOrderServiceFactory payExOrderServiceFactory)
        {
            MarketId = marketId;
            PaymentMethod = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
            
            if(PaymentMethod != null)
            {
                PayExOrderService = payExOrderServiceFactory.Create(PaymentMethod.GetConnectionConfiguration(marketId));
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
            var exceptionMessage = string.Empty;
            switch (ex)
            {
                case PayExException apiException:
                
                    exceptionMessage =
                        $"{apiException.Error.Instance}" +
                        $"{apiException.ErrorCode} " +
                        $"{string.Join(", ", apiException.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}";
                    break;
                case WebException webException:
                    exceptionMessage = webException.Message;
                    break;
            }
            return exceptionMessage;
        }
    }
}
