using Novell.Directory.Ldap;

namespace DotWindowsEnum.Services.Ldap {
    public interface ILdapService {
        LdapEntry[] GetAllDomainMachines(string ipAddress, int port);
        LdapEntry[] GetAllUserAccounts(string ipAddress, int port);
        LdapEntry[] GetRootLevelObjects(string ipAddress, int port);
        bool SupportsNullCredentials(string ipAddress, int port);
        bool ValidCredentials(string ipAddress, int port, string username, string password);
    }
}