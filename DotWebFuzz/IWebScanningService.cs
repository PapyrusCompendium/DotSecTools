
using DotWebFuzz.Commands.Settings;

namespace DotWebFuzz {
    public interface IWebScanningService {
        Task SendRequest(WebScanCommandSettings settings, string fuzz, Action<HttpResponseMessage, string> callback);
    }
}