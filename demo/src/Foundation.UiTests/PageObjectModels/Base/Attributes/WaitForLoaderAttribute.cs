using Atata;

namespace Foundation.UiTests.PageObjectModels.Base.Attributes
{
    public class WaitForLoaderAttribute : WaitForElementAttribute
    {
        public WaitForLoaderAttribute(TriggerEvents on = TriggerEvents.Init)
            : base(WaitBy.Class, "px-loader-circle", Until.VisibleThenMissingOrHidden, on)
        {
            PresenceTimeout = 3;
            ThrowOnPresenceFailure = false;
            AbsenceTimeout = 20;
        }
    }
}
