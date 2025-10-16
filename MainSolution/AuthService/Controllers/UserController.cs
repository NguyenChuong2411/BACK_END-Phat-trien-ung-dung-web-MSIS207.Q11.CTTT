using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelClass.UserInfo;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("GetUserProfile")]
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
