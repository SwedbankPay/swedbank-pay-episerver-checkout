namespace PayEx.Net.Api
{
	using Newtonsoft.Json;
	using PayEx.Net.Api.Exceptions;
	using PayEx.Net.Api.Models;
	using PayEx.Net.Api.Utils;
	using RestSharp;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;

	public class ResponseValidator
    {
        public IRestResponse Response { get; }

        public ResponseValidator(IRestResponse response)
        {
            Response = response;
        }

        public ResponseValidator Status(HttpStatusCode status)
        {
            if (!Response.StatusCode.Equals(status))
            {
                throw new PayExException($"Response has wrong StatusCode. Should be {status} but is {Response.StatusCode}");
            }
            return this;
        }

        public ResponseValidator Status(HttpStatusCode[] expected)
        {
            if (!new List<HttpStatusCode>(expected).Contains(Response.StatusCode))
            {
                throw new PayExException($"Response has wrong StatusCode {Response.StatusCode}");
            }
            return this;
        }

        /// <summary>
        /// Gets the response header value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public T GetResponseHeaderValue<T>(string name) where T : class
        {
            return Response.Headers.FirstOrDefault(x => x.Name == name)?.Value as T;
        }
    }


    public class ResponseValidator<T> where T : new()
    {
        public IRestResponse<T> Response { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseValidator{T}"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        public ResponseValidator(IRestResponse<T> response)
        {
            Response = response;
        }

        public ResponseValidator<T> Status(HttpStatusCode status)
        {
            if (!Response.StatusCode.Equals(status))
            {
                var error = JsonConvert.DeserializeObject<Error>(Response.Content);
                if (error.Status > 0)
                {
                    throw new PayExException(error.Status, error.Detail, error);
                }
                throw new PayExException($"Response has wrong StatusCode. Should be {status} but is {Response.StatusCode}");
            }
            return this;
        }

        public ResponseValidator<T> Status(HttpStatusCode[] expected)
        {
            if (!new List<HttpStatusCode>(expected).Contains(Response.StatusCode))
            {
                throw new PayExException($"Response has wrong StatusCode {Response.StatusCode}");
            }
            return this;
        }

        /// <summary>
        /// Gets the header response value.
        /// </summary>
        /// <typeparam name="THeaderValue">The type of the header value.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public THeaderValue GetHeaderResponseValue<THeaderValue>(string name) where THeaderValue : class
        {
            return Response.GetHeaderResponseValue<THeaderValue>(name);
        }
    }
}
