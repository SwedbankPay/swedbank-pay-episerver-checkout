using Atata;
using Foundation.UiTests.PageObjectModels.Base.Attributes;

namespace Foundation.UiTests.PageObjectModels.Payment
{
    using _ = CardConfirmationPage;

    [WaitForLoader]
    public class CardConfirmationPage: Page<_>
    {
        [Wait(1, TriggerEvents.BeforeClick)]
        [FindFirst()] public Button<_> Confirm { get; set; }

    }
}