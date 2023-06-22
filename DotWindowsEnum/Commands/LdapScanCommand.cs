
using System.Diagnostics.CodeAnalysis;

using DotWindowsEnum.Commands.Settings;
using DotWindowsEnum.Services;
using DotWindowsEnum.Services.Ldap;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotWindowsEnum.Commands {
    public class LdapScanCommand : Command<LdapScanSettings> {
        private const string DEEP_PINK = "deeppink4_2";
        private const string LIGHT_GREEN = "chartreuse1";
        private readonly ILdapEnumerationService _ldapEnumerationService;
        private readonly IUserEnumerationService _userEnumerationService;
        private readonly IMachineEnumerationService _machineEnumerationService;

        public LdapScanCommand(ILdapEnumerationService ldapEnumerationService,
            IUserEnumerationService userEnumerationService, IMachineEnumerationService machineEnumerationService) {
            _ldapEnumerationService = ldapEnumerationService;
            _userEnumerationService = userEnumerationService;
            _machineEnumerationService = machineEnumerationService;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] LdapScanSettings settings) {
            var rootNode = new Tree($"[orange3]Ldap Root Info ({settings.ServerIp})[/]");

            AnsiConsole.Status()
                .Start("Querying Ldap Service", ctx => {
                    AnsiConsole.Cursor.Show();
                    ctx.SpinnerStyle(Style.Parse("green"));
                    ctx.Spinner(Spinner.Known.Dots3);

                    _ldapEnumerationService.AppendAuthenticationInfo(settings, rootNode);
                    _ldapEnumerationService.AppendConnectionInfo(settings, rootNode);
                    if (!_ldapEnumerationService.AnonymousInformationLeaking(settings.ServerIp!, settings.LdapPort ?? 0)) {
                        rootNode.AddNode($"[{DEEP_PINK}]Cannot preform anonymous query![/]");
                        AnsiConsole.Write(rootNode);
                        return;
                    }

                    _userEnumerationService.EnumerateUsers(settings, rootNode);
                    _machineEnumerationService.EnumerateMachines(settings, rootNode);
                    _ldapEnumerationService.AppendDseInfo(settings, rootNode);

                    AnsiConsole.Write(rootNode);
                });
            return 1;
        }
    }
}
