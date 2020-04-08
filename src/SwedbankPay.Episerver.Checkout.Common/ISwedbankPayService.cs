using EPiServer.Commerce.Order;

namespace SwedbankPay.Episerver.Checkout.Common
{
    public interface ISwedbankPayService
    {
        IPurchaseOrder GetPurchaseOrderBySwedbankPayOrderId(string orderId);
    }
}
