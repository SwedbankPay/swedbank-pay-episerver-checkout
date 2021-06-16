using Atata;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Return
{
    using _ = CreateRefundFramePage;

    public class CreateRefundFramePage : Page<_>
    {
        [Wait(1, TriggerEvents.BeforeSet)]
        [FindById("ctl01_tbAmount")]
        public TextInput<_> Amount { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindById("ctl01_btnSave")]
        public Button<_> Confirm { get; private set; }
    }
}
