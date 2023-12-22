using System.Security.Claims;

using AvaBPMS.Application.Common.Interfaces;

namespace AvaBPMS.WebUi.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public string? Id => _httpContextAccessor.HttpContext?.Session.Get<string>("UserId");
    //public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
