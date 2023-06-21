using Novell.Directory.Ldap;

namespace DotWindowsEnum.Services {
    public interface ILdapEnumerationService {
        bool AnonymousInformationLeaking(string ipAddress, int port);
        LdapEntry[] GetAllDomainMachines(string ipAddress, int port);
        LdapEntry[] GetAllUserAccounts(string ipAddress, int port);
        RootDseInfo GetRootInfo(string ipAddress, int port);
        LdapConnectionOptions GetValidConnectionOptions(string ipAddress, int port);
        bool SupportsNullCredentials(string ipAddress, int port);
        bool ValidCredentials(string ipAddress, int port, string username, string password);
    }
}