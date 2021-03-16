using Atata;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.Base
{
    [ControlDefinition(ContainingClass = "link", ComponentTypeName = "Product Category Item")]
    public class ProductCategoryItem<TOwner> : Control<TOwner> where TOwner : PageObject<TOwner>
    {
        
    }
}
