using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MOH.HealthierSG.PSS.FhirProcessor.Api.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Pss.FhirProcessor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetadataController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<MetadataController> _logger;
        private readonly ISeedFileService _seedFileService;

        public MetadataController(IWebHostEnvironment env, ILogger<MetadataController> logger, ISeedFileService seedFileService)
        {
            _env = env;
            _logger = logger;
            _seedFileService = seedFileService;
        }

        /// <summary>
        /// Get the unified validation metadata (single source of truth)
        /// NOW READS FROM SEED FILE DYNAMICALLY
        /// </summary>
        [HttpGet("validation")]
        public async Task<IActionResult> GetValidationMetadata()
        {
            try
            {
                _logger.LogInformation("Loading validation metadata from seed file");
                
                var content = await _seedFileService.GetPublicSeedFileContent("validation-metadata.json");
                
                // Return the content directly as JSON
                return Content(content, "application/json");
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
