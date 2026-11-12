namespace CRMwitAD.Shared.Models
{
    public class LicenseInfo
    {
        public string LicensedTo { get; set; }    // e.g. company name
        public DateTime ExpiryUtc { get; set; }
    }
}
