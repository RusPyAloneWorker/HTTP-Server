using System;
using System.Collections.Generic;
using System.Text;

namespace HTTPResponse.Attributes
{
    internal class HttpPOST: Attribute
    {
        public string MethodURI;
        public HttpPOST(string uri)
        {
            MethodURI = uri;
        }
    }
}
