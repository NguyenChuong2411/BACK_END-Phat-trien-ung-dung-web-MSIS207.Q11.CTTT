using AuthService.Dtos;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Quản lý xác thực và phân quyền (Đăng ký, Đăng nhập)")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        [SwaggerOperation(Summary = "Đăng ký tài khoản mới", Description = "Tạo tài khoản người dùng với Email và Mật khẩu. Role mặc định là User.")]
        [SwaggerResponse(200, "Đăng ký thành công", typeof(object))]
        [SwaggerResponse(400, "Email đã tồn tại trong hệ thống")]
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
        [SwaggerOperation(Summary = "Đăng nhập hệ thống", Description = "Đăng nhập bằng Email/Password để lấy JWT Token.")]
        [SwaggerResponse(200, "Đăng nhập thành công. Trả về Token.", typeof(object))]
        [SwaggerResponse(401, "Email hoặc mật khẩu không chính xác")]
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
        [SwaggerOperation(Summary = "Đăng nhập bằng Google", Description = "Sử dụng Google ID Token để đăng nhập hoặc tự động đăng ký nếu chưa có tài khoản.")]
        [SwaggerResponse(200, "Thành công. Trả về JWT Token hệ thống.", typeof(object))]
        [SwaggerResponse(400, "Google Token không hợp lệ hoặc lỗi xác thực")]
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
