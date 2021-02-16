using Atata;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Base
{
    using _ = HomeManagerPage;

#if DEBUG
    [Url("https://mgrpayexepiserver001dev.azurewebsites.net/")]
#elif RELEASE
    [Url("https://mgrpayexepiserver001tst.azurewebsites.net/")]
#endif
    public class HomeManagerPage : BaseManagerPage<_>
    {
        [FindById("LoginCtrl_UserName")]
        public TextInput<_> UserName { get; private set; }

        [FindById("LoginCtrl_Password")]
        public PasswordInput<_> Password { get; private set; }

        [FindById("LoginCtrl_LoginButton")]
        public Clickable<ManagerPage, _> Login { get; private set; }
    }
}
