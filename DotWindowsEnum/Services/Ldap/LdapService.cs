using Novell.Directory.Ldap;

namespace DotWindowsEnum.Services.Ldap {
    public class LdapService : ILdapService {
        private readonly ILdapConnectionService _ldapConnectionService;

        public LdapService(ILdapConnectionService ldapConnectionService) {
            _ldapConnectionService = ldapConnectionService;
        }

        private LdapEntry[] QueryLdap(string ipAddress, int port, string query, int scope = 2) {
            var options = _ldapConnectionService.GetValidConnectionOptions(ipAddress, port);
            using var connection = new LdapConnection(options);
            connection.Connect(ipAddress, port);

            var ldapSearchResults = connection.Search(string.Empty,
                scope, query, new string[1] { "*" }, typesOnly: false);

            var startWaiting = DateTime.Now.AddSeconds(3);
            while (!ldapSearchResults.HasMore() || DateTime.Now < startWaiting) {
            }

            try {
                return ldapSearchResults.ToArray();
            }
            catch {
                return Array.Empty<LdapEntry>();
            }
        }

        public LdapEntry[] GetAllUserAccounts(string ipAddress, int port) {
            return QueryLdap(ipAddress, port, "(objectCategory=person)(objectClass=user)");
        }

        public LdapEntry[] GetAllDomainMachines(string ipAddress, int port) {
            return QueryLdap(ipAddress, port, "(objectCategory=computer)");
        }

        public LdapEntry[] GetRootLevelObjects(string ipAddress, int port) {
            return QueryLdap(ipAddress, port, "(objectClass=*)", 0);
        }

        public bool SupportsNullCredentials(string ipAddress, int port) {
            return ValidCredentials(ipAddress, port, string.Empty, string.Empty);
        }

        public bool ValidCredentials(string ipAddress, int port, string username, string password) {
            var options = _ldapConnectionService.GetValidConnectionOptions(ipAddress, port);
            using var connection = new LdapConnection(options);
            connection.Connect(ipAddress, port);
            try {
                connection.Bind(username, password);
            }
            catch {
                return false;
            }
            return connection.Bound;
        }
    }
}
