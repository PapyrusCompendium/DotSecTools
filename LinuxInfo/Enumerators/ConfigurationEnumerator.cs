using LinuxInfo.Enumerators.DefaultFileDefinitions;
using LinuxInfo.Logging;
using LinuxInfo.SystemConfigurations;

namespace LinuxInfo.Enumerators {
    public class ConfigurationEnumerator {
        public List<string> DefaultConfigurables { get; set; } = new();
        public List<string> SpecialConfigurables { get; set; } = new();
        public PasswdConfig Passwd { get; set; } = null;


        private readonly ConsoleLoggerService _logger;

        public ConfigurationEnumerator(ConsoleLoggerService consoleLoggerService) {
            _logger = consoleLoggerService;
        }

        public ConfigurationEnumerator Enumerate() {
            Passwd = new PasswdConfig("/etc/passwd");
            FindConfigurables();

            return this;
        }

        private void FindConfigurables() {
            var configDirectory = new DirectoryInfo("/etc");
            var allconfigDirectories = configDirectory.GetDirectories()
                .OrderByDescending(i => i.LastWriteTimeUtc);

            foreach (var configurable in allconfigDirectories) {
                var pathName = configurable.FullName;

                if (Etc.DefaultConfigurables.ContainsKey(pathName)) {
                    DefaultConfigurables.Add(pathName);
                }
                else {
                    SpecialConfigurables.Add(pathName);
                }
            }
        }
    }
}
