using Atata;
using Foundation.UiTests.PageObjectModels.CommerceSite.Checkout;
using Foundation.UiTests.PageObjectModels.CommerceSite.Products;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.Base
{
    [WaitForDocumentReadyState(Timeout = 10)]
    public abstract class BaseCommercePage<TOwner> : Page<TOwner>
        where TOwner : BaseCommercePage<TOwner>
    {
        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByClass("market-selector__dropdown-icon")]
        public Clickable<TOwner> Settings { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByClass("market-list")]
        public ItemsControl<MarketItem<TOwner>, TOwner> MarketList { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByClass("language-list")]
        public ItemsControl<LanguageItem<TOwner>, TOwner> LanguageList { get; private set; }

        [Wait(3, TriggerEvents.BeforeClick)]
        [FindByContent("New")]
        public Clickable<ProductsPage, TOwner> New { get; private set; }

        [FindByContent("| sv | kr")]
        public Text<TOwner> MarketLanguageSelection { get; private set; }

        [FindByClass("alert-success")]
        public Control<TOwner> AlertSuccess { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByClass("jsCartBtn")]
        public Clickable<TOwner> Checkout { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindById("checkoutBtnId")]
        public Button<CheckoutPage, TOwner> ContinueToCheckout { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Checkout")]
        public Link<CheckoutPage, TOwner> CheckoutDirect { get; private set; }

    }
}
