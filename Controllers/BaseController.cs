using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace MarbleServer.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected bool IsAuthenticated =>
            User.Identity?.IsAuthenticated ?? false;

        protected int PlayerId
        {
            get
            {
                string? value =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(value, out int playerId))
                {
                    throw new UnauthorizedAccessException();
                }

                return playerId;
            }
        }

        protected string Username =>
            User.FindFirstValue(ClaimTypes.Name)
            ?? string.Empty;
    }
}