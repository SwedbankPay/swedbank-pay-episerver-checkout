using Mediachase.Commerce;

using Newtonsoft.Json;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Sdk.JsonSerialization;

using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwedbankPay.Episerver.Checkout.Controllers
{
    public class SwedbankPayCheckoutController : Controller
    {
        private readonly ISwedbankPayClientFactory _swedbankPayClientFactory;
        private readonly ICurrentMarket _currentMarket;
        

        public SwedbankPayCheckoutController(ISwedbankPayClientFactory swedbankPayClientFactory, ICurrentMarket currentMarket)
        {
            _swedbankPayClientFactory = swedbankPayClientFactory;
            _currentMarket = currentMarket;
        }


        [HttpPost]
        public async Task<string> GetSwedbankPayShippingDetails(Uri url, string languageId)
        {

            var swedbankPayClient = _swedbankPayClientFactory.Create(_currentMarket.GetCurrentMarket(), languageId);
            var shippingDetails = await swedbankPayClient.Consumers.GetShippingDetails(url);
            return System.Text.Json.JsonSerializer.Serialize(shippingDetails, JsonSerialization.Settings);
        }

        [HttpPost]
        public async Task<string> GetSwedbankPayBillingDetails(Uri url, string languageId)
        {
            var swedbankPayClient = _swedbankPayClientFactory.Create(_currentMarket.GetCurrentMarket(), languageId);
            var billingDetails = await swedbankPayClient.Consumers.GetBillingDetails(url);
            return System.Text.Json.JsonSerializer.Serialize(billingDetails, JsonSerialization.Settings);
        }
    }
}
