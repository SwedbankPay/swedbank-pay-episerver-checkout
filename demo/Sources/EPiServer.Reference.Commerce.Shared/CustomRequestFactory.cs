using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders.Dto;
using SwedbankPay.Checkout.Episerver.Common;
using SwedbankPay.Sdk.PaymentOrders;


namespace EPiServer.Reference.Commerce.Shared
{
    public class CustomRequestFactory : RequestFactory
    {
        public CustomRequestFactory(ICheckoutConfigurationLoader checkoutConfigurationLoader, IOrderGroupCalculator orderGroupCalculator, IShippingCalculator shippingCalculator, ITaxCalculator taxCalculator, ReferenceConverter referenceConverter, IContentRepository contentRepository) : base(checkoutConfigurationLoader, orderGroupCalculator, shippingCalculator, taxCalculator, referenceConverter, contentRepository)
        {
        }
        public override PaymentOrderRequestContainer GetPaymentOrderRequestContainer(IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string consumerProfileRef = null)
        {
            var test = base.GetPaymentOrderRequestContainer(orderGroup, market, paymentMethodDto, consumerProfileRef);
            test.Paymentorder.Amount = 500;
            test.Paymentorder.VatAmount = 0;
            
            test.Paymentorder.OrderItems.Remove(test.Paymentorder.OrderItems.First(x => x.Reference == "SHIPPING"));
            test.Paymentorder.OrderItems.First().Amount = 500;
            test.Paymentorder.OrderItems.First().VatAmount = 0;
            
            return test;

        }
    }
}
