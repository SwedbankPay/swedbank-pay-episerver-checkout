using Atata;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.Base
{
    using _ = HomeCommercePage;

#if DEBUG
    [Url("https://payexepiserver001dev.azurewebsites.net")]
#elif RELEASE
    [Url("https://payexepiserver001tst.azurewebsites.net")]
#endif
    public class HomeCommercePage : BaseCommercePage<_>
    {

    }
}
