namespace CRMwithAD.Blazor.Server.Models.CRMApiModels
{
    public class ActiveContacts
    {
        public string oid { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string accountoid { get; set; }
        public string email { get; set; }
        public string accountname { get; set; }
    }

    public class ContactCreationRequest
    {
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public string accountoid { get; set; }
    }

    public class ContactCreationResponse
    {
        public string oid { get; set; }
    }
}
