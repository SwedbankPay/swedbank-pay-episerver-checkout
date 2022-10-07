using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using Foundation.Commerce.Customer.Services;
using Foundation.Features.Checkout.Services;
using Foundation.Features.MyAccount.AddressBook;
using Foundation.Infrastructure.Services;
using SwedbankPay.Episerver.Checkout;
using SwedbankPay.Episerver.Checkout.Common;
using System.Web.Mvc;

namespace Foundation.Features.MyAccount.OrderConfirmation
{
    public class OrderConfirmationController : OrderConfirmationControllerBase<OrderConfirmationPage>
    {
        private readonly ICampaignService _campaignService;
        private readonly IOrderRepository _orderRepository;
        private readonly ISwedbankPayCheckoutService _swedbankPayCheckoutService;
        private readonly CheckoutService _checkoutService;

        public OrderConfirmationController(
            ICampaignService campaignService,
            ConfirmationService confirmationService,
            IAddressBookService addressBookService,
            IOrderRepository orderRepository,
            IOrderGroupCalculator orderGroupCalculator,
            UrlResolver urlResolver, ICustomerService customerService,
            ISwedbankPayCheckoutService swedbankPayCheckoutService,
            CheckoutService checkoutService) :
            base(confirmationService, addressBookService, orderGroupCalculator, urlResolver, customerService)
        {
            _campaignService = campaignService;
            _orderRepository = orderRepository;
            _swedbankPayCheckoutService = swedbankPayCheckoutService;
            _checkoutService = checkoutService;
        }
        public ActionResult Index(OrderConfirmationPage currentPage, string notificationMessage, int? orderNumber, string payeeReference)
        {
            IPurchaseOrder order = null;
            if (PageEditing.PageIsInEditMode)
            {
                order = _confirmationService.CreateFakePurchaseOrder();
            }
            else if (orderNumber.HasValue)
            {
                order = _confirmationService.GetOrder(orderNumber.Value);
            }

            if (order == null && orderNumber.HasValue)
            {
                var cart = _orderRepository.Load<ICart>(orderNumber.Value);
                if (cart != null)
                {
                    var swedbankPayOrderId = cart.Properties[Constants.SwedbankPayOrderIdField];
                    order = _checkoutService.GetOrCreatePurchaseOrder(orderNumber.Value, swedbankPayOrderId.ToString());
                }
                else
                {
                    order = _swedbankPayCheckoutService.GetByPayeeReference(payeeReference);
                }
            }

            if (order != null && order.CustomerId == _customerService.CurrentContactId)
            {
                var viewModel = CreateViewModel(currentPage, order);
                viewModel.NotificationMessage = notificationMessage;

                _campaignService.UpdateLastOrderDate();
                _campaignService.UpdatePoint(decimal.ToInt16(viewModel.SubTotal.Amount));

                return View(viewModel);
            }

            return Redirect(Url.ContentUrl(ContentReference.StartPage));
        }
    }
}