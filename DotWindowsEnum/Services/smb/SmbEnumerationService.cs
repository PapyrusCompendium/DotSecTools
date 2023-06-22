
using SMBLibrary;
using SMBLibrary.Client;

namespace DotWindowsEnum.Services {
    public class SmbEnumerationService : ISmbEnumerationService {
        public NTStatus SupportsNullCredentials(string address, string domain = "") {
            ISMBClient smbClient = IsVersionOne(address) ? new SMB1Client() : new SMB2Client();
            var connected = smbClient.Connect(address, SMBTransportType.DirectTCPTransport);
            if (!connected) {
                return 0;
            }

            return smbClient.Login(domain, string.Empty, string.Empty);
        }

        public ISMBClient OpenSmbConnection(string address) {
            ISMBClient smbClient = IsVersionOne(address) ? new SMB1Client() : new SMB2Client();
            var connected = smbClient.Connect(address, SMBTransportType.DirectTCPTransport);
            if (!connected) {
                return null!;
            }

            return smbClient;
        }

        public bool IsVersionOne(string address) {
            var client = new SMB1Client();
            var connected = client.Connect(address, SMBTransportType.DirectTCPTransport);
            return connected;
        }

        public bool IsVersionTwo(string address) {
            var client = new SMB2Client();
            var connected = client.Connect(address, SMBTransportType.DirectTCPTransport);
            return connected;
        }
    }
}
