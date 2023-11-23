using System.Diagnostics.CodeAnalysis;

using DotWindowsEnum.Commands.Settings;
using DotWindowsEnum.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotWindowsEnum.Commands {
    public class SmbScanCommand : Command<SmbScanCommandSettings> {
        private readonly ISmbEnumerationService _smbEnumerationService;

        public SmbScanCommand(ISmbEnumerationService smbEnumerationService) {
            _smbEnumerationService = smbEnumerationService;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] SmbScanCommandSettings settings) {
            var rootNode = new Tree($"[orange3]Server Message Block ({settings.ServerIp})[/]");

            var nullCredentials = _smbEnumerationService.SupportsNullCredentials(settings.ServerIp!);
            var subNode = rootNode.AddNode($"Authentication");
            subNode.AddNode($"Null Credentials: {nullCredentials}");

            if (!string.IsNullOrWhiteSpace(settings.Username) || !string.IsNullOrWhiteSpace(settings.Password)) {
                var smbConnection = _smbEnumerationService.OpenSmbConnection(settings.ServerIp!);
                var response = smbConnection.Login(string.Empty, settings.Username, settings.Password);
                var loginSuccess = response == SMBLibrary.NTStatus.STATUS_SUCCESS;

                subNode.AddNode($"User Credentials: {loginSuccess}")
                    .AddNode($"Nt Response: {response}");
            }
            EnumerateShares(settings, rootNode);

            AnsiConsole.Write(rootNode);
            return 1;
        }

        private void EnumerateShares(SmbScanCommandSettings settings, Tree rootNode) {
            var smbConnection = _smbEnumerationService.OpenSmbConnection(settings.ServerIp!);
            smbConnection.Login(settings.Domain, settings.Username, settings.Password);
            var shares = smbConnection.ListShares(out var listStatus);
            if (listStatus != SMBLibrary.NTStatus.STATUS_SUCCESS) {
                return;
            }

            foreach (var share in shares) {
                rootNode.AddNode($"File Share: ({share})");
            }
        }
    }
}
