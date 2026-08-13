using MarbleServer.Data;
using MarbleServer.DTOs.Requests;
using MarbleServer.Exceptions;
using MarbleServer.Models;
using Microsoft.EntityFrameworkCore;

namespace MarbleServer.Services
{
    public class PlayerService
    {
        private readonly MarbleDbContext _db;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public PlayerService(
            MarbleDbContext db,
            JwtService jwtService,
            IConfiguration configuration)
        {
            _db = db;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            bool exists = await _db.Players.AnyAsync(p =>
                p.Username == request.Username);

            if (exists)
            {
                throw new ConflictException(
                    "Username already exists.");
            }

            Player player = new Player
            {
                Username = request.Username,
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password),

                CreatedAt = DateTime.UtcNow
            };

            _db.Players.Add(player);

            await _db.SaveChangesAsync();
        }

        public async Task<string> LoginAsync(
            LoginRequest request)
        {
            // Check game version before authentication
            string latestVersion =
                _configuration["GameVersion:Latest"]
                ?? string.Empty;

            if (!string.Equals(
                    request.GameVersion,
                    latestVersion,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedException(
                    $"This version of the game is outdated. " +
                    $"Please update to version {latestVersion}, " +
                    $"available to download from https://github.com/NaCl586/marble-blast-platinum-unity/releases/");
            }

            // Normal login
            Player? player =
                await _db.Players
                    .FirstOrDefaultAsync(p =>
                        p.Username == request.Username);

            if (player == null)
            {
                throw new UnauthorizedException(
                    "Invalid username or password.");
            }

            bool valid =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    player.PasswordHash);

            if (!valid)
            {
                throw new UnauthorizedException(
                    "Invalid username or password.");
            }

            return _jwtService.GenerateToken(player);
        }
    }
}