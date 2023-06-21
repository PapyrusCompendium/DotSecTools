
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

using Novell.Directory.Ldap;

namespace DotWindowsEnum.Services {
    public class LdapEnumerationService : ILdapEnumerationService {

        public LdapConnectionOptions GetValidConnectionOptions(string ipAddress, int port) {
            var options = new LdapConnectionOptions();
            options.ConfigureRemoteCertificateValidationCallback(CheckCert);

            try {
                using var connectionNoSsl = new LdapConnection(options);
                connectionNoSsl.Connect(ipAddress, port);
                connectionNoSsl.Bind(string.Empty, string.Empty);
                return options;
            }
            catch (InterThreadException threadException) {
                if (threadException.InnerException?.InnerException is not SocketException) {
                    throw threadException;
                }
            }

            options.UseSsl();
            using var connection = new LdapConnection(options);
            connection.Connect(ipAddress, port);
            connection.Bind(string.Empty, string.Empty);
            if (connection.Connected) {
                return options;
            }

            return null!;
        }

        private bool CheckCert(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) {
            return true;
        }

        public RootDseInfo GetRootInfo(string ipAddress, int port) {
            var options = GetValidConnectionOptions(ipAddress, port);
            using var connection = new LdapConnection(options);
            connection.Connect(ipAddress, port);

            return connection.GetRootDseInfo();
        }

        public bool AnonymousInformationLeaking(string ipAddress, int port) {
            var ldapSearchResults = QueryLdap(ipAddress, port, "(objectClass=*)", 0);
            return ldapSearchResults.Length > 0;
        }

        private LdapEntry[] QueryLdap(string ipAddress, int port, string query, int scope = 2) {
            var options = GetValidConnectionOptions(ipAddress, port);
            using var connection = new LdapConnection(options);
            connection.Connect(ipAddress, port);

            var ldapSearchResults = connection.Search(string.Empty,
                scope, query, new string[1] { "*" }, typesOnly: false);

            var startWaiting = DateTime.Now.AddSeconds(5);
            while (!ldapSearchResults.HasMore() || DateTime.Now < startWaiting) {
            }

            return ldapSearchResults.ToArray();
        }

        public LdapEntry[] GetAllUserAccounts(string ipAddress, int port) {
            return QueryLdap(ipAddress, port, "(objectCategory=person)(objectClass=user)");
        }

        public LdapEntry[] GetAllDomainMachines(string ipAddress, int port) {
            return QueryLdap(ipAddress, port, "(objectCategory=computer)");
        }

        public bool SupportsNullCredentials(string ipAddress, int port) {
            return ValidCredentials(ipAddress, port, string.Empty, string.Empty);
        }

        public bool ValidCredentials(string ipAddress, int port, string username, string password) {
            var options = GetValidConnectionOptions(ipAddress, port);
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
