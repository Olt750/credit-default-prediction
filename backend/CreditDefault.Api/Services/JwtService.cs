using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        public JwtService(IConfiguration config) => _config = config;

        public (string AccessToken, DateTime ExpiresAt) GenerateToken(User user)
        {
            var roles = user.UserRoles?.Select(ur => ur.Role.Name).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList()
                ?? new List<string>();
            if (roles.Count == 0 && !string.IsNullOrWhiteSpace(user.Role))
            {
                roles.Add(user.Role);
            }

            var permissions = user.UserRoles?
                .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .ToList() ?? new List<string>();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

            var secret = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT secret is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(int.TryParse(_config["Jwt:AccessTokenMinutes"], out var minutes) ? minutes : 60);
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: expiresAt,
                signingCredentials: creds
            );
            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
