using EPiServer.Commerce.Order;
using EPiServer.Logging;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Sdk;

using System;
using System.Linq;
using TransactionType = Mediachase.Commerce.Orders.TransactionType;

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
                                        ? "Reversing payment."
                                        : returnForm.ReturnComment;

                                var reversalRequest = _requestFactory.GetReversalRequest(returnForm.GetAllReturnLineItems(), _market, returnForm.Shipments.FirstOrDefault(), false, description: transactionDescription);
                                var paymentOrder = AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrder.Get(new Uri(orderId, UriKind.Relative)));

                                if (paymentOrder.Operations.Reversal == null)
                                {
                                    payment.Status = PaymentStatus.Failed.ToString();
                                    message = "Reversal is not a valid operation";
                                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {message}");
                                    Logger.Error($"Reversal is not a valid operation for {orderId}");
                                    return false;
                                }

                                var reversalResponse = AsyncHelper.RunSync(() => paymentOrder.Operations.Reversal(reversalRequest));
                                if (reversalResponse.Reversal.Transaction.Type == Sdk.TransactionType.Reversal && reversalResponse.Reversal.Transaction.State.Equals(State.Completed))
                                {
                                    payment.Status = PaymentStatus.Processed.ToString();
                                    payment.TransactionID = reversalResponse.Reversal.Transaction.Number;
                                    payment.ProviderTransactionID = reversalResponse.Reversal.Transaction.Id.ToString();
                                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Refunded {payment.Amount}");
                                    
                                    return true;
                                }
                            }
                        }
                        
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    payment.Status = PaymentStatus.Failed.ToString();
                    message = ex.Message;
                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {ex.Message}");
                    Logger.Error(ex.Message, ex);
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
