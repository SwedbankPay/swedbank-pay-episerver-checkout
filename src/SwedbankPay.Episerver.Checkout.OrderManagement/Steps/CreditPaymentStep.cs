using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus;
using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Sdk;

namespace SwedbankPay.Episerver.Checkout.OrderManagement.Steps
{
    public class CreditPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CreditPaymentStep));
        private readonly IRequestFactory _requestFactory;
        private readonly IMarket _market; 

        public CreditPaymentStep(IPayment payment, IMarket market, SwedbankPayClientFactory swedbankPayClientFactory, IRequestFactory requestFactory) 
            : base(payment, market, swedbankPayClientFactory)
        {
            _requestFactory = requestFactory;
            _market = market;
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message)
        {
            if (payment.TransactionType == TransactionType.Credit.ToString())
            {
                try
                {
                    var orderId = orderGroup.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
                    if (!string.IsNullOrEmpty(orderId))
                    {
                        if (orderGroup is IPurchaseOrder purchaseOrder)
                        {
                            var returnForm = purchaseOrder.ReturnForms.FirstOrDefault(x => ((OrderForm)x).Status == ReturnFormStatus.Complete.ToString() && ((OrderForm)x).ObjectState == MetaObjectState.Modified);
                            
                            if (returnForm != null)
                            {
                                var transactionDescription = string.IsNullOrWhiteSpace(returnForm.ReturnComment)
                                        ? "credit"
                                        : returnForm.ReturnComment;
                                var reversalRequest =
                                    _requestFactory.GetReversalRequest(payment, _market, shipment, true);

                                var paymentOrder = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrder.Get(new Uri(orderId)));

                                if (paymentOrder.Operations.Reversal != null)
                                {
                                    var reversalResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Reversal(reversalRequest));

                                    if (reversalResponse.Reversal.Transaction.Type == "Reversal" && reversalResponse.Reversal.Transaction.State.Equals(State.Completed))
                                    {
                                        payment.Status = PaymentStatus.Processed.ToString();

                                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Refunded {payment.Amount}");
                                        return true;
                                    }
                                    
                                }

                                payment.Status = PaymentStatus.Failed.ToString();
                                message = "Reversal is not a valid operation";
                                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {message}");
                                Logger.Error($"Reversal is not a valid operation for {orderId}");
                                return false;

                            }
                        }
                        
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    var exceptionMessage = GetExceptionMessage(ex);

                    payment.Status = PaymentStatus.Failed.ToString();
                    message = exceptionMessage;
                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {exceptionMessage}");
                    Logger.Error(exceptionMessage, ex);
                    return false;
                }
            }
            else if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, shipment, ref message);
            }
            return false;
        }
    }
}
