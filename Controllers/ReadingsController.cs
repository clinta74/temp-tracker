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
        public ReadingsController(TempTrackerDbContext context, ILogger<ReadingsController> logger)
        {
            this._context = context;
            this._logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Reading>>> Get()
        {
            return await _context
                .Readings
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Reading>> GetByID(Guid id)
        {
            var reading = await _context
                .Readings
                .AsNoTracking()
                .FirstOrDefaultAsync(_reading => _reading.ReadingID == id);

            if (reading == null)
            {
                return new NotFoundResult();
            }

            return reading;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Guid>> Post([FromBody]Reading reading)
        {
            var entity = await _context.Readings.AddAsync(new Reading
            {
                Value = reading.Value,
                Scale = reading.Scale,
                ReadingID = Guid.NewGuid(),
                Taken = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return entity.Entity.ReadingID;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("data")]
        public async Task<ActionResult<IEnumerable<DataResponse>>> Data(DateTime? start, DateTime? end)
        {
            var data = await _context
                .Readings
                .AsNoTracking()
                .Where(r => r.Taken >= (start ?? DateTime.Today.AddDays(-7)))
                .Where(p => p.Taken <= (end ?? DateTime.Now))
                .GroupBy(p => p.Taken.Date)
                .Select(g => new DataResponse { Value = g.Average(p => p.Value), Taken = g.Key })
                .ToListAsync();

            return data;
        }

        public class DataResponse
        {
            public decimal Value { get; set; }
            public DateTime Taken { get; set; }
        }
    }
}