using System.Net.Http.Headers;

using LinuxInfo.Enumerators;
using LinuxInfo.Logging;

namespace LinuxInfo {
    internal class Program {
        static void Main(string[] args) {
            var consoleLogger = new ConsoleLoggerService(LogLevel.Errors | LogLevel.Warnings | LogLevel.Info, "enumeration.txt");

            // Am I ever going to need configEnum again? Should I just make this static?
            var configEnum = new ConfigurationEnumerator(consoleLogger)
                .Enumerate();

        }
    }
}