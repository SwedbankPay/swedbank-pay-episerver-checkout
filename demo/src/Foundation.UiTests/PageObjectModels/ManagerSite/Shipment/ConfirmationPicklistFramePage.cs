using Atata;
using Foundation.UiTests.PageObjectModels.ManagerSite.Base;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Shipment
{
    using _ = ConfirmationPickListFramePage;

    public class ConfirmationPickListFramePage : BaseManagerPage<_>
    {
        [Wait(1, TriggerEvents.BeforeSet)]
        [FindById("ctl01_Shipments_TrackingNumber_0")]
        public TextInput<_> TrackingNumber { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindById("ctl01_btnSave")]
        public Clickable<_> Confirm { get; private set; }
    }
}
