using Mediachase.Commerce;

using SwedbankPay.Episerver.Checkout.Common;

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
        public async Task<JsonResult> GetSwedbankPayShippingDetails(Uri url, string languageId)
        {
	        var swedbankPayClient = _swedbankPayClientFactory.Create(_currentMarket.GetCurrentMarket(), languageId);
            var shippingDetails = await swedbankPayClient.Consumers.GetShippingDetails(url);
	        return new JsonResult
	        {
		        Data = new AddressDetailsDto(shippingDetails)
	        };
        }

        [HttpPost]
        public async Task<JsonResult> GetSwedbankPayBillingDetails(Uri url, string languageId)
        {
	        var swedbankPayClient = _swedbankPayClientFactory.Create(_currentMarket.GetCurrentMarket(), languageId);

            var billingDetails = await swedbankPayClient.Consumers.GetBillingDetails(url);
	        return new JsonResult
	        {
		        Data = new AddressDetailsDto(billingDetails)
	        };
        }
    }
}