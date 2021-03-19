using NUnit.Framework;
using Foundation.UiTests.Tests.Base;
using Foundation.UiTests.Tests.Helpers;
using System.Collections.Generic;
using SwedbankPay.Sdk;
using System.Threading.Tasks;
using System.Linq;
using SwedbankPay.Sdk.PaymentInstruments;

namespace Foundation.UiTests.Tests.PaymentTest.PaymentSaleTests
{
    [Category(TestCategory.Sale)]
    public class PaymentSaleTests : PaymentTests
    {
        public PaymentSaleTests(Browsers.Browser browser) : base(browser) { }

        [Test]
        [Category(TestCategory.Swish)]
        [TestCaseSource(nameof(TestData), new object[] { false, PaymentMethods.Swish })]
        public async Task Sale_With_SwishAsync(Product[] products, PayexInfo payexInfo)
        {
            var expected = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { PaymentColumns.TransactionType, TransactionType.Sale.ToString() }, { PaymentColumns.Status, PaymentStatus.Processed }}
            };


            // Arrange
            GoToThankYouPage(products, payexInfo);


            // Act
            GoToManagerPage()
                .AssertPaymentOrderTransactions(_orderId, expected, out var _paymentOrderLink);


            // Assert
            var order = await SwedbankPayClient.PaymentOrders.Get(_paymentOrderLink, SwedbankPay.Sdk.PaymentOrders.PaymentOrderExpand.All);

            // Global Order
            Assert.That(order.PaymentOrder.Amount.InLowestMonetaryUnit, Is.EqualTo(_totalAmount * 100));
            Assert.That(order.PaymentOrder.Currency.ToString(), Is.EqualTo("SEK"));
            Assert.That(order.PaymentOrder.State, Is.EqualTo(State.Ready));

            // Operations
            Assert.That(order.Operations[LinkRelation.CreateCancellation], Is.Null);
            Assert.That(order.Operations[LinkRelation.CreatePaymentOrderCapture], Is.Null);
            Assert.That(order.Operations[LinkRelation.CreatePaymentOrderReversal], Is.Not.Null);
            Assert.That(order.Operations[LinkRelation.PaidPaymentOrder], Is.Not.Null);

            // Transactions
            Assert.That(order.PaymentOrder.CurrentPayment.Payment.Transactions.TransactionList.Count, Is.EqualTo(expected.Count));
            Assert.That(order.PaymentOrder.CurrentPayment.Payment.Transactions.TransactionList.First(x => x.Type == TransactionType.Sale).State,
                        Is.EqualTo(State.Completed));

            // Order Items
            Assert.That(order.PaymentOrder.OrderItems.OrderItemList.Count, Is.EqualTo(products.Count() + 1));
            for (var i = 0; i < products.Count(); i++)
            {
                Assert.That(order.PaymentOrder.OrderItems.OrderItemList.ElementAt(i).Name, Is.EqualTo(products[i].Name));
                Assert.That(order.PaymentOrder.OrderItems.OrderItemList.ElementAt(i).UnitPrice.InLowestMonetaryUnit, Is.EqualTo(products[i].UnitPrice * 100));
                Assert.That(order.PaymentOrder.OrderItems.OrderItemList.ElementAt(i).Quantity, Is.EqualTo(products[i].Quantity));
                Assert.That(order.PaymentOrder.OrderItems.OrderItemList.ElementAt(i).Amount.InLowestMonetaryUnit, Is.EqualTo(products[i].UnitPrice * 100 * products[i].Quantity));
            }
        }
    }
}
