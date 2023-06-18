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
            var wordList = File.ReadAllLines(settings.WordList!);

            AnsiConsole.Status()
            .Start($"Scanning {settings.WebAddress}", ctx => {
                if (settings.HideCodes.Length > 0) {
                    var hiddenCodes = string.Join(", ", settings.HideCodes);
                    AnsiConsole.WriteLine($"Hiding Http Codes: [{hiddenCodes}]");
                }

                if (settings.HideSizes.Length > 0) {
                    var hiddenSizes = string.Join(", ", settings.HideSizes);
                    AnsiConsole.WriteLine($"Hiding Content Lengths: [{hiddenSizes}]");
                }

                ctx.Spinner(Spinner.Known.Dots3);
                ctx.SpinnerStyle(Style.Parse("green"));

                var speedOptions = new ParallelOptions() {
                    MaxDegreeOfParallelism = settings.Speed ?? 1,
                    TaskScheduler = TaskScheduler.Default
                };

                Parallel.ForEach(wordList, speedOptions, async (line) => {
                    await _webScanningService.SendRequest(settings, line, (response, requestUrl) => {
                        if (settings.HideCodes.Contains((int)response.StatusCode)) {
                            return;
                        }

                        var contentLength = response.Content.ReadAsStream().Length;
                        if (settings.HideSizes.Contains((ulong)contentLength)) {
                            return;
                        }

                        AnsiConsole.WriteLine($"Http/{response.Version} {response.StatusCode} {requestUrl} Payload: {line}");
                    });
                });
            });

            return 1;
        }
    }
}
