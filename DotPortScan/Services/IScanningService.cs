
namespace DotPortScan.Services {
    public interface IScanningService {
        Task<bool> ScanPort(string host, int port, CancellationToken cancellationToken);
    }
}