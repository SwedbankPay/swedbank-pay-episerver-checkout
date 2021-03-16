using Atata;
using Foundation.UiTests.PageObjectModels.ManagerSite.Order;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Return
{
    [ControlDefinition("span[contains(@id, 'ctl03_xmlStruct_ReturnOrderRepeater_ObjRepeater_RepItem_')]/table", ComponentTypeName = "Return Row")]
    public class ReturnTableRow<TOwner> : Table<TOwner> where TOwner : Page<TOwner>
    {
        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Complete Return")]
        public Button<TOwner> CompleteReturn { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Acknowledge Receipt Items")]
        public Button<TOwner> AcknowledgeReceiptItems { get; private set; }

        [FindById(TermMatch.Contains, "ctl03_xmlStruct_ReturnOrderRepeater_ObjRepeater_RepItem_0_LineItemsGrid_0_MyListView_0_MainGrid_")]
        public Table<PaymentRowItem<TOwner>, TOwner> TableReturns { get; private set; }
    }
}
