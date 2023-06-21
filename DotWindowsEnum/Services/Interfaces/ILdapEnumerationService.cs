using Novell.Directory.Ldap;

namespace DotWindowsEnum.Services {
    public interface ILdapEnumerationService {
        RootDseInfo GetRootInfo(string ipAddress, int port);
        LdapConnectionOptions GetValidConnectionOptions(string ipAddress, int port);
        bool SupportsNullCredentials(string ipAddress, int port);
        bool ValidCredentials(string ipAddress, int port, string username, string password);
    }
}