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

        // Endpoint: POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            if (!result)
            {
                // Trả về lỗi 400 Bad Request nếu email đã được sử dụng
                return BadRequest("Email đã được sử dụng.");
            }
            // Trả về 200 OK nếu đăng ký thành công
            return Ok(new { Message = "Đăng ký thành công." });
        }

        // Endpoint: POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _authService.LoginAsync(loginDto);
            if (token == null)
            {
                // Trả về lỗi 401 Unauthorized nếu thông tin đăng nhập sai
                return Unauthorized("Email hoặc mật khẩu không chính xác.");
            }
            // Trả về 200 OK cùng với token
            return Ok(new { Token = token });
        }
    }
}
