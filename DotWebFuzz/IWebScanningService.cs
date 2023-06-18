
using DotWebFuzz.Commands.Settings;

namespace DotWebFuzz {
    public interface IWebScanningService {
        void SendRequest(WebScanCommandSettings settings, string fuzz, Action<HttpResponseMessage, string> callback);
    }
}