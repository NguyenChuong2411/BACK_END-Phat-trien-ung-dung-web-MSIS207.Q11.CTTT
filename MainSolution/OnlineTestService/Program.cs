using Microsoft.EntityFrameworkCore;
using ModelClass.connection;

var builder = WebApplication.CreateBuilder(args);

// Kết nối PostgreSQL
builder.Services.AddDbContext<OnlineTestDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
