using NUnit.Framework;
using Foundation.UiTests.Tests.Base;
using Foundation.UiTests.Tests.Helpers;
using System.Collections.Generic;
using SwedbankPay.Sdk;
using System.Threading.Tasks;
using System.Linq;

namespace Foundation.UiTests.Tests.PaymentTest.PaymentCancellationTests
{
    [Category(TestCategory.Cancellation)]
    public class PaymentCancellationTests : PaymentTests
    {
        public PaymentCancellationTests(Browsers.Browser browser) : base(browser) { }

        [Test]
        [Category(TestCategory.Card)]
        [TestCaseSource(nameof(TestData), new object[] { false, PaymentMethods.Card })]
        public async Task Cancellation_With_CardAsync(Product[] products, PayexInfo payexInfo)
        {
            // "Void" is the terminology used in EPiServer to qualify a cancellation
            var expected = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { PaymentColumns.TransactionType, TransactionType.Authorization.ToString() }, { PaymentColumns.Status, PaymentStatus.Processed }},
                new Dictionary<string, string> { { PaymentColumns.TransactionType, "Void" },                                   { PaymentColumns.Status, PaymentStatus.Processed }}
            };

            // Arrange
            GoToThankYouPage(products, payexInfo);


            // Act
            GoToManagerPage()
                .CancelOrder(_orderId)
                .AssertPaymentOrderTransactions(_orderId, expected, out var _paymentOrderLink);


            // Assert
            var order = await SwedbankPayClient.PaymentOrder.Get(_paymentOrderLink, SwedbankPay.Sdk.PaymentOrders.PaymentOrderExpand.All);

            // Operations
            Assert.That(order.Operations[LinkRelation.CreatePaymentOrderCancel], Is.Null);
            Assert.That(order.Operations[LinkRelation.CreatePaymentOrderCapture], Is.Null);
            Assert.That(order.Operations[LinkRelation.CreatePaymentOrderReversal], Is.Null);
            Assert.That(order.Operations[LinkRelation.PaidPaymentOrder], Is.Not.Null);

            // Transactions
            Assert.That(order.PaymentOrderResponse.CurrentPayment.Payment.Transactions.TransactionList.Count, Is.EqualTo(expected.Count));
            Assert.That(order.PaymentOrderResponse.CurrentPayment.Payment.Transactions.TransactionList.First(x => x.Type == TransactionType.Authorization).State,
                        Is.EqualTo(State.Completed));
            Assert.That(order.PaymentOrderResponse.CurrentPayment.Payment.Transactions.TransactionList.First(x => x.Type == TransactionType.Cancellation).State,
                        Is.EqualTo(State.Completed));
        }

    }
}
