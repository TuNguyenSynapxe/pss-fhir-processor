using System;
using Microsoft.AspNetCore.Mvc;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Api.Models;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Api.Services;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Api.Seed;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FhirController : ControllerBase
    {
        private readonly IFhirProcessorService _fhirService;

        public FhirController(IFhirProcessorService fhirService)
        {
            _fhirService = fhirService;
        }

        [HttpPost("validate")]
        public IActionResult Validate([FromBody] ValidateRequest request)
        {
            try
            {
                var result = _fhirService.ValidateOnly(request.FhirJson, request.ValidationMetadata, request.LogLevel, request.StrictDisplayMatch);

                return Ok(new
                {
                    success = result.Validation?.IsValid ?? false,
                    validation = result.Validation,
                    originalBundle = result.OriginalBundle,
                    logs = result.Logs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("extract")]
        public IActionResult Extract([FromBody] ExtractRequest request)
        {
            try
            {
                var result = _fhirService.ExtractOnly(request.FhirJson, request.LogLevel);

                return Ok(new
                {
                    success = result.Flatten != null,
                    flatten = result.Flatten,
                    originalBundle = result.OriginalBundle,
                    logs = result.Logs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("process")]
        public IActionResult Process([FromBody] ProcessRequest request)
        {
            try
            {
                var result = _fhirService.Process(request.FhirJson, request.ValidationMetadata, request.LogLevel, request.StrictDisplayMatch);

                return Ok(new
                {
                    success = result.Validation.IsValid,
                    validation = result.Validation,
                    flatten = result.Flatten,
                    originalBundle = result.OriginalBundle,
                    logs = result.Logs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("test-cases")]
        public IActionResult GetTestCases()
        {
            var testCases = TestCaseSeed.GetAllTestCases();
            return Ok(testCases);
        }

        [HttpGet("test-cases/{name}")]
        public IActionResult GetTestCase(string name)
        {
            var testCase = TestCaseSeed.GetTestCase(name);
            if (testCase == null)
            {
                return NotFound(new { error = "Test case not found" });
            }
            return Ok(testCase);
        }
    }
}
