using System;
using Mediachase.Commerce;

namespace SwedbankPay.Episerver.Checkout.Common.Helpers
{
    public static class AmountHelper
    {
        public static int GetAmount(decimal amount)
        {
            return (int)Math.Round(amount * 100);
        }

        public static int GetAmount(Money money)
        {
            if (money.Amount > 0)
            {
                return GetAmount((decimal) money.Amount);
            }
            return 0;
        }
    }
}
