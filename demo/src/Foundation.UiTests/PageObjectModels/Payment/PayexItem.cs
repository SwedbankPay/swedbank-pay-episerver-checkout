using Atata;

namespace Foundation.UiTests.PageObjectModels.Payment
{
    [ControlDefinition("div", ContainingClass = "custom-menu-card")]
    public class PayexItem<TOwner> : Control<TOwner> where TOwner : Page<TOwner>
    {
        [Wait(1, TriggerEvents.BeforeAndAfterClick)]
        [FindByClass("menu-card-title")] public Text<TOwner> Name { get; private set; }

        [WaitFor(Until.Visible, TriggerEvents.BeforeAccess)]
        public Frame<TOwner> PaymentFrame { get; private set; }
    }
}
