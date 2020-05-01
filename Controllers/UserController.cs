using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using temp_tracker.Context;
using temp_tracker.Models;
using temp_tracker.Services;

namespace temp_tracker.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController
    {
        private readonly TempTrackerDbContext _context;
        private readonly ILogger<UserController> _logger;
        public UserController(TempTrackerDbContext context, ILogger<UserController> logger)
        {
            this._context = context;
            this._logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post([FromBody]UserRequest request)
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
                Password = hash,
                Salt = salt,
                Created = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync();
            return result.Entity.UserID;
        }

        [HttpPut("{id}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> ChangePassword(int id, [FromBody]UserChangePasswordRequest request)
        {
            var user = await _context
                .Users
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user != null)
            {
                var hash = await HashService.HashPassword(request.Password, user.Salt);

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
}