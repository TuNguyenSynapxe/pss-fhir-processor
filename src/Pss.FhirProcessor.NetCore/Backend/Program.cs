using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MOH.HealthierSG.PSS.FhirProcessor;
using MOH.HealthierSG.PSS.FhirProcessor.Api.Services;
using Pss.FhirProcessor.Api.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PSS FHIR Processor API", Version = "v1" });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Register FhirProcessor as singleton
builder.Services.AddSingleton<IFhirProcessorService, FhirProcessorService>();

// Register API Key authentication filter
builder.Services.AddScoped<ApiKeyAuthFilter>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowFrontend");
}
else
{
    // Production: Serve static files and enable SPA fallback
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

// SPA fallback route - serve index.html for non-API routes
if (!app.Environment.IsDevelopment())
{
    app.MapFallbackToFile("index.html");
}

app.Run();
