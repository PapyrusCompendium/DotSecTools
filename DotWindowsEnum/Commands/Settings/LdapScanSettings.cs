using System.ComponentModel;
using System.Text;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotWindowsEnum.Commands.Settings {
    public class LdapScanSettings : CommandSettings {
        [CommandOption("-i|--ipAddress")]
        [Description("The Ip Address of the Ldap server you are enumerating.")]
        public string? ServerIp { get; set; }

        [CommandOption("-p|--port")]
        [Description("The port of the ldap service.")]
        [DefaultValue(636)]
        public int? LdapPort { get; set; }

        [CommandOption("-u|--username")]
        [Description("Authenticate with username.")]
        [DefaultValue("")]
        public string? Username { get; set; } = null;

        [CommandOption("-P|--password")]
        [Description("The password of the specified user.")]
        [DefaultValue("")]
        public string? Password { get; set; } = null;

        [CommandOption("-V|--verbose")]
        [Description("The verbocity of the output.")]
        [DefaultValue(false)]
        public bool? Verbose { get; set; } = null;

        [CommandOption("-d|--dump")]
        [Description("Dump all objects to a file.")]
        public string? DumpFile { get; set; } = null;

        [CommandOption("-f|--overwrite")]
        [Description("If Dump file exists, overwrite it.")]
        [DefaultValue(false)]
        public bool Overwrite { get; set; }

        public override ValidationResult Validate() {
            var errorMessage = new StringBuilder();

            if (ServerIp is null) {
                errorMessage.AppendLine($"The {nameof(ServerIp)} argument cannot be empty!");
            }

            if (!string.IsNullOrWhiteSpace(Username) && !Username!.Contains(@"\\")) {
                errorMessage.AppendLine(@"Username must be a distinguished name, ex: domain\\username");
            }

            if (!string.IsNullOrWhiteSpace(errorMessage.ToString())) {
                return ValidationResult.Error(errorMessage.ToString());
            }

            if (!string.IsNullOrWhiteSpace(DumpFile) && @File.Exists(DumpFile) && !Overwrite) {
                errorMessage.AppendLine("Specified dump file already exists!");
            }

            return ValidationResult.Success();
        }
    }
}
