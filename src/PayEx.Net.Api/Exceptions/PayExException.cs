using System;

namespace PayEx.Net.Api.Exceptions
{
    using PayEx.Net.Api.Models;

    [Serializable]
    public class PayExException : Exception
    {
        public int ErrorCode { get; }
        public Error Error { get; }

        public PayExException()
        {
        }


        public PayExException(string message) : base(message)
        {
        }

        public PayExException(int errorCode, string message, Error error) : base(message)
        {
            ErrorCode = errorCode;
            Error = error;
        }


        //public PayExException(ErrorResponse errorResponse) : base(string.Join(",", errorResponse.Errors.Select(x => x.ErrorMessage)))
        //{
        //    ErrorCode = (int)errorResponse.Code;
        //}

        public PayExException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PayExException(int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

    }
}
