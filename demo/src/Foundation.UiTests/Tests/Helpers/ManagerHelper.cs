using Atata;
using Foundation.UiTests.PageObjectModels.ManagerSite.Base;
using Foundation.UiTests.PageObjectModels.ManagerSite.Order;
using Foundation.UiTests.PageObjectModels.ManagerSite.Return;
using Foundation.UiTests.PageObjectModels.ManagerSite.Shipment;
using Foundation.UiTests.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foundation.UiTests.Tests.Helpers
{
    public static class ManagerHelper
    {
        #region Expand tree

        public static ManagerPage ExpandOrders(this ManagerPage frame)
        {
            return frame
                .Do(x =>
                {
                    if (x.Today.IsVisible.Value == false)
                    {
                        x
                        .OrderManagement.DoubleClick()
                        .Today.IsVisible.WaitTo.BeTrue();
                    }
                });
        }

        public static ManagerPage ExpandShipmentsAndPickLists(this ManagerPage frame)
        {
            return frame
                .Do(x =>
                {
                    if (x.Shipments.IsVisible.Value == false)
                    {
                        x
                        .ShippingReceiving.DoubleClick()
                        .Shipments.IsVisible.WaitTo.BeTrue();
                    }
                    if (x.ReleasedForShipping.IsVisible.Value == false)
                    {
                        x
                        .Shipments.DoubleClick()
                        .ReleasedForShipping.IsVisible.WaitTo.BeTrue();
                    }
                });
        }

        #endregion

        #region EPiServer Operations

        public static ManagerPage CancelOrder(this ManagerPage frame, string orderId)
        {
            return frame
                .ExpandOrders()
                .Today.DoubleClick()
                .RightFrame.DoWithin<OrdersFramePage>(x =>
                {
                    x
                    .OrderTable.IsVisible.WaitTo.BeTrue()
                    .OrderTable.Rows[x => x.Link.Content.Value.Contains(orderId)].Link.ClickAndGo()
                    .Summary.Click()
                    .CancelOrder.Click();
                },
                true);
        }

        public static ManagerPage CompleteAndReleaseShipment(this ManagerPage frame, string orderId)
        {
            return frame
                .ExpandOrders()
                .Today.DoubleClick()
                .RightFrame.DoWithin<OrdersFramePage>(x =>
                {
                    x
                    .OrderTable.IsVisible.WaitTo.BeTrue()
                    .OrderTable.Rows[x => x.Link.Content.Value.Contains(orderId)].Link.ClickAndGo()
                    .Details.Click()
                    .CompleteShipment.Click()
                    .ReleaseShipment.Click();
                },
                true);
        }

        public static ManagerPage ReleaseShipment(this ManagerPage frame, string orderId)
        {
            return frame
                .ExpandOrders()
                .Today.DoubleClick()
                .RightFrame.DoWithin<OrdersFramePage>(x =>
                {
                    x
                    .OrderTable.IsVisible.WaitTo.BeTrue()
                    .OrderTable.Rows[x => x.Link.Content.Value.Contains(orderId)].Link.ClickAndGo()
                    .Details.Click()
                    .ReleaseShipment.Click();
                },
                true);
        }

        public static ManagerPage AddShipmentToPickList(this ManagerPage frame, string orderId)
        {
            return frame
                .ExpandShipmentsAndPickLists()
                .ReleasedForShipping.DoubleClick()
                .RightFrame.DoWithin<ShipmentsFramePage>(x =>
                {
                    x
                    .OrderTable.Rows[x => x.Link.Content.Value.Contains(orderId)].CheckBox.Check()
                    .AddShipmentToPickLlist.Click()
                    .ShipmentConfirmationFrame.SwitchTo<ConfirmationShipmentFramePage>()
                    .Confirm.Click()
                    .Confirm.IsVisible.WaitTo.BeFalse();
                },
                true);
        }

        public static ManagerPage CompletePickListShipment(this ManagerPage frame, string orderId)
        {
            return frame
                .ExpandShipmentsAndPickLists()
                .PickLists.DoubleClick()
                .RightFrame.DoWithin<PickListsFramePage>(x =>
                {
                    x
                    .SortByName.Click().SortByName.Click()
                    .OrderTable.Rows[0].Link.Click()
                    .OrderTable.Rows[x => x.Link.Content.Value.Contains(orderId)].CheckBox.Check()
                    .CompleteShipment.Click()
                    .ShipmentConfirmationFrame.SwitchTo<ConfirmationPickListFramePage>()
                    .TrackingNumber.Set("123")
                    .Confirm.Click()
                    .Confirm.IsVisible.WaitTo.BeFalse();
                },
                true);
        }

        public static ManagerPage CreateReturn(this ManagerPage frame, string orderId, Product[] products)
        {
            return frame
                .ExpandOrders()
                .Today.DoubleClick()
                .RightFrame.DoWithin<OrdersFramePage>(x =>
                {
                    x
                    .OrderTable.IsVisible.WaitTo.BeTrue()
                    .OrderTable.Rows[x => x.Link.Content.Value.Contains(orderId)].Link.ClickAndGo()
                    .Details.Click()
                    .CreateReturn.Click()
                    .CreateOrEditReturnFrame.DoWithin<CreateOrEditReturnFramePage>(x =>
                    {
                        var count = 0;

                        foreach (var item in products)
                        {
                            x
                            .NewLineItem.Click()
                            .NewLineItemFrame.DoWithin<NewLineItemFramePage>(x =>
                            {
                                x
                                .Item.Set(item.Name)
                                .Quantity.Set(item.Quantity.ToString())
                                .Confirm.Click()
                                .Confirm.IsVisible.WaitTo.BeFalse();
                            },
                            true)
                            .SwitchToRoot<ManagerPage>().RightFrame.SwitchTo<OrderFramePage>().CreateOrEditReturnFrame.SwitchTo<CreateOrEditReturnFramePage>()
                            .ReturnTable.Rows.Count.WaitTo.Equal(++count);
                        }

                        x
                        .Confirm.Click()
                        .Confirm.IsVisible.WaitTo.BeFalse();
                    },
                    true)
                    .SwitchToRoot<ManagerPage>();
                },
                true);
        }

        public static ManagerPage CompleteReturn(this ManagerPage frame, Product[] products, bool partial, int index = 0)
        {
            return frame
                .RightFrame.DoWithin<OrderFramePage>(x =>
                {
                    x
                    .SaveChanges.Click()
                    .SaveChanges.IsVisible.WaitTo.BeFalse()
                    .Returns.Click()
                    .ReturnRows[index].AcknowledgeReceiptItems.IsEnabled.WaitTo.BeTrue()
                    .ReturnRows[index].AcknowledgeReceiptItems.Click()
                    //.ReturnRows[index].TableReturns.Rows.Count.WaitTo.Equal(products.Length)
                    .ReturnRows[index].CompleteReturn.IsEnabled.WaitTo.BeTrue()
                    .OrderTotal.StoreValue(out var totalAmount)
                    .ReturnRows[index].CompleteReturn.Click()
                    .CreateRefundFrame.DoWithin<CreateRefundFramePage>(x =>
                    {
                        x
                        .Do(x =>
                        {
                            if (!partial)
                            {
                                x.Amount.Set(totalAmount.Split(',').First().Replace(".",""));
                            }
                        })
                        .Confirm.Click()
                        .Confirm.IsVisible.WaitTo.BeFalse();
                        
                    });
                },
                true);
        }

        public static ManagerPage AssertPaymentOrderTransactions(this ManagerPage frame, string orderId, List<Dictionary<string, string>> list, out Uri paymentLink)
        {
            return frame
                .ExpandOrders()
                .Today.DoubleClick()
                .RightFrame.SwitchTo<OrdersFramePage>()
                .OrderTable.IsVisible.WaitTo.BeTrue()
                .OrderTable.Rows[x => x.Link.Content.Value.Contains(orderId)].Link.ClickAndGo()
                .Payments.Click()
                .TablePayment.Rows.Count.Should.Equal(list.Count)
                .Do(x =>
                {
                    foreach (var dictionary in list)
                    {
                        x.TablePayment.Rows[y => y.TransactionType == dictionary[PaymentColumns.TransactionType]].Should.BeVisible();
                        x.TablePayment.Rows[y => y.Status == dictionary[PaymentColumns.Status]].Should.BeVisible();
                    }
                })
                .Summary.Click()
                .PaymentLink.StoreUri(out paymentLink)
                .SwitchToRoot<ManagerPage>();
        }

        #endregion

        #region Payex Operations

        public static ManagerPage CreateCancellaton(this ManagerPage frame, string orderId)
        {
            return frame
                .CancelOrder(orderId);
        }

        public static ManagerPage CreateCapture(this ManagerPage frame, string orderId)
        {
            return frame
                .CompleteAndReleaseShipment(orderId)
                .AddShipmentToPickList(orderId)
                .CompletePickListShipment(orderId);
        }

        public static ManagerPage CreateReversal(this ManagerPage frame, string orderId, Product[] products, bool partial = true, int index = 0)
        {
            return frame
                .CreateReturn(orderId, products)
                .CompleteReturn(products, partial, index);
        }

        public static ManagerPage CompleteSale(this ManagerPage frame, string orderId)
        {
            return frame
                .ReleaseShipment(orderId)
                .AddShipmentToPickList(orderId)
                .CompletePickListShipment(orderId);
        }

        #endregion
    }
}
