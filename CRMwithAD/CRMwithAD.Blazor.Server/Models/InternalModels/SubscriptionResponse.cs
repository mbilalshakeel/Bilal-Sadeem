namespace CRMwithAD.Blazor.Server.Models.InternalModels
{
    public class SubscriptionResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<CMP_Plans> data { get; set; } = null;
    }

    public class CMP_Plans
    {
        public string planName { get; set; }
        public string provisionType { get; set; }
        public int cores { get; set; }
        public int memoryGB { get; set; }
        public int storageGB { get; set; }
        public string price { get; set; }
    }
}