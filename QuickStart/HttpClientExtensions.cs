using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Microsoft.Crm.Sdk.Samples {

    public static class HttpClientExtensions {
        /// <summary>
        /// Sends an HTTP message containing a JSON payload to the target URL.  
        /// </summary>
        /// <typeparam name="T">The type of the data to send in the message content (payload).</typeparam>
        /// <param name="client">A preconfigured HTTP client.</param>
        /// <param name="method">The HTTP method to invoke.</param>
        /// <param name="requestUri">The relative URL of the message request.</param>
        /// <param name="value">The data to send in the payload. The data will be converted to a serialized JSON payload.</param>
        /// <returns>An HTTP response message.</returns>
        public static Task<HttpResponseMessage> SendAsJsonAsync<T>(this HttpClient client, HttpMethod method, string requestUri, T value) {
            string content = String.Empty;
            if (value != null) {
                if (value.GetType().Name.Equals("JObject"))
                    content = value.ToString();
                else
                    content = JsonConvert.SerializeObject(value, new JsonSerializerSettings() {
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
            }
            HttpRequestMessage request = new HttpRequestMessage(method, requestUri);
            if (!string.IsNullOrEmpty(content)) {
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            return client.SendAsync(request);
        }


    }

}