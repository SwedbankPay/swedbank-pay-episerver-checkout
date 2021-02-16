using Atata;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.Base
{
    [ControlDefinition(ContainingClass = "navigation__item", ComponentTypeName = "Navigation Item")]
    public class NavigationItem<TOwner> : Control<TOwner> where TOwner : PageObject<TOwner>
    {
        public ItemsControl<ProductCategoryItem<TOwner>, TOwner> ProductCategories { get; private set; }
    }
}
