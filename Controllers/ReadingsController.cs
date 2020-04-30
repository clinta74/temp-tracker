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
            
            int skip = Math.Max(((page ?? 1) - 1) * (limit ?? 0), 0);
            int take = Math.Max((limit ?? count), 0);
            
            var readings = await _context
                .Readings
                .AsNoTracking()
                .OrderBy(reading => reading.Taken)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            _httpContext.HttpContext.Response.Headers.Add("X-Total-Count", count.ToString());

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
                Taken = reading.Taken ?? DateTime.Now
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
                .Where(r => r.Taken >= (start ?? DateTime.Today.AddDays(-7)))
                .Where(p => p.Taken <= (end ?? DateTime.Now))
                .GroupBy(p => p.Taken.Date)
                .Select(g => new ReadingGraphData { Value = g.Average(p => p.Value), Taken = g.Key })
                .ToListAsync();

            return data;
        }
    }
}