using Atata;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.Products
{
    [ControlDefinition(ContainingClass = "product-tile-grid", ComponentTypeName = "Product Item")]
    public class ProductItem<TOwner> : Control<TOwner> where TOwner : PageObject<TOwner>
    {
        [FindByClass("price__discount")]
        public Text<TOwner> Price { get; private set; }

        [FindByXPath("//*[@class = 'product-tile-grid__title']/a")]
        public Text<TOwner> Name { get; private set; }

        [FindByClass("product-tile-grid__image-icon")]
        public Link<ProductPage, TOwner> Link { get; private set; }

        [FindByClass("jsQuickView hover-feather-icon")]
        public Clickable<TOwner> OpenModalWindow { get; private set; }

    }
}
