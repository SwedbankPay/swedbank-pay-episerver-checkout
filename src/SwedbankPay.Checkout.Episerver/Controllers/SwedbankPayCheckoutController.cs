using System.Web.Mvc;
using SwedbankPay.Checkout.Episerver.Common.Helpers;

namespace SwedbankPay.Checkout.Episerver.Controllers
{
    public class SwedbankPayCheckoutController : Controller
    {
        private readonly ISwedbankPayCheckoutService _swedbankPayCheckoutService;

        public SwedbankPayCheckoutController(ISwedbankPayCheckoutService swedbankPayCheckoutService)
        {
            _swedbankPayCheckoutService = swedbankPayCheckoutService;
        }


        [HttpPost]
        public JsonResult GetSwedbankPayShippingDetails(string url)
        {
            var shippingDetails = _swedbankPayCheckoutService.GetShippingDetails(url);

            if (!string.IsNullOrWhiteSpace(shippingDetails?.ShippingAddress?.CountryCode))
            {
                shippingDetails.ShippingAddress.CountryCode = CountryCodeHelper.GetThreeLetterCountryCode(shippingDetails.ShippingAddress.CountryCode);
            }

            return new JsonResult
            {
                Data = shippingDetails
            };
        }
    }
}
