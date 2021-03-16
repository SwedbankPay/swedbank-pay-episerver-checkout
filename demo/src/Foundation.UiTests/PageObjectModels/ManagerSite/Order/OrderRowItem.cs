using Atata;
using Foundation.UiTests.PageObjectModels.ManagerSite.Base;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Order
{
    [ControlDefinition("tr", ComponentTypeName = "Row")]
    public class OrderRowItem<TOwner> : TableRow<TOwner> where TOwner : BaseManagerPage<TOwner>
    {
        [Wait(1, TriggerEvents.BeforeClick)]
        [FindFirst]
        public CheckBox<TOwner> CheckBox { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindFirst]
        public Link<OrderFramePage, TOwner> Link { get; private set; }
    }
}