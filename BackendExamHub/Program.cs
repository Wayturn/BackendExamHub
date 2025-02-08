using BackendExamHub.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 讀取 appsettings.json 內的 SQL 連線字串，並設定 DbContext
builder.Services.AddDbContext<BackendExamHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 註冊 Controllers
builder.Services.AddControllers();

// 註冊 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 啟用 Swagger (確保在開發模式可用)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BackendExamHub API v1");
        c.RoutePrefix = string.Empty; // 讓 Swagger UI 直接顯示於 `http://localhost:5114/`
    });
}

// 啟用強制 HTTPS
app.UseHttpsRedirection();

app.UseRouting(); // 確保路由生效

// 設定 Controllers 路由
app.UseAuthorization();
app.MapControllers();

// 讓 `http://localhost:5114/` 預設導向 Swagger UI
app.MapFallbackToFile("index.html");

// 測試用的 WeatherForecast API (原有功能保留)
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
