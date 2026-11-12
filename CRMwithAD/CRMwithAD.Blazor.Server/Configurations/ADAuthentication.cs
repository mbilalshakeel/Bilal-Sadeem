namespace CRMwithAD.Blazor.Server.Configurations
{
    public class ADAuthentication
    {
        public string Server { get; set; } = null!;
        public int Port { get; set; }
        public bool UseLdaps { get; set; }
        public bool UseStartTls { get; set; }
        public string Domain { get; set; } = null!;
        public string BaseDN { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
