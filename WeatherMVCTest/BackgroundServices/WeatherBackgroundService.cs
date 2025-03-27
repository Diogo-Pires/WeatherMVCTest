using Application.Interfaces;
using Domain.Entitties;
using Shared.Interfaces;
using WeatherMVCTest.Contexts;

namespace WeatherMVCTest.BackgroundServices;

public class WeatherBackgroundService(IServiceProvider serviceProvider,
                                      IOpenWeatherMapService openWeatherMapService,
                                      IDateTimeProvider dateTimeProvider,
                                      ILogger<WeatherBackgroundService> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IOpenWeatherMapService _openWeatherMapService = openWeatherMapService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<WeatherBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await FetchAndStoreWeatherData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather data");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task FetchAndStoreWeatherData()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        string[] _cities = ["London,UK", "New York,US", "Lisbon,PT"];
        foreach (var city in _cities)
        {
            try
            {
                var content = await _openWeatherMapService.GetWeatherContentAsync() ?? throw new Exception("No data found");

                if (content != null)
                {
                    var weatherEntry = new WeatherData
                    {
                        Country = content.Sys.Country,
                        City = city,
                        Temperature = content.Main.Temp,
                        LastUpdated = _dateTimeProvider.GetUTCNow()
                    };

                    dbContext.WeatherData.Add(weatherEntry);
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Saved weather data for {city}: {weatherEntry.Temperature}°C");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching data for {city}");
            }
        }
    }
}