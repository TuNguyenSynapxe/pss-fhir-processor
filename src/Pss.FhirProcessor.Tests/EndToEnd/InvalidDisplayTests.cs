using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end tests for bundles with invalid display values
    /// </summary>
    [TestClass]
    public class InvalidDisplayTests
    {
        private FhirProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new FhirProcessor();
            
            var codesMaster = @"{
                ""Questions"": [
                    {
                        ""QuestionCode"": ""SQ-L2H9-00000001"",
                        ""QuestionDisplay"": ""Currently wearing hearing aid(s)?"",
                        ""ScreeningType"": ""HS"",
                        ""AllowedAnswers"": [""Yes"", ""No""]
                    }
                ]
            }";
            
            var rules = new Dictionary<string, string>
            {
                { "HS", @"{
                    ""Scope"": ""HS"",
                    ""Rules"": [
                        {
                            ""RuleType"": ""CodesMaster"",
                            ""Path"": ""Entry[].Resource.Component""
                        }
                    ]
                }" }
            };
            
            _processor.LoadCodesMaster(codesMaster);
            _processor.LoadRuleSets(rules);
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });
        }

        [TestMethod]
        public void Bundle_WrongDisplay_StrictMode_FailsValidation()
        {
            _processor.SetLoggingOptions(new LoggingOptions { LogLevel = "Verbose" });
            
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter"",
                            ""status"": ""completed""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""identifier"": [{ ""system"": ""https://fhir.synapxe.sg/identifier/nric"", ""value"": ""S1234567A"" }]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": { ""coding"": [{ ""code"": ""HS"" }] },
                            ""component"": [
                                {
                                    ""code"": {
                                        ""coding"": [{
                                            ""code"": ""SQ-L2H9-00000001"",
                                            ""display"": ""Currently wearing hearing aids?""
                                        }]
                                    },
                                    ""valueString"": ""No""
                                }
                            ]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": { ""coding"": [{ ""code"": ""OS"" }] },
                            ""component"": []
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": { ""coding"": [{ ""code"": ""VS"" }] },
                            ""component"": []
                        }
                    }
                ]
            }";

            var result = _processor.Process(json);

            // Debug output
            foreach (var log in result.Logs)
            {
                System.Console.WriteLine(log);
            }
            System.Console.WriteLine($"\nValidation.IsValid: {result.Validation.IsValid}");
            System.Console.WriteLine($"Validation.Errors.Count: {result.Validation.Errors.Count}");
            foreach (var error in result.Validation.Errors)
            {
                System.Console.WriteLine($"  Error: [{error.Code}] {error.Message}");
            }

            Assert.IsFalse(result.Validation.IsValid, "Bundle should be invalid");
            Assert.IsTrue(result.Validation.Errors.Exists(e => e.Code == "QUESTION_DISPLAY_MISMATCH"), 
                "Should have error about display mismatch");
        }

        [TestMethod]
        public void Bundle_WrongDisplay_NonStrictMode_PassesValidation()
        {
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });

            var json = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter"",
                            ""status"": ""completed""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""identifier"": [{ ""system"": ""https://fhir.synapxe.sg/identifier/nric"", ""value"": ""S1234567A"" }]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": { ""coding"": [{ ""code"": ""HS"" }] },
                            ""component"": [
                                {
                                    ""code"": {
                                        ""coding"": [{
                                            ""code"": ""SQ-L2H9-00000001"",
                                            ""display"": ""Currently wearing hearing aids?""
                                        }]
                                    },
                                    ""valueString"": ""No""
                                }
                            ]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": { ""coding"": [{ ""code"": ""OS"" }] },
                            ""component"": []
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": { ""coding"": [{ ""code"": ""VS"" }] },
                            ""component"": []
                        }
                    }
                ]
            }";

            var result = _processor.Process(json);

            Assert.IsTrue(result.Validation.IsValid, "Bundle should be valid in non-strict mode");
        }

        [TestMethod]
        public void Bundle_MissingDisplay_FailsValidation()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter"",
                            ""status"": ""completed""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""identifier"": [{ ""system"": ""https://fhir.synapxe.sg/identifier/nric"", ""value"": ""S1234567A"" }]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": { ""coding"": [{ ""code"": ""HS"" }] },
                            ""component"": [
                                {
                                    ""code"": {
                                        ""coding"": [{
                                            ""code"": ""SQ-L2H9-00000001""
                                        }]
                                    },
                                    ""valueString"": ""No""
                                }
                            ]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": { ""coding"": [{ ""code"": ""OS"" }] },
                            ""component"": []
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": { ""coding"": [{ ""code"": ""VS"" }] },
                            ""component"": []
                        }
                    }
                ]
            }";

            var result = _processor.Process(json);

            Assert.IsFalse(result.Validation.IsValid, "Bundle should be invalid");
            Assert.IsTrue(result.Validation.Errors.Exists(e => e.Code == "QUESTION_DISPLAY_MISSING"), 
                "Should have error about missing display");
        }
    }
}
