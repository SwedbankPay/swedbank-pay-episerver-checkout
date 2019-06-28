namespace PayEx.Net.Api.Controllers
{
    using Newtonsoft.Json;
    using PayEx.Net.Api.Exceptions;
    using PayEx.Net.Api.Models;
    using RestSharp;
    using System;
    using System.Net;

    public abstract class ControllerAbstract
    {
        internal IRestClient Client { get; set; }

        protected ControllerAbstract(IRestClient client)
        {
            Client = client;
        }

        /// <summary>
        /// Executes the specified request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="restRequest">The request.</param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected ResponseValidator<T> Execute<T>(RestRequest restRequest, object request = null) where T : new()
        {
            var response = GetRestResponse<T>(restRequest, request);
            return new ResponseValidator<T>(response);
        }


        /// <summary>
        /// Executes the specified request.
        /// </summary>
        /// <param name="restRequest">The request.</param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected ResponseValidator Execute(RestRequest restRequest, object request = null)
        {
            var response = GetResponse(restRequest, request);
            return new ResponseValidator(response);
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <param name="restRequest">The rest request.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private IRestResponse<T> GetRestResponse<T>(RestRequest restRequest, object request) where T : new()
        {
            UpdateRestRequest(restRequest, request);
            var response = Client.Execute<T>(restRequest);
            ValidateResponse(response);
            return response;
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <param name="restRequest">The rest request.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private IRestResponse GetResponse(RestRequest restRequest, object request)
        {
            UpdateRestRequest(restRequest, request);
            var response = Client.Execute(restRequest);
            ValidateResponse(response);
            return response;
        }



        /// <summary>
        /// Validates the response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <exception cref="System.UnauthorizedAccessException">Unauthorized</exception>
        /// <exception cref="System.Exception">Bad request</exception>
        private static void ValidateResponse(IRestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }

            if (response.ErrorException != null)
            {
                const string msg = "Error retrieving response. Check inner details for more info.";
                //_logger.Error($"Error retrieving response - {response.ErrorException.Message}");
                var applicationException = new ApplicationException(msg, response.ErrorException);
                throw applicationException;
            }
            var error = JsonConvert.DeserializeObject<Error>(response.Content);
            if (error.Status > 0)
            {
                throw new PayExException(error.Status, error.Detail, error);
            }
        }

        /// <summary>
        /// Updates the rest request with parameters.
        /// </summary>
        /// <param name="restRequest">The rest request.</param>
        /// <param name="request">The request.</param>
        private void UpdateRestRequest(RestRequest restRequest, object request)
        {
            if (request != null)
            {
                var jsonString = Utils.Utils.GetRequestBody(request);
                var json = SimpleJson.SimpleJson.DeserializeObject(jsonString);
                restRequest.AddJsonBody(json);
            }

            restRequest.AddHeader("Content-Type", "application/json");
        }

        protected string ConvertUriToRelative(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                return null;
            }

            Uri result;
            if (Uri.TryCreate(uri, UriKind.Absolute, out result))
            {
                return result.PathAndQuery;
            }

            return uri;
        }
    }
}
