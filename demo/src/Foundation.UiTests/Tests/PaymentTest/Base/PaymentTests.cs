using Atata;
using Foundation.UiTests.PageObjectModels.CommerceSite.Base;
using Foundation.UiTests.PageObjectModels.CommerceSite.Checkout;
using Foundation.UiTests.PageObjectModels.CommerceSite.Products;
using Foundation.UiTests.PageObjectModels.CommerceSite.ThankYou;
using Foundation.UiTests.PageObjectModels.ManagerSite.Base;
using Foundation.UiTests.PageObjectModels.Payment;
using Foundation.UiTests.Services;
using Foundation.UiTests.Tests.Base;
using Foundation.UiTests.Tests.Helpers;
using NUnit.Framework;
using SwedbankPay.Sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Foundation.UiTests.Tests.PaymentTest
{
    public abstract class PaymentTests : TestBase
    {
        protected double _totalAmount;
        protected double _shippingAmount;
        protected string _currency;
        protected string _orderId;

        protected SwedbankPayClient SwedbankPayClient { get; private set; }

        public PaymentTests(Browsers.Browser browser) : base(browser) { }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var defaultUri = "https://api.externalintegration.payex.com";
            var defaultToken = ConfigurationManager.AppSettings["payexTestToken"];
            #if DEBUG
            var baseUri = new Uri(defaultUri);
            var bearer = defaultToken;
            #elif RELEASE
            var baseUri = new Uri(Environment.GetEnvironmentVariable("Payex.Api.Url", EnvironmentVariableTarget.User) ?? defaultUri);
            var bearer = Environment.GetEnvironmentVariable("Payex.Api.Token", EnvironmentVariableTarget.User) ?? defaultToken;
            #endif

            var httpClient = new HttpClient()
            {
                BaseAddress = baseUri
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);

            SwedbankPayClient = new SwedbankPayClient(httpClient);
        }

        #region Method Helpers

        public HomeCommercePage GoToCommerceHomePage()
        {
            return GoTo<HomeCommercePage>()
                .Settings.Click()
                .MarketList.Items[x => x.Content.Value.ToLower().Contains("sweden")].ClickAndGo<HomeCommercePage>()
                .MarketLanguageSelection.IsVisible.Should.Within(10).BeTrue();
            //.Settings.Click()
            //.LanguageList.Items[x => x.Content.Value.ToLower().Contains("english")].ClickAndGo<HomeCommercePage>()
            //.PageUrl.WaitTo.Contain("en");
        }

        public ProductsPage SelectProducts(Product[] products)
        {
            var home = GoToCommerceHomePage();

            return GoTo<ProductsPage>()
                .ProductList[0].IsVisible.Should.BeTrue()
                .Do(x =>
                {
                    var index = 0;

                    foreach (var product in products)
                    {
                        x.ProductList[index].Hover();

                        x.ProductList[index].OpenModalWindow.Click();

                        x.Modal.AddToCart.IsVisible.WaitTo.BeTrue();
                        x.Modal.Title.StoreValue(out var name);
                        x.Modal.Price.StoreAmount(out var price);

                        x.AlertSuccess.IsVisible.Should.Within(10).BeFalse();

                        for (int i = 0; i < product.Quantity; i++)
                        {
                            x.Modal.AddToCart.Click();
                        }

                        x.Modal.CloseModalWindow.Click();

                        product.UnitPrice = double.Parse(price.ToString());
                        product.Name = name;

                        index += 2;
                    }
                });
        }

        public CheckoutPage GoToCheckoutPage(Product[] products)
        {
            SelectProducts(products);

            return GoTo<CheckoutPage>()
                .IdentificationFrame.IsVisible.WaitTo.BeTrue()
                .TotalAmount.StoreAmount(out _totalAmount);
        }

        public ThankYouPage GoToThankYouPage(Product[] products, PayexInfo payexInfo)
        {
            ThankYouPage page;

            var identificationFrame = GoToCheckoutPage(products)
                    .IdentificationFrame.SwitchTo<IdentificationFramePage>();

            identificationFrame.Email.Set(TestDataService.Email)
                .Phone.Set(TestDataService.SwedishPhoneNumber)
                .Next.Click().Wait(1);

            if (identificationFrame.ContinueWithoutSaving.IsPresent)
            {
                identificationFrame.ContinueWithoutSaving.Click()
                    .Firstname.Set(TestDataService.FirstName)
                    .Lastname.Set(TestDataService.LastName)
                    .Street.Set(TestDataService.Street)
                    .Zipcode.Set(TestDataService.ZipCode)
                    .City.Set(TestDataService.City)
                    .Next.Click();
            }

            AtataContext.Current.Driver.SwitchTo().DefaultContent();

            var paymentFrame = identificationFrame.SwitchToRoot<CheckoutPage>()
                .PaymentFrame.IsPresent.Should.Within(15).BeTrue()
                .PaymentFrame.SwitchTo<PaymentFramePage>();

            page = payexInfo switch
            {
                PayexCardInfo _ => paymentFrame.PerformPaymentWithCard<ThankYouPage>($"{_totalAmount} {_currency}"),
                PayexSwishInfo _ => paymentFrame.PerformPaymentWithSwish<ThankYouPage>($"{_totalAmount} {_currency}"),
                _ => paymentFrame.PerformPaymentWithCard<ThankYouPage>($"{_totalAmount} {_currency}"),
            };

            return page.OrderId.IsVisible.WaitTo.Within(20).BeTrue()
                .OrderId.StoreOrderId(out _orderId);
        }

        public HomeManagerPage GoToManagerHomePage()
        {
            return GoTo<HomeManagerPage>();
        }

        public ManagerPage GoToManagerPage()
        {
            return GoTo<HomeManagerPage>()
                .UserName.Set(TestDataService.ManagerUsername)
                .Password.Set(TestDataService.ManagerPassword)
                .Login.ClickAndGo();
        }

        #endregion

        protected static IEnumerable TestData(bool singleProduct = true, string paymentMethod = PaymentMethods.Card)
        {
            var data = new List<object>();

            if (singleProduct)
                data.Add(new[]
                {
                    new Product { Name = "", Quantity = 1 }
                });
            else
                data.Add(new[]
                {
                    new Product { Name = "", Quantity = 3 },
                    new Product { Name = "", Quantity = 2 }
                });

            switch (paymentMethod)
            {
                case PaymentMethods.Card:
                    data.Add(new PayexCardInfo(TestDataService.CreditCardNumber, TestDataService.CreditCardExpiratioDate,
                                               TestDataService.CreditCardCvc));
                    break;

                case PaymentMethods.Swish:
                    data.Add(new PayexSwishInfo(TestDataService.SwishPhoneNumber));
                    break;
            }

            yield return data.ToArray();
        }

    }
}
