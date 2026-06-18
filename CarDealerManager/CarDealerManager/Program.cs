using CarDealerManager.Common.AppSettings;
using CarDealerManager.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
AppSettingsProvider.AddAppSettings(builder.Configuration);
builder.Services.ConfigureDbContextService();
builder.Services.ConfigureRepositories();
builder.Services.ConfigureServices();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
