namespace PayEx.Net.Api.Utils
{
	using Newtonsoft.Json;
	using PayEx.Net.Api.ContractResolver;
	using PayEx.Net.Api.Models;
	using RestSharp;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class Utils
    {
        public static string GetRequestBody(object request)
        {
            string requestBody = string.Empty;
            if (request != null)
            {
                requestBody = JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings
                {
                    //DefaultValueHandling = DefaultValueHandling.Ignore,
                    ContractResolver = new LowercaseContractResolver(),
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                });
            }
            return requestBody;
        }

        public static string GetExpandQueryString<T>(T expandParameter) where T : Enum
        {
	        List<string> s = new List<string>();
	        foreach (var enumValue in Enum.GetValues(typeof(T)))
	        {
		        var name = Enum.GetName(typeof(T), enumValue);
		        if (expandParameter.HasFlag((T)enumValue) && name != "None" && name != "All")
				{
			        s.Add(name.ToLower());
		        }
	        }

	        var queryString = string.Join(",", s);
	        return queryString;
        }

		public static THeaderValue GetHeaderResponseValue<THeaderValue>(this IRestResponse response, string name) where THeaderValue : class
        {
            return response.Headers.FirstOrDefault(x => x.Name == name)?.Value as THeaderValue;
        }
    }
}
