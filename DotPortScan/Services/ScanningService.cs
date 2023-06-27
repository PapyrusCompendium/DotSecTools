using System.Net.Sockets;

namespace DotPortScan.Services {
    public class ScanningService : IScanningService {
        public async Task<bool> ScanPort(string host, int port, CancellationToken cancellationToken) {
            using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.LingerState = new LingerOption(false, 0);

            try {
                await socket.ConnectAsync(host, port, cancellationToken);
                return true;
            }
            catch {
            }
            finally {
                socket.Close();
                socket.Dispose();
            }

            return false;
        }
    }
}
