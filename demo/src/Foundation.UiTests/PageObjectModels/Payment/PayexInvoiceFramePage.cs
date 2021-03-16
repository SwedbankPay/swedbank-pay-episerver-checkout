using Atata;
using Foundation.UiTests.PageObjectModels.Base;
using Foundation.UiTests.PageObjectModels.Base.Attributes;
using Foundation.UiTests.PageObjectModels.CommerceSite;
using Foundation.UiTests.PageObjectModels.ManagerSite.Base;

namespace Foundation.UiTests.PageObjectModels.Payment
{
    using _ = PayexInvoiceFramePage;

    [WaitForLoader]
    public class PayexInvoiceFramePage : BaseManagerPage<_>
    {
        [FindById("emailInput")] public TextInput<_> Email { get; set; }

        [FindById("px-submit")] public Button<_> Next { get; set; }

        [WaitForElement(WaitBy.Id, "consumer-input", Until.Visible, TriggerEvents.BeforeClick)]
        [Wait(1, TriggerEvents.BeforeClick)]
        [FindById("px-submit")]
        public ButtonDelegate<_> Pay { get; set; }

        [FindById("ssnInput")] public TelInput<_> PersonalNumber { get; set; }

        [FindById("msisdnInput")] public TelInput<_> PhoneNumber { get; set; }

        [FindById("zipCodeInput")] public TelInput<_> ZipCode { get; set; }
    }
}