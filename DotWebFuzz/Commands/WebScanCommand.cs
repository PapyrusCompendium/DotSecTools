using System.Diagnostics.CodeAnalysis;

using DotWebFuzz.Commands.Settings;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotWebFuzz.Commands {
    public class WebScanCommand : Command<WebScanCommandSettings> {

        private readonly IWebScanningService _webScanningService;

        public WebScanCommand(IWebScanningService webScanningService) {
            _webScanningService = webScanningService;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] WebScanCommandSettings settings) {

            AnsiConsole.Status()
            .Start($"Scanning {settings.WebAddress}", ctx => {
                DisplayHiddenData(settings);
                ctx.SpinnerStyle(Style.Parse("green"));

                AnsiConsole.Cursor.Show();

                EnumerateDictionary(settings, ctx);
            });
            return 1;
        }

        private static void DisplayHiddenData(WebScanCommandSettings settings) {
            if (settings.HideCodes.Length > 0) {
                var hiddenCodes = string.Join(", ", settings.HideCodes);
                AnsiConsole.WriteLine($"Hiding Http Codes: [{hiddenCodes}]");
            }

            if (settings.HideSizes.Length > 0) {
                var hiddenSizes = string.Join(", ", settings.HideSizes);
                AnsiConsole.WriteLine($"Hiding Content Lengths: [{hiddenSizes}]");
            }
        }

        private void EnumerateDictionary(WebScanCommandSettings settings, StatusContext ctx) {
            var streamReader = new StreamReader(File.OpenRead(settings.WordList!));
            var requestsSent = 0;
            var startTime = DateTime.Now;

            while (!streamReader.EndOfStream) {
                requestsSent = SendRequests(settings, ctx, streamReader, requestsSent, startTime);
            }
        }

        private int SendRequests(WebScanCommandSettings settings, StatusContext ctx, StreamReader streamReader, int requestsSent, DateTime startTime) {
            var scansPerSecond = Math.Round(requestsSent / DateTime.Now.Subtract(startTime).TotalSeconds, 1);
            if (settings.RateLimit > 0) {
                ctx.Spinner(Spinner.Known.CircleHalves);
                Thread.Sleep(TimeSpan.FromSeconds((int)(scansPerSecond / settings.RateLimit!)));
            }
            else {
                ctx.Spinner(Spinner.Known.Dots3);
            }

            var taskCount = settings.RateLimit < settings.Concurrent ? (int)settings.RateLimit! : settings.Concurrent;
            taskCount = taskCount == 0 ? 1 : taskCount;
            var runningRequests = new Task[settings.Concurrent ?? 1];

            for (var x = 0; x < runningRequests.Length; x++) {
                var payload = streamReader.ReadLine();
                var runningTask = new TaskFactory().StartNew(() => {
                    _webScanningService.SendRequest(settings, payload!, (response, requestUrl) => {
                        requestsSent++;
                        ctx.Status($"({scansPerSecond} http/sec) Scanning {requestUrl}");

                        if (IsHiddenResponse(settings, response)) {
                            return;
                        }

                        var httpVersionColor = response.Version.Minor < 1 ? "red3" : "darkslategray1";
                        var statusCodeColor = response.IsSuccessStatusCode ? "palegreen1" : "deeppink4_2";

                        var columns = new Columns(
                            new Markup($"[{httpVersionColor}]Http/{response.Version}[/] [{statusCodeColor}]{(int)response.StatusCode}[/] Payload: [palegreen1]{payload}[/]"),
                            new Markup($"Url: [link]{requestUrl}[/]")
                        );

                        AnsiConsole.Write(columns);
                    });
                });
                runningRequests[x] = runningTask;
            }

            Task.WaitAll(runningRequests);
            return requestsSent;
        }

        private bool IsHiddenResponse(WebScanCommandSettings settings, HttpResponseMessage response) {
            if (settings.HideCodes.Contains((int)response.StatusCode)) {
                return true;
            }

            var contentLength = response.Content.ReadAsStream().Length;
            if (settings.HideSizes.Contains((ulong)contentLength)) {
                return true;
            }

            return false;
        }
    }
}
