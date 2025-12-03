using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using System.IO;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end tests validating complete valid FHIR bundles
    /// </summary>
    [TestClass]
    public class ValidBundleTests
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
                        ""AllowedAnswers"": [""Yes"", ""No""]
                    }
                ]
            }";
            
            _processor.LoadCodesMaster(codesMaster);
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });
        }

        [TestMethod]
        public void ValidBundle_AllFieldsCorrect_PassesValidation()
        {
            var json = TestDataHelper.GetValidCompleteBundle();
            
            var result = _processor.Process(json);

            Assert.IsTrue(result.Validation.IsValid, "Bundle should be valid");
            Assert.AreEqual(0, result.Validation.Errors.Count, "Should have no errors");
        }

        [TestMethod]
        public void ValidBundle_ExtractsAllData()
        {
            var json = TestDataHelper.GetValidCompleteBundle();
            
            var result = _processor.Process(json);

            Assert.IsNotNull(result.Flatten, "Flatten should not be null");
            Assert.IsNotNull(result.Flatten.Event, "Event data should be extracted");
            Assert.IsNotNull(result.Flatten.Participant, "Participant data should be extracted");
            Assert.IsNotNull(result.Flatten.HearingRaw, "Hearing screening should be extracted");
            Assert.IsNotNull(result.Flatten.OralRaw, "Oral screening should be extracted");
            Assert.IsNotNull(result.Flatten.VisionRaw, "Vision screening should be extracted");
        }

        [TestMethod]
        public void ValidBundle_LookupHelpers_Work()
        {
            var json = TestDataHelper.GetValidCompleteBundle();
            
            var result = _processor.Process(json);

            var hearingAid = result.Flatten.GetHearing("SQ-L2H9-00000001");
            var dentures = result.Flatten.GetOral("SQ-L2O9-00000001");
            var glasses = result.Flatten.GetVision("SQ-L2V9-00000001");

            Assert.IsNotNull(hearingAid, "Should find hearing question");
            Assert.IsNotNull(dentures, "Should find oral question");
            Assert.IsNotNull(glasses, "Should find vision question");
        }
    }

    public static class TestDataHelper
    {
        public static string GetValidCompleteBundle()
        {
            return @"{
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
                            ""resourceType"": ""Location"",
                            ""name"": ""ABC Community Center"",
                            ""address"": { ""postalCode"": ""123456"" }
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""HealthcareService"",
                            ""name"": ""Mobile Screening Team""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization"",
                            ""name"": ""XYZ Provider""
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
                            ""component"": [
                                {
                                    ""code"": {
                                        ""coding"": [{
                                            ""code"": ""SQ-L2O9-00000001"",
                                            ""display"": ""Currently wearing dentures?""
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
                            ""code"": { ""coding"": [{ ""code"": ""VS"" }] },
                            ""component"": [
                                {
                                    ""code"": {
                                        ""coding"": [{
                                            ""code"": ""SQ-L2V9-00000001"",
                                            ""display"": ""Currently wearing glasses?""
                                        }]
                                    },
                                    ""valueString"": ""Yes""
                                }
                            ]
                        }
                    }
                ]
            }";
        }
    }
}
