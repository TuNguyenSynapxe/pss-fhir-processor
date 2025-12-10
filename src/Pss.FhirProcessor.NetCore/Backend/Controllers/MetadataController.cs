using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Pss.FhirProcessor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetadataController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<MetadataController> _logger;

        public MetadataController(IWebHostEnvironment env, ILogger<MetadataController> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Get the unified validation metadata (single source of truth)
        /// </summary>
        [HttpGet("validation")]
        public IActionResult GetValidationMetadata()
        {
            try
            {
                // Path to the SINGLE source of truth in Pss.FhirProcessor/Metadata
                var metadataPath = Path.Combine(
                    _env.ContentRootPath,
                    "../../Pss.FhirProcessor/Metadata/validation-metadata.json"
                );

                _logger.LogInformation($"Loading metadata from: {metadataPath}");

                if (!System.IO.File.Exists(metadataPath))
                {
                    _logger.LogError($"Metadata file not found at: {metadataPath}");
                    return NotFound(new { error = "Validation metadata not found" });
                }

                var json = System.IO.File.ReadAllText(metadataPath);
                
                // Return raw JSON (already structured with Version, PathSyntax, RuleSets, CodesMaster)
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load validation metadata");
                return StatusCode(500, new { error = "Failed to load validation metadata", details = ex.Message });
            }
        }

        /// <summary>
        /// Get metadata info (version, last modified, etc.)
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetMetadataInfo()
        {
            try
            {
                var metadataPath = Path.Combine(
                    _env.ContentRootPath,
                    "../../Pss.FhirProcessor/Metadata/validation-metadata.json"
                );

                if (!System.IO.File.Exists(metadataPath))
                {
                    return NotFound(new { error = "Metadata file not found" });
                }

                var fileInfo = new FileInfo(metadataPath);
                var json = System.IO.File.ReadAllText(metadataPath);
                var metadata = Newtonsoft.Json.Linq.JObject.Parse(json);

                return Ok(new
                {
                    version = metadata["Version"]?.ToString(),
                    pathSyntax = metadata["PathSyntax"]?.ToString(),
                    ruleSetsCount = (metadata["RuleSets"] as JArray)?.Count ?? 0,
                    codesMasterQuestionsCount = (metadata["CodesMaster"]?["Questions"] as JArray)?.Count ?? 0,
                    lastModified = fileInfo.LastWriteTimeUtc,
                    fileSize = fileInfo.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get metadata info");
                return StatusCode(500, new { error = "Failed to get metadata info", details = ex.Message });
            }
        }
    }
}
