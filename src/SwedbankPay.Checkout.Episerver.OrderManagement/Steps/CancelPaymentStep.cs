using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using SwedbankPay.Checkout.Episerver.Common;
using SwedbankPay.Checkout.Episerver.Common.Helpers;
using SwedbankPay.Checkout.Episerver.OrderManagement.Extensions;
using SwedbankPay.Sdk.Models.Request.Transaction;

namespace SwedbankPay.Checkout.Episerver.OrderManagement.Steps
{
    public class CancelPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CancelPaymentStep));

        public CancelPaymentStep(IPayment payment, IMarket market, SwedbankPayOrderServiceFactory swedbankPayOrderServiceFactory) : base(payment, market, swedbankPayOrderServiceFactory)
        {
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message)
        {
            if (payment.TransactionType == TransactionType.Void.ToString())
            {
                try
                {
                    var orderId = orderGroup.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
                    var amount = AmountHelper.GetAmount((decimal)payment.Amount);
                    var previousPayment = orderForm.Payments.FirstOrDefault(x => x.IsSwedbankPayPayment());
                    if (previousPayment != null && previousPayment.TransactionType == TransactionType.Sale.ToString())
                    {
                        var transaction = new TransactionRequest
                        {
                            Amount = amount,
                            Description = "Order canceled"
                        };
                        var captureRequestObject = new TransactionRequestContainer(transaction);
                        
                        var reversalResponseObject = AsyncHelper.RunSync(() => SwedbankPayOrderService.Reversal(captureRequestObject, orderId));
                        if (reversalResponseObject == null)
                        {
                            payment.Status = PaymentStatus.Failed.ToString();
                            message = "Reversal is not a valid operation";
                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {message}");
                            Logger.Error($"Reversal is not a valid operation for {orderId}");
                            return false;
                        }

                        payment.Status = PaymentStatus.Processed.ToString();

                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Refunded {payment.Amount}");
                    }
                    
                  
                    else if (!string.IsNullOrEmpty(orderId))
                    {
                        var cancelResponseObject = SwedbankPayOrderService.CancelOrder(orderId);
                        if (cancelResponseObject != null)
                        {
                            payment.Status = PaymentStatus.Processed.ToString();
                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Order cancelled at SwedbankPay");
                            return true;
                        }
                        else
                        {
                            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Cancel is not possible on this order {orderId}");
                            return false;
                        }
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