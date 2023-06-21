using SMBLibrary;
using SMBLibrary.Client;

namespace DotWindowsEnum.Services {
    public interface ISmbEnumerationService {
        bool IsVersionOne(string address);
        bool IsVersionTwo(string address);
        ISMBClient OpenSmbConnection(string address);
        NTStatus SupportsNullCredentials(string address, string domain = "");
    }
}