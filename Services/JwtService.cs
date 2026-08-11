using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarbleServer.Configuration;
using MarbleServer.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MarbleServer.Services
{
    public class JwtService
    {
        private readonly JwtSettings _settings;

        public JwtService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public string GenerateToken(Player player)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    player.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    player.Username)
            };

            SymmetricSecurityKey key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_settings.Secret));

            SigningCredentials credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token =
                new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(
                        _settings.ExpirationDays),
                    signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}