using System;
using System.Net;

namespace Saiive.BlockCypher.Core
{
    public class EndpointException : Exception
    {
        public EndpointException(string message, HttpStatusCode httpStatsuCode, Exception innerException) : base(message, innerException)
        {
            HttpStatsuCode = httpStatsuCode;
        }

        public HttpStatusCode HttpStatsuCode { get; }
    }
}
