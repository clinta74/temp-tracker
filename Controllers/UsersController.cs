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
using temp_tracker.Extensions;

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
            var users = await _context
                .Users
                .AsNoTracking()
                .Include(user => user.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderBy(user => user.Lastname)
                .Paged(count, page, limit)
                .Select(user => new UserResponse
                {
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

        [HttpGet("{UserId}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponse>> GetUser(int UserId)
        {
            var user = await _context
                .Users
                .AsNoTracking()
                .Include(user => user.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(user => new UserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Created = user.Created,
                    Roles = user.UserRoles.Select(userRoles => userRoles.Role),
                })
                .FirstOrDefaultAsync(user => user.UserId == UserId);

            if (user != null)
            {
                if (_claimsPrincipal.SubjectId().Equals(user.Username, StringComparison.OrdinalIgnoreCase) || _claimsPrincipal.IsInRole("admin"))
                {
                    return user;
                }

                return new ForbidResult();
            }

            return new NotFoundResult();
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

            var roles = await _context.Roles.Where(role => request.Roles.Contains(role.Name)).ToListAsync();

            var userRoles = _context.UserRoles.AddRangeAsync(roles.Select(role => new UserRole
            {
                UserId = result.Entity.UserId,
                RoleId = role.RoleId,
            }).ToArray());

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

                    if (Request.Roles != null && _claimsPrincipal.IsInRole("admin"))
                    {
                        user.UserRoles.Clear();
                        foreach (var roleName in Request.Roles)
                        {
                            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                            if (role != null)
                            {
                                user.UserRoles.Add(new UserRole
                                {
                                    UserId = user.UserId,
                                    RoleId = role.RoleId
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();

                    return new OkResult();
                }

                return new ForbidResult();
            }

            return new BadRequestResult();
        }

        [HttpPost("{id}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        [Authorize(Roles = "admin")]
        [HttpPost("{id}/resetpassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> ResetPassword(int id, [FromBody] UserResetPasswordRequest request)
        {
            var user = await _context
                .Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user != null)
            {

                var salt = SaltGenerator.MakeSalty();
                var newHash = await HashService.HashPassword(request.Password, salt);

                user.Password = newHash;
                user.Salt = salt;

                await _context.SaveChangesAsync();
                return new OkResult();

            }

            return new BadRequestResult();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Remove(int id)
        {
            var user = await _context
                .Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user != null)
            {
                _context.Remove(user);

                await _context.SaveChangesAsync();
                return new OkResult();
            }

            return new BadRequestResult();
        }

    }

    public static class ClaimsExtenstion
    {
        public static string SubjectId(this ClaimsPrincipal user) { return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value; }
    }
}