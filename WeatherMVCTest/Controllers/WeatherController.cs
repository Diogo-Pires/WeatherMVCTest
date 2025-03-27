using Domain.Entitties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherMVCTest.Contexts;

namespace WeatherMVCTest.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    /// <summary>
    /// Gets Weather Logs
    /// </summary>
    /// <returns><see cref="List<WeatherData>"/></returns>
    /// <remarks>
    /// Usage Example:
    /// GET api/weather/
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeatherData>>> GetWeatherData()
    {
        var weatherData = await _dbContext.WeatherData
            .OrderByDescending(w => w.LastUpdated) 
            .ToListAsync();

        return Ok(weatherData);
    }

    /// <summary>
    /// Gets Weather Logs
    /// </summary>
    /// <returns><see cref="List<WeatherData>"/></returns>
    /// <remarks>
    /// Usage Example:
    /// GET api/weather/{city}
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    [HttpGet("{city}")]
    public async Task<ActionResult<IEnumerable<WeatherData>>> GetWeatherByCity(string city)
    {
        if (city == null)
        {
            return BadRequest("A city must be provided");
        }

        var weatherData = await _dbContext.WeatherData
            .Where(w => w.City.Equals(city, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(w => w.LastUpdated)
            .ToListAsync();

        if (weatherData.Count == 0)
        {
            return NotFound($"No weather data found for city: {city}");
        }

        return Ok(weatherData);
    }
}