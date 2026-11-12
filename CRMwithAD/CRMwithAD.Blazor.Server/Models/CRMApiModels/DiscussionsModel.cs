namespace CRMwithAD.Blazor.Server.Models.CRMApiModels
{
    public class DiscussionsModel
    {
        public string Comment { get; set; }
        public string Attachment { get; set; }
        public string AttachmentName { get; set; }
        public string RepliedBy { get; set; }
        public string RepliedByOid { get; set; }
        public string PostedBy { get; set; }
        public string time { get; set; }
    }
    public class DiscussionRequest
    {
        public string CaseID { get; set; }
        public string ContactID { get; set; }
        public string Replay { get; set; }
        public IFormFile Attachment { get; set; } = null;
    }
}
