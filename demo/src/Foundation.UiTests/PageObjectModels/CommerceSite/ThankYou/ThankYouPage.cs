using Atata;
using Foundation.UiTests.PageObjectModels.CommerceSite.Base;

namespace Foundation.UiTests.PageObjectModels.CommerceSite.ThankYou
{
    using _ = ThankYouPage;

    public class ThankYouPage : BaseCommercePage<_>
    {
        [FindByContent(TermMatch.Contains, "Order ID")]
        public H4<_> OrderId { get; private set; }
    }
}
