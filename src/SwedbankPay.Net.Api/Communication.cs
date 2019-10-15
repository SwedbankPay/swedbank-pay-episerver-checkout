namespace PayEx.Net.Api
{
    using PayEx.Net.Api.Controllers;
    using RestSharp;
    using RestSharp.Authenticators;
    using System;
    using System.Collections.Generic;

    public abstract class Communication
    {
        private readonly Dictionary<Type, object> _loaders = new Dictionary<Type, object>();

        protected RestClient Client { get; }

        protected Communication(string apiAddress, string token)
        {
            if (!string.IsNullOrWhiteSpace(apiAddress))
            {
                Client = new RestClient(new Uri(apiAddress))
                {

                    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token, "Bearer")
                };
            }
        }

        protected void RegisterControllerLoader<T>(Func<T> lazyLoaderFunc, bool overrideIfExists = true) where T : ControllerAbstract
        {
            var loaderType = typeof(T);
            if (overrideIfExists || !_loaders.ContainsKey(loaderType))
            {
                _loaders[loaderType] = new Lazy<T>(lazyLoaderFunc);
            }
        }

        protected T GetRegisteredController<T>() where T : ControllerAbstract
        {
            return (_loaders[typeof(T)] as Lazy<T>)?.Value;
        }
    }
}
