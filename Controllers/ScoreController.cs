using MarbleServer.DTOs.Requests;
using MarbleServer.DTOs.Responses;
using MarbleServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarbleServer.Controllers;

[ApiController]
[Route("api/scores")]
[Authorize]
public class ScoreController : BaseController
{
    private readonly ScoreService _scoreService;

    public ScoreController(ScoreService scoreService)
    {
        _scoreService = scoreService;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitScore(
        SubmitScoreRequest request)
    {
        SubmitScoreResponse response =
            await _scoreService.SubmitScoreAsync(
                PlayerId,
                request);

        return Ok(
            ApiResponse<SubmitScoreResponse>.Success(response));
    }
}