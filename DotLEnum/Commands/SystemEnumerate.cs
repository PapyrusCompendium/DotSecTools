using System.Diagnostics.CodeAnalysis;

using DotLEnum.Commands.Settings;

using Spectre.Console.Cli;

namespace DotLEnum.Commands {
    public class SystemEnumerate : Command<SystemEnumerateSettings> {
        public SystemEnumerate() {

        }
        public override int Execute([NotNull] CommandContext context, [NotNull] SystemEnumerateSettings settings) {

            return 1;
        }
    }
}
