using CRMwithAD.Blazor.Server.Models.HelpdeskDashboardModels;
using CRMwithAD.Blazor.Server.Services;
using CRMwithAD.Module.BusinessObjects.Configurations;
using CRMwithAD.Module.BusinessObjects.CustomerManagement;
using CRMwithAD.Module.BusinessObjects.Helpdesk_Tickets;
using CRMwithAD.Module.BusinessObjects.HelpdeskTickets;
using CRMwithAD.Module.Models;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMwithAD.Blazor.Server.Controllers
{
    [Route("api/helpdesk")]
    [ApiController]
    [Authorize]
    public class HelpdeskDashboardApiController : ControllerBase
    {
        IObjectSpaceFactory objectSpaceFactory;
        public HelpdeskDashboardApiController(IObjectSpaceFactory objectSpaceFactory)
        {
            this.objectSpaceFactory = objectSpaceFactory;
        }

        [HttpPost("report")]
        public IActionResult GetTicketsStatistics(GetTicketsStatistics ticketsStatistics)
        {
            try
            {
                if (!DateTime.TryParse(ticketsStatistics.StartDate, out var startDate) ||
                    !DateTime.TryParse(ticketsStatistics.EndDate, out var endDate))
                {
                    return BadRequest("Invalid date format.");
                }

                using var objectSpace = objectSpaceFactory.CreateObjectSpace<Ticket>();

                // Base query filtered by date
                var ticketsQuery = objectSpace.GetObjectsQuery<Ticket>()
                    .Where(t => t.Date >= startDate && t.Date < endDate);

                // Filter by university if provided
                string universityName = null;
                if (!string.IsNullOrWhiteSpace(ticketsStatistics.UniversityOid))
                {
                    var account = objectSpace.FirstOrDefault<Account>(a => a.Oid.ToString() == ticketsStatistics.UniversityOid);
                    if (account == null)
                        return BadRequest("University not found.");

                    universityName = account.Name;
                    ticketsQuery = ticketsQuery.Where(t => t.Account.Oid.ToString() == ticketsStatistics.UniversityOid);
                }

                var tickets = ticketsQuery.ToList();

                var result = new TicketStatisticsResponse
                {
                    UniversityName = universityName, // null when global
                    TotalTickets = tickets.Count,
                    AssignedTickets = tickets.Count(t => t.Technician != null && !string.IsNullOrEmpty(t.Technician.Name)),
                    UnassignedTickets = tickets.Count(t => t.Technician == null || string.IsNullOrEmpty(t.Technician.Name)),
                    PriorityCounts = new PriorityCounts
                    {
                        Severe = tickets.Count(t => t.Priority == Priority.Severe),
                        Critical = tickets.Count(t => t.Priority == Priority.Critical),
                        High = tickets.Count(t => t.Priority == Priority.High),
                        Moderate = tickets.Count(t => t.Priority == Priority.Moderate)
                    },
                    StatusCounts = new StatusCounts
                    {
                        Open = tickets.Count(t => t.Status == TicketStatus.Open),
                        Hold = tickets.Count(t => t.Status == TicketStatus.Hold),
                        Resolved = tickets.Count(t => t.Status == TicketStatus.Resolved),
                        Failed = tickets.Count(t => t.Status == TicketStatus.Failed)
                    },
                    CategoryCounts = tickets
                        .Where(t => t.Category != null)
                        .GroupBy(t => t.Category.Title)
                        .Select(g => new CategoryCount { CategoryName = g.Key, Count = g.Count() })
                        .ToList(),
                    TechnicianCounts = tickets
                        .Where(t => t.Technician != null && !string.IsNullOrEmpty(t.Technician.Name))
                        .GroupBy(t => t.Technician.Name)
                        .Select(g => new TechnicianCount { TechnicianName = g.Key, Count = g.Count() })
                        .ToList(),
                    UniversityCounts = tickets
                        .Where(t => t.Account != null && !string.IsNullOrEmpty(t.Account.Name))
                        .GroupBy(t => t.Account.Name)
                        .Select(g => new UniversityCount { UniversityName = g.Key, Count = g.Count() })
                        .ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex, "HelpdeskDashboard/GetTicketsStatistics");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("report/v2")]
        public IActionResult GetTicketsStatisticsV2(GetTicketsStatistics request)
        {
            try
            {
                if (!DateTime.TryParse(request.StartDate, out var startDate) ||
                    !DateTime.TryParse(request.EndDate, out var endDate))
                {
                    return BadRequest("Invalid date format.");
                }

                using var objectSpace = objectSpaceFactory.CreateObjectSpace<Ticket>();

                // Determine if this is cumulative or individual account report
                bool isCumulativeReport = string.IsNullOrWhiteSpace(request.AccountId) && string.IsNullOrWhiteSpace(request.UniversityOid);
                string reportType = isCumulativeReport ? "Cumulative" : "Individual";

                // Base query filtered by date - compare only date part, ignoring time
                var ticketsQuery = objectSpace.GetObjectsQuery<Ticket>()
                    .Where(t => t.Date.Date >= startDate.Date && t.Date.Date <= endDate.Date);

                // Filter by account if provided (priority to AccountId, fallback to UniversityOid)
                string accountId = !string.IsNullOrWhiteSpace(request.AccountId) ? request.AccountId : request.UniversityOid;
                Account targetAccount = null;
                string universityName = null;

                if (!string.IsNullOrWhiteSpace(accountId))
                {
                    targetAccount = objectSpace.FirstOrDefault<Account>(a => a.Oid.ToString() == accountId);
                    if (targetAccount == null)
                        return BadRequest("Account not found.");

                    universityName = targetAccount.Name;
                    ticketsQuery = ticketsQuery.Where(t => t.Account.Oid.ToString() == accountId);
                }

                var tickets = ticketsQuery.ToList();

                // Calculate all metrics
                var result = new TicketStatisticsResponseV2
                {
                    ReportType = reportType,
                    UniversityName = universityName,
                    UniversityOid = accountId,

                    // Account Overview
                    TotalAccounts = isCumulativeReport ? objectSpace.GetObjectsQuery<Account>().Count() : 1,
                    TotalTickets = tickets.Count,
                    TotalOpen = tickets.Count(t => t.Status == TicketStatus.Open),
                    TotalResolved = tickets.Count(t => t.Status == TicketStatus.Resolved),
                    TotalFailed = tickets.Count(t => t.Status == TicketStatus.Failed),
                    TotalHold = tickets.Count(t => t.Status == TicketStatus.Hold),
                    AssignedTickets = tickets.Count(t => t.Technician != null && !string.IsNullOrEmpty(t.Technician.Name)),
                    UnassignedTickets = tickets.Count(t => (t.Technician == null || string.IsNullOrEmpty(t.Technician.Name)) &&
                                                            (t.Status == TicketStatus.Open || t.Status == TicketStatus.Hold)),

                    // Priority Distribution
                    PriorityCounts = new PriorityCounts
                    {
                        Severe = tickets.Count(t => t.Priority == Priority.Severe),
                        Critical = tickets.Count(t => t.Priority == Priority.Critical),
                        High = tickets.Count(t => t.Priority == Priority.High),
                        Moderate = tickets.Count(t => t.Priority == Priority.Moderate)
                    },

                    // Status Distribution
                    StatusCounts = new StatusCounts
                    {
                        Open = tickets.Count(t => t.Status == TicketStatus.Open),
                        Hold = tickets.Count(t => t.Status == TicketStatus.Hold),
                        Resolved = tickets.Count(t => t.Status == TicketStatus.Resolved),
                        Failed = tickets.Count(t => t.Status == TicketStatus.Failed)
                    },

                    // Category Breakdown
                    CategoryCounts = tickets
                        .Where(t => t.Category != null)
                        .GroupBy(t => t.Category.Title)
                        .Select(g => new CategoryCountV2
                        {
                            CategoryName = g.Key,
                            Count = g.Count(),
                            Percentage = Math.Round(tickets.Count > 0 ? (double)g.Count() / tickets.Count * 100 : 0, 2)
                        })
                        .OrderByDescending(x => x.Count)
                        .ToList(),

                    // Sub-Category Breakdown
                    SubCategoryCounts = tickets
                        .Where(t => t.Category != null && t.SubCategory != null)
                        .GroupBy(t => new { Category = t.Category.Title, SubCategory = t.SubCategory.Title })
                        .Select(g => new SubCategoryCountV2
                        {
                            CategoryName = g.Key.Category,
                            SubCategoryName = g.Key.SubCategory,
                            Count = g.Count(),
                            Percentage = Math.Round(tickets.Count > 0 ? (double)g.Count() / tickets.Count * 100 : 0, 2)
                        })
                        .OrderByDescending(x => x.Count)
                        .ToList(),

                    // Technician Performance
                    TechnicianCounts = tickets
                        .Where(t => t.Technician != null && !string.IsNullOrEmpty(t.Technician.Name))
                        .GroupBy(t => t.Technician.Name)
                        .Select(g => new TechnicianCountV2
                        {
                            TechnicianName = g.Key,
                            TotalAssigned = g.Count(),
                            OpenTickets = g.Count(t => t.Status == TicketStatus.Open),
                            ResolvedTickets = g.Count(t => t.Status == TicketStatus.Resolved),
                            AverageResolutionTimeHours = g.Where(t => t.Status == TicketStatus.Resolved && t.CloseDate > t.Date)
                                .Select(t => (t.CloseDate - t.Date).TotalHours)
                                .DefaultIfEmpty(0)
                                .Average(),
                            ResolutionRate = g.Count() > 0 ? Math.Round((double)g.Count(t => t.Status == TicketStatus.Resolved) / g.Count() * 100, 2) : 0
                        })
                        .OrderByDescending(x => x.ResolvedTickets)
                        .ToList(),

                    // Resolution Time Metrics
                    ResolutionTimeMetrics = CalculateResolutionTimeMetrics(tickets),

                    // Top Items
                    TopCategories = tickets
                        .Where(t => t.Category != null)
                        .GroupBy(t => t.Category.Title)
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .Select(g => new TopCategoryItem
                        {
                            CategoryName = g.Key,
                            Count = g.Count(),
                            Percentage = Math.Round(tickets.Count > 0 ? (double)g.Count() / tickets.Count * 100 : 0, 2)
                        })
                        .ToList(),

                    TopSubCategories = tickets
                        .Where(t => t.Category != null && t.SubCategory != null)
                        .GroupBy(t => new { Category = t.Category.Title, SubCategory = t.SubCategory.Title })
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .Select(g => new TopSubCategoryItem
                        {
                            CategoryName = g.Key.Category,
                            SubCategoryName = g.Key.SubCategory,
                            Count = g.Count(),
                            Percentage = Math.Round(tickets.Count > 0 ? (double)g.Count() / tickets.Count * 100 : 0, 2)
                        })
                        .ToList(),

                    TopTicketsMaxResolution = tickets
                        .Where(t => t.Status == TicketStatus.Resolved && t.CloseDate > t.Date)
                        .OrderByDescending(t => t.CloseDate - t.Date)
                        .Take(5)
                        .Select(t => new TopTicketItem
                        {
                            TicketId = t.TicketId,
                            Title = t.Title,
                            Category = t.Category?.Title ?? "N/A",
                            SubCategory = t.SubCategory?.Title ?? "N/A",
                            UniversityName = t.Account?.Name ?? "N/A",
                            Priority = t.Priority.ToString(),
                            Technician = t.Technician?.Name ?? "N/A",
                            Status = t.Status.ToString(),
                            CreatedAt = t.Date,
                            ResolvedAt = t.CloseDate,
                            ResolutionTimeHours = (t.CloseDate - t.Date).TotalHours,
                            FormattedResolutionTime = FormatResolutionTime(t.CloseDate - t.Date)
                        })
                        .ToList(),

                    TopTicketsMinResolution = tickets
                        .Where(t => t.Status == TicketStatus.Resolved && t.CloseDate > t.Date)
                        .OrderBy(t => t.CloseDate - t.Date)
                        .Take(5)
                        .Select(t => new TopTicketItem
                        {
                            TicketId = t.TicketId,
                            Title = t.Title,
                            Category = t.Category?.Title ?? "N/A",
                            SubCategory = t.SubCategory?.Title ?? "N/A",
                            UniversityName = t.Account?.Name ?? "N/A",
                            Priority = t.Priority.ToString(),
                            Technician = t.Technician?.Name ?? "N/A",
                            Status = t.Status.ToString(),
                            CreatedAt = t.Date,
                            ResolvedAt = t.CloseDate,
                            ResolutionTimeHours = (t.CloseDate - t.Date).TotalHours,
                            FormattedResolutionTime = FormatResolutionTime(t.CloseDate - t.Date)
                        })
                        .ToList(),

                    // Monthly Statistics
                    MonthlyTicketCounts = CalculateMonthlyTicketCounts(tickets),

                    // Peak/Hot Day (Day with most tickets)
                    PeakDay = CalculatePeakDay(tickets),

                    // For Cumulative Reports
                    UniversityCounts = isCumulativeReport ? tickets
                        .Where(t => t.Account != null && !string.IsNullOrEmpty(t.Account.Name))
                        .GroupBy(t => new { Name = t.Account.Name, Oid = t.Account.Oid })
                        .Select(g => new UniversityCountV2
                        {
                            UniversityName = g.Key.Name,
                            UniversityOid = g.Key.Oid.ToString(),
                            Count = g.Count(),
                            Percentage = Math.Round(tickets.Count > 0 ? (double)g.Count() / tickets.Count * 100 : 0, 2),
                            OpenTickets = g.Count(t => t.Status == TicketStatus.Open),
                            ResolvedTickets = g.Count(t => t.Status == TicketStatus.Resolved)
                        })
                        .OrderByDescending(x => x.Count)
                        .ToList()
                        : null
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex, $"HelpdeskDashboard/GetTicketsStatisticsV2 - StartDate: {request.StartDate}, EndDate: {request.EndDate}, AccountId: {request.AccountId}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("tickets")]
        public IActionResult GetTicketsData([FromQuery] string startDate = null, [FromQuery] string endDate = null)
        {
            try
            {
                var start = DateTime.TryParse(startDate, out var sd)
                    ? sd
                    : new DateTime(DateTime.UtcNow.Year, 1, 1);

                var end = DateTime.TryParse(endDate, out var ed)
                    ? ed
                    : new DateTime(DateTime.UtcNow.Year, 12, 31);

                if (start > end)
                    return BadRequest("StartDate cannot be greater than EndDate.");

                using var objectSpace = objectSpaceFactory.CreateObjectSpace<Ticket>();

                var tickets = objectSpace.GetObjectsQuery<Ticket>()
                    .Where(t => t.Date >= start && t.Date <= end)
                    .ToList();

                var result = new
                {
                    TotalTickets = tickets.Count,
                    TicketList = tickets.Select(t => new
                    {
                        t.TicketId,
                        Category = t.Category?.Title ?? "N/A",
                        SubCategory = t.SubCategory?.Title ?? "N/A",
                        Tenant = t.Account?.Name ?? "N/A",
                        Priority = t.Priority.ToString(),
                        Technician = t.Technician?.Name ?? "N/A",
                        Status = t.Status.ToString(),
                        CreatedAt = t.Date,
                        ResolvedAt = t.CloseDate,
                        ResolutionTime = (t.CloseDate != default(DateTime) && t.CloseDate > t.Date)
                            ? FormatResolutionTime(t.CloseDate - t.Date)
                            : "Not Resolved",
                        FirstResponseTime = GetFirstResponseTime(t),
                        Description = string.IsNullOrWhiteSpace(t.Description) ? "N/A" : t.Description,
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex, "HelpdeskDashboard/GetTicketsData");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("tickets/v2")]
        public IActionResult GetTicketsDataV2([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string accountId = null)
        {
            try
            {
                // startDate and endDate are mandatory
                if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                    return BadRequest("StartDate and EndDate are required.");

                if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
                    return BadRequest("Invalid date format. Please use a valid datetime format.");

                if (start > end)
                    return BadRequest("StartDate cannot be greater than EndDate.");

                using var objectSpace = objectSpaceFactory.CreateObjectSpace<Ticket>();

                // Base query filtered by date - compare only date part
                var ticketsQuery = objectSpace.GetObjectsQuery<Ticket>()
                    .Where(t => t.Date.Date >= start.Date && t.Date.Date <= end.Date);

                // Filter by accountId if provided
                string accountName = null;
                if (!string.IsNullOrWhiteSpace(accountId))
                {
                    var account = objectSpace.FirstOrDefault<Account>(a => a.Oid.ToString() == accountId);
                    if (account == null)
                        return BadRequest("Account not found.");

                    accountName = account.Name;
                    ticketsQuery = ticketsQuery.Where(t => t.Account.Oid.ToString() == accountId);
                }

                var tickets = ticketsQuery.ToList();

                var result = new
                {
                    StartDate = start.ToString("yyyy-MM-dd"),
                    EndDate = end.ToString("yyyy-MM-dd"),
                    AccountName = accountName,
                    AccountId = accountId,
                    TotalTickets = tickets.Count,
                    OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
                    ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
                    FailedTickets = tickets.Count(t => t.Status == TicketStatus.Failed),
                    HoldTickets = tickets.Count(t => t.Status == TicketStatus.Hold),
                    TicketList = tickets.Select(t => new
                    {
                        t.TicketId,
                        Title = t.Title ?? "N/A",
                        Category = t.Category?.Title ?? "N/A",
                        SubCategory = t.SubCategory?.Title ?? "N/A",
                        Tenant = t.Account?.Name ?? "N/A",
                        TenantOid = t.Account?.Oid.ToString() ?? "N/A",
                        Priority = t.Priority.ToString(),
                        Technician = t.Technician?.Name ?? "N/A",
                        Status = t.Status.ToString(),
                        CreatedAt = t.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                        ResolvedAt = t.CloseDate != default(DateTime) ? t.CloseDate.ToString("yyyy-MM-dd HH:mm:ss") : "Not Resolved",
                        ResolutionTime = (t.CloseDate != default(DateTime) && t.CloseDate > t.Date)
                            ? FormatResolutionTime(t.CloseDate - t.Date)
                            : "Not Resolved",
                        ResolutionTimeHours = (t.CloseDate != default(DateTime) && t.CloseDate > t.Date)
                            ? Math.Round((t.CloseDate - t.Date).TotalHours, 2)
                            : 0.0,
                        FirstResponseTime = GetFirstResponseTime(t),
                        FirstResponseTimeHours = GetFirstResponseTimeInHours(t),
                        Description = string.IsNullOrWhiteSpace(t.Description) ? "N/A" : t.Description,
                    }).OrderByDescending(x => x.CreatedAt).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex, $"HelpdeskDashboard/GetTicketsDataV2 - StartDate: {startDate}, EndDate: {endDate}, AccountId: {accountId}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("university")]
        public IActionResult GetAllUniversities()
        {
            try
            {
                using var objectSpace = objectSpaceFactory.CreateObjectSpace<Account>();
                var universities = objectSpace.GetObjectsQuery<Account>()
                    .Select(a => new { a.Name, a.Oid })
                    .ToList();
                return Ok(universities);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("categories")]
        public IActionResult GetTicketCategories()
        {
            try
            {
                using var objectSpace = objectSpaceFactory.CreateObjectSpace<Category>();
                var universities = objectSpace.GetObjectsQuery<Category>()
                    .Select(a => new { Name = a.Title })
                    .ToList();

                return Ok(universities);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        private static string FormatResolutionTime(TimeSpan duration)
        {
            if (duration.TotalMinutes < 1)
                return "Less than a minute";

            if (duration.TotalHours < 1)
                return $"{duration.Minutes}m";

            if (duration.TotalDays < 1)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";

            return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
        }

        private ResolutionTimeMetrics CalculateResolutionTimeMetrics(List<Ticket> tickets)
        {
            var resolvedTickets = tickets
                .Where(t => t.Status == TicketStatus.Resolved && t.CloseDate > t.Date)
                .Select(t => t.CloseDate - t.Date)
                .ToList();

            if (resolvedTickets.Count == 0)
            {
                return new ResolutionTimeMetrics
                {
                    AverageResolutionTimeHours = 0,
                    AverageResolutionTimeDays = 0,
                    MaxResolutionTimeHours = 0,
                    MaxResolutionTimeDays = 0,
                    MinResolutionTimeHours = 0,
                    MinResolutionTimeMinutes = 0,
                    AverageFirstResponseTimeHours = 0,
                    ResolvedTicketsCount = 0
                };
            }

            var avgTimespan = TimeSpan.FromMilliseconds(resolvedTickets.Average(ts => ts.TotalMilliseconds));
            var maxTimespan = resolvedTickets.Max();
            var minTimespan = resolvedTickets.Min();

            // Calculate average first response time
            var firstResponseTimes = tickets
                .Where(t => !string.IsNullOrEmpty(t.TicketId))
                .Select(t => GetFirstResponseTimeInHours(t))
                .Where(hours => hours > 0)
                .ToList();

            double avgFirstResponseHours = firstResponseTimes.Count > 0 ? firstResponseTimes.Average() : 0;

            return new ResolutionTimeMetrics
            {
                AverageResolutionTimeHours = avgTimespan.TotalHours,
                AverageResolutionTimeDays = avgTimespan.TotalDays,
                MaxResolutionTimeHours = maxTimespan.TotalHours,
                MaxResolutionTimeDays = maxTimespan.TotalDays,
                MinResolutionTimeHours = minTimespan.TotalHours,
                MinResolutionTimeMinutes = minTimespan.TotalMinutes,
                AverageFirstResponseTimeHours = Math.Round(avgFirstResponseHours, 2),
                ResolvedTicketsCount = resolvedTickets.Count
            };
        }

        private double GetFirstResponseTimeInHours(Ticket ticket)
        {
            // Get the earliest comment made by someone who is NOT the contact (i.e. support/technician)
            var firstSupportComment = ticket.Comments
                .Cast<Comment>()
                .Where(c => c.Date != default(DateTime)
                         && c.TenantUser == null          // not posted by the contact/tenant
                         && c.CRMSupport != null)         // posted by a CRM/support user
                .OrderBy(c => c.Date)
                .FirstOrDefault();

            if (firstSupportComment == null)
                return 0;

            var diff = firstSupportComment.Date - ticket.Date;
            return diff.TotalSeconds <= 0 ? 0 : diff.TotalHours;
        }

        private string GetFirstResponseTime(Ticket ticket)
        {
            // Get the earliest comment made by someone who is NOT the contact (i.e. support/technician)
            var firstSupportComment = ticket.Comments
                .Cast<Comment>()
                .Where(c => c.Date != default(DateTime)
                         && c.TenantUser == null          // not posted by the contact/tenant
                         && c.CRMSupport != null)         // posted by a CRM/support user
                .OrderBy(c => c.Date)
                .FirstOrDefault();

            if (firstSupportComment == null)
                return "N/A";

            var diff = firstSupportComment.Date - ticket.Date;

            return diff.TotalSeconds <= 0
                ? "N/A"
                : FormatResolutionTime(diff);
        }

        private List<MonthlyTicketCount> CalculateMonthlyTicketCounts(List<Ticket> tickets)
        {
            var monthlyStats = tickets
                .GroupBy(t => new { Year = t.Date.Year, Month = t.Date.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyTicketCount
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalTickets = g.Count(),
                    OpenTickets = g.Count(t => t.Status == TicketStatus.Open),
                    ResolvedTickets = g.Count(t => t.Status == TicketStatus.Resolved),
                    FailedTickets = g.Count(t => t.Status == TicketStatus.Failed),
                    HoldTickets = g.Count(t => t.Status == TicketStatus.Hold)
                })
                .ToList();

            return monthlyStats;
        }

        private DailyTicketCount CalculatePeakDay(List<Ticket> tickets)
        {
            if (tickets.Count == 0)
                return null;

            // Find the day with the most tickets
            var dailyStats = tickets
                .GroupBy(t => t.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalTickets = g.Count(),
                    OpenTickets = g.Count(t => t.Status == TicketStatus.Open),
                    ResolvedTickets = g.Count(t => t.Status == TicketStatus.Resolved),
                    FailedTickets = g.Count(t => t.Status == TicketStatus.Failed),
                    HoldTickets = g.Count(t => t.Status == TicketStatus.Hold)
                })
                .OrderByDescending(d => d.TotalTickets)
                .FirstOrDefault();

            if (dailyStats == null)
                return null;

            return new DailyTicketCount
            {
                Date = dailyStats.Date.ToString("yyyy-MM-dd"),
                DayOfWeek = dailyStats.Date.DayOfWeek.ToString(),
                TotalTickets = dailyStats.TotalTickets,
                OpenTickets = dailyStats.OpenTickets,
                ResolvedTickets = dailyStats.ResolvedTickets,
                FailedTickets = dailyStats.FailedTickets,
                HoldTickets = dailyStats.HoldTickets
            };
        }
    }
}
