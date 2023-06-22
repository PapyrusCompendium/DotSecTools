using DotWindowsEnum.Commands.Settings;

using Novell.Directory.Ldap;

using Spectre.Console;

namespace DotWindowsEnum.Services {
    public interface ILdapEnumerationService {
        bool AnonymousInformationLeaking(string ipAddress, int port);
        void AppendAuthenticationInfo(LdapScanSettings settings, Tree rootNode);
        void AppendConnectionInfo(LdapScanSettings settings, Tree rootNode);
        void AppendDseInfo(LdapScanSettings settings, Tree rootNode);
    }
}