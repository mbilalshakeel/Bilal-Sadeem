namespace CRMwithAD.Blazor.Server.Models.CRMApiModels
{
    [Serializable]
    public class ActiveCases
    {
        public string Oid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Status { get; set; }
        public string Account { get; set; }
        public string TicketDate { get; set; }
        public string ClosingDate { get; set; }
        public string TicketId { get; set; }
        public string Priority { get; set; }
        public string Attachment { get; set; }
        public string AttachmentName { get; set; }
    }
    public class CaseRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubjectOid { get; set; }
        public string SubCategoryOid { get; set; }
        public string AccountOid { get; set; }
        public string ContactOid { get; set; }
        public string? Priority { get; set; }
        public IFormFile? Attachment { get; set; } = null;
    }
    public class CaseRequestV2
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubjectOid { get; set; }
        public string AccountOid { get; set; }
        public string ContactOid { get; set; }
        public string Attachment { get; set; }
        public string Priority { get; set; }

    }
}
