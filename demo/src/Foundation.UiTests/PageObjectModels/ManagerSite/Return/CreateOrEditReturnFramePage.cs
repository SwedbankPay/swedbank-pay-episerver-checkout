using Atata;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Return
{
    using _ = CreateOrEditReturnFramePage;

    public class CreateOrEditReturnFramePage : Page<_>
    {
        [Wait(1, TriggerEvents.BeforeAndAfterClick)]
        [FindByContent("New Line Item")]
        public Button<_> NewLineItem { get; private set; }

        [FindById("ctl01_MyListView_MainListView_lvTable")]
        public Table<_> ReturnTable { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindById("ctl01_btnSave")]
        public Button<_> Confirm { get; private set; }

        [FindById("McCommandHandlerFrameContainer_McCommandHandlerFrameIFrame")]
        public Frame<_> NewLineItemFrame { get; private set; }

    }
}
