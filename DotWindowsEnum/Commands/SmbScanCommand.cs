using System.Diagnostics.CodeAnalysis;

using DotWindowsEnum.Commands.Settings;

using Spectre.Console.Cli;

namespace DotWindowsEnum.Commands {
    public class SmbScanCommand : Command<SmbScanCommandSettings> {
        public override int Execute([NotNull] CommandContext context, [NotNull] SmbScanCommandSettings settings) {

        }
    }
}
