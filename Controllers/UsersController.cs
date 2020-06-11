using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using temp_tracker.Context;
using temp_tracker.Models;
using temp_tracker.Services;
using System.Collections.Generic;

namespace temp_tracker.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController
    {
        private readonly TempTrackerDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly IHttpContextAccessor _httpContext;
        public UsersController(TempTrackerDbContext context, ILogger<UsersController> logger, IPrincipal principal, IHttpContextAccessor httpContext)
        {
            this._context = context;
            this._logger = logger;
            this._claimsPrincipal = principal as ClaimsPrincipal;
            this._httpContext = httpContext;
        }

        public class UserResponse
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Firstname { get; set; }
            public string Lastname { get; set; }
            public DateTime Created { get; set; }
            public IEnumerable<Role> Roles { get; set; }
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserResponse>>> Get(int? page, int? limit)
        {
            int count = await _context
                .Users
                .Include(user => user.UserRoles)
                .AsNoTracking()
                .CountAsync();

            int skip = Math.Max(((page ?? 1) - 1) * (limit ?? 0), 0);
            int take = Math.Max((limit ?? count), 0);

            var users = await _context
                .Users
                .AsNoTracking()
                .Include(user => user.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderBy(user => user.Lastname)
                .Skip(skip)
                .Take(take)
                .Select(user => new UserResponse {
                    UserId = user.UserId,
                    Username = user.Username,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Created = user.Created,
                    Roles = user.UserRoles.Select(userRoles => userRoles.Role),
                })
                .ToListAsync();

            _httpContext.HttpContext.Response.Headers.Add("X-Total-Count", count.ToString());

            return users;
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Post([FromBody] UserRequest request)
        {
            var user = await _context
                .Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (user != null)
            {
                return new BadRequestResult();
            }

            var salt = SaltGenerator.MakeSalty();
            var hash = await HashService.HashPassword(request.Password, salt);
            var result = _context.Users.Add(new User
            {
                Username = request.Username,
                Firstname = request.Firstname,
                Lastname = request.Lastname,
                Password = hash,
                Salt = salt,
                Created = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync();
            return result.Entity.UserId;
        }

        [HttpPut("{UserId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Put(int UserId, [FromBody] UserUpdateRequest Request)
        {
            var user = await _context
                .Users
                .FirstOrDefaultAsync(u => u.UserId == UserId);

            if (user != null)
            {
                if (_claimsPrincipal.SubjectId().Equals(user.Username, StringComparison.OrdinalIgnoreCase) || _claimsPrincipal.IsInRole("admin"))
                {
                    user.Firstname = Request.Firstname;
                    user.Lastname = Request.Lastname;

                    await _context.SaveChangesAsync();

                    return new OkResult();
                }

                return new ForbidResult();
            }

            return new BadRequestResult();
        }

        [HttpPost("{id}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> ChangePassword(int id, [FromBody] UserChangePasswordRequest request)
        {
            var user = await _context
                .Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user != null)
            {
                var hash = await HashService.HashPassword(request.OldPassword, user.Salt);

                if (hash == user.Password)
                {
                    var salt = SaltGenerator.MakeSalty();
                    var newHash = await HashService.HashPassword(request.Password, salt);

                    user.Password = newHash;
                    user.Salt = salt;

                    await _context.SaveChangesAsync();
                    return new OkResult();
                }

                return new ForbidResult();
            }

            return new BadRequestResult();
        }
    }

    public static class ClaimsExtenstion
    {
        public static string SubjectId(this ClaimsPrincipal user) { return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value; }
    }
}