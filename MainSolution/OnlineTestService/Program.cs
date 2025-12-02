using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using ModelClass.connection;
using OnlineTestService.Service;
using OnlineTestService.Service.Impl;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "https://enly-theta.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Cấu hình DbContext và kết nối PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OnlineTestDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Đăng ký các services cho Dependency Injection
builder.Services.AddScoped<IOnlineTest, OnlineTestImpl>();
builder.Services.AddScoped<ITestAdminService, TestAdminServiceImpl>();
builder.Services.AddScoped<IFileService, FileServiceImpl>();

builder.Services.AddSingleton(builder.Environment);
// Controllers
builder.Services.AddControllers();

// Swagger để test API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.CustomSchemaIds(type => type.ToString());

    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Enly Online Test Service",
        Version = "v1",
        Description = "API documentation for Enly Online Test Service"
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Các thông số này phải khớp chính xác với appsettings.json của AuthService
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Enly Online Test Service v1");
    options.RoutePrefix = "api/doc";
    options.HeadContent = "<style>.swagger-ui .info .url { display: none !important; }</style>";
});

//app.UseHttpsRedirection();
app.UseCors("AllowVueApp");
app.UseStaticFiles();
var storagePath = Path.Combine(builder.Environment.ContentRootPath, "Storage");
if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/storage"
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();