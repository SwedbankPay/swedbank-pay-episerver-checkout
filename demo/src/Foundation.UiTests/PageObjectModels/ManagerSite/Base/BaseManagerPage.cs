using Atata;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Base
{
    [WaitForDocumentReadyState(Timeout = 10)]
    public abstract class BaseManagerPage<TOwner> : Page<TOwner>
        where TOwner : BaseManagerPage<TOwner>
    {

    }
}
