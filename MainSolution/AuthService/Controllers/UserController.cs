using AuthService.Dtos;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelClass.UserInfo;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SwaggerTag("Quản lý thông tin người dùng")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("GetUserProfile")]
        [SwaggerOperation(Summary = "Lấy thông tin cá nhân (Profile)", Description = "Lấy thông tin User dựa trên Token đang đăng nhập.")]
        [SwaggerResponse(200, "Lấy thông tin thành công", typeof(UserProfileDto))]
        [SwaggerResponse(401, "Chưa đăng nhập hoặc Token hết hạn")]
        [SwaggerResponse(404, "Không tìm thấy thông tin người dùng trong DB")]
        public async Task<IActionResult> GetUserProfile()
        {
            // `User` ở đây là ClaimsPrincipal, được tự động điền bởi ASP.NET Core
            // sau khi xác thực token thành công.
            var userProfile = await _userService.GetUserProfileAsync(User);

            if (userProfile == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            return Ok(userProfile);
        }
    }
}
