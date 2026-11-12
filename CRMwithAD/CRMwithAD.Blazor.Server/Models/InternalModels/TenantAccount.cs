namespace CRMwithAD.Blazor.Server.Models.InternalModels
{
    public class Accountss
    {
        public int id { get; set; }
        public string name { get; set; }
        //public object description { get; set; }
        //public object subdomain { get; set; }
        //public string currency { get; set; }
        //public object externalId { get; set; }
        //public string customerNumber { get; set; }
        //public object accountNumber { get; set; }
        //public object accountName { get; set; }
        //public bool active { get; set; }
        //public bool master { get; set; }
        //public Role role { get; set; }
        //public Stats stats { get; set; }
        //public DateTime dateCreated { get; set; }
        //public DateTime lastUpdated { get; set; }
    }

    public class TenantMeta
    {
        public int offset { get; set; }
        public int max { get; set; }
        public int size { get; set; }
        public int total { get; set; }
    }

    //public class Role
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //    public string authority { get; set; }
    //    public string description { get; set; }
    //}

    public class TenantAccounts
    {
        public List<Accountss> accounts { get; set; }
        public TenantMeta meta { get; set; }
    }

    //public class Stats
    //{
    //    public int instanceCount { get; set; }
    //    public int userCount { get; set; }
    //}
}
