// =====================================================================
//  This file is part of the Microsoft Dynamics CRM SDK code samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
// =====================================================================
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace System.Net.Http
{
    /// <summary>
    /// Adds new classes and methods to the System.Net.Http namespace for improved user
    /// productivity when developing applications for the Dynamics CRM Web API.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Sends an HTTP message containing a JSON payload to the target URL.  
        /// </summary>
        /// <typeparam name="T">The type of the data to send in the message content (payload).</typeparam>
        /// <param name="client">A preconfigured HTTP client.</param>
        /// <param name="method">The HTTP method to invoke.</param>
        /// <param name="requestUri">The relative URL of the message request.</param>
        /// <param name="value">The data to send in the payload. The data will be converted to a serialized JSON payload.</param>
        /// <returns>An HTTP response message.</returns>
        public static Task<HttpResponseMessage> SendAsJsonAsync<T>(this HttpClient client, HttpMethod method, string requestUri, T value)
        {
            string content = String.Empty;
            if (value.GetType().Name.Equals("JObject"))
                content = value.ToString();
            else
                content = JsonConvert.SerializeObject(value, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore });

            HttpRequestMessage request = new HttpRequestMessage(method, requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            return client.SendAsync(request);
        }
    }
}
