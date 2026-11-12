using CRMwithAD.Module.Models;

namespace CRMwithAD.Blazor.Server.Models.HelpdeskDashboardModels
{
    public class GetTicketsStatistics
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string UniversityOid { get; set; }
        public string AccountId { get; set; } // For V2: support individual or null for cumulative
    }

    public class GetTicketsData
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string UniversityName { get; set; }
        public string Category { get; set; }
        public Priority Priority { get; set; }
        public TicketStatus Status { get; set; }
    }
}
