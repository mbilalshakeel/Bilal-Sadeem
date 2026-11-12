namespace CRMwithAD.Blazor.Server.Models.InternalModels
{
    public class Accounts
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Cloud
    {
        public int id { get; set; }
        public string name { get; set; }
        public string uuid { get; set; }
    }

    public class invoicesGroup
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Instance
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Invoices
    {
        public int id { get; set; }
        //public int ownerId { get; set; }
        public Accounts account { get; set; }
        //public invoicesGroup group { get; set; }
        //public Cloud cloud { get; set; }
        //public Instance instance { get; set; }
        //public Server server { get; set; }
        //public object cluster { get; set; }
        //public User user { get; set; }
        public Plan plan { get; set; }
        //public List<Tag> tags { get; set; }
        //public object project { get; set; }
        //public string refType { get; set; }
        //public int refId { get; set; }
        //public string refUuid { get; set; }
        //public string refName { get; set; }
        //public string refCategory { get; set; }
        //public object resourceId { get; set; }
        //public object resourceUuid { get; set; }
        //public object resourceType { get; set; }
        //public object resourceName { get; set; }
        //public string resourceExternalId { get; set; }
        //public object resourceInternalId { get; set; }
        //public string interval { get; set; }
        //public string period { get; set; }
        //public bool estimate { get; set; }
        //public bool summaryInvoice { get; set; }
        //public bool active { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        //public DateTime refStart { get; set; }
        //public DateTime refEnd { get; set; }
        //public double estimatedComputePrice { get; set; }
        //public double estimatedComputeCost { get; set; }
        //public double estimatedMemoryPrice { get; set; }
        //public double estimatedMemoryCost { get; set; }
        //public double estimatedStoragePrice { get; set; }
        //public double estimatedStorageCost { get; set; }
        //public double estimatedNetworkPrice { get; set; }
        //public double estimatedNetworkCost { get; set; }
        //public double estimatedLicensePrice { get; set; }
        //public double estimatedLicenseCost { get; set; }
        //public double estimatedSoftwareOrServicePrice { get; set; }
        //public double estimatedSoftwareOrServiceCost { get; set; }
        //public double estimatedExtraPrice { get; set; }
        //public double estimatedExtraCost { get; set; }
        //public double estimatedTotalPrice { get; set; }
        //public double estimatedTotalCost { get; set; }
        //public double estimatedRunningPrice { get; set; }
        //public double estimatedRunningCost { get; set; }
        //public string estimatedCurrency { get; set; }
        //public double estimatedConversionRate { get; set; }
        //public double actualComputePrice { get; set; }
        //public double actualComputeCost { get; set; }
        //public double actualMemoryPrice { get; set; }
        //public double actualMemoryCost { get; set; }
        //public double actualStoragePrice { get; set; }
        //public double actualStorageCost { get; set; }
        //public double actualNetworkPrice { get; set; }
        //public double actualNetworkCost { get; set; }
        //public double actualLicensePrice { get; set; }
        //public double actualLicenseCost { get; set; }
        //public double actualSoftwareOrServicePrice { get; set; }
        //public double actualSoftwareOrServiceCost { get; set; }
        //public double actualExtraPrice { get; set; }
        //public double actualExtraCost { get; set; }
        //public double actualTotalPrice { get; set; }
        //public double actualTotalCost { get; set; }
        //public double actualRunningPrice { get; set; }
        //public double actualRunningCost { get; set; }
        //public string actualCurrency { get; set; }
        //public double actualConversionRate { get; set; }
        //public double computePrice { get; set; }
        //public double computeCost { get; set; }
        //public double memoryPrice { get; set; }
        //public double memoryCost { get; set; }
        //public double storagePrice { get; set; }
        //public double storageCost { get; set; }
        //public double networkPrice { get; set; }
        //public double networkCost { get; set; }
        //public double licensePrice { get; set; }
        //public double licenseCost { get; set; }
        //public double softwareOrServicePrice { get; set; }
        //public double softwareOrServiceCost { get; set; }
        //public double extraPrice { get; set; }
        //public double extraCost { get; set; }
        public double totalPrice { get; set; }
        //public double totalCost { get; set; }
        //public double runningPrice { get; set; }
        //public double runningCost { get; set; }
        //public string currency { get; set; }
        //public double conversionRate { get; set; }
        //public string costType { get; set; }
        //public object offTime { get; set; }
        //public string powerState { get; set; }
        //public DateTime powerDate { get; set; }
        //public double runningMultiplier { get; set; }
        //public object usageType { get; set; }
        //public object usageCategory { get; set; }
        //public DateTime? lastCostDate { get; set; }
        //public object lastActualDate { get; set; }
        //public DateTime dateCreated { get; set; }
        //public DateTime lastUpdated { get; set; }
        //public int lineItemCount { get; set; }
    }

    public class CMPInvoicesMeta
    {
        public long offset { get; set; }
        public long max { get; set; }
        public long size { get; set; }
        public long total { get; set; }
    }

    public class Plan
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class CMPInvoices
    {
        public List<Invoices> invoices { get; set; }
        public bool masterAccount { get; set; }
        public CMPInvoicesMeta meta { get; set; }
    }

    //public class Server
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    //public class Tag
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //    public object value { get; set; }
    //}

    //public class User
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}
    public class CRMInvoice
    {
        public string tenantName { get; set; }
        public string invoiceNumber { get; set; }
        public string invoiceDate { get; set; }
        public string amount { get; set; }
        public string status { get; set; }
    }
}
