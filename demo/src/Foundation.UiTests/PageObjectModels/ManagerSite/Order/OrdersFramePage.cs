using Atata;
using Foundation.UiTests.PageObjectModels.ManagerSite.Base;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Order
{
    using _ = OrdersFramePage;

    public class OrdersFramePage : BaseManagerPage<_>
    {
        [FindById("ctl03_MyListView_MainListView_lvTable")]
        public Table<OrderRowItem<_>, _> OrderTable { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindById("ctl03_MyListView_MainListView_header_cb")]
        public CheckBox<_> SelectAll { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Delete Selected")]
        public Button<_> DeleteSelected { get; private set; }

    }
}
