using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.WebApp.Models;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.WebApp.Seed;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.WebApp.Controllers
{
    public class TestController : Controller
    {
        private readonly FhirProcessor _processor;

        public TestController()
        {
            _processor = new FhirProcessor();
            
            // Load metadata
            _processor.LoadRuleSets(RuleSetSeed.GetRuleSets());
            _processor.LoadCodesMaster(CodesMasterSeed.GetCodesMaster());
        }

        public ActionResult Index()
        {
            var testCases = TestCaseSeed.GetAllTestCases();
            return View(testCases);
        }

        [HttpPost]
        public ActionResult RunTest(string testName)
        {
            var testCase = TestCaseSeed.GetTestCase(testName);
            if (testCase == null)
            {
                return Json(new { success = false, message = "Test case not found" });
            }

            _processor.SetLoggingOptions(new LoggingOptions { LogLevel = "info" });
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });

            var result = _processor.Process(testCase.InputJson);

            return Json(new
            {
                success = true,
                testName = testCase.Name,
                expectedValid = testCase.ExpectedIsValid,
                actualValid = result.Validation.IsValid,
                passed = result.Validation.IsValid == testCase.ExpectedIsValid,
                errors = result.Validation.Errors,
                logs = result.Logs
            });
        }

        [HttpPost]
        public ActionResult RunAllTests()
        {
            var testCases = TestCaseSeed.GetAllTestCases();
            var results = new List<object>();

            _processor.SetLoggingOptions(new LoggingOptions { LogLevel = "info" });
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });

            foreach (var testCase in testCases)
            {
                var result = _processor.Process(testCase.InputJson);

                results.Add(new
                {
                    testName = testCase.Name,
                    expectedValid = testCase.ExpectedIsValid,
                    actualValid = result.Validation.IsValid,
                    passed = result.Validation.IsValid == testCase.ExpectedIsValid,
                    errorCount = result.Validation.Errors.Count
                });
            }

            var passedCount = results.Count(r => (bool)((dynamic)r).passed);

            return Json(new
            {
                success = true,
                totalTests = results.Count,
                passedTests = passedCount,
                failedTests = results.Count - passedCount,
                results = results
            });
        }
    }
}
