using System.ComponentModel;
using System.Text;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotWindowsEnum.Commands.Settings {
    public class SmbScanCommandSettings : CommandSettings {
        [CommandOption("-i|--ipAddress")]
        [Description("The Ip Address of the Ldap server you are enumerating.")]
        public string? ServerIp { get; set; }

        [CommandOption("-p|--port")]
        [Description("The port of the ldap service.")]
        [DefaultValue(636)]
        public int? LdapPort { get; set; }

        public override ValidationResult Validate() {
            var errorMessage = new StringBuilder();
            if (ServerIp is null) {
                errorMessage.AppendLine($"The {nameof(ServerIp)} argument cannot be empty!");
            }

            if (!string.IsNullOrWhiteSpace(errorMessage.ToString())) {
                return ValidationResult.Error(errorMessage.ToString());
            }
            return ValidationResult.Success();
        }
    }
}
