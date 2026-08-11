using MarbleServer.DTOs.Requests;
using MarbleServer.DTOs.Responses;
using MarbleServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MarbleServer.Controllers
{
    [ApiController]
    [Route("api/players")]
    public class PlayerController : BaseController
    {
        private readonly PlayerService _service;

        public PlayerController(PlayerService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            await _service.RegisterAsync(request);

            return Ok(
                ApiResponse<object>.Success(
                    message: "Registration successful."));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request)
        {
            string token =
                await _service.LoginAsync(request);

            return Ok(
                ApiResponse<LoginResponse>.Success(
                    new LoginResponse
                    {
                        Token = token
                    }));
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(
                ApiResponse<MeResponse>.Success(
                    new MeResponse
                    {
                        PlayerId = PlayerId,
                        Username = Username
                    }));
        }
    }
}