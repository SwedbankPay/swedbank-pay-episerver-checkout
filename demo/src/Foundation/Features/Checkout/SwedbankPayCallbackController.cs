using EPiServer.Commerce.Order;
using EPiServer.Logging;

using Foundation.Features.Checkout.Services;

using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;

using SwedbankPay.Episerver.Checkout;
using SwedbankPay.Episerver.Checkout.Callback;
using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.Common.Extensions;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.PaymentOrders;

using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;

using TransactionType = SwedbankPay.Sdk.TransactionType;

namespace Foundation.Features.Checkout
{
    [RoutePrefix("swedbankpay")]
    public class SwedbankPayCallbackController : ApiController
    {
        private ILogger _logger = LogManager.GetLogger(typeof(SwedbankPayCallbackController));

        private readonly CheckoutService _checkoutService;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly ISwedbankPayCheckoutService _swedbankPayCheckoutService;

        public SwedbankPayCallbackController(
            CheckoutService checkoutService,
            IOrderGroupFactory orderGroupFactory,
            IOrderRepository orderRepository,
            ISwedbankPayCheckoutService swedbankPayCheckoutService)
        {
            _checkoutService = checkoutService;
            _orderGroupFactory = orderGroupFactory;
            _orderRepository = orderRepository;
            _swedbankPayCheckoutService = swedbankPayCheckoutService;
        }

        [HttpPost]
        [Route("cart/{orderGroupId}/callback")]
        public IHttpActionResult PaymentCallback([FromBody] PaymentCallbackDto callback, int orderGroupId)
        {
            if (!string.IsNullOrWhiteSpace(callback?.PaymentOrder?.Id?.ToString()))
            {
                var purchaseOrder = _checkoutService.GetOrCreatePurchaseOrder(orderGroupId, callback.PaymentOrder.Id.OriginalString);
                if (purchaseOrder == null)
                {
                    return new StatusCodeResult(HttpStatusCode.NotFound, this);
                }

                var purchaseOrderContainsPaymentTransaction = purchaseOrder.Forms.SelectMany(x => x.Payments)
                    .Any(p => p.ProviderTransactionID == callback.Transaction.Number.ToString());

                if (!purchaseOrderContainsPaymentTransaction)
                {
                    var paymentOrder = _swedbankPayCheckoutService.GetPaymentOrder(purchaseOrder, PaymentOrderExpand.All);
                    var transaction = paymentOrder.PaymentOrderResponse.CurrentPayment.Payment.Transactions.TransactionList
                        .FirstOrDefault(x => x.Number == callback.Transaction.Number.ToString());
                    
                    var swedbankPayCheckoutPaymentMethodDto = PaymentManager.GetPaymentMethodBySystemName(Constants.SwedbankPayCheckoutSystemKeyword, paymentOrder.PaymentOrderResponse.Language.TwoLetterISOLanguageName);
                    var paymentMethod = swedbankPayCheckoutPaymentMethodDto?.PaymentMethod?.FirstOrDefault();
                    if (paymentMethod != null && transaction != null)
                    {
                        if (paymentOrder.PaymentOrderResponse.CurrentPayment.Payment.Instrument == PaymentInstrument.Invoice
                            && transaction.Type == TransactionType.Authorization)
                        {
                            //Already added a authorization transaction for Invoice when creating payment.
                            return Ok();
                        }

                        var payment = purchaseOrder.CreatePayment(_orderGroupFactory);
                        payment.PaymentType = PaymentType.Other;
                        payment.PaymentMethodId = paymentMethod.PaymentMethodId;
                        payment.PaymentMethodName = Constants.SwedbankPayCheckoutSystemKeyword;
                        payment.TransactionType = transaction.Type.ConvertToEpiTransactionType().ToString();
                        payment.ProviderTransactionID = transaction.Number;
                        payment.Amount = transaction.Amount.Value / (decimal)100;
                        payment.Status = PaymentStatus.Processed.ToString();
                        purchaseOrder.AddPayment(payment);
                        _orderRepository.Save(purchaseOrder);
                    }
                }

                return Ok();
            }

            return new StatusCodeResult(HttpStatusCode.Accepted, this);
        }
    }
}
