using AuthService.Dtos;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            if (!result)
            {
                return BadRequest("Email đã được sử dụng.");
            }
            return Ok(new { Message = "Đăng ký thành công." });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _authService.LoginAsync(loginDto);
            if (token == null)
            {
                return Unauthorized("Email hoặc mật khẩu không chính xác.");
            }
            return Ok(new { Token = token });
        }
        [HttpPost("googlelogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto loginDto)
        {
            var token = await _authService.GoogleLoginAsync(loginDto.IdToken);
            if (token == null)
            {
                return BadRequest("Google Token không hợp lệ.");
            }
            return Ok(new { Token = token });
        }
    }
}
