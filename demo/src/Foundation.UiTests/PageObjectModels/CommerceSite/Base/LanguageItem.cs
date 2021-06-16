using Atata;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.Base
{
    [ControlDefinition(ContainingClass = "language-list__language-text", ComponentTypeName = "Language Item")]
    public class LanguageItem<TOwner> : Control<TOwner> where TOwner : PageObject<TOwner>
    {
    }
}
