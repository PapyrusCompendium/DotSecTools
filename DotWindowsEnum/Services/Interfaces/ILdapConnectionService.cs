using Novell.Directory.Ldap;

namespace DotWindowsEnum.Services.Ldap {
    public interface ILdapConnectionService {
        LdapConnectionOptions GetValidConnectionOptions(string ipAddress, int port);
    }
}