using Domain.Entitties;
using Microsoft.EntityFrameworkCore;

namespace Repository.Contexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<WeatherData> WeatherData { get; set; }
}
