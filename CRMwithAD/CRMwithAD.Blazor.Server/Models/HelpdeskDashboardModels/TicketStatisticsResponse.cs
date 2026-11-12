namespace CRMwithAD.Blazor.Server.Models.HelpdeskDashboardModels
{
    public class TicketStatisticsResponse
    {
        public string UniversityName { get; set; }
        public int TotalTickets { get; set; }
        public int AssignedTickets { get; set; }
        public int UnassignedTickets { get; set; }
        public PriorityCounts PriorityCounts { get; set; }
        public StatusCounts StatusCounts { get; set; }
        public List<CategoryCount> CategoryCounts { get; set; }
        public List<UniversityCount> UniversityCounts { get; set; }
        public List<TechnicianCount> TechnicianCounts { get; set; }
    }

    /// <summary>
    /// Enhanced V2 Response with comprehensive metrics for both cumulative and individual account reports
    /// </summary>
    public class TicketStatisticsResponseV2
    {
        // Report Type Info
        public string ReportType { get; set; } // "Cumulative" or "Individual"
        public string UniversityName { get; set; }
        public string UniversityOid { get; set; }

        // Account Overview
        public int TotalAccounts { get; set; }
        public int TotalTickets { get; set; }
        public int TotalOpen { get; set; }
        public int TotalResolved { get; set; }
        public int TotalFailed { get; set; }
        public int TotalHold { get; set; }
        public int AssignedTickets { get; set; }
        public int UnassignedTickets { get; set; }

        // Priority Distribution
        public PriorityCounts PriorityCounts { get; set; }

        // Status Distribution
        public StatusCounts StatusCounts { get; set; }

        // Category Breakdown
        public List<CategoryCountV2> CategoryCounts { get; set; }
        public List<SubCategoryCountV2> SubCategoryCounts { get; set; }

        // Technician Performance
        public List<TechnicianCountV2> TechnicianCounts { get; set; }

        // Resolution Time Metrics
        public ResolutionTimeMetrics ResolutionTimeMetrics { get; set; }

        // Top Items
        public List<TopCategoryItem> TopCategories { get; set; }
        public List<TopSubCategoryItem> TopSubCategories { get; set; }
        public List<TopTicketItem> TopTicketsMaxResolution { get; set; }
        public List<TopTicketItem> TopTicketsMinResolution { get; set; }

        // Monthly Statistics
        public List<MonthlyTicketCount> MonthlyTicketCounts { get; set; }

        // Peak/Hot Day (Day with most tickets)
        public DailyTicketCount PeakDay { get; set; }

        // For Cumulative Reports Only
        public List<UniversityCountV2> UniversityCounts { get; set; }
    }

    public class PriorityCounts
    {
        public int Severe { get; set; }
        public int Critical { get; set; }
        public int High { get; set; }
        public int Moderate { get; set; }
    }

    public class StatusCounts
    {
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Resolved { get; set; }
        public int Failed { get; set; }
    }

    public class CategoryCount
    {
        public string CategoryName { get; set; }
        public int Count { get; set; }
    }

    public class CategoryCountV2
    {
        public string CategoryName { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class SubCategoryCountV2
    {
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class UniversityCount
    {
        public string UniversityName { get; set; }
        public int Count { get; set; }
    }

    public class UniversityCountV2
    {
        public string UniversityName { get; set; }
        public string UniversityOid { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedTickets { get; set; }
    }

    public class TechnicianCount
    {
        public string TechnicianName { get; set; }
        public int Count { get; set; }
    }

    public class TechnicianCountV2
    {
        public string TechnicianName { get; set; }
        public int TotalAssigned { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public double AverageResolutionTimeHours { get; set; }
        public double ResolutionRate { get; set; } // Percentage of resolved tickets
    }

    public class ResolutionTimeMetrics
    {
        public double AverageResolutionTimeHours { get; set; }
        public double AverageResolutionTimeDays { get; set; }
        public double MaxResolutionTimeHours { get; set; }
        public double MaxResolutionTimeDays { get; set; }
        public double MinResolutionTimeHours { get; set; }
        public double MinResolutionTimeMinutes { get; set; }
        public double AverageFirstResponseTimeHours { get; set; }
        public int ResolvedTicketsCount { get; set; }
    }

    public class TopCategoryItem
    {
        public string CategoryName { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TopSubCategoryItem
    {
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TopTicketItem
    {
        public string TicketId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string UniversityName { get; set; }
        public string Priority { get; set; }
        public string Technician { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ResolvedAt { get; set; }
        public double ResolutionTimeHours { get; set; }
        public string FormattedResolutionTime { get; set; }
    }

    public class MonthlyTicketCount
    {
        public string Month { get; set; } // Format: "2025-08"
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int FailedTickets { get; set; }
        public int HoldTickets { get; set; }
    }

    public class DailyTicketCount
    {
        public string Date { get; set; } // Format: "2025-08-15"
        public string DayOfWeek { get; set; } // "Monday", "Tuesday", etc.
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int FailedTickets { get; set; }
        public int HoldTickets { get; set; }
    }

    public class AssignmentCount
    {
        public string Assignment { get; set; }
        public int Count { get; set; }
    }

}
