using Atata;
using Foundation.UiTests.PageObjectModels.ManagerSite.Base;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Shipment
{
    using _ = ConfirmationShipmentFramePage;

    public class ConfirmationShipmentFramePage : BaseManagerPage<_>
    {
        [FindByContent(TermMatch.Contains, "OK")]
        public Button<_> Confirm { get; private set; }
    }
}
