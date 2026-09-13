namespace aspAPI.Models
{
    public class ReportsSummaryDto
    {
        public long TotalReports { get; set; }
        public Dictionary<string, long> ByPriority { get; set; }
        public Dictionary<string, long> ByTheater { get; set; }
        public Dictionary<string, long> ByReportType { get; set; }
    }
}
