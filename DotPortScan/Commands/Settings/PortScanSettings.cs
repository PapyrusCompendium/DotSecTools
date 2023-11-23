using System.ComponentModel;
using System.Text;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotPortScan.Commands.Settings {
    public class PortScanSettings : CommandSettings {
        [CommandOption("-h|--host")]
        [Description("The host to scan.")]
        public string? Host { get; set; }

        [CommandOption("--ratelimit")]
        [Description("Maximum rate of requests per second, 0 mean no maximum. (Caution)")]
        [DefaultValue(0)]
        public int? RateLimit { get; set; }

        [CommandOption("-t|--timeout")]
        [Description("Maximum number of milliseconds to wait for a port to respond. (Caution)")]
        [DefaultValue(500)]
        public int Timeout { get; set; }

        [CommandOption("-c|--concurrent")]
        [Description("How many requests to send concurrently (Caution)")]
        [DefaultValue(1500)]
        public int? Concurrent { get; set; }

        [CommandOption("-p|--port")]
        [Description("Ports to scan on the host, comma seperated.")]
        [DefaultValue("20,21,22,53,80,123,139,135,179,443,464,500,587,636,3389,3268,3269,5985,47001")]
        public string? Port { get; set; }
        public int[] Ports { get; set; } = Array.Empty<int>();

        public override ValidationResult Validate() {
            var errorMessage = new StringBuilder();

            if (string.IsNullOrWhiteSpace(Host)) {
                errorMessage.AppendLine("The host cannot be emtpy!");
            }

            if (string.IsNullOrWhiteSpace(Port)) {
                errorMessage.AppendLine("Ports cannot be empty!");
            }

            if (!string.IsNullOrWhiteSpace(Port) && Port != "*") {
                var stringPorts = Port!.Split(",");
                Ports = stringPorts.Select(stringValue => {
                    stringValue = stringValue.Trim();
                    if (int.TryParse(stringValue, out var parsedInt)) {
                        return parsedInt;
                    }
                    errorMessage.AppendLine($"Could not parse '{stringValue}' as an integer value.");
                    return 0;
                }).ToArray();
            }

            if (Timeout == 0) {
                errorMessage.AppendLine($"Timeout cannot be {Timeout}!");
            }

            if (!string.IsNullOrWhiteSpace(errorMessage.ToString())) {
                return ValidationResult.Error(errorMessage.ToString());
            }
            return ValidationResult.Success();
        }
    }
}
