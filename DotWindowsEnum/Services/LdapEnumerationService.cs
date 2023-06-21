
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Novell.Directory.Ldap;

namespace DotWindowsEnum.Services {
    public class LdapEnumerationService : ILdapEnumerationService {
        public LdapEnumerationService() {

        }

        public LdapConnectionOptions GetValidConnectionOptions(string ipAddress, int port) {
            var options = new LdapConnectionOptions();
            options.ConfigureRemoteCertificateValidationCallback(CheckCert);

            try {
                using var connectionNoSsl = new LdapConnection(options);
                connectionNoSsl.Connect(ipAddress, port);
                connectionNoSsl.WhoAmI();
                return options;
            }
            catch {
                options.UseSsl();
            }

            var connection = new LdapConnection(options);
            connection.Connect(ipAddress, port);
            connection.WhoAmI();
            return options;
        }

        private bool CheckCert(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) {
            return true;
        }

        public RootDseInfo GetRootInfo(string ipAddress, int port) {
            var options = GetValidConnectionOptions(ipAddress, port);
            using var connection = new LdapConnection(options);
            connection.Connect(ipAddress, port);

            if (SupportsNullCredentials(ipAddress, port)) {
                connection.Bind(string.Empty, string.Empty);
            }

            return connection.GetRootDseInfo();
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
