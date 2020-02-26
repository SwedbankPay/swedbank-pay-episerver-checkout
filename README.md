# PayEx.Episerver.Checkout

![Swedbank Pay Episerver Checkout][opengraph-image]

## About

**IMPORTANT**: This extension is at an early stage and not yet used in production.
We do not offer support for this version, but will release supported versions
in the future. Feel free to play around, but for full functionality and support,
please wait for the supported, stable release.

The Official Swedbank Pay Checkout Extension for Episerver provides seamless
integration with Swedbank Pay Checkout, allowing your customers to pay swiftly and
securely with Credit Card, Invoice (Norway and Sweden), Vipps (Norway) and
Swish (Sweden). Credit Card Payments are available world-wide.

[opengraph-image]: https://repository-images.githubusercontent.com/171851967/01256480-53e7-11ea-9c0f-da3e3b5811b3



# The installation assumes that you have Quicksilver installed
https://github.com/episerver/Quicksilver

# OBS ngrok for callbacks https://ngrok.com/

# Test cards 
https://developer.swedbankpay.com/resources/test-data.html#callback-test-data

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

![image-b78eff3f-9a7d-480f-a940-12e2aee48bb5](https://user-images.githubusercontent.com/1358504/75348200-a25a6200-58a2-11ea-8f9c-fd0487ee3911.png)



```
Token: {Your token}
ApiUrl: https://api.externalintegration.payex.com
MerchantId: {Your merchant id}

Host URLs: https://swedbankpay.ngrok.io;
CompleteUrl URL: https://swedbankpay.ngrok.io/sv/checkout-sv/SwedbankPayCheckoutConfirmation/?orderGroupId={orderGroupId} 
Cancel URL: https://swedbankpay.ngrok.io/payment-canceled?orderGroupId={orderGroupId}
Callback URL: https://swedbankpay.ngrok.io/payment-callback?orderGroupId={orderGroupId}
Terms of Service URL: https://swedbankpay.ngrok.io/payment-completed.pdf
Payment URL: https://swedbankpay.ngrok.io/sv/checkout-sv/
```
The host in the URL's should be updated to your host.

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
using SwedbankPay.Sdk.PaymentOrders;

using System;
using System.ComponentModel;
using SwedbankPay.Sdk;
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

            CheckoutConfiguration = _swedbankPayCheckoutService.LoadCheckoutConfiguration(market);
            Culture = _languageService.GetCurrentLanguage().TextInfo.CultureName;

            var orderId = cart.Properties[Constants.SwedbankPayCheckoutOrderIdCartField]?.ToString();
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

            var payment = orderGroup.CreatePayment(_orderGroupFactory);
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = Constants.SwedbankPayCheckoutSystemKeyword;
            payment.Amount = amount;
            var isSwishPayment = currentPayment.Instrument.Equals(PaymentInstrument.Swish);
            payment.Status = isSwishPayment ? PaymentStatus.Processed.ToString() : PaymentStatus.Pending.ToString();
            payment.TransactionType = isSwishPayment ? TransactionType.Sale.ToString() : TransactionType.Authorization.ToString();
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
       public IPurchaseOrder CreatePurchaseOrderForSwedbankPay(ICart cart)
        {
            cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator);
            var checkoutOrderId = cart.Properties[Constants.SwedbankPayCheckoutOrderIdCartField]?.ToString();
            var totalProcessedAmount = cart.GetFirstForm().Payments.Where(x => x.Status.Equals(PaymentStatus.Processed.ToString())).Sum(x => x.Amount);
            if (totalProcessedAmount != cart.GetTotal(_orderGroupCalculator).Amount)
            {
                throw new InvalidOperationException("Wrong amount");
            }

            var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
            var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            _orderRepository.Delete(cart.OrderLink);

            if (purchaseOrder == null)
            {
                _swedbankPayCheckoutService.CancelOrder(cart);
                return null;
            }
            else
            {
                _swedbankPayCheckoutService.Complete(purchaseOrder);
                purchaseOrder.Properties[Constants.SwedbankPayOrderIdField] = checkoutOrderId;

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
                    x.Type.Equals(TransactionType.Authorization) ||
                    x.Type.Equals(TransactionType.Sale));

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

```
# Frontend

In Views\Checkout\SingleShipmentCheckout.cshtml on .jsCheckoutForm add data attribute **data-addpaymentinfourl**
````html
 <form class="jsCheckoutForm" action="@Url.Action("Purchase", "Checkout")" method="post" novalidate data-addpaymentinfourl="@Url.Action("AddPaymentAndAddressInformation", null, null)" data-updateurl="@Url.Action("Update", null, null)">
````

Add view Views\Shared\\_SwedbankPayCheckoutConfirmation.cshtml
```html
<div class="quicksilver-well">
    <h4>@Html.Translate("/OrderConfirmation/PaymentDetails")</h4>
    <p>
        SwedbankPay
    </p>
</div>
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

**Note: If QuickSilvers test project is used, you need to add a parameter in CheckoutServiceTests where _subject is created since an extra parameter is added to CheckoutService.cs.
You could go ahead and add just null.**


**Note: If you want to update the checkout when changing shipment, you could add following line at the init function in Scripts/js/Checkout.js**
```Javascript
.on('change', '.jsChangeShipment', Checkout.refreshView) 
```
