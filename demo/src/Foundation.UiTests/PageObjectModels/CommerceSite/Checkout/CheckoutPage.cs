using Atata;
using Foundation.UiTests.PageObjectModels.CommerceSite.Base;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.Checkout
{
    using _ = CheckoutPage;
    
#if DEBUG
    [Url("https://payexepiserver001dev.azurewebsites.net/sv/checkout/?isGuest=1")]
#elif RELEASE
    [Url("https://payexepiserver001tst.azurewebsites.net/sv/checkout/?isGuest=1")]
#endif

    public class CheckoutPage : BaseCommercePage<_>
    {
        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByClass("jsContinueCheckoutMethod")]
        public Button<_> ContinueAsGuest { get; private set; }

        [FindById("swedbankpay-checkout")]
        public Frame<_> IdentificationFrame { get; private set; }

        [FindById("paymentMenuFrame")]
        public Frame<_> PaymentFrame { get; private set; }

        [FindById("OrderSummary_PaymentTotal")]
        public TextInput<_> TotalAmount { get; private set; }
    }
}
