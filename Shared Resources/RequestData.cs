﻿using System.Collections.Specialized;
using System.Net;

namespace ASCOM.Remote
{
    public class RequestData
    {
        public RequestData(int clientID, int clientTransactionID, int serverTransactionID, NameValueCollection suppliedParameters, HttpListenerRequest request, HttpListenerResponse response, string[] elements, string deviceKey)
        {
            ClientID = clientID;
            ClientTransactionID = clientTransactionID;
            ServerTransactionID = serverTransactionID;
            SuppliedParameters = suppliedParameters;
            Request = request;
            Response = response;
            Elements = elements;
            DeviceKey = deviceKey;
        }

        public int ClientID { get; set; }
        public int ClientTransactionID { get; set; }
        public int ServerTransactionID { get; set; }
        public NameValueCollection SuppliedParameters { get; set; }
        public HttpListenerRequest Request { get; set; }
        public HttpListenerResponse Response { get; set; }
        public string[] Elements { get; set; }
        public string DeviceKey { get; set; }
    }
}
