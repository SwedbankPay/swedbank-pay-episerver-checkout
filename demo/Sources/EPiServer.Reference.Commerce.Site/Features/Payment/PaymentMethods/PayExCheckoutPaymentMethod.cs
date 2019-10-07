namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    using EPiServer.Commerce.Order;
    using EPiServer.Framework.Localization;
    using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
    using EPiServer.Reference.Commerce.Site.Features.Market.Services;
    using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
    using EPiServer.ServiceLocation;

    using Mediachase.Commerce.Markets;
    using Mediachase.Commerce.Orders;

    using PayEx.Checkout.Episerver;
    using PayEx.Checkout.Episerver.Common;

    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    [ServiceConfiguration(typeof(IPaymentMethod))]
    public class PayExCheckoutPaymentMethod : PaymentMethodBase, IDataErrorInfo
    {
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly ICartService _cartService;
        private readonly IMarketService _marketService;
        private readonly IPayExCheckoutService _payExCheckoutService;
        private bool _isInitalized;

        public PayExCheckoutPaymentMethod()
            : this(
                LocalizationService.Current,
                ServiceLocator.Current.GetInstance<IOrderGroupFactory>(),
                ServiceLocator.Current.GetInstance<LanguageService>(),
                ServiceLocator.Current.GetInstance<IPaymentManagerFacade>(),
                ServiceLocator.Current.GetInstance<ICartService>(),
                ServiceLocator.Current.GetInstance<IMarketService>(),
                ServiceLocator.Current.GetInstance<IPayExCheckoutService>())
        {
        }

        public PayExCheckoutPaymentMethod(
            LocalizationService localizationService,
            IOrderGroupFactory orderGroupFactory,
            LanguageService languageService,
            IPaymentManagerFacade paymentManager,
            ICartService cartService,
            IMarketService marketService,
            IPayExCheckoutService payExCheckoutService)
            : base(localizationService, orderGroupFactory, languageService, paymentManager)
        {
            _orderGroupFactory = orderGroupFactory;
            _cartService = cartService;
            _marketService = marketService;
            _payExCheckoutService = payExCheckoutService;
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

            var cart = _cartService.LoadCart(_cartService.DefaultCartName);
            var market = _marketService.GetMarket(cart.MarketId);
            CheckoutConfiguration = _payExCheckoutService.LoadCheckoutConfiguration(market);
            Culture = market.DefaultLanguage.TextInfo.CultureName;

            var orderId = cart.Properties[Constants.PayExCheckoutOrderIdCartField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId) || CheckoutConfiguration.UseAnonymousCheckout)
            {
                GetCheckoutJavascriptSource(cart);
            }
            else
            {
                GetCheckInJavascriptSource(cart);
            }

            _isInitalized = true;
        }

        private void GetCheckoutJavascriptSource(ICart cart)
        {
            var orderData = _payExCheckoutService.CreateOrUpdateOrder(cart, HttpContext.Current.Request.UserAgent);
            JavascriptSource = orderData.Operations.FirstOrDefault(x => x.Rel == Operations.ViewPaymentOrder)?.Href;
            UseCheckoutSource = true;
        }

        private void GetCheckInJavascriptSource(ICart cart)
        {
            string email = "PayexTester@payex.com";
            string phone = "+46739000001";
            string ssn = "199710202392";

            var orderData = _payExCheckoutService.InitiateConsumerSession(email, phone, ssn);
            JavascriptSource = orderData.Operations.FirstOrDefault(x => x.Rel == Operations.ViewConsumerIdentification)?.Href;
        }

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreatePayment(_orderGroupFactory);
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = Constants.PayExCheckoutSystemKeyword;
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Authorization.ToString();
            return payment;
        }

        public override bool ValidateData()
        {
            return true;
        }

        public override string SystemKeyword => Constants.PayExCheckoutSystemKeyword;

        public string this[string columnName] => string.Empty;

        public string Error { get; }
        public CheckoutConfiguration CheckoutConfiguration { get; set; }
        public string Culture { get; set; }
        public string JavascriptSource { get; set; }
        public bool UseCheckoutSource { get; set; }
    }
}
