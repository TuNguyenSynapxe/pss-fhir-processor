using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class RequiredRuleTests
    {
        private FhirProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new FhirProcessor();
            
            var rules = new Dictionary<string, string>
            {
                { "Test", @"{
                    ""Scope"": ""Test"",
                    ""Rules"": [{
                        ""RuleType"": ""Required"",
                        ""Path"": ""Entry[0].Resource.Status""
                    }]
                }" }
            };
            
            _processor.LoadRuleSets(rules);
        }

        [TestMethod]
        public void RequiredField_Missing_ReturnsError()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [{
                    ""resource"": {
                        ""resourceType"": ""Encounter""
                    }
                }]
            }";

            var result = _processor.Validate(json);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Exists(e => e.Code == "MANDATORY_MISSING"));
        }

        [TestMethod]
        public void RequiredField_Present_Passes()
        {
            var processor2 = new FhirProcessor();
            processor2.SetLoggingOptions(new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation.LoggingOptions { LogLevel = "info" });
            processor2.LoadRuleSets(new Dictionary<string, string>
            {
                { "Test", @"{
                    ""Scope"": ""Test"",
                    ""Rules"": [{
                        ""RuleType"": ""Required"",
                        ""Path"": ""Entry[0].Resource.Status""
                    }]
                }" }
            });

            var json = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [{
                    ""resource"": {
                        ""resourceType"": ""Encounter"",
                        ""status"": ""completed""
                    }
                }]
            }";

            var result2 = processor2.Process(json);
            
            System.Console.WriteLine($"\n=== DEBUG: RequiredField_Present_Passes ===");
            System.Console.WriteLine($"Validation: {(result2.Validation.IsValid ? "VALID" : "INVALID")}");
            System.Console.WriteLine($"Errors: {result2.Validation.Errors.Count}");
            foreach (var err in result2.Validation.Errors)
            {
                System.Console.WriteLine($"  Error: [{err.Code}] {err.FieldPath} - {err.Message}");
            }
            System.Console.WriteLine($"Logs: {result2.Logs.Count}");
            foreach (var log in result2.Logs)
            {
                System.Console.WriteLine($"  {log}");
            }
            System.Console.WriteLine($"=== END DEBUG ===\n");

            var result = _processor.Validate(json);

            Assert.IsTrue(result.IsValid || !result.Errors.Exists(e => e.Code == "MANDATORY_MISSING" && e.FieldPath.Contains("Status")));
        }

        [TestMethod]
        public void RequiredField_EmptyString_ReturnsError()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [{
                    ""resource"": {
                        ""resourceType"": ""Encounter"",
                        ""status"": """"
                    }
                }]
            }";

            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "MANDATORY_MISSING"));
        }
    }
}
