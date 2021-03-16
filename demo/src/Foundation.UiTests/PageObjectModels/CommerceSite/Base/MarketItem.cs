using Atata;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.Base
{
    [ControlDefinition(ContainingClass = "market-selector__market-text", ComponentTypeName = "Market Item")]
    public class MarketItem<TOwner> : Control<TOwner> where TOwner : PageObject<TOwner>
    {
    }
}
