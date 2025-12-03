using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using System.Linq;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Extraction
{
    [TestClass]
    public class ScreeningExtractionTests
    {
        [TestMethod]
        public void ExtractScreening_HS_ExtractsCorrectly()
        {
            var processor = new FhirProcessor();
            processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });
            
            var codesMaster = @"{
                ""Questions"": [
                    {
                        ""QuestionCode"": ""HS-001"",
                        ""QuestionDisplay"": ""Hearing Test"",
                        ""ScreeningType"": ""HS"",
                        ""AllowedAnswers"": [""Yes"", ""No""]
                    }
                ]
            }";
            processor.LoadCodesMaster(codesMaster);
            
            var json = GetBundleWithHsObservation();
            var result = processor.Process(json);

            Assert.IsNotNull(result.Flatten?.HearingRaw);
            Assert.AreEqual("HS", result.Flatten.HearingRaw.ScreeningType);
            Assert.IsTrue(result.Flatten.HearingRaw.Items.Count > 0);
            Assert.AreEqual("HS-001", result.Flatten.HearingRaw.Items[0].Question.Code);
        }

        [TestMethod]
        public void ExtractScreening_AllThreeTypes_ExtractsCorrectly()
        {
            var processor = new FhirProcessor();
            processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });
            
            var json = GetBundleWithAllScreenings();
            var result = processor.Process(json);

            Assert.IsNotNull(result.Flatten?.HearingRaw);
            Assert.IsNotNull(result.Flatten?.OralRaw);
            Assert.IsNotNull(result.Flatten?.VisionRaw);
            Assert.AreEqual("HS", result.Flatten.HearingRaw.ScreeningType);
            Assert.AreEqual("OS", result.Flatten.OralRaw.ScreeningType);
            Assert.AreEqual("VS", result.Flatten.VisionRaw.ScreeningType);
        }

        private string GetBundleWithHsObservation()
        {
            return @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""identifier"": [{ ""value"": ""S1234567A"" }]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Location""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""HealthcareService""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization""
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
                                            ""code"": ""HS-001"",
                                            ""display"": ""Hearing Test""
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
                    }
                ]
            }";
        }

        private string GetBundleWithAllScreenings()
        {
            return @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""identifier"": [{ ""value"": ""S1234567A"" }]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Location""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""HealthcareService""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization""
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
                            ""component"": []
                        }
                    }
                ]
            }";
        }
    }
}
