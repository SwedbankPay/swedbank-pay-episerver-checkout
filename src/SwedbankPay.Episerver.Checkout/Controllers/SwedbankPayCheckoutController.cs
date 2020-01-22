using SwedbankPay.Episerver.Checkout.Common;

using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Mediachase.Commerce;
using Newtonsoft.Json;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.JsonSerialization;

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
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Converters = new System.Collections.Generic.List<JsonConverter>
                {
                    new CustomEmailAddressConverter(typeof(EmailAddress)),
                    new CustomMsisdnConverter(typeof(Msisdn))
                }
            };

            return JsonConvert.SerializeObject(shippingDetails, jsonSerializerSettings);

        }

        [HttpPost]
        public async Task<string> GetSwedbankPayBillingDetails(Uri url)
        {
            var swedbankPayClient = _swedbankPayClientFactory.Create(_currentMarket.GetCurrentMarket());

            var billingDetails = await swedbankPayClient.Consumers.GetBillingDetails(url);
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Converters = new System.Collections.Generic.List<JsonConverter>
                {
                    new CustomEmailAddressConverter(typeof(EmailAddress)),
                    new CustomMsisdnConverter(typeof(Msisdn))
                }
            };

            return JsonConvert.SerializeObject(billingDetails, jsonSerializerSettings);
        }
    }
}
