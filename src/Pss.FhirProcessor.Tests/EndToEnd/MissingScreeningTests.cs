using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end tests for bundles missing required screening types
    /// </summary>
    [TestClass]
    public class MissingScreeningTests
    {
        private FhirProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new FhirProcessor();
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });
        }

        [TestMethod]
        public void Bundle_MissingHearingScreening_FailsValidation()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter"",
                            ""status"": ""completed"",
                            ""actualPeriod"": {
                                ""start"": ""2025-01-10T09:00:00+08:00"",
                                ""end"": ""2025-01-10T09:20:00+08:00""
                            }
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""identifier"": [
                                { ""system"": ""https://fhir.synapxe.sg/identifier/nric"", ""value"": ""S1234567A"" }
                            ],
                            ""name"": [{ ""text"": ""John Tan"" }],
                            ""gender"": ""male"",
                            ""birthDate"": ""1950-01-01""
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
            Assert.IsTrue(result.Validation.Errors.Exists(e => e.Message.Contains("HS")), 
                "Should have error about missing Hearing Screening");
        }

        [TestMethod]
        public void Bundle_MissingOralScreening_FailsValidation()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter"",
                            ""status"": ""completed"",
                            ""actualPeriod"": {
                                ""start"": ""2025-01-10T09:00:00+08:00"",
                                ""end"": ""2025-01-10T09:20:00+08:00""
                            }
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""identifier"": [
                                { ""system"": ""https://fhir.synapxe.sg/identifier/nric"", ""value"": ""S1234567A"" }
                            ],
                            ""name"": [{ ""text"": ""John Tan"" }],
                            ""gender"": ""male"",
                            ""birthDate"": ""1950-01-01""
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
                            ""code"": { ""coding"": [{ ""code"": ""VS"" }] },
                            ""component"": []
                        }
                    }
                ]
            }";

            var result = _processor.Process(json);

            Assert.IsFalse(result.Validation.IsValid, "Bundle should be invalid");
            Assert.IsTrue(result.Validation.Errors.Exists(e => e.Message.Contains("OS")), 
                "Should have error about missing Oral Screening");
        }

        [TestMethod]
        public void Bundle_MissingVisionScreening_FailsValidation()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter"",
                            ""status"": ""completed"",
                            ""actualPeriod"": {
                                ""start"": ""2025-01-10T09:00:00+08:00"",
                                ""end"": ""2025-01-10T09:20:00+08:00""
                            }
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""identifier"": [
                                { ""system"": ""https://fhir.synapxe.sg/identifier/nric"", ""value"": ""S1234567A"" }
                            ],
                            ""name"": [{ ""text"": ""John Tan"" }],
                            ""gender"": ""male"",
                            ""birthDate"": ""1950-01-01""
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
                    }
                ]
            }";

            var result = _processor.Process(json);

            Assert.IsFalse(result.Validation.IsValid, "Bundle should be invalid");
            Assert.IsTrue(result.Validation.Errors.Exists(e => e.Message.Contains("VS")), 
                "Should have error about missing Vision Screening");
        }

        [TestMethod]
        public void Bundle_MissingAllScreenings_ReportsAllErrors()
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
                            ""identifier"": [
                                { ""system"": ""https://fhir.synapxe.sg/identifier/nric"", ""value"": ""S1234567A"" }
                            ]
                        }
                    }
                ]
            }";

            var result = _processor.Process(json);

            Assert.IsFalse(result.Validation.IsValid);
            var errorMsg = string.Join(" ", result.Validation.Errors.ConvertAll(e => e.Message));
            Assert.IsTrue(errorMsg.Contains("HS") || errorMsg.Contains("Hearing"));
            Assert.IsTrue(errorMsg.Contains("OS") || errorMsg.Contains("Oral"));
            Assert.IsTrue(errorMsg.Contains("VS") || errorMsg.Contains("Vision"));
        }
    }
}
