﻿using System;

namespace stellar_dotnetcore_sdk.requests
{
    public class HttpResponseException : Exception
    {
        public HttpResponseException(int statusCode, string s)
            : base(s)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; set; }
    }
}