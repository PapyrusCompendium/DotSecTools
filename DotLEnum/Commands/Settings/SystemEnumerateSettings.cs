using System.ComponentModel;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotLEnum.Commands.Settings {
    public class SystemEnumerateSettings : CommandSettings {
        [CommandOption("-f|--keyword")]
        [DefaultValue("FUZZ")]
        [Description("This will be replaced where ever it is used with words from the word list")]
        public string? FuzzKeyword { get; set; }


        public override ValidationResult Validate() {
            return ValidationResult.Success();
        }
    }
}
