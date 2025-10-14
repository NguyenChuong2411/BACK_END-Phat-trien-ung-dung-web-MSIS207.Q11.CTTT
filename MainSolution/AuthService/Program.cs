using AuthService.Services;       // Namespace của IAuthService
using AuthService.Services.impl;  // Namespace của AuthServiceImpl
using Microsoft.EntityFrameworkCore;
using ModelClass.Connection;      // Để sử dụng AuthDbContext

var builder = WebApplication.CreateBuilder(args);

// --- Cấu hình Services ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Đăng ký AuthDbContext từ project ModelClass
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Đăng ký Service và Implementation cho Dependency Injection
builder.Services.AddScoped<IAuthService, AuthServiceImpl>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --- Xây dựng App ---
var app = builder.Build();

// --- Cấu hình Middleware Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowVueApp");
app.UseAuthorization();
app.MapControllers();
app.Run();