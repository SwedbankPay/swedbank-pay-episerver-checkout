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
        public async Task<string> GetSwedbankPayShippingDetails(Uri url)
        {

            var swedbankPayClient = _swedbankPayClientFactory.Create(_currentMarket.GetCurrentMarket());
            var shippingDetails = await swedbankPayClient.Consumers.GetShippingDetails(url);
            return JsonConvert.SerializeObject(shippingDetails, JsonSerialization.Settings);
        }

        [HttpPost]
        public async Task<string> GetSwedbankPayBillingDetails(Uri url)
        {
            var swedbankPayClient = _swedbankPayClientFactory.Create(_currentMarket.GetCurrentMarket());
            var billingDetails = await swedbankPayClient.Consumers.GetBillingDetails(url);
            return JsonConvert.SerializeObject(billingDetails, JsonSerialization.Settings);
        }
    }
}
