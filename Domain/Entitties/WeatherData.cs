using System.ComponentModel.DataAnnotations;

namespace Domain.Entitties;

public class WeatherData
{
    [Key]
    public int Id { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public double Temperature { get; set; }
    public DateTime LastUpdated { get; set; }
}
