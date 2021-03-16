using Atata;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Return
{
    using _ = NewLineItemFramePage;

    public class NewLineItemFramePage : Page<_>
    {
        [Wait(1, TriggerEvents.BeforeSet)]
        [FindById("ctl01_OriginalLineItems")]
        public Select<_> Item { get; private set; }

        [Wait(1, TriggerEvents.BeforeSet)]
        [FindById("ctl01_ReturnQuantity")]
        public TextInput<_> Quantity { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindById("ctl01_btnSave")]
        public Button<_> Confirm { get; private set; }
    }
}
