using SwedbankPay.Sdk;

using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwedbankPay.Episerver.Checkout.Controllers
{
    public class SwedbankPayCheckoutController : Controller
    {
        private readonly ISwedbankPayClient _swedbankPayClient;

        public SwedbankPayCheckoutController(ISwedbankPayClient swedbankPayClient)
        {
            _swedbankPayClient = swedbankPayClient;
        }


        [HttpPost]
        public async Task<JsonResult> GetSwedbankPayShippingDetails(Uri url)
        {
            var shippingDetails = await _swedbankPayClient.Consumers.GetShippingDetails(url);
            
            return new JsonResult
            {
                Data = shippingDetails
            };
        }

        [HttpPost]
        public async Task<JsonResult> GetSwedbankPayBillingDetails(Uri url)
        {
            var billingDetails = await _swedbankPayClient.Consumers.GetBillingDetails(url);

            return new JsonResult
            {
                Data = billingDetails
            };
        }
    }
}
