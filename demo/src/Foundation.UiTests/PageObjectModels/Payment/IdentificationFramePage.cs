using Atata;
using Foundation.UiTests.PageObjectModels.Base.Attributes;

namespace Foundation.UiTests.PageObjectModels.Payment
{
    using _ = IdentificationFramePage;

    [WaitForLoader]
    public class IdentificationFramePage : Page<_>
    {
        [FindById("email")]
        public EmailInput<_> Email { get; set; }

        [FindById("msisdn")]
        public TelInput<_> Phone { get; set; }

        [FindById("firstName")]
        public TextInput<_> Firstname { get; set; }

        [FindById("lastName")]
        public TextInput<_> Lastname { get; set; }

        [FindById("streetAddress")]
        public TextInput<_> Street { get; set; }

        [FindById("zipCode")]
        public TextInput<_> Zipcode { get; set; }

        [FindById("city")]
        public TextInput<_> City { get; set; }

        [FindById("px-submit")]
        public Button<_> Next { get; set; }

        [FindByClass("custom-secondary-button")]
        public Button<_> ContinueWithoutSaving { get; set; }

    }
}