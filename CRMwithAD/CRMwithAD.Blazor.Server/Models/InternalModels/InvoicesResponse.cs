namespace CRMwithAD.Blazor.Server.Models.InternalModels
{
    public class InvoicesResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<CRMInvoice> data { get; set; } = null;
    }
}