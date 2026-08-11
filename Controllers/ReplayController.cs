using MarbleServer.DTOs.Responses;
using MarbleServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarbleServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/scores/{scoreId:int}/replay")]
    public class ReplayController : BaseController
    {
        private readonly ReplayService _replayService;

        public ReplayController(ReplayService replayService)
        {
            _replayService = replayService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(
            int scoreId,
            [FromForm] int timeMs,
            IFormFile replay)
        {
            int replayId =
                await _replayService.UploadReplayAsync(
                    PlayerId,
                    scoreId,
                    timeMs,
                    replay);

            return CreatedAtAction(
                nameof(Download),
                new { scoreId },
                ApiResponse<UploadReplayResponse>.Success(
                    new UploadReplayResponse
                    {
                        ReplayId = replayId
                    }));
        }

        [HttpGet]
        public async Task<IActionResult> Download(
            int scoreId)
        {
            (byte[] Data, string FileName) replay =
                await _replayService.DownloadReplayAsync(scoreId);

            return File(
                replay.Data,
                "application/octet-stream",
                replay.FileName);
        }
    }
}