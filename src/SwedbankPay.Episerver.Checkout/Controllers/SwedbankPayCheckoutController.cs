using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.Personalization;
using SwedbankPay.Episerver.Checkout.Common.Helpers;
using SwedbankPay.Sdk;

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
    }
}
