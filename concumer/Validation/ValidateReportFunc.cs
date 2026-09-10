using concumer.Models;

namespace concumer.Validation
{
    public class ValidateReportFunc
    {
        public static string? ValidateReport(Report report)
        {
            if (string.IsNullOrWhiteSpace(report.reportId)) return "Missing or empty reportId";
            if (string.IsNullOrWhiteSpace(report.agentId)) return "Missing or empty agentId";
            if (string.IsNullOrWhiteSpace(report.unit)) return "Missing or empty unit";
            if (string.IsNullOrWhiteSpace(report.theater)) return "Missing or empty theater";
            if (string.IsNullOrWhiteSpace(report.sector)) return "Missing or empty sector";
            if (string.IsNullOrWhiteSpace(report.location)) return "Missing or empty location";
            if (string.IsNullOrWhiteSpace(report.reportType)) return "Missing or empty reportType";
            if (string.IsNullOrWhiteSpace(report.priority)) return "Missing or empty priority";
            if (string.IsNullOrWhiteSpace(report.sourceType)) return "Missing or empty sourceType";
            if (string.IsNullOrWhiteSpace(report.message)) return "Missing or empty message";

            var allowedPriorities = new[] { "Low", "Medium", "High", "Critical" };
            if (!Array.Exists(allowedPriorities, p => p.Equals(report.priority, StringComparison.Ordinal)))
                return $"Invalid priority value: {report.priority}";

            var allowedTypes = new[] { "Observation", "Movement", "Meeting", "Access", "Communication", "Logistics", "Incident" };
            if (!Array.Exists(allowedTypes, t => t.Equals(report.reportType, StringComparison.Ordinal)))
                return $"Invalid reportType value: {report.reportType}";

            bool hasSubjectId = !string.IsNullOrWhiteSpace(report.subjectId);
            bool hasSubjectType = !string.IsNullOrWhiteSpace(report.subjectType);
            if (hasSubjectId != hasSubjectType)
                return "subjectId and subjectType must either both appear or both be absent";

            return null;
        }
    }
}
