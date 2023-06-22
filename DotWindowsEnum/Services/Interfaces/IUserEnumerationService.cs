using DotWindowsEnum.Commands.Settings;

using Spectre.Console;

namespace DotWindowsEnum.Services.Ldap {
    public interface IUserEnumerationService {
        void EnumerateUsers(LdapScanSettings settings, Tree rootNode);
    }
}