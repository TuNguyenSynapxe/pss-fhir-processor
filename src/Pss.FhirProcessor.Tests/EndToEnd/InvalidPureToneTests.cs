using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end tests for bundles with invalid PureTone audiometry values
    /// </summary>
    [TestClass]
    public class InvalidPureToneTests
    {
        private FhirProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new FhirProcessor();
            
            var codesMaster = @"{
                ""Questions"": [
                    {
                        ""QuestionCode"": ""SQ-L2H9-00000002"",
                        ""QuestionDisplay"": ""Pure Tone Audiometry Test Result"",
                        ""ScreeningType"": ""HS"",
                        ""AllowedAnswers"": [""500Hz – R"", ""500Hz – NR"", ""1000Hz – R"", ""1000Hz – NR"", ""2000Hz – R"", ""2000Hz – NR"", ""3000Hz – R"", ""3000Hz – NR"", ""4000Hz – R"", ""4000Hz – NR""]
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
        public void Bundle_InvalidPureToneFormat_FailsValidation()
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
                                            ""code"": ""SQ-L2H9-00000002"",
                                            ""display"": ""Pure Tone Audiometry Test Result""
                                        }]
                                    },
                                    ""valueString"": ""500Hz|1000Hz""
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
            Assert.IsTrue(result.Validation.Errors.Count > 0, "Should have validation errors");
        }

        [TestMethod]
        public void Bundle_ValidPureTonePipeSeparated_PassesValidation()
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
                                            ""code"": ""SQ-L2H9-00000002"",
                                            ""display"": ""Pure Tone Audiometry Test Result""
                                        }]
                                    },
                                    ""valueString"": ""500Hz – R|1000Hz – R|2000Hz – NR""
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
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Location"",
                            ""id"": ""loc1""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""HealthcareService"",
                            ""id"": ""hs1""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization"",
                            ""id"": ""org1""
                        }
                    }
                ]
            }";

            var result = _processor.Process(json);

            Assert.IsTrue(result.Validation.IsValid, "Bundle should be valid");
        }

        [TestMethod]
        public void Bundle_InvalidPureToneValue_FailsValidation()
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
                                            ""code"": ""SQ-L2H9-00000002"",
                                            ""display"": ""Pure Tone Audiometry Test Result""
                                        }]
                                    },
                                    ""valueString"": ""500Hz – R|1000Hz – XYZ|2000Hz – NR""
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
            Assert.IsTrue(result.Validation.Errors.Exists(e => e.Message.Contains("1000Hz – XYZ") || e.Message.Contains("allowed")), 
                "Should have error about invalid PureTone value");
        }

        [TestMethod]
        public void Bundle_EmptyPureTone_FailsValidation()
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
                                            ""code"": ""SQ-L2H9-00000002"",
                                            ""display"": ""Pure Tone Audiometry Test Result""
                                        }]
                                    },
                                    ""valueString"": """"
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

            Assert.IsFalse(result.Validation.IsValid);
        }

        [TestMethod]
        public void Bundle_SinglePureToneValue_PassesValidation()
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
                                            ""code"": ""SQ-L2H9-00000002"",
                                            ""display"": ""Pure Tone Audiometry Test Result""
                                        }]
                                    },
                                    ""valueString"": ""500Hz – R""
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
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Location"",
                            ""id"": ""loc1""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""HealthcareService"",
                            ""id"": ""hs1""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization"",
                            ""id"": ""org1""
                        }
                    }
                ]
            }";

            var result = _processor.Process(json);

            Assert.IsTrue(result.Validation.IsValid, "Bundle should be valid with single value");
        }
    }
}
