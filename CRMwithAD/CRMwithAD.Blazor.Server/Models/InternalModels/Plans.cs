namespace CRMwithAD.Blazor.Server.Models.InternalModels
{
    public class Config
    {
    }

    public class PlansMeta
    {
        public int offset { get; set; }
        public int max { get; set; }
        public int size { get; set; }
        public int total { get; set; }
    }

    public class PlanPriceSet
    {
        public int id { get; set; }
        //public string name { get; set; }
        //public string code { get; set; }
        public string priceUnit { get; set; }
    }

    public class ProvisionType
    {
        public int id { get; set; }
        //public string name { get; set; }
        //public string code { get; set; }
        //public bool rootDiskCustomizable { get; set; }
        //public bool addVolumes { get; set; }
        //public bool customizeVolume { get; set; }
        //public bool hasConfigurableCpuSockets { get; set; }
    }

    public class Plans
    {
        public List<ServicePlan> servicePlans { get; set; }
        public PlansMeta meta { get; set; }
    }

    public class ServicePlan
    {
        public int id { get; set; }
        public string name { get; set; }
        //public string code { get; set; }
        public bool active { get; set; }
        //public int sortOrder { get; set; }
        public string description { get; set; }
        public long? maxStorage { get; set; }
        public long? maxMemory { get; set; }
        public long? maxCpu { get; set; }
        public long? maxCores { get; set; }
        //public object maxDisks { get; set; }
        //public int coresPerSocket { get; set; }
        //public bool customCpu { get; set; }
        //public bool customCores { get; set; }
        //public bool customMaxStorage { get; set; }
        //public bool customMaxDataStorage { get; set; }
        //public bool customMaxMemory { get; set; }
        //public bool addVolumes { get; set; }
        //public object memoryOptionSource { get; set; }
        //public object cpuOptionSource { get; set; }
        //public DateTime dateCreated { get; set; }
        //public DateTime lastUpdated { get; set; }
        //public object regionCode { get; set; }
        //public string visibility { get; set; }
        //public bool editable { get; set; }
        public ProvisionType provisionType { get; set; } //key
        public string tenants { get; set; }
        public List<PlanPriceSet> priceSets { get; set; }
        public Config config { get; set; }
        public List<PlanZone> zones { get; set; }
    }

    public class PlanZone
    {
        public int id { get; set; }  //key
        //public string name { get; set; }
        //public string code { get; set; }
    }
}
