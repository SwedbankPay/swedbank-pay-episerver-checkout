using Atata;
using Foundation.UiTests.PageObjectModels.Base;
using Foundation.UiTests.PageObjectModels.Base.Attributes;

namespace Foundation.UiTests.PageObjectModels.Payment
{
    using _ = PayexCardFramePage;

    [WaitForLoader]
    public class PayexCardFramePage : Page<_>
    {
        [Wait(1, TriggerEvents.BeforeAnyAction)]
        [FindByClass("cards")]
        public UnorderedList<_> CardList { get; set; }

        [FindByName("cccvc")]
        public TelInput<_> CvcOnly { get; set; }

        [FindByClass("custom-link")]
        public Link<_> AddNewCard { get; private set; }

        [FindById("panInput")] 
        public TelInput<_> CreditCardNumber { get; set; }

        [FindById(TermMatch.Contains, "cvcInput")]
        public TelInput<_> Cvc { get; set; }

        [FindById("expiryInput")] public TelInput<_> ExpiryDate { get; set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindById("px-submit")] public Button<_> Pay { get; set; }

    }
}