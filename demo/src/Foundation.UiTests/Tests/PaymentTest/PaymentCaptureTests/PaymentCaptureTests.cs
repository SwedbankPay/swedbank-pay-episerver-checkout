using Foundation.UiTests.Tests.Base;
using Foundation.UiTests.Tests.Helpers;
using NUnit.Framework;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.PaymentInstruments;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foundation.UiTests.Tests.PaymentTest.PaymentCaptureTests
{
    [Category(TestCategory.Capture)]
    public class PaymentCaptureTests : PaymentTests
    {
        public PaymentCaptureTests(Browsers.Browser browser) : base(browser) { }

        [Test]
        [Category(TestCategory.Card)]
        [TestCaseSource(nameof(TestData), new object[] { false, PaymentMethods.Card })]
        public async Task Capture_With_CardAsync(Product[] products, PayexInfo payexInfo)
        {
            var expected = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { PaymentColumns.TransactionType, TransactionType.Authorization.ToString() }, { PaymentColumns.Status, PaymentStatus.Processed } },
                new Dictionary<string, string> { { PaymentColumns.TransactionType, TransactionType.Capture.ToString() },       { PaymentColumns.Status, PaymentStatus.Processed } },
            };

            // Arrange
            GoToThankYouPage(products, payexInfo);


            // Act
            GoToManagerPage()
                .CreateCapture(_orderId)
                .AssertPaymentOrderTransactions(_orderId, expected, out var _paymentOrderLink);


            // Assert
            var order = await SwedbankPayClient.PaymentOrders.Get(_paymentOrderLink, SwedbankPay.Sdk.PaymentOrders.PaymentOrderExpand.All);

            // Operations
            Assert.That(order.Operations[LinkRelation.CreatePaymentOrderCancel], Is.Null);
            Assert.That(order.Operations[LinkRelation.CreatePaymentOrderCapture], Is.Null);
            Assert.That(order.Operations[LinkRelation.PaidPaymentOrder], Is.Not.Null);
            Assert.That(order.Operations[LinkRelation.CreatePaymentOrderReversal], Is.Not.Null);

            // Transactions
            Assert.That(order.PaymentOrder.CurrentPayment.Payment.Transactions.TransactionList.Count, Is.EqualTo(expected.Count));
            Assert.That(order.PaymentOrder.CurrentPayment.Payment.Transactions.TransactionList.First(x => x.Type == TransactionType.Authorization).State,
                        Is.EqualTo(State.Completed));
            Assert.That(order.PaymentOrder.CurrentPayment.Payment.Transactions.TransactionList.First(x => x.Type == TransactionType.Capture).State,
                        Is.EqualTo(State.Completed));
        }
    }
}
