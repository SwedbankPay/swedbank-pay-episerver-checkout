using Atata;
using Foundation.UiTests.PageObjectModels.ManagerSite.Base;
using Foundation.UiTests.PageObjectModels.ManagerSite.Order;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Shipment
{
    using _ = PickListsFramePage;

    public class PickListsFramePage : BaseManagerPage<_>
    {
        [FindById("ctl03_MyListView_MainListView_lvTable")]
        public Table<OrderRowItem<_>, _> OrderTable { get; private set; }

        [Wait(1, TriggerEvents.BeforeAndAfterClick)]
        [FindById("ctl03_MyListView_MainListView_btn_1_Name")]
        public Link<_> SortByName { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Complete Shipment")]
        public Button<_> CompleteShipment { get; private set; }

        [FindById("McCommandHandlerFrameContainer_McCommandHandlerFrameIFrame")]
        public Frame<_> ShipmentConfirmationFrame { get; private set; }
    }
}
