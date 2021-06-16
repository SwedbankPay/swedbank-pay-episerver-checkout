using Atata;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Base
{
    using _ = ManagerPage;

    public class ManagerPage : BaseManagerPage<_>
    {
        #region Tree

        [FindByContent("Order Management")]
        public Clickable<_> OrderManagement { get; private set; }

        [Wait(3, TriggerEvents.AfterClick)]
        [FindByContent("Today")]
        public Clickable<_> Today { get; private set; }

        [FindByContent("Shipping/Receiving")]
        public Clickable<_> ShippingReceiving { get; private set; }

        [FindByContent("Shipments")]
        public Clickable<_> Shipments { get; private set; }

        [FindByContent("Released for Shipping")]
        public Clickable<_> ReleasedForShipping { get; private set; }

        [FindByContent("Pick Lists")]
        public Clickable<_> PickLists { get; private set; }

        #endregion Tree

        [FindById("right")]
        public Frame<_> RightFrame { get; private set; }
    }
}
