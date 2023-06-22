using System.Net.Sockets;

namespace DotPortScan.Services {
    public class ScanningService : IScanningService {
        public ScanningService() {
        }

        public async Task<bool> ScanPort(string host, int port) {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try {
                await socket.ConnectAsync(host, port);
            }
            catch (SocketException) {
                return false;
            }
            return socket.Connected;
        }
    }

    public struct PortStatus {
        public bool open;
    }
}
