namespace CRMwithAD.Blazor.Server.Models.InternalModels
{
    public class UserSubscriptionsResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<UserSubscriptions> data { get; set; } = null;
    }

    public class UserSubscriptions
    {
        public string planName { get; set; }
        public List<string> tenants { get; set; } = null;
    }
}