//using Microsoft.EntityFrameworkCore;
//using ModelClass.connection;

//var builder = WebApplication.CreateBuilder(args);

//// Kết nối PostgreSQL
//builder.Services.AddDbContext<OnlineTestDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddControllers();

//var app = builder.Build();

//app.MapControllers();
//app.Run();
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/db-test", () =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    try
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT current_database();", conn);
        var dbName = cmd.ExecuteScalar()?.ToString();

        return Results.Ok($"Connected to PostgreSQL. Current DB: {dbName}");
    }
    catch (Exception ex)
    {
        return Results.Problem("Connection failed: " + ex.Message);
    }
});

app.Run();
