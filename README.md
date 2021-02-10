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

---

## Table of Contents

- [Pre-installation](#pre-installation)
- [Test cards](#test-cards)
- [How to get started](#how-to-get-started)
- [Configure Commerce Manager](#configure-commerce-manager)
- [Setup](#setup)

---

## Pre-installation

The installation assumes that you have Foundation installed, and are using ngrok for callbacks.

[Foundation](https://github.com/episerver/foundation)
[ngrok](https://ngrok.com/)

---

## Test cards 

[Test data](https://developer.swedbankpay.com/resources/test-data.html)

---

## How to get started
### Install following NuGet packages
For project Foundation:
```
Install-Package SwedbankPay.Episerver.Checkout 
```
For project Foundation.Commerce:

```
Install-Package SwedbankPay.Episerver.Checkout.CommerceManager
```

---

## Configure Commerce Manager
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

Host URLs: https://{Your domain 1}; https://{Your domain 2}; https://{Your domain 3}
CompleteUrl URL: https://{Your domain}/en/order-confirmation/?orderNumber={orderGroupId}&payeeReference={payeeReference} 
Cancel URL: https://{Your domain}/payment-canceled?orderGroupId={orderGroupId} (Not to be filled out if Payment URL is used)
Callback URL: https://{Your domain}/swedbankpay/cart/{orderGroupId}/callback
Terms of Service URL: https://{Your domain}/payment-completed.pdf
Payment URL: https://{Your domain}/sv/checkout/
```
{Your domain} in the URL's should be updated to your host. The Payment URL is the URI that Swedbank Pay will redirect back to when the view-operation needs to be loaded, to inspect and act on the current status of the payment. Only used in Seamless Views. If both cancelUrl and paymentUrl is sent, the paymentUrl will used.

---


## Setup
### Payment Method
You need to add a PaymentMethod to the site.
Following is an example of a PaymentMethod using SwedbankPay Checkout

```Csharp
using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;

using Foundation.Commerce.Markets;
using Foundation.Features.Checkout.Services;

using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;

using SwedbankPay.Episerver.Checkout;
using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.Common.Extensions;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.PaymentOrders;

using System;
using System.ComponentModel;
using System.Linq;

using TransactionType = Mediachase.Commerce.Orders.TransactionType;

namespace Foundation.Features.Checkout.Payments
{
	[ServiceConfiguration(typeof(IPaymentMethod))]
    public class SwedbankPayCheckoutPaymentOption : PaymentOptionBase, IDataErrorInfo
    {
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartService _cartService;
        private readonly LanguageService _languageService;
        private readonly IMarketService _marketService;
        private readonly ISwedbankPayCheckoutService _swedbankPayCheckoutService;
        private bool _isInitalized;

        public SwedbankPayCheckoutPaymentOption() : this(
            LocalizationService.Current,
            ServiceLocator.Current.GetInstance<IOrderGroupFactory>(),
            ServiceLocator.Current.GetInstance<IOrderRepository>(),
            ServiceLocator.Current.GetInstance<ICartService>(),
            ServiceLocator.Current.GetInstance<ICurrentMarket>(),
            ServiceLocator.Current.GetInstance<LanguageService>(),
            ServiceLocator.Current.GetInstance<IMarketService>(),
            ServiceLocator.Current.GetInstance<IPaymentService>(),
            ServiceLocator.Current.GetInstance<ISwedbankPayCheckoutService>())
        {
        }

        public SwedbankPayCheckoutPaymentOption(
            LocalizationService localizationService,
            IOrderGroupFactory orderGroupFactory,
            IOrderRepository orderRepository,
            ICartService cartService,
            ICurrentMarket currentMarket,
            LanguageService languageService,
            IMarketService marketService,
            IPaymentService paymentService,
            ISwedbankPayCheckoutService swedbankPayCheckoutService) : base(localizationService, orderGroupFactory, currentMarket, languageService, paymentService)
        {
            _orderGroupFactory = orderGroupFactory;
            _orderRepository = orderRepository;
            _cartService = cartService;
            _languageService = languageService;
            _marketService = marketService;
            _swedbankPayCheckoutService = swedbankPayCheckoutService;
        }

        public void InitializeValues()
        {
            var cart = _cartService.LoadCart(_cartService.DefaultCartName, true)?.Cart;
            InitializeValues(cart);
        }

        public void InitializeValues(ICart cart)
        {
            if (_isInitalized || cart == null)
            {
                return;
            }

            var market = _marketService.GetMarket(cart.MarketId);

            var currentLanguage = _languageService.GetCurrentLanguage();
            Culture = currentLanguage.TextInfo.CultureName;
            CheckoutConfiguration = _swedbankPayCheckoutService.LoadCheckoutConfiguration(market, currentLanguage.TwoLetterISOLanguageName);

            var orderId = cart.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
			if (!CheckoutConfiguration.UseAnonymousCheckout)
			{
                GetCheckInJavascriptSource(cart);
            }
            if (!string.IsNullOrWhiteSpace(orderId) || CheckoutConfiguration.UseAnonymousCheckout)
            {
                GetCheckoutJavascriptSource(cart, $"description cart {cart.OrderLink.OrderGroupId}");
            }

            _isInitalized = true;
        }

        private void GetCheckoutJavascriptSource(ICart cart, string description)
        {
            var consumerProfileRef = cart.Properties[Constants.ConsumerProfileRef]?.ToString();
            var orderData = _swedbankPayCheckoutService.CreateOrUpdatePaymentOrder(cart, description, consumerProfileRef);
            JavascriptSource = orderData.Operations.View?.Href;
            UseCheckoutSource = true;
        }

        private void GetCheckInJavascriptSource(ICart cart)
        {
            var consumerUiScriptSource = cart.Properties[Constants.ConsumerUiScriptSource]?.ToString();
			if (!string.IsNullOrWhiteSpace(consumerUiScriptSource))
            {
                ConsumerUiScriptSource = new Uri(consumerUiScriptSource, UriKind.RelativeOrAbsolute);
                return;
            }

            var orderData = _swedbankPayCheckoutService.InitiateConsumerSession(_languageService.GetCurrentLanguage());
            cart.Properties[Constants.ConsumerUiScriptSource] = orderData.Operations.ViewConsumerIdentification?.Href;
            _orderRepository.Save(cart);
            ConsumerUiScriptSource = orderData.Operations.ViewConsumerIdentification?.Href;
        }

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var paymentOrder = _swedbankPayCheckoutService.GetPaymentOrder(orderGroup, PaymentOrderExpand.All);
            var currentPayment = paymentOrder.PaymentOrder.CurrentPayment.Payment;
            var transaction = currentPayment?.Transactions?.TransactionList?.FirstOrDefault();
            var transactionType = transaction?.Type.ConvertToEpiTransactionType() ?? TransactionType.Authorization;

            var payment = orderGroup.CreatePayment(_orderGroupFactory);
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = Constants.SwedbankPayCheckoutSystemKeyword;
            payment.ProviderTransactionID = transaction?.Number.ToString();
            payment.Amount = amount;
            var isSwishPayment = currentPayment?.Instrument.Equals(PaymentInstrument.Swish) ?? false;
            payment.Status = isSwishPayment ? PaymentStatus.Processed.ToString() : PaymentStatus.Pending.ToString();

            payment.TransactionType = transactionType.ToString();

            return payment;
        }

        public override bool ValidateData() => true;
        public override string SystemKeyword => Constants.SwedbankPayCheckoutSystemKeyword;
        public string this[string columnName] => string.Empty;
        public string Error { get; }
        public CheckoutConfiguration CheckoutConfiguration { get; set; }
        public string Culture { get; set; }
        public Uri ConsumerUiScriptSource { get; set; }
        public Uri JavascriptSource { get; set; }
        public bool UseCheckoutSource { get; set; }
    }
}
```

Add following method for creating Purchase Order in e.g. Foundation/Features/Checkout/Services/CheckoutService.cs
To be able to use this code you need to constructor inject ISwedbankPayCheckoutService and ICartService.

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

    var paymentOrder = _swedbankPayCheckoutService.GetPaymentOrder(cart, PaymentOrderExpand.All);
    var currentPayment = paymentOrder.PaymentOrder.CurrentPayment.Payment;

    if (currentPayment.State.Equals(State.Failed) || currentPayment.Amount == 0)
    {
        cart.AddNote($"Payment {currentPayment.Number} failed", $"Payment {currentPayment.Number} failed");
        return null;
    }

    var transaction = currentPayment.Transactions?.TransactionList?.FirstOrDefault(x =>
        x.State.Equals(State.Completed) &&
        (x.Type.Equals(SwedbankPay.Sdk.PaymentInstruments.TransactionType.Authorization) ||
        x.Type.Equals(SwedbankPay.Sdk.PaymentInstruments.TransactionType.Sale)));

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

To initialize SwedbankPay checkout when loading the GUI, update `UpdatePayments` method in Foundation/Features/Checkout/ViewModels/CheckoutViewModelFactory.cs  
Add following snippet
```Csharp
if (selectedPaymentMethod.SystemKeyword == Constants.SwedbankPayCheckoutSystemKeyword)
{
    var swedbankPayCheckoutPaymentOption = selectedPaymentMethod.PaymentOption as SwedbankPayCheckoutPaymentOption;
    swedbankPayCheckoutPaymentOption?.InitializeValues();
}
```

### Endpoints

Add a controller for callbacks, e.g Foundation/Features/Checkout/SwedbankPayCallbackController.cs
```Csharp
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
                    var transaction = paymentOrder.PaymentOrder.CurrentPayment.Payment.Transactions.TransactionList
                        .FirstOrDefault(x => x.Number == callback.Transaction.Number);
                    
                    var swedbankPayCheckoutPaymentMethodDto = PaymentManager.GetPaymentMethodBySystemName(Constants.SwedbankPayCheckoutSystemKeyword, paymentOrder.PaymentOrder.Language.ToString());
                    var paymentMethod = swedbankPayCheckoutPaymentMethodDto?.PaymentMethod?.FirstOrDefault();
                    if (paymentMethod != null && transaction != null)
                    {
                        if (paymentOrder.PaymentOrder.CurrentPayment.Payment.Instrument == PaymentInstrument.Invoice
                            && transaction.Type == SwedbankPay.Sdk.PaymentInstruments.TransactionType.Authorization)
                        {
                            //Already added a authorization transaction for Invoice when creating payment.
                            return Ok();
                        }

                        var payment = purchaseOrder.CreatePayment(_orderGroupFactory);
                        payment.PaymentType = PaymentType.Other;
                        payment.PaymentMethodId = paymentMethod.PaymentMethodId;
                        payment.PaymentMethodName = Constants.SwedbankPayCheckoutSystemKeyword;
                        payment.TransactionType = transaction.Type.ConvertToEpiTransactionType().ToString();
                        payment.ProviderTransactionID = transaction.Number.ToString();
                        payment.Amount = transaction.Amount / (decimal)100;
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


Add following methods to Foundation/Features/Checkout/CheckoutController.cs

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
    // shipping information
    if (!viewModel.UseShippingingAddressForBilling)
    {
        for (var i = 0; i < viewModel.Shipments.Count; i++)
        {
            if (viewModel.Shipments[i].ShippingAddressType == 0)
            {
                var addressName = viewModel.Shipments[i].Address.FirstName + " " + viewModel.Shipments[i].Address.LastName;
                viewModel.Shipments[i].Address.AddressId = null;
                viewModel.Shipments[i].Address.Name = addressName + " " + DateTime.Now.ToString();
                viewModel.Shipments[i].Address = viewModel.Shipments[i].Address;
            }
            else
            {
                if (string.IsNullOrEmpty(viewModel.Shipments[i].Address.AddressId))
                {
                    viewModel.Shipments[i].ShippingAddressType
                        = 1;
                    ModelState.AddModelError("Shipments[" + i + "].Address.AddressId", "Address is required.");
                }

                _addressBookService.LoadAddress(viewModel.Shipments[i].Address);
                viewModel.Shipments[i].Address = viewModel.Shipments[i].Address;
            }
        }
    }

    _checkoutService.UpdateShippingAddresses(CartWithValidationIssues.Cart, viewModel);

    // subscription
    AddSubscription(viewModel);


    // Clean up payments in cart on payment provider site.
    foreach (var form in CartWithValidationIssues.Cart.Forms)
    {
        form.Payments.Clear();
    }
    var payment = paymentMethod.CreatePayment(CartWithValidationIssues.Cart.GetTotal().Amount, CartWithValidationIssues.Cart);
    CartWithValidationIssues.Cart.AddPayment(payment);
    CartWithValidationIssues.Cart.Properties[Constants.SwedbankPayPaymentIdField] = paymentId;

    // billing address
    UpdatePaymentAddress(viewModel);

    //payment.BillingAddress = _addressBookService.ConvertToAddress(viewModel.BillingAddress, CartWithValidationIssues.Cart);
    _orderRepository.Save(CartWithValidationIssues.Cart);

    return new JsonResult
    {
        Data = true
    };
}

[HttpGet]
public ActionResult ConsumerProfileRef()
{
    var value = CartWithValidationIssues.Cart.Properties[Constants.ConsumerProfileRef]?.ToString();
    if (!string.IsNullOrWhiteSpace(value))
    {
        return new JsonResult
        {
            Data = new
            {
                ConsumerProfileRef = value
            },
            JsonRequestBehavior = JsonRequestBehavior.AllowGet
        };
    }

    return new HttpNotFoundResult();
}


[HttpPost]
public string GetViewPaymentOrderHref(string consumerProfileRef)
{
    var paymentOrderResponse = _swedbankPayCheckoutService.GetPaymentOrder(CartWithValidationIssues.Cart);
    if (paymentOrderResponse != null)
    {
        return paymentOrderResponse.Operations.View.Href.OriginalString;
    }

    var paymentOrderResponseObject = _swedbankPayCheckoutService.CreateOrUpdatePaymentOrder(Cart, "description", consumerProfileRef);
    return paymentOrderResponseObject.Operations.View.Href.OriginalString;
}

[HttpPost]
public async Task<string> GetSwedbankPayShippingDetails(Uri url)
{
    var market = _marketService.GetMarket(CartWithValidationIssues.Cart.MarketId);
    var swedbankPayClient = _swedbankPayClientFactory.Create(market, _languageService.GetCurrentLanguage().TwoLetterISOLanguageName);
    var shippingDetails = await swedbankPayClient.Consumers.GetShippingDetails(url);
    return System.Text.Json.JsonSerializer.Serialize(shippingDetails, JsonSerialization.Settings);
}

[HttpPost]
public async Task<string> GetSwedbankPayBillingDetails(Uri url)
{
    var market = _marketService.GetMarket(CartWithValidationIssues.Cart.MarketId);
    var swedbankPayClient = _swedbankPayClientFactory.Create(market, _languageService.GetCurrentLanguage().TwoLetterISOLanguageName);
    
    var billingDetails = await swedbankPayClient.Consumers.GetBillingDetails(url);
    return System.Text.Json.JsonSerializer.Serialize(billingDetails, JsonSerialization.Settings);
}

```


Update OrderConfirmationController to load purchase order payed by swedbank pay
```Csharp
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

```

### Frontend

In Foundation/Features/Checkout/Checkout.cshtml on #jsCheckoutForm add data attribute **data-addpaymentinfourl**
```Csharp
@using (Html.BeginForm("PlaceOrder", "Checkout", FormMethod.Post, new { @class = "row jsCheckoutForm", id = "jsCheckoutForm", novalidate = "novalidate", data_addpaymentinfourl=Url.Action("AddPaymentAndAddressInformation", null, null) }))

```

Add view Foundation/Features/Checkout/_SwedbankPayCheckoutPeymentMethod.cshtml
```Csharp
@model Foundation.Features.Checkout.Payments.SwedbankPayCheckoutPaymentOption
@{
	var containerId = Guid.NewGuid();
}

@Html.HiddenFor(x => x.SystemKeyword)

<h3>SwedbankPay</h3>

<div id="swedbankpay-checkout">
	@if (Model.ConsumerUiScriptSource == null && (Model.CheckoutConfiguration.UseAnonymousCheckout || Model.UseCheckoutSource))
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
	var serializeToArray = function (form) {
		// Setup our serialized data
		var serialized = [];

		// Loop through each field in the form
		for (var i = 0; i < form.elements.length; i++) {

			var field = form.elements[i];

			// Don't serialize fields without a name, submits, buttons, file and reset inputs, and disabled fields
			if (!field.name || field.disabled || field.type === 'file' || field.type === 'reset' || field.type === 'submit' || field.type === 'button') continue;

			// If a multi-select, get all selections
			if (field.type === 'select-multiple') {
				for (var n = 0; n < field.options.length; n++) {
					if (!field.options[n].selected) continue;
					serialized.push(encodeURIComponent(field.name) + "=" + encodeURIComponent(field.options[n].value));
				}
			}

			// Convert field data to a query string
			else if ((field.type !== 'checkbox' && field.type !== 'radio') || field.checked) {
				serialized.push(encodeURIComponent(field.name) + "=" + encodeURIComponent(field.value));
			}
		}

		return serialized;
	};

	function onCreatedPaymentHandler(paymentCreatedEvent) {
		//console.log(paymentCreatedEvent);
	}

	function onPaymentCanceledHandler(paymentCanceledEvent) {
		//console.log(paymentCanceledEvent);
	}

	function onPaymentFailedHandler(paymentFailedEvent) {
		console.log("Ooopsy daisy");
		console.log(paymentFailedEvent);
	};

	function onPaymentCompletedHandler(paymentCompletedEvent) {
		var form = document.querySelector('#jsCheckoutForm');
		var dataArray = serializeToArray(form);
		dataArray.push('paymentId=' + paymentCompletedEvent.id);
		var data = dataArray.join('&');

		var xhr = new XMLHttpRequest();
		xhr.open("POST", form.getAttribute('data-addpaymentinfourl'), false);
		xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
		xhr.onload = function () {
			if (xhr.status === 200) {
				console.log('payment created');
				var url = paymentCompletedEvent.redirectUrl;
				console.log(url);
				window.location.href = url;
			}
			else if (xhr.status !== 200) {
				alert('Request failed.  Returned status of ' + xhr.status);
			}
		};

		xhr.send(data);
		console.log('address saved');
	}


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

	var paymentMenuConfig = {
		container: 'swedbankpay-paymentmenu-@containerId',
		culture: '@Culture',
		style: style,
		onPaymentCreated: onCreatedPaymentHandler,
		onPaymentCompleted: onPaymentCompletedHandler,
		onPaymentFailed: onPaymentFailedHandler,
		onPaymentCanceled: onPaymentCanceledHandler
	};

	function createPaymentMenuElements(menuScript) {
		if (window.payex?.hostedView?.paymentMenu !== undefined)
		{
			window.payex.hostedView.paymentMenu = undefined;
		}

		var paymentMenuScriptTag = document.getElementById('paymentMenuScript');
		if (paymentMenuScriptTag !== null) {
			paymentMenuScriptTag.remove();
		}

		var script = document.createElement('script');
		script.setAttribute("id", "paymentMenuScript");
		script.onload = function () {
			window.payex.hostedView.paymentMenu(paymentMenuConfig).open();
		};
		script.src = menuScript;
		document.getElementsByTagName('head')[0].appendChild(script);
	}
</script>


@if (Model.ConsumerUiScriptSource == null && (Model.CheckoutConfiguration.UseAnonymousCheckout || Model.UseCheckoutSource))
{
	<script type="text/javascript">
		createPaymentMenuElements('@Model.JavascriptSource');
	</script>
}
else
{
	<script type="text/javascript">

		var consumerConfig = {
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
				document.querySelector('#Shipments_0__Address_Email').value = response.email;
				document.querySelector('#Shipments_0__Address_FirstName').value = billingAddress.addressee;
				document.querySelector('#Shipments_0__Address_LastName').value = billingAddress.addressee;
				document.querySelector('#Shipments_0__Address_Line1').value = billingAddress.streetAddress;
				document.querySelector('#Shipments_0__Address_PostalCode').value = billingAddress.zipCode;
				document.querySelector('#Shipments_0__Address_City').value = billingAddress.city;
			});
			request.open('POST', '@Url.Action("GetSwedbankPayBillingDetails", "Checkout", null)', true);
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
				document.querySelector('#Shipments_0__Address_Email').value = response.email;
				document.querySelector('#Shipments_0__Address_FirstName').value = shippingAddress.addressee;
				document.querySelector('#Shipments_0__Address_LastName').value = shippingAddress.addressee;
				document.querySelector('#Shipments_0__Address_Line1').value = shippingAddress.streetAddress;
				document.querySelector('#Shipments_0__Address_PostalCode').value = shippingAddress.zipCode;
				document.querySelector('#Shipments_0__Address_City').value = shippingAddress.city;
			});
			request.open('POST', '@Url.Action("GetSwedbankPayShippingDetails", "Checkout", null)', true);
			request.setRequestHeader('Content-Type', 'application/json; charset=utf-8');
			request.send(JSON.stringify(data));
		}

		function onIdentifiedConsumerHandler(data) {
			console.log('onIdentifiedConsumerHandler:' + JSON.stringify(data));
			var paymentMenuFrame = document.getElementById("paymentMenuFrame");
			paymentMenuFrame.removeAttribute("hidden");

			var request = new XMLHttpRequest();
			request.addEventListener('load', function () {
				createPaymentMenuElements(this.responseText);
			});
			request.open('POST', '@Url.Action("GetViewPaymentOrderHref", "Checkout", null)', true);
			request.setRequestHeader('Content-Type', 'application/json; charset=utf-8');
			request.send(JSON.stringify(data));
		}

		if (window.payex?.hostedView?.consumer === undefined) {
			var script = document.createElement('script');
			script.setAttribute('src', '@Model.ConsumerUiScriptSource');
			script.onload = function () {
				window.payex.hostedView.consumer(consumerConfig).open();
			}

			var head = document.getElementsByTagName('head')[0];
			head.appendChild(script);
		}
		else
		{
			window.payex.hostedView.consumer(consumerConfig).open();
		}

		var request = new XMLHttpRequest();
		request.onreadystatechange = function() {
			if (this.readyState == 4 && this.status == 200) {
				onIdentifiedConsumerHandler(JSON.parse(this.responseText));
			}
		}
		request.open('GET', '@Url.Action("ConsumerProfileRef", "Checkout", null)', true);
		request.send();
	</script>
}
```

Add view Foundation/Features/MyAccount/OrderConfirmation/_SwedbankPayCheckoutConfirmation.cshtml
```Html
@model EPiServer.Commerce.Order.IPayment
<div>
	<h4>@Html.Translate("/OrderConfirmation/PaymentDetails")</h4>
	<p>@Model.PaymentMethodName</p>
	<p>TransactionId: @Model.ProviderTransactionID</p>

</div>
```


**Note: If Foundation is used, you need to update customHeaders in web.config.
```xml
    <httpProtocol>
      <customHeaders>
        <remove name="X-Powered-By" />
        <add name="Content-Security-Policy" value="default-src 'self' https://ecom.externalintegration.payex.com ws: wss: data:; script-src 'self' 'unsafe-inline' 'unsafe-eval' https://ecom.externalintegration.payex.com https://dc.services.visualstudio.com https://az416426.vo.msecnd.net https://code.jquery.com https://maxcdn.bootstrapcdn.com *.facebook.com *.facebook.net *.episerver.net *.bing.com *.virtualearth.net; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com *.episerver.net *.bing.com; font-src 'self' https://fonts.gstatic.com data:; connect-src 'self' https://dc.services.visualstudio.com ws: wss: *.bing.com *.virtualearth.net; img-src 'self' data: http: https:; child-src 'self' *.payex.com *.powerbi.com *.vimeo.com *.youtube.com *.facebook.com;" />
        <add name="X-XSS-Protection" value="1; mode=block" />
        <add name="X-Content-Type-Options" value="nosniff " />
      </customHeaders>
```

[opengraph-image]: https://repository-images.githubusercontent.com/171851967/01256480-53e7-11ea-9c0f-da3e3b5811b3
