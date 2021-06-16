using Atata;

namespace Foundation.UiTests.PageObjectModels.Base.Attributes
{
    public class ControlDefinitionAutomationAttribute : ControlDefinitionAttribute
    {
        public ControlDefinitionAutomationAttribute(string automation) : base($"*[@automation='{automation}']")
        {
        }
    }
}
