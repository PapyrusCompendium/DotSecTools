namespace LinuxInfo.SystemConfigurations {
    public class UserAccount {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Uid { get; set; }
        public int Gid { get; set; }
        public string Description { get; set; }
        public string HomeDirectory { get; set; }
        public string Shell { get; set; }
    }
}
