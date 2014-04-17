using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace HighscoreAPI
{
    public class Communicator
    {
        private readonly IApiRequest _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Context containing config.</param>
        public Communicator(IApiRequest context)
        {
            _context = context;
        }

        /// <summary>
        /// Makes an API-request
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="method">GET or POST</param>
        /// <param name="urlSuffix">API resource</param>
        /// <param name="data">Parameters or post data</param>
        /// <param name="callback">Action callback</param>
        public void Request<T>(string method, string urlSuffix, IDictionary<string, object> data, Action<Response<T>> callback)
        {
            string url;
            string postData = "";

            if (method == "GET")
            {
                url = GenerateUrl(urlSuffix) + "&" +  DataToParameters(data);   
            }
            else
            {
                postData = "data=" + JsonConvert.SerializeObject(data);
                url = GenerateUrl(urlSuffix, postData);  
            }

            var request = (HttpWebRequest)WebRequest.Create(url);

            // Set request method
            request.Method = method;
            // Set proxy to null, speeds up connection
            request.Proxy = null;
            // Connection timeout
            request.Timeout = 6000;
            request.ReadWriteTimeout = 6000;
            request.KeepAlive = false;


            if (method == "GET")
            {
                request.BeginGetResponse(FinishWebResponse<T>, new RequestState<T> { Request = request, Callback = callback });
            }
            else
            {
                // Encode post data to byte array
                var requestData = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = requestData.Length;

                request.BeginGetRequestStream(InitializePostWebRequest<T>, new RequestState<T> { Request = request, Data = requestData, Callback = callback });
         
            }

        }

        /// <summary>
        /// Initialize the post request with data
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="result"></param>
        private static void InitializePostWebRequest<T>(IAsyncResult result)
        {
            var state = (RequestState<T>)result.AsyncState;

            using (var dataStream = state.Request.EndGetRequestStream(result))
            {
                dataStream.Write(state.Data, 0, state.Data.Length);
                dataStream.Close();
            }

            state.Request.BeginGetResponse(FinishWebResponse<T>, state);
        }

        /// <summary>
        /// Finish the web request with the response
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="result"></param>
        private static void FinishWebResponse<T>(IAsyncResult result)
        {

            var state = (ResponseState<T>)result.AsyncState;

            try
            {
                // Get the response
                using (var webResponse = (HttpWebResponse)state.Request.EndGetResponse(result))
                {
                    // Make sure the callback is set
                    if (state.Callback != null)
                    {
                        // Create a new successful response
                        var response = Response<T>.Success();

                        // Set the raw data
                        response.Data = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();

                        state.Callback(response);
                    }
                }
            }
            catch (Exception ex)
            {

                if (state.Callback != null)
                {
                    // Callback with a failed response
                    state.Callback(Response<T>.Error());
                }
            }
           
        }

        /// <summary>
        /// Encodes a dictionary to an url string of parameters
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string DataToParameters(IDictionary<string, object> data)
        {
            var parameters = new StringBuilder();
            foreach (var kvp in data)
            {
                parameters.Append(kvp.Key +"="+ Uri.EscapeDataString(kvp.Value.ToString()) + "&");
            }

            return parameters.ToString();
        }

        /// <summary>
        /// Generates a correct url for the api queries
        /// </summary>
        /// <param name="urlSuffix"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GenerateUrl(string urlSuffix, string data = "")
        {
            return "http://" + _context.URL + urlSuffix + "?sig=" + CreateSignature(urlSuffix + data + _context.Secret, _context.Secret) + "&key=" + _context.Key;
        }

        /// <summary>
        /// Creates a valid signature based on the posted data with the use of the private key
        /// </summary>
        /// <param name="message"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        private string CreateSignature(string message, string secret)
        {
            // Set the correct encoding
            var encoding = new System.Text.ASCIIEncoding();

            // Set the private key
            var keyByte = encoding.GetBytes(secret);

            // Set the data
            var messageBytes = encoding.GetBytes(message);

            // Use SHA-256 to hash the signature
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                var hashmessage = hmacsha256.ComputeHash(messageBytes);

                // Return a signature with escaped characters
                return Uri.EscapeDataString(Convert.ToBase64String(hashmessage));
            }
        }

        /// <summary>
        /// State information of the asynchronous callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class ResponseState<T>
        {
            public HttpWebRequest Request { get; set; }
            public Action<Response<T>> Callback { get; set; }
        }

        /// <summary>
        /// Extends the ResponseState with the post data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class RequestState<T> : ResponseState<T>
        {
            public byte[] Data { get; set; }
        }
    }
}
