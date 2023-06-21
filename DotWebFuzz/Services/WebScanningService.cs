using System.Web;

using DotWebFuzz.Commands.Settings;

namespace DotWebFuzz {
    public class WebScanningService : IWebScanningService {
        private readonly HttpClient _httpClient;

        public WebScanningService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public void SendRequest(WebScanCommandSettings settings, string fuzz, Action<HttpResponseMessage, string> callback) {
            HttpResponseMessage? response;
            HttpRequestMessage? request;
            try {
                request = GenerateHttpRequestMessage(settings, fuzz);
                response = _httpClient.Send(request);
            }
            catch {
                return;
            }

            if (response is not null) {
                callback.Invoke(response, request.RequestUri!.ToString());
            }
        }

        internal HttpRequestMessage GenerateHttpRequestMessage(WebScanCommandSettings settings, string fuzz) {
            var stringReplacedSettings = new WebScanCommandSettings() {
                WebAddress = settings.WebAddress!.Replace(settings.FuzzKeyword!, HttpUtility.UrlEncode(fuzz)),
                Headers = settings.Headers.Select(i => i.Replace(settings.FuzzKeyword!, fuzz)).ToArray(),
                FormData = settings.FormData.Select(i => i.Replace(settings.FuzzKeyword!, fuzz)).ToArray(),
                HttpMethod = settings.HttpMethod!.Replace(settings.FuzzKeyword!, fuzz)
            };

            var requestMessage = new HttpRequestMessage(new HttpMethod(stringReplacedSettings.HttpMethod), stringReplacedSettings.WebAddress);
            foreach (var header in stringReplacedSettings.Headers) {
                var keyPair = header.Split(":");
                requestMessage.Headers.Add(keyPair[0].Trim(), keyPair[1].Trim());
            }

            return requestMessage;
        }
    }
}
