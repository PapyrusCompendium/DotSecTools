using System.Diagnostics.CodeAnalysis;

using DotPortScan.Commands.Settings;
using DotPortScan.Properties;
using DotPortScan.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotPortScan.Commands {
    public class PortScanCommand : Command<PortScanSettings> {
        internal const string DEEP_PINK = "deeppink4_2";
        internal const string LIGHT_GREEN = "chartreuse1";
        private const int MAX_PORTS = 65535;
        private readonly IScanningService _scanningService;
        private readonly Dictionary<string, string> _portDescriptions;

        public PortScanCommand(IScanningService scanningService) {
            _scanningService = scanningService;

            _portDescriptions = new();
            foreach (var portDescription in Resources.PortDescriptions.Split("\n")) {
                var port = portDescription.Split(":")[0];
                var description = string.Join(":", portDescription.Split(":").Skip(1));

                _portDescriptions.TryAdd(port, description);
            }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] PortScanSettings settings) {
            AnsiConsole.Status()
            .Start($"Scanning {settings.Host}", ctx => {
                AnsiConsole.Cursor.Show();
                ctx.SpinnerStyle(Style.Parse("green"));

                ScanPorts(settings, ctx, (port, state) => {
                    if (settings.Port == "*" && !state) {
                        return;
                    }

                    var stateColor = state ? LIGHT_GREEN : DEEP_PINK;
                    var status = state ? "Open" : "Closed";
                    _portDescriptions.TryGetValue(port.ToString(), out var description);
                    AnsiConsole.MarkupLine($"{port}: [{stateColor}]{status}[/] {description}");
                });
            });
            return 1;
        }

        private void ScanPorts(PortScanSettings settings, StatusContext ctx, Action<int, bool> consoleCallback) {
            var startTime = DateTime.Now;
            var scanningPorts = GeneratePortScanArray(settings);
            for (var portIndex = 0; portIndex < scanningPorts.Length;) {
                var scansPerSecond = Math.Round(portIndex / DateTime.Now.Subtract(startTime).TotalSeconds, 1);
                if (settings.RateLimit > 0) {
                    ctx.Spinner(Spinner.Known.CircleHalves);
                    Thread.Sleep(TimeSpan.FromSeconds((int)(scansPerSecond / settings.RateLimit!)));
                }
                else {
                    ctx.Spinner(Spinner.Known.Dots3);
                }

                var runningRequests = ScanChunkConcurrently(settings, ctx, consoleCallback, scanningPorts, portIndex, scansPerSecond);
                portIndex += runningRequests.Length;
            }
        }

        // TODO: Clean up this code!
        private Task[] ScanChunkConcurrently(PortScanSettings settings, StatusContext ctx, Action<int, bool> consoleCallback, int[] scanningPorts, int portIndex, double scansPerSecond) {
            var taskCount = settings.RateLimit < settings.Concurrent ? (int)settings.RateLimit! : settings.Concurrent;
            taskCount = settings.RateLimit == 0 ? settings.Concurrent : taskCount;
            taskCount = scanningPorts.Length - portIndex < taskCount ? scanningPorts.Length - portIndex : taskCount;
            taskCount = taskCount == 0 ? 1 : taskCount;

            var runningRequests = new Task[taskCount ?? 0];
            for (var x = 0; x < taskCount; x++) {
                var port = scanningPorts[portIndex + x];
                var runningTask = new TaskFactory().StartNew(async () => {
                    ctx.Status($"({scansPerSecond}/sec) Scanning {port}");

                    var state = await _scanningService.ScanPort(settings.Host!, port);
                    consoleCallback.Invoke(port, state);
                });
                runningRequests[x] = runningTask;
            }

            Task.WaitAll(runningRequests);
            return runningRequests;
        }

        private static int[] GeneratePortScanArray(PortScanSettings settings) {
            var ports = new List<int>();
            if (settings.Port == "*") {
                for (var x = 0; x < MAX_PORTS; x++) {
                    ports.Add(x);
                }
            }
            else {
                foreach (var port in settings.Ports) {
                    ports.Add(port);
                }
            }

            return ports.ToArray();
        }
    }
}
