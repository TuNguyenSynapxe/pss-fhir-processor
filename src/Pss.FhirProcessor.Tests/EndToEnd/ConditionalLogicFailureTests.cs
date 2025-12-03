using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end tests for bundles failing conditional logic rules
    /// </summary>
    [TestClass]
    public class ConditionalLogicFailureTests
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
                        ""QuestionCode"": ""SQ-L2H9-00000003"",
                        ""QuestionDisplay"": ""Type of hearing aid"",
                        ""ScreeningType"": ""HS"",
                        ""AllowedAnswers"": [""Behind-the-ear"", ""In-the-ear"", ""In-the-canal""]
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
        public void Bundle_ConditionalFieldMissing_WhenRequired_FailsValidation()
        {
            // If wearing hearing aid = Yes, then type of hearing aid is required
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
                                    ""valueString"": ""Yes""
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

            Assert.IsFalse(result.Validation.IsValid, "Bundle should be invalid");
            Assert.IsTrue(result.Validation.Errors.Exists(e => 
                e.Message.Contains("SQ-L2H9-00000003") || e.Message.Contains("conditional")), 
                "Should have error about missing conditional field");
        }

        [TestMethod]
        public void Bundle_ConditionalFieldPresent_WhenNotRequired_PassesValidation()
        {
            // If wearing hearing aid = No, then type field should not be present (but we allow it)
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

            Assert.IsTrue(result.Validation.IsValid, "Bundle should be valid when condition not met");
        }

        [TestMethod]
        public void Bundle_ConditionalFieldProvided_WhenRequired_PassesValidation()
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
                                    ""valueString"": ""Yes""
                                },
                                {
                                    ""code"": {
                                        ""coding"": [{
                                            ""code"": ""SQ-L2H9-00000003"",
                                            ""display"": ""Type of hearing aid""
                                        }]
                                    },
                                    ""valueString"": ""Behind-the-ear""
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
    }
}
