namespace EPiServer.Reference.Commerce.Manager
{
    using EPiServer.Commerce.Order;
    using EPiServer.Framework;
    using EPiServer.Framework.Initialization;
    using EPiServer.ServiceLocation;

    using Mediachase.Commerce.Catalog;

    using SwedbankPay.Checkout.Episerver.Common;

    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class SiteInitialization : IConfigurableModule
    {
        public void Initialize(InitializationEngine context)
        {

        }

        public void Uninitialize(InitializationEngine context)
        {

        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var services = context.Services;
            services.Intercept<ITaxCalculator>((locator, calculator) =>
                new SwedbankPayTaxCalculator(locator.GetInstance<IContentRepository>(),
                    locator.GetInstance<ReferenceConverter>()));
        }
    }
}
