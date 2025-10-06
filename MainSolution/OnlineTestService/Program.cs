using Microsoft.EntityFrameworkCore;
using ModelClass.connection;
using OnlineTestService.Service;
using OnlineTestService.Service.Impl;

var builder = WebApplication.CreateBuilder(args);

// --- Cấu hình Services ---

// 1. Thêm CORS Policy để cho phép Vue App gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        // Thay đổi port nếu cần cho khớp với port của dự án frontend
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 2. Cấu hình DbContext và kết nối PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OnlineTestDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// 3. Đăng ký các services cho Dependency Injection
// (Bạn sẽ cần tạo các file IOnlineTestService và OnlineTestService)
 builder.Services.AddScoped<IOnlineTest, OnlineTestImpl>();

// 4. Thêm Controllers
builder.Services.AddControllers();

// 5. Thêm Swagger để dễ dàng test API
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

// Kích hoạt CORS Policy đã khai báo
app.UseCors("AllowVueApp");

app.UseAuthorization();

app.MapControllers();

app.Run();