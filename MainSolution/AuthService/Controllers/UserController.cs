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
        [SwaggerOperation(Summary = "Lấy thông tin cá nhân", Description = "Lấy thông tin User dựa trên Token đang đăng nhập.")]
        [SwaggerResponse(200, "Lấy thông tin thành công", typeof(UserProfileDto))]
        [SwaggerResponse(401, "Chưa đăng nhập hoặc Token hết hạn", typeof(ErrorResponse))]
        [SwaggerResponse(404, "Không tìm thấy thông tin người dùng", typeof(ErrorResponse))]
        [SwaggerResponse(500, "Lỗi server", typeof(ErrorResponse))]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var userProfile = await _userService.GetUserProfileAsync(User);

                if (userProfile == null)
                {
                    return NotFound(new ErrorResponse(404, "Không tìm thấy thông tin người dùng trong cơ sở dữ liệu."));
                }

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(500, "Lỗi server khi lấy thông tin user: " + ex.Message));
            }
        }
    }
}
