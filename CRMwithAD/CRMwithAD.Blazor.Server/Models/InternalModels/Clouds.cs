namespace CRMwithAD.Blazor.Server.Models.InternalModels
{
    public class Clouds
    {
        public List<Zone> zones { get; set; }
        public Meta meta { get; set; }
    }

    public class Zone
    {
        public int id { get; set; }
        //public string uuid { get; set; }
        //public object externalId { get; set; }
        public string name { get; set; }
        //public string code { get; set; }
        //public List<object> labels { get; set; }
        //public string location { get; set; }
        ////public Owner owner { get; set; }
        //public int accountId { get; set; }
        ////public Account account { get; set; }
        //public string visibility { get; set; }
        //public bool enabled { get; set; }
        //public string status { get; set; }
        //public string statusMessage { get; set; }
        //public DateTime statusDate { get; set; }
        //public DateTime lastSync { get; set; }
        //public DateTime nextRunDate { get; set; }
        //public int lastSyncDuration { get; set; }
        //public string costStatus { get; set; }
        //public object costStatusMessage { get; set; }
        ////public DateTime costStatusDate { get; set; }
        ////public int costLastSyncDuration { get; set; }
        ////public DateTime costLastSync { get; set; }
        ////public ZoneType zoneType { get; set; }
        //public int zoneTypeId { get; set; }
        //public string guidanceMode { get; set; }
        //public string agentMode { get; set; }
        //public object userDataLinux { get; set; }
        //public object userDataWindows { get; set; }
        //public string consoleKeymap { get; set; }
        //public string costingMode { get; set; }
        //public object serviceVersion { get; set; }
        //public string securityMode { get; set; }
        //public string inventoryLevel { get; set; }
        //public string timezone { get; set; }
        //public object apiProxy { get; set; }
        //public object provisioningProxy { get; set; }
        ////public NetworkDomain networkDomain { get; set; }
        //public string domainName { get; set; }
        //public string regionCode { get; set; }
        //public bool autoRecoverPowerState { get; set; }
        //public int scalePriority { get; set; }
        ////public Config config { get; set; }
        ////public Credential credential { get; set; }
        //public object imagePath { get; set; }
        //public object darkImagePath { get; set; }
        //public DateTime dateCreated { get; set; }
        //public DateTime lastUpdated { get; set; }
        //public List<Group> groups { get; set; }
        //public object securityServer { get; set; }
        ////public NetworkServer networkServer { get; set; }
        ////public Stats stats { get; set; }
        //public int serverCount { get; set; }
    }
    //public class Group
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //    public int accountId { get; set; }
    //}

    public class Meta
    {
        public int offset { get; set; }
        public int max { get; set; }
        public int size { get; set; }
        public int total { get; set; }
    }
}
