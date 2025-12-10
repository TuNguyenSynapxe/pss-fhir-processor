using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pss.FhirProcessor.Api.Filters;
using MOH.HealthierSG.PSS.FhirProcessor.Api.Services;
using System;

namespace Pss.FhirProcessor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SandboxController : ControllerBase
    {
        private readonly IFhirProcessorService _fhirService;
        private readonly MetadataController _metadataController;
        private readonly ILogger<SandboxController> _logger;

        public SandboxController(
            IFhirProcessorService fhirService,
            MetadataController metadataController,
            ILogger<SandboxController> logger)
        {
            _fhirService = fhirService;
            _metadataController = metadataController;
            _logger = logger;
        }

        /// <summary>
        /// Process FHIR Bundle: validate and extract
        /// </summary>
        [HttpPost("process")]
        [ServiceFilter(typeof(ApiKeyAuthFilter))]
        public IActionResult Process([FromBody] SandboxRequest request)
        {
            try
            {
                var apiKeyConfig = HttpContext.Items["ApiKeyConfig"] as ApiKeyConfig;
                _logger.LogInformation("Sandbox Process request from {Org}", apiKeyConfig?.Organization);

                // Get server-side metadata (single source of truth)
                var metadataResult = _metadataController.GetValidationMetadata();
                if (metadataResult is not ContentResult contentResult)
                {
                    return StatusCode(500, new { error = "Failed to load validation metadata" });
                }

                var result = _fhirService.Process(
                    request.FhirJson,
                    contentResult.Content,
                    request.LogLevel ?? "info",
                    request.StrictDisplay ?? true
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sandbox Process failed");
                return BadRequest(new { error = "Processing failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Validate FHIR Bundle only
        /// </summary>
        [HttpPost("validate")]
        [ServiceFilter(typeof(ApiKeyAuthFilter))]
        public IActionResult Validate([FromBody] SandboxRequest request)
        {
            try
            {
                var apiKeyConfig = HttpContext.Items["ApiKeyConfig"] as ApiKeyConfig;
                _logger.LogInformation("Sandbox Validate request from {Org}", apiKeyConfig?.Organization);

                var metadataResult = _metadataController.GetValidationMetadata();
                if (metadataResult is not ContentResult contentResult)
                {
                    return StatusCode(500, new { error = "Failed to load validation metadata" });
                }

                var result = _fhirService.ValidateOnly(
                    request.FhirJson,
                    contentResult.Content,
                    request.LogLevel ?? "info",
                    request.StrictDisplay ?? true
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sandbox Validate failed");
                return BadRequest(new { error = "Validation failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Extract data from FHIR Bundle only
        /// </summary>
        [HttpPost("extract")]
        [ServiceFilter(typeof(ApiKeyAuthFilter))]
        public IActionResult Extract([FromBody] SandboxRequest request)
        {
            try
            {
                var apiKeyConfig = HttpContext.Items["ApiKeyConfig"] as ApiKeyConfig;
                _logger.LogInformation("Sandbox Extract request from {Org}", apiKeyConfig?.Organization);

                var result = _fhirService.ExtractOnly(
                    request.FhirJson,
                    request.LogLevel ?? "info"
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sandbox Extract failed");
                return BadRequest(new { error = "Extraction failed", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Sandbox API request model
    /// </summary>
    public class SandboxRequest
    {
        public string FhirJson { get; set; } = string.Empty;
        public string? LogLevel { get; set; }
        public bool? StrictDisplay { get; set; }
    }
}
