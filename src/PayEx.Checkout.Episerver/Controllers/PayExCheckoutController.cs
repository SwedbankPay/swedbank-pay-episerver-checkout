namespace PayEx.Checkout.Episerver.Controllers
{
    using PayEx.Checkout.Episerver.Helpers;
    using System.Web.Mvc;

    public class PayExCheckoutController : Controller
    {
        private readonly IPayExCheckoutService _payExCheckoutService;

        public PayExCheckoutController(IPayExCheckoutService payExCheckoutService)
        {
            _payExCheckoutService = payExCheckoutService;
        }


        [HttpPost]
        public JsonResult GetPayExShippingDetails(string url)
        {
            var shippingDetails = _payExCheckoutService.GetShippingDetails(url);

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
