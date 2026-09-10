using aspAPI.Models;
using aspAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace aspAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class reportsController : ControllerBase
    {
        private readonly IReportRepo _reportRepo;
        public reportsController(IReportRepo reportRepo)
        {
            _reportRepo = reportRepo;
        }
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Report>>> SearchInMessage([FromQuery] string? text,
                    string? theater,
                    string? sector,
                    string? location,
                    List<string>? priorities,
                    string? reportType,
                    DateTime? from,
                    DateTime? to)
        {
            var result = await _reportRepo.searchAsync(text, theater, sector, location, priorities, reportType, from, to);
            return Ok(result);
        }
        [HttpGet("subjects/{id}/reports")]
        public async Task<ActionResult<IEnumerable<Report>>> GetBysubjectId(string id)
        {
            var result = await _reportRepo.GetBysubjectIdAsync(id);
            return Ok(result);
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetByCriteria([FromQuery] string? location,
            [FromQuery] string? theater,
            [FromQuery] string? sector,
            [FromQuery] string? priorities,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _reportRepo.GetFilteredReportsAsync(location, theater, sector, priorities, from, to);
            return Ok(result);
        }
        [HttpGet("statistics")]
        public async Task<ActionResult<IEnumerable<ReportsSummaryDto>>> GetReportsSummary()
        {
            var reault = await _reportRepo.GetReportsSummaryAsync();
            return Ok(reault);
        }
    }
}
