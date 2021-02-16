using Atata;
using Foundation.UiTests.PageObjectModels.CommerceSite.Base;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.Products
{
    using _ = ProductsPage;

#if DEBUG
    [Url("https://payexepiserver001dev.azurewebsites.net/sv/fashion/mens/mens-shoes/")]
#elif RELEASE
    [Url("https://payexepiserver001tst.azurewebsites.net/sv/fashion/mens/mens-shoes/")]
#endif
    public class ProductsPage : BaseCommercePage<_>
    {
        [FindByClass("category-page__products")]
        public ControlList<ProductItem<_>, _> ProductList { get; private set; }

        [FindByClass("modal-dialog")]
        public ProductModalItem<_> Modal { get; private set; }
    }
}

