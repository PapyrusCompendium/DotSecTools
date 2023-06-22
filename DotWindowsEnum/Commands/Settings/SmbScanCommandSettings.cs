using System.ComponentModel;
using System.Text;

using SMBLibrary.Client;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotWindowsEnum.Commands.Settings {
    public class SmbScanCommandSettings : CommandSettings {
        [CommandOption("-i|--ipAddress")]
        [Description("The Ip Address of the smb server you are enumerating.")]
        public string? ServerIp { get; set; }

        [CommandOption("-p|--port")]
        [Description("The port of the smb service.")]
        [DefaultValue(445)]
        public int? SmbPort { get; set; }

        [CommandOption("-u|--username")]
        [Description("Authenticate with username.")]
        [DefaultValue("")]
        public string? Username { get; set; } = null;

        [CommandOption("-P|--password")]
        [Description("The password of the specified user.")]
        [DefaultValue("")]
        public string? Password { get; set; } = null;

        [CommandOption("-d|--domain")]
        [Description("The domain of the specified user.")]
        [DefaultValue("")]
        public string? Domain { get; set; } = null;

        public override ValidationResult Validate() {
            UpdatePortValuesInMemory();

            var errorMessage = new StringBuilder();
            if (ServerIp is null) {
                errorMessage.AppendLine($"The {nameof(ServerIp)} argument cannot be empty!");
            }

            if (!string.IsNullOrWhiteSpace(errorMessage.ToString())) {
                return ValidationResult.Error(errorMessage.ToString());
            }
            return ValidationResult.Success();
        }

        private void UpdatePortValuesInMemory() {
            //var portField = typeof(SMB1Client).GetField(nameof(SMB1Client.DirectTCPPort))!;
            //portField.SetValue(null, SmbPort);
            //portField = typeof(SMB1Client).GetField(nameof(SMB1Client.NetBiosOverTCPPort))!;
            //portField.SetValue(null, SmbPort);

            //portField = typeof(SMB2Client).GetField(nameof(SMB2Client.DirectTCPPort))!;
            //portField.SetValue(null, SmbPort);
            //portField = typeof(SMB2Client).GetField(nameof(SMB2Client.NetBiosOverTCPPort))!;
            //portField.SetValue(null, SmbPort);
        }
    }
}
