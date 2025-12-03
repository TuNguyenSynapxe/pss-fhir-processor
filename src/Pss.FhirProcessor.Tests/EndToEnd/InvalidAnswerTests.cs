using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end tests for bundles with invalid answer values
    /// </summary>
    [TestClass]
    public class InvalidAnswerTests
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
                    },
                    {
                        ""QuestionCode"": ""SQ-L2O9-00000001"",
                        ""QuestionDisplay"": ""Currently wearing dentures?"",
                        ""ScreeningType"": ""OS"",
                        ""AllowedAnswers"": [""Yes"", ""No""]
                    },
                    {
                        ""QuestionCode"": ""SQ-L2V9-00000001"",
                        ""QuestionDisplay"": ""Currently wearing glasses?"",
                        ""ScreeningType"": ""VS"",
                        ""AllowedAnswers"": [""Yes"", ""No"", ""Sometimes""]
                    }
                ]
            }";
            
            _processor.LoadCodesMaster(codesMaster);
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });
        }

        [TestMethod]
        public void Bundle_InvalidAnswer_FailsValidation()
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
                                            ""code"": ""SQ-L2H9-00000001"",
                                            ""display"": ""Currently wearing hearing aid(s)?""
                                        }]
                                    },
                                    ""valueString"": ""Maybe""
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
            Assert.IsTrue(result.Validation.Errors.Exists(e => 
                e.Message.Contains("allowed") || e.Message.Contains("valid")), 
                "Should have error about invalid answer value");
        }

        [TestMethod]
        public void Bundle_MultipleInvalidAnswers_ReportsAllErrors()
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
                                            ""code"": ""SQ-L2H9-00000001"",
                                            ""display"": ""Currently wearing hearing aid(s)?""
                                        }]
                                    },
                                    ""valueString"": ""Maybe""
                                }
                            ]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": { ""coding"": [{ ""code"": ""OS"" }] },
                            ""component"": [
                                {
                                    ""code"": {
                                        ""coding"": [{
                                            ""code"": ""SQ-L2O9-00000001"",
                                            ""display"": ""Currently wearing dentures?""
                                        }]
                                    },
                                    ""valueString"": ""Unknown""
                                }
                            ]
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
            Assert.IsTrue(result.Validation.Errors.Count >= 2, "Should report multiple errors");
        }

        [TestMethod]
        public void Bundle_EmptyAnswer_FailsValidation()
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
                                            ""code"": ""SQ-L2H9-00000001"",
                                            ""display"": ""Currently wearing hearing aid(s)?""
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
        public void Bundle_ValidAlternativeAnswer_PassesValidation()
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
                            ""component"": []
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
                            ""component"": [
                                {
                                    ""code"": {
                                        ""coding"": [{
                                            ""code"": ""SQ-L2V9-00000001"",
                                            ""display"": ""Currently wearing glasses?""
                                        }]
                                    },
                                    ""valueString"": ""Sometimes""
                                }
                            ]
                        }
                    }
                ]
            }";

            var result = _processor.Process(json);

            Assert.IsTrue(result.Validation.IsValid, "Bundle should be valid with alternative answer");
        }
    }
}
