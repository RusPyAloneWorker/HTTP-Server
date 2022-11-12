using System;
using System.Collections.Generic;
using System.Text;

namespace HTTPResponse.Attributes
{
    internal class HttpGET : Attribute
    {
        public string MethodURI;
        public HttpGET(string uri)
        {
            MethodURI = uri;
        }
    }
}
