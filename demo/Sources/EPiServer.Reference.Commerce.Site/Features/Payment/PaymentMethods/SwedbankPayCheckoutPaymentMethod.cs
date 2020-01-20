using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;

using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;

using SwedbankPay.Episerver.Checkout;
using SwedbankPay.Episerver.Checkout.Common;

using System;
using System.ComponentModel;
using System.Linq;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.PaymentOrders;
using SwedbankPay.Sdk.Payments;
using PaymentType = Mediachase.Commerce.Orders.PaymentType;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{

    [ServiceConfiguration(typeof(IPaymentMethod))]
    public class SwedbankPayCheckoutPaymentMethod : PaymentMethodBase, IDataErrorInfo
    {
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly ICartService _cartService;
        private readonly IMarketService _marketService;
        private readonly ISwedbankPayCheckoutService _swedbankPayCheckoutService;
        private bool _isInitalized;
        private readonly CustomerContextFacade _customerContext;
        private readonly IOrderRepository _orderRepository;
        

        public SwedbankPayCheckoutPaymentMethod()
            : this(
                LocalizationService.Current,
                ServiceLocator.Current.GetInstance<IOrderGroupFactory>(),
                ServiceLocator.Current.GetInstance<LanguageService>(),
                ServiceLocator.Current.GetInstance<IPaymentManagerFacade>(),
                ServiceLocator.Current.GetInstance<ICartService>(),
                ServiceLocator.Current.GetInstance<IMarketService>(), 
                ServiceLocator.Current.GetInstance<ISwedbankPayCheckoutService>(),
                ServiceLocator.Current.GetInstance<CustomerContextFacade>(), ServiceLocator.Current.GetInstance<IOrderRepository>()
        ){
        }

        public SwedbankPayCheckoutPaymentMethod(
            LocalizationService localizationService,
            IOrderGroupFactory orderGroupFactory,
            LanguageService languageService,
            IPaymentManagerFacade paymentManager,
            ICartService cartService,
            IMarketService marketService,
            ISwedbankPayCheckoutService swedbankPayCheckoutService, CustomerContextFacade customerContext, IOrderRepository orderRepository)
            : base(localizationService, orderGroupFactory, languageService, paymentManager)
        {
            _orderGroupFactory = orderGroupFactory;
            _cartService = cartService;
            _marketService = marketService;
            _swedbankPayCheckoutService = swedbankPayCheckoutService;
            _orderRepository = orderRepository;
            _customerContext = customerContext;
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
            Culture = market.DefaultLanguage.TextInfo.CultureName;

            var orderId = cart.Properties[Constants.SwedbankPayCheckoutOrderIdCartField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId) || CheckoutConfiguration.UseAnonymousCheckout)
            {
                GetCheckoutJavascriptSource(cart, "description");
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
            
            var orderData = _swedbankPayCheckoutService.InitiateConsumerSession(email, phone, ssn);
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
            var isSwishPayment = currentPayment.Instrument.Equals("Swish", StringComparison.InvariantCultureIgnoreCase);
            payment.Status = isSwishPayment ?  PaymentStatus.Processed.ToString() :  PaymentStatus.Pending.ToString();
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
