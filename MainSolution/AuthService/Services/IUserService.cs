using AuthService.Dtos;
using System.Security.Claims;

namespace AuthService.Services
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(ClaimsPrincipal user);
    }
}
