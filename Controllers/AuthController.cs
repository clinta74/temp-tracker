using System;
using System.IdentityModel.Tokens.Jwt;
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
                        UserID = user.UserID,
                        Token = GenerateJSONWebToken(user),
                    };
                }
            }

            return new EmptyResult();

        }

        public class UserResponse
        {
            public int UserID { get; set; }
            public string Token { get; set; }
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private string GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}