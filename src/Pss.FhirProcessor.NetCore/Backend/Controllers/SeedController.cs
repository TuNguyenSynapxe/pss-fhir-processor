using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MOH.HealthierSG.PSS.FhirProcessor.Api.Services;

namespace MOH.HealthierSG.PSS.FhirProcessor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ISeedFileService _seedFileService;
        private readonly ILogger<SeedController> _logger;

        public SeedController(ISeedFileService seedFileService, ILogger<SeedController> logger)
        {
            _seedFileService = seedFileService;
            _logger = logger;
        }

        /// <summary>
        /// Validate admin password
        /// </summary>
        [HttpPost("validate-password")]
        public async Task<IActionResult> ValidatePassword([FromBody] PasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Password is required" });
            }

            var isValid = await _seedFileService.ValidatePassword(request.Password);
            
            if (isValid)
            {
                _logger.LogInformation("Admin password validated successfully");
                return Ok(new { valid = true, message = "Password validated" });
            }

            _logger.LogWarning("Invalid admin password attempt");
            return Unauthorized(new { valid = false, message = "Invalid password" });
        }

        /// <summary>
        /// Get seed file content
        /// </summary>
        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetSeedFile(string fileName, [FromHeader(Name = "X-Admin-Password")] string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return Unauthorized(new { message = "Admin password required" });
            }

            var isValidPassword = await _seedFileService.ValidatePassword(password);
            if (!isValidPassword)
            {
                return Unauthorized(new { message = "Invalid password" });
            }

            try
            {
                var content = await _seedFileService.GetSeedFileContent(fileName);
                _logger.LogInformation($"Retrieved seed file: {fileName}");
                return Ok(new { fileName, content });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Invalid file name: {fileName}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reading seed file: {fileName}");
                return StatusCode(500, new { message = "Error reading file" });
            }
        }

        /// <summary>
        /// Update seed file content
        /// </summary>
        [HttpPost("{fileName}")]
        public async Task<IActionResult> UpdateSeedFile(string fileName, [FromBody] UpdateSeedFileRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Password))
            {
                return Unauthorized(new { message = "Admin password required" });
            }

            var isValidPassword = await _seedFileService.ValidatePassword(request.Password);
            if (!isValidPassword)
            {
                return Unauthorized(new { message = "Invalid password" });
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { message = "Content is required" });
            }

            try
            {
                await _seedFileService.SaveSeedFileContent(fileName, request.Content);
                _logger.LogInformation($"Updated seed file: {fileName}");
                return Ok(new { message = $"File '{fileName}' updated successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Invalid file name: {fileName}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving seed file: {fileName}");
                return StatusCode(500, new { message = "Error saving file" });
            }
        }

        /// <summary>
        /// Get list of available seed files
        /// </summary>
        [HttpGet("files")]
        public async Task<IActionResult> GetAvailableFiles([FromHeader(Name = "X-Admin-Password")] string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return Unauthorized(new { message = "Admin password required" });
            }

            var isValidPassword = await _seedFileService.ValidatePassword(password);
            if (!isValidPassword)
            {
                return Unauthorized(new { message = "Invalid password" });
            }

            var files = new[]
            {
                new { name = "happy-sample-full.json", description = "Happy Path Sample FHIR Bundle" },
                new { name = "validation-metadata.json", description = "Validation Rules Metadata" }
            };

            return Ok(files);
        }

        /// <summary>
        /// Get seed file content (public endpoint - no auth required)
        /// </summary>
        [HttpGet("public/{fileName}")]
        public async Task<IActionResult> GetPublicSeedFile(string fileName)
        {
            try
            {
                var content = await _seedFileService.GetPublicSeedFileContent(fileName);
                _logger.LogInformation($"Retrieved seed file (public): {fileName}");
                return Content(content, "application/json");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Invalid file name: {fileName}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reading seed file: {fileName}");
                return StatusCode(500, new { message = "Error reading file" });
            }
        }
    }

    public class PasswordRequest
    {
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateSeedFileRequest
    {
        public string Password { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
