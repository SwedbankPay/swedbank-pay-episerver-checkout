using Atata;
using Foundation.UiTests.PageObjectModels.Base.Attributes;
using Foundation.UiTests.PageObjectModels.CommerceSite.Base;

namespace Foundation.UiTests.PageObjectModels.Payment
{
    using _ = PaymentFramePage;

    [WaitForLoader]
    public class PaymentFramePage : BaseCommercePage<_>
    {
        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByClass("paymentmenu-container")]
        public ControlList<PayexItem<_>, _> PaymentMethods { get; set; }
    }
}