using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ModelClass.connection;
using OnlineTestService.Service;
using OnlineTestService.Service.Impl;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        // Thay đổi port nếu cần cho khớp với port của frontend
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
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

// Controllers
builder.Services.AddControllers();

// Swagger để test API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowVueApp");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Storage")),
    RequestPath = "/storage"
});

app.UseAuthorization();
app.MapControllers();
app.Run();