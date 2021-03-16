using Atata;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.Products
{
    [ControlDefinition(ContainingClass = "modal-dialog", ComponentTypeName = "Modal Product Item")]
    public class ProductModalItem<TOwner> : Control<TOwner> where TOwner : PageObject<TOwner>
    {
        [FindByClass("title")]
        public H4<TOwner> Title { get; private set; }

        [FindByClass("price__discount")]
        public Text<TOwner> Price { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByClass("addToCart")]
        public Button<TOwner> AddToCart { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByClass("close")]
        public Button<TOwner> CloseModalWindow { get; private set; }

    }
}
