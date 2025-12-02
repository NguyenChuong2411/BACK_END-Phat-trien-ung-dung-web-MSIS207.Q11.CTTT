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
        [SwaggerOperation(Summary = "Đăng ký tài khoản mới", Description = "Tạo tài khoản người dùng với Email và Mật khẩu.")]
        [SwaggerResponse(200, "Đăng ký thành công", typeof(object))]
        [SwaggerResponse(400, "Email đã tồn tại", typeof(ErrorResponse))]
        [SwaggerResponse(500, "Lỗi server", typeof(ErrorResponse))]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                if (!result)
                {
                    return BadRequest(new ErrorResponse(400, "Email đã được sử dụng."));
                }
                return Ok(new { Message = "Đăng ký thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(500, "Lỗi khi đăng ký: " + ex.Message));
            }
        }

        [HttpPost("login")]
        [SwaggerOperation(Summary = "Đăng nhập hệ thống", Description = "Đăng nhập bằng Email/Password để lấy JWT Token.")]
        [SwaggerResponse(200, "Đăng nhập thành công", typeof(object))]
        [SwaggerResponse(401, "Sai email hoặc mật khẩu", typeof(ErrorResponse))]
        [SwaggerResponse(500, "Lỗi server", typeof(ErrorResponse))]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var token = await _authService.LoginAsync(loginDto);
                if (token == null)
                {
                    return Unauthorized(new ErrorResponse(401, "Email hoặc mật khẩu không chính xác."));
                }
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(500, "Lỗi khi đăng nhập: " + ex.Message));
            }
        }

        [HttpPost("googlelogin")]
        [SwaggerOperation(Summary = "Đăng nhập bằng Google", Description = "Sử dụng Google ID Token để đăng nhập/đăng ký.")]
        [SwaggerResponse(200, "Thành công", typeof(object))]
        [SwaggerResponse(400, "Google Token không hợp lệ", typeof(ErrorResponse))]
        [SwaggerResponse(500, "Lỗi server", typeof(ErrorResponse))]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto loginDto)
        {
            try
            {
                var token = await _authService.GoogleLoginAsync(loginDto.IdToken);
                if (token == null)
                {
                    return BadRequest(new ErrorResponse(400, "Google Token không hợp lệ hoặc lỗi xác thực với Google."));
                }
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(500, "Lỗi server khi đăng nhập Google: " + ex.Message));
            }
        }
    }
}
