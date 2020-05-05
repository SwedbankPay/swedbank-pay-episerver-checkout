# SwedbankPay.Episerver.Checkout

![Swedbank Pay Episerver Checkout][opengraph-image]

## About

**UNSUPPORTED**: This extension is at an early stage of development and is not
supported as of yet by Swedbank Pay. It is provided as a convenience to speed
up your development, so please feel free to play around. However, if you need
support, please wait for a future, stable release.

The Official Swedbank Pay Checkout Extension for Episerver provides seamless
integration with Swedbank Pay Checkout, allowing your customers to pay swiftly and
securely with Credit Card, Invoice (Norway and Sweden), Vipps (Norway) and
Swish (Sweden). Credit Card Payments are available world-wide.

# The installation assumes that you have Quicksilver installed
[Quicksilver](https://github.com/episerver/Quicksilver)

# OBS ngrok for callbacks https://ngrok.com/

# Test cards 
[Test data](https://developer.swedbankpay.com/resources/test-data.html)

# How to get started
## Install following NuGet packages
For CMS:
```
Install-Package SwedbankPay.Episerver.Checkout 
```
For Commerce:

```
Install-Package SwedbankPay.Episerver.Checkout.CommerceManager
```

# Configure Commerce Manager
Login into Commerce Manager and open Administration -> Order System -> Payments. Then click New and in Overview tab fill:
- Name
- System Keyword
- Language
- Class Name: choose SwedbankPay.Episerver.Checkout.SwedbankPayCheckoutGateway
- Payment Class: choose MediaChase.Commerce.Orders.OtherPayment
- IsActive: Yes
![image-49ac7256-8095-4e4d-ad5f-05f46ad9c0d3](https://user-images.githubusercontent.com/1358504/75347529-37f4f200-58a1-11ea-8d20-44b5db29865e.png)

In Markets tab select market for which this payment will be available.

![image-5b25e408-14ee-4f6b-9338-1366ae23d569](https://user-images.githubusercontent.com/1358504/75347528-36c3c500-58a1-11ea-8374-9313f12efc1f.png)

**Press OK to save and then configure the newly added payment. Parameters won't be visible before it has been saved.** 

![merchantUrls](https://user-images.githubusercontent.com/1358504/78787158-32172400-79aa-11ea-8a60-1080bb50d7e3.png)

```
Token: {Your token}
ApiUrl: https://api.externalintegration.payex.com
MerchantId: {Your merchant id}

Host URLs: {Your domain 1}; {Your domain 2}; {Your domain 3}
CompleteUrl URL: {Your domain}/sv/checkout-sv/order-confirmation-sv/?orderNumber={orderGroupId}&payeeReference={payeeReference} 
Cancel URL: {Your domain}/payment-canceled?orderGroupId={orderGroupId} (Not to be filled out if Payment URL is used)
Callback URL: {Your domain}/swedbankpay/cart/{orderGroupId}/callback
Terms of Service URL: {Your domain}/payment-completed.pdf
Payment URL: {Your domain}/sv/checkout-sv/
```
{Your domain} in the URL's should be updated to your host. The Payment URL is the URI that Swedbank Pay will redirect back to when the view-operation needs to be loaded, to inspect and act on the current status of the payment. Only used in Seamless Views. If both cancelUrl and paymentUrl is sent, the paymentUrl will used.

# Setup
## Payment Method
You need to add a PaymentMethod to the site.
Following is an example of a PaymentMethod using SwedbankPay Checkout

```Csharp
using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.ServiceLocation;

using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;

using SwedbankPay.Episerver.Checkout;
using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.PaymentOrders;

using System;
using System.ComponentModel;
using System.Linq;
using SwedbankPay.Episerver.Checkout.Common.Extensions;
using PaymentType = Mediachase.Commerce.Orders.PaymentType;
using TransactionType = Mediachase.Commerce.Orders.TransactionType;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{

    [ServiceConfiguration(typeof(IPaymentMethod))]
    public class SwedbankPayCheckoutPaymentMethod : PaymentMethodBase, IDataErrorInfo
    {
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly LanguageService _languageService;
        private readonly ICartService _cartService;
        private readonly IMarketService _marketService;
        private readonly ISwedbankPayCheckoutService _swedbankPayCheckoutService;
        private bool _isInitalized;
        
        public SwedbankPayCheckoutPaymentMethod()
            : this(
                LocalizationService.Current,
                ServiceLocator.Current.GetInstance<IOrderGroupFactory>(),
                ServiceLocator.Current.GetInstance<LanguageService>(),
                ServiceLocator.Current.GetInstance<IPaymentManagerFacade>(),
                ServiceLocator.Current.GetInstance<ICartService>(),
                ServiceLocator.Current.GetInstance<IMarketService>(),
                ServiceLocator.Current.GetInstance<ISwedbankPayCheckoutService>())
        {
        }

        public SwedbankPayCheckoutPaymentMethod(
            LocalizationService localizationService,
            IOrderGroupFactory orderGroupFactory,
            LanguageService languageService,
            IPaymentManagerFacade paymentManager,
            ICartService cartService,
            IMarketService marketService,
            ISwedbankPayCheckoutService swedbankPayCheckoutService)
            : base(localizationService, orderGroupFactory, languageService, paymentManager)
        {
            _orderGroupFactory = orderGroupFactory;
            _languageService = languageService;
            _cartService = cartService;
            _marketService = marketService;
            _swedbankPayCheckoutService = swedbankPayCheckoutService;
        }

        public void InitializeValues()
        {
            InitializeValues(_cartService.DefaultCartName);
        }

        public void InitializeValues(string cartName)
        {
            if (_isInitalized)
            {
                return;
            }
            
            var cart = _cartService.LoadCart(cartName);
            var market = _marketService.GetMarket(cart.MarketId);

            var currentLanguage = _languageService.GetCurrentLanguage();
            Culture = currentLanguage.TextInfo.CultureName;
            CheckoutConfiguration = _swedbankPayCheckoutService.LoadCheckoutConfiguration(market, currentLanguage.TwoLetterISOLanguageName);

            var orderId = cart.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId) || CheckoutConfiguration.UseAnonymousCheckout)
            {
                GetCheckoutJavascriptSource(cart, $"description cart {cart.OrderLink.OrderGroupId}");
            }
            else
            {
                GetCheckInJavascriptSource(cart);
            }

            _isInitalized = true;
        }

        private void GetCheckoutJavascriptSource(ICart cart, string description)
        {
            var orderData = _swedbankPayCheckoutService.CreateOrUpdatePaymentOrder(cart, description);
            JavascriptSource = orderData.Operations.View?.Href;
            UseCheckoutSource = true;
        }

        private void GetCheckInJavascriptSource(ICart cart)
        {
            string email = "PayexTester@payex.com";
            string phone = "+46739000001";
            string ssn = "199710202392";

            var orderData = _swedbankPayCheckoutService.InitiateConsumerSession(_languageService.GetCurrentLanguage(), email, phone, ssn);
            JavascriptSource = orderData.Operations.ViewConsumerIdentification?.Href;
        }

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var paymentOrder = _swedbankPayCheckoutService.GetPaymentOrder(orderGroup, PaymentOrderExpand.All);
            var currentPayment = paymentOrder.PaymentOrderResponse.CurrentPayment.Payment;
            var transaction = currentPayment?.Transactions?.TransactionList?.FirstOrDefault();
            var transactionType = transaction?.Type.ConvertToEpiTransactionType() ?? TransactionType.Authorization;

            var payment = orderGroup.CreatePayment(_orderGroupFactory);
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = Constants.SwedbankPayCheckoutSystemKeyword;
            payment.ProviderTransactionID = transaction?.Number;
            payment.Amount = amount;
            var isSwishPayment = currentPayment?.Instrument.Equals(PaymentInstrument.Swish) ?? false;
            var isInvoicePayment = currentPayment?.Instrument.Equals(PaymentInstrument.Invoice) ?? false;
            payment.Status = isSwishPayment || isInvoicePayment ? PaymentStatus.Processed.ToString() : PaymentStatus.Pending.ToString();
            
            payment.TransactionType = isInvoicePayment ? TransactionType.Other.ToString() : transactionType.ToString();

            
            return payment;
        }

        public override bool ValidateData()
        {
            return true;
        }

        public override string SystemKeyword => Constants.SwedbankPayCheckoutSystemKeyword;

        public string this[string columnName] => string.Empty;

        public string Error { get; }
        public CheckoutConfiguration CheckoutConfiguration { get; set; }
        public string Culture { get; set; }
        public Uri JavascriptSource { get; set; }
        public bool UseCheckoutSource { get; set; }
    }
}



```

Add following method for creating Purchase Order in e.g. Features/Checkout/Services/CheckoutService.cs
To be able to use this code you need to constructor inject ISwedbankPayCheckoutService.

```Csharp
          public IPurchaseOrder GetOrCreatePurchaseOrder(int orderGroupId, string swedbankPayOrderId)
        {
            // Check if the order has been created already
            var purchaseOrder = _swedbankPayCheckoutService.GetPurchaseOrderBySwedbankPayOrderId(swedbankPayOrderId);
            if (purchaseOrder != null)
            {
                return purchaseOrder;
            }

            // Check if we still have a cart and can create an order
            var cart = _orderRepository.Load<ICart>(orderGroupId);
            var cartSwedbankPayId = cart?.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
            if (cartSwedbankPayId == null || !cartSwedbankPayId.Equals(swedbankPayOrderId))
            {
                return null;
            }

            var order = _swedbankPayCheckoutService.GetPaymentOrder(cart, PaymentOrderExpand.All);

            var paymentResponse = order.PaymentOrderResponse.CurrentPayment;
            var transaction = paymentResponse.Payment.Transactions?.TransactionList?.FirstOrDefault(x =>
                x.State.Equals(State.Completed) &&
                x.Type.Equals(SwedbankPay.Sdk.TransactionType.Authorization) ||
                x.Type.Equals(SwedbankPay.Sdk.TransactionType.Sale));

            if (transaction != null)
            {
                var finalPurchaseOrderCheck = _swedbankPayCheckoutService.GetPurchaseOrderBySwedbankPayOrderId(swedbankPayOrderId);
                purchaseOrder = finalPurchaseOrderCheck ?? CreatePurchaseOrderForSwedbankPay(cart);
                
                return purchaseOrder;
            }

            // Won't create order, SwedbankPay checkout not complete
            return null;
        }


        private IPurchaseOrder CreatePurchaseOrderForSwedbankPay(ICart cart)
        {
            cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator);
            var totalProcessedAmount = cart.GetFirstForm().Payments.Where(x => x.Status.Equals(PaymentStatus.Processed.ToString())).Sum(x => x.Amount);
            if (totalProcessedAmount != cart.GetTotal(_orderGroupCalculator).Amount)
            {
                throw new InvalidOperationException("Wrong amount");
            }
            
            var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
            var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            
            _orderRepository.Delete(cart.OrderLink);
            var validationIssues = _cartService.RequestInventory(cart);

            if (purchaseOrder == null || validationIssues != null && validationIssues.Any())
            {
                _swedbankPayCheckoutService.CancelOrder(cart);
                return null;
            }
            else
            {
                _swedbankPayCheckoutService.Complete(purchaseOrder);
            

                _orderRepository.Save(purchaseOrder);
                return purchaseOrder;
            }
        }
```

To initialize SwedbankPay checkout when loading the GUI, update CreatePaymentMethodSelectionViewModel methods in Features/Payment/ViewModelFactories/PaymentMethodViewModelFactory.cs
```Csharp
  public PaymentMethodSelectionViewModel CreatePaymentMethodSelectionViewModel(Guid paymentMethodId)
        {
            var viewModel = CreatePaymentMethodSelectionViewModel();
            viewModel.SelectedPaymentMethod = viewModel.PaymentMethods.Single(x => x.PaymentMethod.PaymentMethodId == paymentMethodId);

            if (viewModel.SelectedPaymentMethod?.PaymentMethod.SystemKeyword == Constants.SwedbankPayCheckoutSystemKeyword)
            {
                var swedbankPayCheckoutPaymentMethod = viewModel.SelectedPaymentMethod.PaymentMethod as SwedbankPayCheckoutPaymentMethod;
                swedbankPayCheckoutPaymentMethod?.InitializeValues();
            }

            return viewModel;
        }

        public PaymentMethodSelectionViewModel CreatePaymentMethodSelectionViewModel(IPaymentMethod paymentMethod)
        {
            var viewModel = CreatePaymentMethodSelectionViewModel();
            if (paymentMethod != null)
            {
                viewModel.SelectedPaymentMethod = viewModel.PaymentMethods.Single(x => x.PaymentMethod.PaymentMethodId == paymentMethod.PaymentMethodId);
                viewModel.SelectedPaymentMethod.PaymentMethod = paymentMethod;
            }

            if (viewModel.SelectedPaymentMethod?.PaymentMethod.SystemKeyword == Constants.SwedbankPayCheckoutSystemKeyword)
            {
                var swedbankPayCheckoutPaymentMethod = viewModel.SelectedPaymentMethod.PaymentMethod as SwedbankPayCheckoutPaymentMethod;
                swedbankPayCheckoutPaymentMethod?.InitializeValues();
            }
            return viewModel;
        }
```

## Endpoints

Add a controller for callbacks, e.g Features/Payment/Controllers/SwedbankPayCallbackController.cs
```Csharp
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;

using Mediachase.Commerce.Orders;

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

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Controllers
{
    [RoutePrefix("swedbankpay")]
    public class SwedbankPayCallbackController : ApiController
    {
        private ILogger _logger = LogManager.GetLogger(typeof(SwedbankPayCallbackController));

        private readonly CheckoutService _checkoutService;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentManagerFacade _paymentManager;
        private readonly ISwedbankPayCheckoutService _swedbankPayCheckoutService;

        public SwedbankPayCallbackController(
            CheckoutService checkoutService,
            IOrderGroupFactory orderGroupFactory,
            IOrderRepository orderRepository,
            IPaymentManagerFacade paymentManager,
            ISwedbankPayCheckoutService swedbankPayCheckoutService)
        {
            _checkoutService = checkoutService;
            _orderGroupFactory = orderGroupFactory;
            _orderRepository = orderRepository;
            _paymentManager = paymentManager;
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

                    var swedbankPayCheckoutPaymentMethodDto = _paymentManager.GetPaymentMethodBySystemName(Constants.SwedbankPayCheckoutSystemKeyword, paymentOrder.PaymentOrderResponse.Language.TwoLetterISOLanguageName);
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
```


Add following methods to Features/Checkout/Controllers/CheckoutController.cs

Constructor inject following Interfaces to be able to use following code

```Csharp
ISwedbankPayCheckoutService swedbankPayCheckoutService,
IContentLoader contentLoader,
IAddressBookService addressBookService
```
```Csharp
        [HttpPost]
        [AllowDBWrite]
        public JsonResult AddPaymentAndAddressInformation(CheckoutViewModel viewModel, IPaymentMethod paymentMethod, string paymentId)
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
            Cart.Properties[Constants.SwedbankPayPaymentIdField] = paymentId;
            _orderRepository.Save(Cart);

            return new JsonResult
            {
                Data = true
            };
        }

        [HttpPost]
        public string GetViewPaymentOrderHref(string consumerProfileRef)
        {
            var paymentOrderResponseObject = _swedbankPayCheckoutService.CreateOrUpdatePaymentOrder(Cart, "description", consumerProfileRef);
            return paymentOrderResponseObject.Operations.View.Href.OriginalString;
        }

```



Update OrderConfirmationController to load purchase order payed by swedbank pay
```Csharp
    public class OrderConfirmationController : OrderConfirmationControllerBase<OrderConfirmationPage>
    {
        private readonly CheckoutService _checkoutService;
        private readonly ICartService _cartService;
        private readonly IOrderRepository _orderRepository;
        private readonly IRecommendationService _recommendationService;
        private readonly ISwedbankPayCheckoutService _swedbankPayCheckoutService;

        public OrderConfirmationController(
            ConfirmationService confirmationService,
            AddressBookService addressBookService,
            CustomerContextFacade customerContextFacade,
            CheckoutService checkoutService,
            ICartService cartService,
            IOrderGroupCalculator orderGroupCalculator,
            IOrderRepository orderRepository,
            IMarketService marketService,
            IRecommendationService recommendationService,
            ISwedbankPayCheckoutService swedbankPayCheckoutService)
            : base(confirmationService, addressBookService, customerContextFacade, orderGroupCalculator, marketService)
        {
            _checkoutService = checkoutService;
            _cartService = cartService;
            _orderRepository = orderRepository;
            _recommendationService = recommendationService;
            _swedbankPayCheckoutService = swedbankPayCheckoutService;
        }

        [HttpGet]
        public async Task<ActionResult> Index(OrderConfirmationPage currentPage, string notificationMessage, int? orderNumber, string payeeReference)
        {
            IPurchaseOrder order = null;
            if (PageEditing.PageIsInEditMode)
            {
                order = ConfirmationService.CreateFakePurchaseOrder();
            }
            else if (orderNumber.HasValue)
            {
                order = ConfirmationService.GetOrder(orderNumber.Value);

                if (order != null)
                {
                    await _recommendationService.TrackOrderAsync(HttpContext, order);
                }
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

            if (order != null && order.CustomerId == CustomerContext.CurrentContactId)
            {
                var viewModel = CreateViewModel(currentPage, order);
                viewModel.NotificationMessage = notificationMessage;

                return View(viewModel);
            }


            return Redirect(Url.ContentUrl(ContentReference.StartPage));
        }
    }

```

# Frontend

In Views\Checkout\SingleShipmentCheckout.cshtml on .jsCheckoutForm add data attribute **data-addpaymentinfourl**
```html
 <form class="jsCheckoutForm" action="@Url.Action("Purchase", "Checkout")" method="post" novalidate data-addpaymentinfourl="@Url.Action("AddPaymentAndAddressInformation", null, null)" data-updateurl="@Url.Action("Update", null, null)">
```

Do the same for Views\Checkout\MultiShipmentCheckout.cshtml

```Csharp
    @using (Html.BeginForm("Purchase", "Checkout", FormMethod.Post, new { @class = "jsCheckoutForm", @data_UpdateUrl = Url.Action("Update", null, null), @data_addpaymentinfourl = Url.Action("AddPaymentAndAddressInformation", null, null)}))
```


Add view Views\Payment\\_SwedbankPayCheckout.cshtml
```Html
@model EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods.SwedbankPayCheckoutPaymentMethod
@{
	var containerId = Guid.NewGuid();
}
@Html.HiddenFor(x => x.SystemKeyword)

<h3>SwedbankPay</h3>

<div id="swedbankpay-checkout">

	@if (Model.CheckoutConfiguration.UseAnonymousCheckout || Model.UseCheckoutSource)
	{
		<div id="paymentMenuFrame">
			<div id="swedbankpay-paymentmenu-@containerId">

			</div>
		</div>
	}
	else
	{
		<div>
			<div id="swedbankpay-consumer-@containerId">

			</div>
		</div>

		<div id="paymentMenuFrame" hidden>
			<div id="swedbankpay-paymentmenu-@containerId">

			</div>
		</div>
	}
</div>



<script type="text/javascript">
    var loadScriptAsync = function (uri) {
        return new Promise(function (resolve, reject) {
            var tag = document.createElement('script');
            tag.src = uri;
            tag.async = true;
            tag.onload = function () {
                resolve();
            };
            var firstScriptTag = document.getElementsByTagName('script')[0];
            firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
        });
    }
    var scriptLoaded = loadScriptAsync('@(Model.JavascriptSource)');

    var style = {
        body: {
            //backgroundColor: "#555",
            //color: "#bbb"
        },
        button: {
			backgroundColor: "#337ab7",
			color: "#fff"
        },
        secondaryButton: {
            backgroundColor: "#555",
            border: "solid 1px #bbb"
        },
        formGroup: {
            color: "#bbb",
            backgroundColor: "#555"
        },
        label: {
            color: "#bbb"
        }
    };

    var config1 = {
        container: 'swedbankpay-paymentmenu-@containerId',
        culture: '@Culture',
        style: style,
        onPaymentCreated: onCreatedPaymentHandler
    };

    function onCreatedPaymentHandler(paymentCreatedEvent) {
        console.log(paymentCreatedEvent);
        var form = $('.jsCheckoutForm');
		var data = form.serializeArray();
		data.push({name: 'paymentId', value: paymentCreatedEvent.id})
        $.ajax({
            async: false,
            type: "POST",
            cache: false,
            url: $(form).data('addpaymentinfourl'),
            data: data,
            success: function(result) {
                console.log('payment created');
            }
        });
        console.log('address saved');
    }
</script>


@if (Model.CheckoutConfiguration.UseAnonymousCheckout || Model.UseCheckoutSource)
{
	<script type="text/javascript">
		scriptLoaded.then(function () {
			payex.hostedView.paymentMenu(config1).open();
		});

	</script>
}
else
{
	<script type="text/javascript">

        var paymentMenuConfig = {
            container: "swedbankpay-consumer-@containerId",
            culture: '@Culture',
            style: style,
            onConsumerIdentified: onIdentifiedConsumerHandler,
			onShippingDetailsAvailable: onShippingDetailsAvailableHandler,
			onBillingDetailsAvailable: OnBillingDetailsAvailableHandler
        };

        function OnBillingDetailsAvailableHandler(data) {
	        console.log(data);
	        var request = new XMLHttpRequest();

	        request.addEventListener('load', function() {
		        var response = JSON.parse(this.responseText);
		        console.log(response);
		        var billingAddress = response.billingAddress;
		        $('#BillingAddress_Email').val(response.email);
		        $('#BillingAddress_FirstName').val(billingAddress.addressee);
		        $('#BillingAddress_LastName').val(billingAddress.addressee);
		        $('#BillingAddress_Line1').val(billingAddress.streetAddress);
		        $('#BillingAddress_PostalCode').val(billingAddress.zipCode);
		        $('#BillingAddress_City').val(billingAddress.city);
		        $('#BillingAddress_CountryCode').val(billingAddress.CountryCode.ThreeLetterISORegionName);

	        });
	        request.open('POST', '@Url.Action("GetSwedbankPayBillingDetails", "SwedbankPayCheckout", null)', true);
	        request.setRequestHeader('Content-Type', 'application/json; charset=utf-8');
	        request.send(JSON.stringify(data));
        }

        function onShippingDetailsAvailableHandler(data) {
            console.log(data);
            var request = new XMLHttpRequest();

            request.addEventListener('load', function() {
                var response = JSON.parse(this.responseText);
                console.log(response);
                var shippingAddress = response.shippingAddress;
                $('#BillingAddress_Email').val(response.Email);
                $('#BillingAddress_FirstName').val(shippingAddress.addressee);
                $('#BillingAddress_LastName').val(shippingAddress.addressee);
                $('#BillingAddress_Line1').val(shippingAddress.streetAddress);
                $('#BillingAddress_PostalCode').val(shippingAddress.zipCode);
                $('#BillingAddress_City').val(shippingAddress.city);
                $('#BillingAddress_CountryCode').val(shippingAddress.CountryCode.ThreeLetterISORegionName);

            });
            request.open('POST', '@Url.Action("GetSwedbankPayShippingDetails", "SwedbankPayCheckout", null)', true);
            request.setRequestHeader('Content-Type', 'application/json; charset=utf-8');
            request.send(JSON.stringify(data));
        }

        function onIdentifiedConsumerHandler(data) {
            var paymentMenuFrame = document.getElementById("paymentMenuFrame");
            paymentMenuFrame.removeAttribute("hidden");

            var request = new XMLHttpRequest();
            request.addEventListener('load', function () {
				var script = document.createElement('script');
                // This assumses the operations from the response of the POST of the
                // payment order is returned verbatim from the server to the Ajax:
                script.setAttribute('src', this.responseText);
                script.onload = function() {
                    // When the 'view-paymentorder' script is loaded, we can initialize the payment
                    // menu inside our 'checkin' container.
                    payex.hostedView.paymentMenu(config1).open();
                };
				var head = document.getElementsByTagName('head')[0];
				head.appendChild(script);
            });
            request.open('POST', '@Url.Action("GetViewPaymentOrderHref", "Checkout", null)', true);
            request.setRequestHeader('Content-Type', 'application/json; charset=utf-8');
            request.send(JSON.stringify(data));
        }


        scriptLoaded.then(function () {
	        payex.hostedView.consumer(paymentMenuConfig).open();
        });

	</script>
}
```

Add view Views\Shared\_SwedbankPayCheckoutConfirmation.cshtml
```Html
<div class="quicksilver-well">
	<h4>@Html.Translate("/OrderConfirmation/PaymentDetails")</h4>
	<p>
		SwedbankPay
	</p>
</div>
```


**Note: If QuickSilvers test project is used, you need to add a parameter in CheckoutServiceTests where _subject is created since an extra parameter is added to CheckoutService.cs.
You could go ahead and add just null.**


**Note: If you want to update the checkout when changing shipment, you could add following line at the init function in Scripts/js/Checkout.js**
```Javascript
.on('change', '.jsChangeShipment', Checkout.refreshView) 
```

[opengraph-image]: https://repository-images.githubusercontent.com/171851967/01256480-53e7-11ea-9c0f-da3e3b5811b3
