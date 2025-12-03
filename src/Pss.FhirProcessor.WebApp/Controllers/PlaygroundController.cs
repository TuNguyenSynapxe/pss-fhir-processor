using System.Web.Mvc;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.WebApp.Models;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.WebApp.Seed;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.WebApp.Controllers
{
    public class PlaygroundController : Controller
    {
        private readonly FhirProcessor _processor;

        public PlaygroundController()
        {
            _processor = new FhirProcessor();
            
            // Load metadata
            _processor.LoadRuleSets(RuleSetSeed.GetRuleSets());
            _processor.LoadCodesMaster(CodesMasterSeed.GetCodesMaster());
        }

        public ActionResult Index()
        {
            var model = new PlaygroundViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Process(string fhirJson, string logLevel = "info", bool strictDisplay = true)
        {
            // Set options
            _processor.SetLoggingOptions(new LoggingOptions { LogLevel = logLevel });
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = strictDisplay });

            // Process
            var result = _processor.Process(fhirJson);

            var model = new PlaygroundResultViewModel
            {
                InputJson = fhirJson,
                ProcessResult = result
            };

            return View("Result", model);
        }

        [HttpGet]
        public ActionResult GetSampleJson(string sampleName)
        {
            var sample = TestCaseSeed.GetTestCase(sampleName);
            return Json(new { json = sample?.InputJson }, JsonRequestBehavior.AllowGet);
        }
    }
}
