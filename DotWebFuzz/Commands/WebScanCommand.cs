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
                ctx.Spinner(Spinner.Known.Dots3);
                ctx.SpinnerStyle(Style.Parse("green"));

                using var streamReader = new StreamReader(File.OpenRead(settings.WordList!));

                var requestsSent = 0;
                var startTime = DateTime.Now;

                while (!streamReader.EndOfStream) {
                    var scansPerSecond = Math.Round(requestsSent / DateTime.Now.Subtract(startTime).TotalSeconds, 1);
                    if (settings.RateLimit > 0) {
                        Thread.Sleep(TimeSpan.FromSeconds((int)(scansPerSecond / settings.RateLimit!)));
                    }

                    var taskCount = settings.RateLimit < settings.Concurrent ? (int)settings.RateLimit! : settings.Concurrent;
                    taskCount = taskCount == 0 ? 1 : taskCount;
                    var runningRequests = new Task[settings.Concurrent ?? 1];

                    for (var x = 0; x < runningRequests.Length; x++) {
                        var line = streamReader.ReadLine();
                        var runningTask = new TaskFactory().StartNew(() => {
                            _webScanningService.SendRequest(settings, line!, (response, requestUrl) => {
                                requestsSent++;
                                ctx.Status($"({scansPerSecond} http/sec) Scanning {requestUrl}");

                                if (settings.HideCodes.Contains((int)response.StatusCode)) {
                                    return;
                                }

                                var contentLength = response.Content.ReadAsStream().Length;
                                if (settings.HideSizes.Contains((ulong)contentLength)) {
                                    return;
                                }

                                AnsiConsole.WriteLine($"Http/{response.Version} {(int)response.StatusCode} {requestUrl} Payload: {line}");
                            });
                        });
                        runningRequests[x] = runningTask;
                    }

                    Task.WaitAll(runningRequests);
                }
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
    }
}
