using Application.Services;
using Application.Settings;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using Shared;
using Shared.Interfaces;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using WeatherMVCTest.BackgroundServices;
using WeatherMVCTest.Contexts;

var builder = WebApplication.CreateBuilder(args);
var corsPolicyName = "AllowFrontend";

// Add services to the container.
builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var fallbackPolicy = Policy<HttpResponseMessage>
    .Handle<HttpRequestException>()
    .OrResult(response => response.StatusCode == HttpStatusCode.ServiceUnavailable)
    .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent(string.Empty)
    });

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

builder.Services.AddHttpClient<IOpenWeatherMapService, OpenWeatherMapService>("openweather", (serviceProvider, client) =>
{
    var settings = serviceProvider
        .GetRequiredService<IOptions<OpenWeatherMapSettings>>().Value;

    client.BaseAddress = new Uri(settings.Url);
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(fallbackPolicy.WrapAsync(retryPolicy));

var corsSettings = builder.Configuration.GetSection("CORS:FrontendURLs").Get<List<string>>()!;
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, builder =>
    {
        builder.WithOrigins([.. corsSettings])
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<WeatherBackgroundService>();

builder.Services.AddSingleton<IOpenWeatherMapService, OpenWeatherMapService>();
builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();

builder.Services.Configure<OpenWeatherMapSettings>(builder.Configuration.GetSection("OpenWeatherMapSettings"));


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(corsPolicyName);
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
