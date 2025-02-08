using BackendExamHub.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Ū�� appsettings.json ���� SQL �s�u�r��A�ó]�w DbContext
builder.Services.AddDbContext<BackendExamHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ���U Controllers
builder.Services.AddControllers();

// ���U Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// �ҥ� Swagger (�T�O�b�}�o�Ҧ��i��)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BackendExamHub API v1");
        c.RoutePrefix = string.Empty; // �� Swagger UI ������ܩ� `http://localhost:5114/`
    });
}

// �ҥαj�� HTTPS
app.UseHttpsRedirection();

app.UseRouting(); // �T�O���ѥͮ�

// �]�w Controllers ����
app.UseAuthorization();
app.MapControllers();

// �� `http://localhost:5114/` �w�]�ɦV Swagger UI
app.MapFallbackToFile("index.html");

// ���եΪ� WeatherForecast API (�즳�\��O�d)
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
