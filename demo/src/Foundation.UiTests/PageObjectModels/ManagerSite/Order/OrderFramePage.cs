using Atata;
using Foundation.UiTests.PageObjectModels.ManagerSite.Return;

namespace Foundation.UiTests.PageObjectModels.ManagerSite.Order
{
    using _ = OrderFramePage;

    public class OrderFramePage : Page<_>
    {
        #region Tabs

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Summary")]
        public ListItem<_> Summary { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Details")]
        public ListItem<_> Details { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Payments")]
        public ListItem<_> Payments { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Returns")]
        public ListItem<_> Returns { get; private set; }

        #endregion Tabs

        #region Summary

        [FindByXPath("//span[contains(text(),'/psp/paymentorders/')]")]
        public Text<_> PaymentLink { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [CloseConfirmBox]
        [FindByContent("Cancel Order")]
        public Button<_> CancelOrder { get; private set; }

        #endregion

        #region Details

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Complete Shipment")]
        public Button<_> CompleteShipment { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Release Shipment")]
        public Button<_> ReleaseShipment { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Cancel Shipment")]
        public Button<_> CancelShipment { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindByContent("Create Return")]
        public Button<_> CreateReturn { get; private set; }

        [Wait(1, TriggerEvents.BeforeClick)]
        [FindById("ctl03_xmlStruct_EditModeCtrl_btnHolder_SaveButton")]
        public Button<_> SaveChanges { get; private set; }

        [FindById("McCommandHandlerFrameContainer_McCommandHandlerFrameIFrame")]
        public Frame<_> CreateOrEditReturnFrame { get; private set; }

        #endregion

        #region Payment

        [FindById("ctl03_xmlStruct_PaymentsGrid_MyListView_MainGrid")]
        public Table<PaymentRowItem<_>, _> TablePayment { get; private set; }

        #endregion

        #region Returns

        [FindByClass("blockSelected")]
        public ItemsControl<ReturnTableRow<_>, _> ReturnRows { get; private set; }

        [FindById("ctl03_xmlStruct_GeneralInfoCtrl1_lblTotal")]
        public Text<_> OrderTotal { get; private set; }

        [FindById("McCommandHandlerFrameContainer_McCommandHandlerFrameIFrame")]
        public Frame<_> CreateRefundFrame { get; private set; }

        #endregion
    }
}
