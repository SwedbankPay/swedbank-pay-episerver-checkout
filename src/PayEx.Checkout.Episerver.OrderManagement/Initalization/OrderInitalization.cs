namespace PayEx.Checkout.Episerver.OrderManagement.Initalization
{
    using EPiServer.Commerce.Order;
    using EPiServer.Framework;
    using EPiServer.Framework.Initialization;
    using EPiServer.ServiceLocation;
    using Mediachase.Commerce.Orders;
    using PayEx.Checkout.Episerver.OrderManagement.Events;

    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]

    internal class OrderInitalization : IInitializableModule
    {
        private IServiceLocator _locator;

        public void Initialize(InitializationEngine context)
        {
            _locator = context.Locate.Advanced;
            OrderContext.Current.OrderGroupUpdated += Current_OrderGroupUpdated;

        }

        private void Current_OrderGroupUpdated(object sender, OrderGroupEventArgs e)
        {
            var orderRepository = _locator.GetInstance<IOrderRepository>();
            if (e.OrderGroupType != OrderGroupEventType.PurchaseOrder) return;

            var orderAfterSave = sender as IPurchaseOrder;
            var orderBeforeSave = orderRepository.Load<IPurchaseOrder>(e.OrderGroupId);
            if (orderBeforeSave == null || orderAfterSave == null) return;

            if (orderAfterSave.OrderStatus == OrderStatus.Cancelled)
            {
                var orderCancelledEventHandler = _locator.GetInstance<OrderCancelledEventHandler>();
                orderCancelledEventHandler.Handle(new OrderCancelledEvent(orderAfterSave));
            }
        }

        public void Uninitialize(InitializationEngine context)
        {
            OrderContext.Current.OrderGroupUpdated -= Current_OrderGroupUpdated;
        }
    }
}
