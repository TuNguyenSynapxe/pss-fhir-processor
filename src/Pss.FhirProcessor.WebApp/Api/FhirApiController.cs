using System.Web.Http;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.WebApp.Seed;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.WebApp.Api
{
    [RoutePrefix("api/fhir")]
    public class FhirApiController : ApiController
    {
        private readonly FhirProcessor _processor;

        public FhirApiController()
        {
            _processor = new FhirProcessor();
            
            // Load metadata
            _processor.LoadRuleSets(RuleSetSeed.GetRuleSets());
            _processor.LoadCodesMaster(CodesMasterSeed.GetCodesMaster());
        }

        [HttpPost]
        [Route("validate")]
        public IHttpActionResult Validate([FromBody] string fhirJson)
        {
            _processor.SetLoggingOptions(new LoggingOptions { LogLevel = "info" });
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });

            var result = _processor.Process(fhirJson);

            return Ok(new
            {
                success = result.Validation.IsValid,
                validation = result.Validation,
                flatten = (object)null,
                logs = result.Logs
            });
        }

        [HttpPost]
        [Route("extract")]
        public IHttpActionResult Extract([FromBody] string fhirJson)
        {
            _processor.SetLoggingOptions(new LoggingOptions { LogLevel = "info" });
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });

            var result = _processor.Process(fhirJson);

            return Ok(new
            {
                success = result.Validation.IsValid,
                validation = result.Validation,
                flatten = result.Flatten,
                logs = result.Logs
            });
        }

        [HttpPost]
        [Route("process")]
        public IHttpActionResult Process([FromBody] string fhirJson)
        {
            _processor.SetLoggingOptions(new LoggingOptions { LogLevel = "info" });
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });

            var result = _processor.Process(fhirJson);

            return Ok(new
            {
                success = result.Validation.IsValid,
                validation = result.Validation,
                flatten = result.Flatten,
                logs = result.Logs
            });
        }

        [HttpGet]
        [Route("codes-master")]
        public IHttpActionResult GetCodesMaster()
        {
            var json = CodesMasterSeed.GetCodesMaster();
            return Ok(json);
        }

        [HttpGet]
        [Route("rules")]
        public IHttpActionResult GetRules()
        {
            var rules = RuleSetSeed.GetRuleSets();
            return Ok(rules);
        }
    }
}
