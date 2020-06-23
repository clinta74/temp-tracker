using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using temp_tracker.Context;
using temp_tracker.Extensions;
using temp_tracker.Models;

namespace temp_tracker.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController
    {
        private readonly TempTrackerDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly IHttpContextAccessor _httpContext;
        public RolesController(TempTrackerDbContext context, ILogger<UsersController> logger, IHttpContextAccessor httpContext)
        {
            this._context = context;
            this._logger = logger;
            this._httpContext = httpContext;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Role>>> Get(int? page, int? limit)
        {
            int count = await _context
                .Roles
                .AsNoTracking()
                .CountAsync();

            var roles = await _context
                .Roles
                .AsNoTracking()
                .Paged(count, page, limit)
                .ToListAsync();

            _httpContext.HttpContext.Response.Headers.Add("X-Total-Count", count.ToString());

            return roles;
        }
    }
}