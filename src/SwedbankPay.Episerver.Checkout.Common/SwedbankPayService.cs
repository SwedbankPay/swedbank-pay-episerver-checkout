using EPiServer.Commerce.Order;

using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Search;

using System.Linq;

namespace SwedbankPay.Episerver.Checkout.Common
{
    public abstract class SwedbankPayService : ISwedbankPayService
    {
        private readonly IOrderRepository _orderRepository;

        protected SwedbankPayService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public IPurchaseOrder GetPurchaseOrderBySwedbankPayOrderId(string orderId)
        {
            OrderSearchOptions searchOptions = new OrderSearchOptions
            {
                CacheResults = false,
                StartingRecord = 0,
                RecordsToRetrieve = 1,
                Classes = new System.Collections.Specialized.StringCollection { "PurchaseOrder" },
                Namespace = "Mediachase.Commerce.Orders"
            };

            var parameters = new OrderSearchParameters
            {
                SqlMetaWhereClause = $"META.{Constants.SwedbankPayOrderIdField} LIKE '{orderId}'"
            };

            var purchaseOrder = OrderContext.Current.Search<PurchaseOrder>(parameters, searchOptions)?.FirstOrDefault();

            if (purchaseOrder != null)
            {
                return _orderRepository.Load<IPurchaseOrder>(purchaseOrder.OrderGroupId);
            }
            return null;
        }


        public IPurchaseOrder GetByPayeeReference(string payeeReference)
        {
            OrderSearchOptions searchOptions = new OrderSearchOptions
            {
                CacheResults = false,
                StartingRecord = 0,
                RecordsToRetrieve = 1,
                Classes = new System.Collections.Specialized.StringCollection { "PurchaseOrder" },
                Namespace = "Mediachase.Commerce.Orders"
            };

            var parameters = new OrderSearchParameters
            {
                SqlMetaWhereClause = $"META.{Constants.SwedbankPayPayeeReference} = '{payeeReference}'"
            };

            var purchaseOrder = OrderContext.Current.Search<PurchaseOrder>(parameters, searchOptions)?.FirstOrDefault();

            if (purchaseOrder != null)
            {
                return _orderRepository.Load<IPurchaseOrder>(purchaseOrder.OrderGroupId);
            }

            return null;
        }
    }
}