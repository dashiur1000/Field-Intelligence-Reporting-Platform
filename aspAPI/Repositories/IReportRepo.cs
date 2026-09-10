using aspAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace aspAPI.Repositories
{
    public interface IReportRepo
    {
        Task<List<Report>> searchAsync(
                    string? text,
                    string? theater,
                    string? sector,
                    string? location,
                    List<string>? priorities,
                    string? reportType,
                    DateTime? from,
                    DateTime? to);
        Task<List<Report>> GetBysubjectIdAsync(string id);
        Task<List<Report>> GetFilteredReportsAsync(
                    string? theater,
                    string? sector,
                    string? location,
                    string? priorities,
                    DateTime? from,
                    DateTime? to);
        Task<ReportsSummaryDto> GetReportsSummaryAsync();
    }
}