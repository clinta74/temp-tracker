using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using temp_tracker.Context;
using temp_tracker.Models;
using temp_tracker.Extensions;

namespace temp_tracker.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReadingsController
    {
        private readonly TempTrackerDbContext _context;
        private readonly ILogger<ReadingsController> _logger;
        private readonly IHttpContextAccessor  _httpContext;
        public ReadingsController(TempTrackerDbContext context, ILogger<ReadingsController> logger, IHttpContextAccessor httpContext)
        {
            this._context = context;
            this._logger = logger;
            this._httpContext = httpContext;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Reading>>> Get(int? page, int? limit)
        {

            int count = await _context
                .Readings
                .AsNoTracking()
                .CountAsync();
            
            var readings = await _context
                .Readings
                .AsNoTracking()
                .OrderByDescending(reading => reading.Taken)
                .Paged(count, page, limit)
                .ToListAsync();

            _httpContext.HttpContext.Response.Headers.Add("x-total-count", count.ToString());

            return readings;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Reading>> GetByID(Guid id)
        {
            var reading = await _context
                .Readings
                .AsNoTracking()
                .FirstOrDefaultAsync(_reading => _reading.ReadingId == id);

            if (reading == null)
            {
                return new NotFoundResult();
            }

            return reading;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Guid>> Post([FromBody]ReadingRequest reading)
        {
            var entity = await _context.Readings.AddAsync(new Reading
            {
                Value = reading.Value,
                Scale = reading.Scale,
                ReadingId = Guid.NewGuid(),
                Taken = reading.Taken ?? DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return entity.Entity.ReadingId;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Reading>> DeleteByID(Guid id)
        {
            var reading = await _context
                .Readings
                .FirstOrDefaultAsync(_reading => _reading.ReadingId == id);

            if (reading == null)
            {
                return new NotFoundResult();
            }

            _context.Remove(reading);

            await _context.SaveChangesAsync();

            return new OkResult();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("data")]
        public async Task<ActionResult<IEnumerable<ReadingGraphData>>> Data(DateTime? start, DateTime? end)
        {
            var data = await _context
                .Readings
                .AsNoTracking()
                .Where(r => r.Taken >= (start ?? DateTime.Today.AddDays(-14)))
                .Where(p => p.Taken <= (end ?? DateTime.Now))
                .GroupBy(p => p.Taken.Date)
                .Select(g => new ReadingGraphData { Value = g.Average(p => p.Value), Taken = g.Key })
                .ToListAsync();

            var f = data.Where(d => d.Taken.DayOfWeek < DayOfWeek.Friday).ToList();

            return data;
        }
    }
}