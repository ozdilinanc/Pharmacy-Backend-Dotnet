using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyProject.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace PharmacyProject.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class SystemLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemLogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, [FromQuery] string? level = null)
        {
            var query = _context.Logs.AsNoTracking();

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(l => l.Level == level);
            }

            var totalCount = await query.CountAsync();
            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new 
                {
                    l.Id,
                    l.Message,
                    l.Level,
                    l.Timestamp,
                    l.Exception
                })
                .ToListAsync();

            var response = new
            {
                Items = logs,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(response);
        }
    }
}
