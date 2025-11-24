using AuthService.Dtos;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ModelClass.Connection;
using ModelClass.UserInfo;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services.impl
{
    public class AuthServiceImpl : IAuthService
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthServiceImpl(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return false;
            }

            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                // Mã hóa mật khẩu bằng BCrypt trước khi lưu
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow,
                RoleId = 2 // Mặc định là role "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // BCrypt.Verify sẽ so sánh mật khẩu người dùng nhập với chuỗi hash trong DB
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }
            return GenerateJwtToken(user);
        }

        public async Task<string?> GoogleLoginAsync(string idToken)
        {
            try
            {
                // Xác thực Token với Google
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { _configuration["Jwt:GoogleClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                var email = payload.Email;
                var name = payload.Name;

                // Kiểm tra xem user đã tồn tại trong DB chưa
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    // Nếu chưa tồn tại -> Tự động đăng ký
                    user = new User
                    {
                        FullName = name,
                        Email = email,
                        // Tạo một password ngẫu nhiên vì user này dùng Google
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                        CreatedAt = DateTime.UtcNow,
                        RoleId = 2 // Mặc định là User thường
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                return GenerateJwtToken(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google Login Error: {ex.Message}");
                return null;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // Lấy chuỗi bí mật từ appsettings.json
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    // "Claims" là các thông tin về người dùng sẽ được lưu trong token
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim("role_id", user.RoleId.ToString())
                }),
                // Token sẽ hết hạn sau 7 ngày
                Expires = DateTime.UtcNow.AddDays(7),
                // Lấy Issuer và Audience từ appsettings.json
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                // Sử dụng thuật toán mã hóa HmacSha256
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
