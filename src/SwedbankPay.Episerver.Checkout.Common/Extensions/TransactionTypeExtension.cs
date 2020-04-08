namespace SwedbankPay.Episerver.Checkout.Common.Extensions
{
    using SwedbankPay.Sdk;

    public static class TransactionTypeExtension
    {
        public static Mediachase.Commerce.Orders.TransactionType ConvertToEpiTransactionType(this TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.Authorization:
                    return Mediachase.Commerce.Orders.TransactionType.Authorization;
                case TransactionType.Cancellation:
                    return Mediachase.Commerce.Orders.TransactionType.Void;
                case TransactionType.Capture:
                    return Mediachase.Commerce.Orders.TransactionType.Capture;
                case TransactionType.Reversal:
                    return Mediachase.Commerce.Orders.TransactionType.Credit;
                case TransactionType.Sale:
                    return Mediachase.Commerce.Orders.TransactionType.Sale;
                default:
                    return Mediachase.Commerce.Orders.TransactionType.Other;
            }
        }
    }
}