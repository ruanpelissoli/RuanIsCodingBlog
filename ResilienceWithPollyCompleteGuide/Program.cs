using Polly;
using ResilienceWithPollyCompleteGuide.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ICatFactService, CatFactService>();

builder.Services.AddHttpClient("CatFacts", client =>
{
    client.BaseAddress = new Uri("https://catfact.ninja/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddTransientHttpErrorPolicy(builder =>
    builder.WaitAndRetryAsync(
        retryCount: 5,
        sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500)));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/catfact", async (ICatFactService catFactService) =>
{
    var catFacts = await catFactService.GetDailyFact();

    return string.IsNullOrEmpty(catFacts) 
        ? Results.NotFound() 
        : Results.Ok(catFacts);
})
.WithName("GetCatFact");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}