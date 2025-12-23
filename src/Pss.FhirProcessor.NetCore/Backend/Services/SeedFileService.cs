using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MOH.HealthierSG.PSS.FhirProcessor.Api.Services
{
    public interface ISeedFileService
    {
        Task<string> GetSeedFileContent(string fileName);
        Task SaveSeedFileContent(string fileName, string content);
        Task<bool> ValidatePassword(string password);
        Task<string> GetPublicSeedFileContent(string fileName);
    }

    public class SeedFileService : ISeedFileService
    {
        private const string ADMIN_PASSWORD = "Synapxe@CRM@PSS";
        private readonly string _seedDataPath;
        private readonly ILogger<SeedFileService> _logger;

        // Allowed seed files
        private static readonly string[] AllowedFiles = new[]
        {
            "happy-sample-full.json",
            "validation-metadata.json"
        };

        public SeedFileService(IWebHostEnvironment environment, ILogger<SeedFileService> logger)
        {
            _logger = logger;
            
            // Use /home/data/seed ONLY for Azure (detected by WEBSITE_INSTANCE_ID)
            // Otherwise use local development path
            if (Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") != null)
            {
                _seedDataPath = "/home/data/seed";
                _logger.LogInformation("Running on Azure - using /home/data/seed");
            }
            else
            {
                // Local development path
                var projectRoot = Directory.GetParent(environment.ContentRootPath)?.FullName 
                    ?? environment.ContentRootPath;
                _seedDataPath = Path.Combine(projectRoot, "Frontend", "src", "seed");
                _logger.LogInformation("Running locally - using Frontend/src/seed");
            }

            _logger.LogInformation($"SeedFileService initialized with path: {_seedDataPath}");
            
            // Ensure directory exists
            try
            {
                if (!Directory.Exists(_seedDataPath))
                {
                    Directory.CreateDirectory(_seedDataPath);
                    _logger.LogInformation($"Created seed directory: {_seedDataPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create seed directory: {_seedDataPath}");
                throw new InvalidOperationException($"Cannot create seed directory at {_seedDataPath}. Ensure the path is writable.", ex);
            }
        }

        public Task<bool> ValidatePassword(string password)
        {
            return Task.FromResult(password == ADMIN_PASSWORD);
        }

        public async Task<string> GetSeedFileContent(string fileName)
        {
            ValidateFileName(fileName);
            
            var filePath = Path.Combine(_seedDataPath, fileName);
            
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"Seed file not found: {filePath}");
                return "{}"; // Return empty JSON if file doesn't exist
            }

            _logger.LogInformation($"Reading seed file: {filePath}");
            return await File.ReadAllTextAsync(filePath);
        }

        public async Task SaveSeedFileContent(string fileName, string content)
        {
            ValidateFileName(fileName);
            
            var filePath = Path.Combine(_seedDataPath, fileName);
            
            _logger.LogInformation($"Saving seed file: {filePath}");
            await File.WriteAllTextAsync(filePath, content);
            _logger.LogInformation($"Successfully saved seed file: {filePath}");
        }

        public async Task<string> GetPublicSeedFileContent(string fileName)
        {
            // Same as GetSeedFileContent but used for public endpoints (no auth required)
            ValidateFileName(fileName);
            
            var filePath = Path.Combine(_seedDataPath, fileName);
            
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"Seed file not found: {filePath}");
                return "{}"; // Return empty JSON if file doesn't exist
            }

            _logger.LogInformation($"Reading seed file (public): {filePath}");
            return await File.ReadAllTextAsync(filePath);
        }

        private void ValidateFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty");
            }

            if (!Array.Exists(AllowedFiles, f => f.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"File name '{fileName}' is not allowed. Allowed files: {string.Join(", ", AllowedFiles)}");
            }

            // Additional security check to prevent path traversal
            if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            {
                throw new ArgumentException("File name contains invalid characters");
            }
        }
    }
}
