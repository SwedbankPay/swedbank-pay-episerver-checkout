using Atata;
using Foundation.UiTests.PageObjectModels.CommerceSite.Base;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.Products
{
    using _ = ProductPage;

    public class ProductPage : BaseCommercePage<_>
    {
        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByClass("jsAddToCart")]
        public Button<_> AddToCart { get; private set; }
    }
}
