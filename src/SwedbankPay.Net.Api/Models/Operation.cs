namespace PayEx.Net.Api.Models
{
    public class Operation
    {
        /// <summary>
        /// The relational name of the operation, used as a programmatic identifier to find the correct operation given the current state of the application.
        /// </summary>
        public string Rel { get; set; }

        /// <summary>
        /// The HTTP method to use when performing the operation.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// The HTTP content type of the target URI. Indicates what sort of resource is to be found at the URI, how it is expected to be used and behave.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The target URI of the operation. 
        /// </summary>
        public string Href { get; set; }
    }
}