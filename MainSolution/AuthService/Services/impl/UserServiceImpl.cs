using AuthService.Dtos;
using Microsoft.EntityFrameworkCore;
using ModelClass.Connection;
using System.Security.Claims;

namespace AuthService.Services.impl
{
    public class UserServiceImpl : IUserService
    {
        private readonly AuthDbContext _context;

        public UserServiceImpl(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(ClaimsPrincipal userPrincipal)
        {
            // Lấy User ID từ claim 'NameIdentifier' trong token
            var userIdString = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                // Không tìm thấy hoặc ID không hợp lệ
                return null;
            }

            // Tìm user trong database bằng ID
            var user = await _context.Users
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            // Map thông tin từ User model sang UserProfileDto
            return new UserProfileDto
            {
                Id = user.Id.ToString(),
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId
            };
        }
    }
}
