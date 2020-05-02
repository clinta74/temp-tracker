using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using temp_tracker.Context;
using temp_tracker.Models;
using temp_tracker.Services;

namespace temp_tracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController
    {
        private readonly TempTrackerDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _config;
        public AuthController(TempTrackerDbContext context, ILogger<AuthController> logger, IConfiguration config)
        {
            this._context = context;
            this._logger = logger;
            this._config = config;
        }

        [HttpPost]
        public async Task<ActionResult<UserResponse>> Login([FromBody]LoginRequest request)
        {
            var user = await _context
                .Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (user != null)
            {
                var hash = await HashService.HashPassword(request.Password, user.Salt);

                if (hash == user.Password)
                {
                    return new UserResponse
                    {
                        UserId = user.UserId,
                        Token = await GenerateJSONWebTokenAsync(user),
                    };
                }
            }

            return new ForbidResult();

        }

        private async Task<string> GenerateJSONWebTokenAsync(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_KEY"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var roles = await _context.Roles
                .AsNoTracking()
                .Where(role => role.UserRoles.Any(ur => ur.UserId == user.UserId))
                .ToListAsync();

            var claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Username));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}