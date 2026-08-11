using MarbleServer.DTOs.Responses;
using MarbleServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarbleServer.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    public class LeaderboardController : BaseController
    {
        private readonly LeaderboardService _leaderboardService;

        public LeaderboardController(
            LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string level,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            LeaderboardResponse leaderboard =
                await _leaderboardService.GetLeaderboardAsync(
                    level,
                    page,
                    pageSize);

            return Ok(
                ApiResponse<LeaderboardResponse>
                    .Success(leaderboard));
        }

        [Authorize]
        [HttpGet("my-rank")]
        public async Task<IActionResult> GetMyRank(
            [FromQuery] string level)
        {
            MyRankResponse? rank =
                await _leaderboardService.GetMyRankAsync(
                    PlayerId,
                    level);

            if (rank == null)
                return NotFound();

            return Ok(
                ApiResponse<MyRankResponse>
                    .Success(rank));
        }
    }
}