namespace CRMwithAD.Blazor.Server.Models.CRMApiModels
{
    public class ActiveAccounts
    {
        public string oid { get; set; }
        public string Name { get; set; }
    }

    public class AccountCreationRequest
    {
        public string Name { get; set; }
    }

    public class AccountCreationResponse
    {
        public string oid { get; set; }
    }
}
