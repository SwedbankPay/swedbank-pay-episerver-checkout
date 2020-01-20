using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;

using SwedbankPay.Episerver.Checkout;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.PaymentOrders;
using SwedbankPay.Sdk.Payments;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
    using EPiServer.Reference.Commerce.Site.Features.Start.Pages;

    using Mediachase.Commerce.Markets;

    public class CheckoutController : PageController<CheckoutPage>
    {
        private readonly IAddressBookService _addressBookService;
        private readonly ICurrencyService _currencyService;
        private readonly ControllerExceptionHandler _controllerExceptionHandler;
        private readonly CheckoutViewModelFactory _checkoutViewModelFactory;
        private readonly OrderSummaryViewModelFactory _orderSummaryViewModelFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartService _cartService;
        private readonly IRecommendationService _recommendationService;
        private readonly OrderValidationService _orderValidationService;
        private readonly ISwedbankPayCheckoutService _swedbankPayCheckoutService;
        private readonly IContentLoader _contentLoader;
        private readonly IMarketService _marketService;
        private ICart _cart;
        private readonly CheckoutService _checkoutService;

        public CheckoutController(
            IAddressBookService addressBookService,
            ICurrencyService currencyService,
            ControllerExceptionHandler controllerExceptionHandler,
            IOrderRepository orderRepository,
            CheckoutViewModelFactory checkoutViewModelFactory,
            ICartService cartService,
            OrderSummaryViewModelFactory orderSummaryViewModelFactory,
            IRecommendationService recommendationService,
            CheckoutService checkoutService,
            OrderValidationService orderValidationService,
            ISwedbankPayCheckoutService swedbankPayCheckoutService,
            IContentLoader contentLoader,
            IMarketService marketService)
        {
            _addressBookService = addressBookService;
            _currencyService = currencyService;
            _controllerExceptionHandler = controllerExceptionHandler;
            _orderRepository = orderRepository;
            _checkoutViewModelFactory = checkoutViewModelFactory;
            _cartService = cartService;
            _orderSummaryViewModelFactory = orderSummaryViewModelFactory;
            _recommendationService = recommendationService;
            _checkoutService = checkoutService;
            _orderValidationService = orderValidationService;
            _swedbankPayCheckoutService = swedbankPayCheckoutService;
            _contentLoader = contentLoader;
            _marketService = marketService;
        }

        [HttpGet]
        [OutputCache(Duration = 0, NoStore = true)]
        public async Task<ActionResult> Index(CheckoutPage currentPage)
        {
            if (CartIsNullOrEmpty())
            {
                return View("EmptyCart");
            }

            var viewModel = CreateCheckoutViewModel(currentPage);

            Cart.Currency = _currencyService.GetCurrentCurrency();

            _checkoutService.UpdateShippingAddresses(Cart, viewModel);
            _checkoutService.UpdateShippingMethods(Cart, viewModel.Shipments);

            _cartService.ApplyDiscounts(Cart);
            _orderRepository.Save(Cart);

            await _recommendationService.TrackCheckoutAsync(HttpContext);

            _checkoutService.ProcessPaymentCancel(viewModel, TempData, ControllerContext);

            return View(viewModel.ViewName, viewModel);
        }

        [HttpGet]
        public ActionResult SingleShipment(CheckoutPage currentPage)
        {
            if (!CartIsNullOrEmpty())
            {
                _cartService.MergeShipments(Cart);
                _orderRepository.Save(Cart);
            }

            return RedirectToAction("Index", new { node = currentPage.ContentLink });
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult ChangeAddress(UpdateAddressViewModel addressViewModel)
        {
            ModelState.Clear();
            var viewModel = CreateCheckoutViewModel(addressViewModel.CurrentPage);
            _checkoutService.CheckoutAddressHandling.ChangeAddress(viewModel, addressViewModel);

            _checkoutService.UpdateShippingAddresses(Cart, viewModel);

            _orderRepository.Save(Cart);

            var addressViewName = addressViewModel.ShippingAddressIndex > -1 ? "SingleShippingAddress" : "BillingAddress";

            return PartialView(addressViewName, viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult OrderSummary()
        {
            var viewModel = _orderSummaryViewModelFactory.CreateOrderSummaryViewModel(Cart);
            return PartialView(viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult AddCouponCode(CheckoutPage currentPage, string couponCode)
        {
            if (_cartService.AddCouponCode(Cart, couponCode))
            {
                _orderRepository.Save(Cart);
            }
            var viewModel = CreateCheckoutViewModel(currentPage);
            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult RemoveCouponCode(CheckoutPage currentPage, string couponCode)
        {
            _cartService.RemoveCouponCode(Cart, couponCode);
            _orderRepository.Save(Cart);
            var viewModel = CreateCheckoutViewModel(currentPage);
            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult Purchase(CheckoutViewModel viewModel, IPaymentMethod paymentMethod)
        {
            if (CartIsNullOrEmpty())
            {
                return Redirect(Url.ContentUrl(ContentReference.StartPage));
            }

            viewModel.Payment = paymentMethod;

            viewModel.IsAuthenticated = User.Identity.IsAuthenticated;

            _checkoutService.CheckoutAddressHandling.UpdateUserAddresses(viewModel);

            if (!_checkoutService.ValidateOrder(ModelState, viewModel, _orderValidationService.ValidateOrder(Cart)))
            {
                return View(viewModel);
            }

            if (!paymentMethod.ValidateData())
            {
                return View(viewModel);
            }

            _checkoutService.UpdateShippingAddresses(Cart, viewModel);

            _checkoutService.CreateAndAddPaymentToCart(Cart, viewModel);

            var purchaseOrder = _checkoutService.PlaceOrder(Cart, ModelState, viewModel);
            if (!string.IsNullOrEmpty(viewModel.RedirectUrl))
            {
                return Redirect(viewModel.RedirectUrl);
            }

            if (purchaseOrder == null)
            {
                return View(viewModel);
            }

            var confirmationSentSuccessfully = _checkoutService.SendConfirmation(viewModel, purchaseOrder);

            return Redirect(_checkoutService.BuildRedirectionUrl(viewModel, purchaseOrder, confirmationSentSuccessfully));
        }

        public ActionResult OnPurchaseException(ExceptionContext filterContext)
        {
            var currentPage = filterContext.RequestContext.GetRoutedData<CheckoutPage>();
            if (currentPage == null)
            {
                return new EmptyResult();
            }

            var viewModel = CreateCheckoutViewModel(currentPage);
            ModelState.AddModelError("Purchase", filterContext.Exception.Message);

            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public JsonResult AddPaymentAndAddressInformation(CheckoutViewModel viewModel, IPaymentMethod paymentMethod)
        {
            viewModel.IsAuthenticated = User.Identity.IsAuthenticated;
            _checkoutService.CheckoutAddressHandling.UpdateUserAddresses(viewModel);
            _checkoutService.UpdateShippingAddresses(Cart, viewModel);

            // Clean up payments in cart on payment provider site.
            foreach (var form in Cart.Forms)
            {
                form.Payments.Clear();
            }

            var payment = paymentMethod.CreatePayment(Cart.GetTotal().Amount, Cart);
            payment.BillingAddress = _addressBookService.ConvertToAddress(viewModel.BillingAddress, Cart);

            Cart.AddPayment(payment);
            _orderRepository.Save(Cart);

            return new JsonResult
            {
                Data = true
            };
        }
        
        [HttpPost]
        public JsonResult CreateSwedbankPayPurchase(string consumerProfileRef)
        {
            var paymentOrderResponseObject = _swedbankPayCheckoutService.CreateOrUpdatePaymentOrder(Cart, "description", consumerProfileRef);
            return new JsonResult
            {
                Data = paymentOrderResponseObject
            };
        }


        [HttpGet]
        public ActionResult SwedbankPayCheckoutConfirmation(int orderGroupId)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupId);
            if (cart != null)
            {
                var order = _swedbankPayCheckoutService.GetPaymentOrder(cart, PaymentOrderExpand.All);
                
                var paymentResponse = order.PaymentOrderResponse.CurrentPayment;
                var transaction = paymentResponse.Payment.Transactions?.TransactionList?.FirstOrDefault(x =>
                    x.State.Equals(State.Completed) &&
                    x.Type.Equals( Intent.Authorization.ToString(), StringComparison.InvariantCultureIgnoreCase) ||
                    x.Type.Equals(  Intent.Sale.ToString(), StringComparison.InvariantCultureIgnoreCase));

                if (transaction != null)
                {
                    var purchaseOrder = _checkoutService.CreatePurchaseOrderForSwedbankPay(cart);
                    if (purchaseOrder == null)
                    {
                        ModelState.AddModelError("", "Error occurred while creating a purchase order");
                        return RedirectToAction("Index");
                    }

                    var checkoutViewModel = new CheckoutViewModel
                    {
                        CurrentPage = _contentLoader.Get<CheckoutPage>(_contentLoader.Get<StartPage>(ContentReference.StartPage).CheckoutPage),
                        BillingAddress = new Shared.Models.AddressModel { Email = purchaseOrder.GetFirstForm().Payments.FirstOrDefault()?.BillingAddress?.Email }
                    };

                    var confirmationSentSuccessfully = _checkoutService.SendConfirmation(checkoutViewModel, purchaseOrder);

                    return Redirect(_checkoutService.BuildRedirectionUrl(checkoutViewModel, purchaseOrder, confirmationSentSuccessfully));
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            return HttpNotFound();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            _controllerExceptionHandler.HandleRequestValidationException(filterContext, "purchase", OnPurchaseException);
        }

        private ViewResult View(CheckoutViewModel checkoutViewModel)
        {
            return View(checkoutViewModel.ViewName, CreateCheckoutViewModel(checkoutViewModel.CurrentPage, checkoutViewModel.Payment));
        }

        private CheckoutViewModel CreateCheckoutViewModel(CheckoutPage currentPage, IPaymentMethod paymentMethod = null)
        {
            return _checkoutViewModelFactory.CreateCheckoutViewModel(Cart, currentPage, paymentMethod);
        }

        private ICart Cart => _cart ?? (_cart = _cartService.LoadCart(_cartService.DefaultCartName));

        private bool CartIsNullOrEmpty()
        {
            return Cart == null || !Cart.GetAllLineItems().Any();
        }
    }
}
