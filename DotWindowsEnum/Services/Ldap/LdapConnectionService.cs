using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

using Novell.Directory.Ldap;

namespace DotWindowsEnum.Services.Ldap {
    public class LdapConnectionService : ILdapConnectionService {
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
    }
}
