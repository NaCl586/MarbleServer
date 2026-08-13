using MarbleServer.DTOs.Requests;
using MarbleServer.DTOs.Responses;
using MarbleServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarbleServer.Controllers
{
    [ApiController]
    [Route("api/integrity")]
    [Authorize]
    public class IntegrityController : ControllerBase
    {
        private readonly IntegrityService _service;

        public IntegrityController(
            IntegrityService service)
        {
            _service = service;
        }

        [HttpPost("check")]
        public async Task<IActionResult> Check(
            IntegrityRequest request)
        {
            IntegrityResponse result =
                await _service.GetHashesAsync(
                    request.GameVersion,
                    request.Files);

            return Ok(
                ApiResponse<IntegrityResponse>.Success(
                    result));
        }
    }
}