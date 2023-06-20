using System.ComponentModel;
using System.Text;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotWebFuzz.Commands.Settings {
    public class WebScanCommandSettings : CommandSettings {
        [CommandOption("-f|--keyword")]
        [DefaultValue("FUZZ")]
        [Description("This will be replaced where ever it is used with words from the word list")]
        public string? FuzzKeyword { get; set; }

        [CommandOption("-H|--header")]
        [Description("Add headers to the web request, you can fuzz 'host: FUZZ.domain.tld'")]
        public string[] Headers { get; set; } = Array.Empty<string>();

        [CommandOption("-d|--data")]
        [Description("Add form data to fuzz")]
        public string[] FormData { get; set; } = Array.Empty<string>();

        [CommandOption("-l|-w|--wordlist")]
        [Description("List of words to generate HTTP requests with")]
        public string? WordList { get; set; }

        [CommandOption("-u|--address")]
        [Description("The web address to fuzz")]
        public string? WebAddress { get; set; }

        [CommandOption("--ratelimit")]
        [Description("Maximum rate of requests per second, 0 mean no maximum. (Caution)")]
        [DefaultValue(200)]
        public int? RateLimit { get; set; }

        [CommandOption("-c|--concurrent")]
        [Description("How many http requests to send concurrently (Caution)")]
        [DefaultValue(10)]
        public int? Concurrent { get; set; }

        [CommandOption("--hc|--hidecodes")]
        [Description("Http response code that you would like to omit from results")]
        public int[] HideCodes { get; set; } = Array.Empty<int>();

        [CommandOption("--hs|--hidesize")]
        [Description("Http content length you would like to omit from results")]
        public ulong[] HideSizes { get; set; } = Array.Empty<ulong>();

        [CommandOption("-X|--method")]
        [Description("The http method you would like to send")]
        [DefaultValue("GET")]
        public string? HttpMethod { get; set; }

        public override ValidationResult Validate() {
            var errorMessageBuilder = new StringBuilder();
            if (string.IsNullOrWhiteSpace(WebAddress)) {
                errorMessageBuilder.AppendLine($"{nameof(WebAddress)} must not be empty!");
            }

            if (!(string.IsNullOrWhiteSpace(WebAddress) || WebAddress.Contains(FuzzKeyword!))
                && (Headers is null || !Headers.Any(i => i.Contains(FuzzKeyword!)))
                && (FormData is null || !FormData.Any(i => i.Contains(FuzzKeyword!)))) {

                errorMessageBuilder.AppendLine($"No '{FuzzKeyword}' keyword provided!");
            }

            if (string.IsNullOrWhiteSpace(WordList)) {

                errorMessageBuilder.AppendLine($"{nameof(WordList)} path cannot be empty!");
            }

            if (!File.Exists(WordList)) {
                errorMessageBuilder.AppendLine($"Path: {WordList} does not exist!");
            }

            if (Headers is not null && !Headers.All(i => i.Contains(":"))) {
                errorMessageBuilder.AppendLine($"Invalid headers!");
                foreach (var header in Headers.Where(i => !i.Contains(":"))) {
                    errorMessageBuilder.AppendLine(header);
                }
            }

            if (FormData is not null && !FormData.All(i => i.Contains(":"))) {
                errorMessageBuilder.AppendLine($"Invalid form data!");
            }

            var aggregateErrorMessage = errorMessageBuilder.ToString();
            if (string.IsNullOrWhiteSpace(aggregateErrorMessage)) {
                return ValidationResult.Success();
            }

            return ValidationResult.Error(aggregateErrorMessage);
        }
    }
}
