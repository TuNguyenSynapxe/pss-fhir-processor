using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Pss.FhirProcessor.Api.Filters
{
    /// <summary>
    /// API Key authentication filter for Sandbox endpoints
    /// </summary>
    public class ApiKeyAuthFilter : IAuthorizationFilter
    {
        private const string API_KEY_HEADER = "X-API-Key";
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyAuthFilter> _logger;

        public ApiKeyAuthFilter(IConfiguration configuration, ILogger<ApiKeyAuthFilter> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Extract API key from header
            if (!context.HttpContext.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
            {
                _logger.LogWarning("API Key missing in request from {IP}", context.HttpContext.Connection.RemoteIpAddress);
                context.Result = new UnauthorizedObjectResult(new
                {
                    error = "API Key is missing",
                    message = $"Please provide a valid API key in the '{API_KEY_HEADER}' header"
                });
                return;
            }

            // Get valid API keys from configuration
            var validApiKeys = _configuration.GetSection("ApiKeys").Get<ApiKeyConfig[]>();
            
            if (validApiKeys == null || validApiKeys.Length == 0)
            {
                _logger.LogError("No API keys configured in appsettings.json");
                context.Result = new StatusCodeResult(500);
                return;
            }

            // Validate the API key
            var apiKey = extractedApiKey.ToString();
            var matchedKey = validApiKeys.FirstOrDefault(k => k.Key == apiKey && k.IsActive);

            if (matchedKey == null)
            {
                _logger.LogWarning("Invalid API Key attempted from {IP}: {Key}", 
                    context.HttpContext.Connection.RemoteIpAddress, 
                    apiKey?.Substring(0, Math.Min(8, apiKey.Length)) + "...");
                    
                context.Result = new UnauthorizedObjectResult(new
                {
                    error = "Invalid API Key",
                    message = "The provided API key is invalid or inactive"
                });
                return;
            }

            // Log successful authentication
            _logger.LogInformation("API request authenticated: Organization={Org}, Key={KeyId}", 
                matchedKey.Organization, 
                matchedKey.KeyId);

            // Store API key info in HttpContext for later use (rate limiting, logging, etc.)
            context.HttpContext.Items["ApiKeyConfig"] = matchedKey;
        }
    }

    /// <summary>
    /// API Key configuration model
    /// </summary>
    public class ApiKeyConfig
    {
        public string KeyId { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int RateLimitPerHour { get; set; } = 100;
        public string[] AllowedEndpoints { get; set; } = new[] { "process", "validate", "extract" };
    }
}
