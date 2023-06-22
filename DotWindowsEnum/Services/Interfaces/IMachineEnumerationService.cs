using DotWindowsEnum.Commands.Settings;

using Spectre.Console;

namespace DotWindowsEnum.Services.Ldap {
    public interface IMachineEnumerationService {
        void EnumerateMachines(LdapScanSettings settings, Tree rootNode);
    }
}