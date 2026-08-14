using MarbleServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarbleServer.Controllers
{
    [ApiController]
    [Route("api/admin/ratings")]
    [Authorize]
    public class RatingAdminController : ControllerBase
    {
        private readonly GlobalRatingService _globalRatingService;

        public RatingAdminController(
            GlobalRatingService globalRatingService)
        {
            _globalRatingService =
                globalRatingService;
        }

        [HttpPost("backfill")]
        public async Task<IActionResult> Backfill()
        {
            await _globalRatingService
                .BackfillMissionRatingsAsync();

            return Ok(new
            {
                message =
                    "Mission ratings backfilled successfully."
            });
        }
    }
}