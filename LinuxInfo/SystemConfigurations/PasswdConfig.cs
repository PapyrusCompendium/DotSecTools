namespace LinuxInfo.SystemConfigurations {
    public class PasswdConfig {
        public List<UserAccount> UserAccounts { get; private set; }

        public PasswdConfig(string filePath) {
            UserAccounts = new List<UserAccount>();
            ParseFile(filePath);
        }

        private void ParseFile(string filePath) {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines) {
                var fields = line.Split(':');
                if (fields.Length >= 7) {
                    UserAccounts.Add(new UserAccount {
                        Username = fields[0],
                        Password = fields[1],
                        Uid = int.Parse(fields[2]),
                        Gid = int.Parse(fields[3]),
                        Description = fields[4],
                        HomeDirectory = fields[5],
                        Shell = fields[6]
                    });
                }
            }
        }
    }
}
