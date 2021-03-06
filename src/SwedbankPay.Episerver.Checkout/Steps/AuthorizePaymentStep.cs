﻿using EPiServer.Commerce.Order;
using EPiServer.Logging;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.OrderManagement.Steps;

using System;
using System.Linq;

namespace SwedbankPay.Episerver.Checkout.Steps
{
    public class AuthorizePaymentStep : AuthorizePaymentStepBase
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(AuthorizePaymentStep));

        public AuthorizePaymentStep(IPayment payment, IMarket market, SwedbankPayClientFactory swedbankPayClientFactory) : base(payment, market, swedbankPayClientFactory)
        {
        }

        public override PaymentStepResult ProcessAuthorization(IPayment payment, IOrderGroup orderGroup)
        {
            var paymentStepResult = new PaymentStepResult();

            var orderId = orderGroup.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
            if (!string.IsNullOrEmpty(orderId))
            {
                try
                {
                    var result =  AsyncHelper.RunSync(() => SwedbankPayClient.PaymentOrders.Get(new Uri(orderId, UriKind.Relative), Sdk.PaymentOrders.PaymentOrderExpand.All));
                    var transaction = result.PaymentOrder.CurrentPayment.Payment.Transactions.TransactionList?.FirstOrDefault();
                    if (transaction != null)
                    {
                        payment.ProviderTransactionID = transaction.Number.ToString();
                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Authorize completed at SwedbankPay, Transaction number: {transaction.Number}");
                        paymentStepResult.Status = true;
                    }
                }
                catch (Exception ex)
                {
                    payment.Status = PaymentStatus.Failed.ToString();
                    paymentStepResult.Message = ex.Message;
                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {ex.Message}");
                    Logger.Error(ex.Message, ex);
                }

                return paymentStepResult;
            }

            return paymentStepResult;
        }
    }
}