using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Extraction
{
    [TestClass]
    public class LookupHelperTests
    {
        [TestMethod]
        public void GetHearing_ExistingCode_ReturnsItem()
        {
            var processor = new FhirProcessor();
            processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });
            
            var json = GetBundleWithHearing();
            var result = processor.Process(json);

            var item = result.Flatten?.GetHearing("HS-001");
            
            Assert.IsNotNull(item);
            Assert.AreEqual("HS-001", item.Question.Code);
        }

        [TestMethod]
        public void GetHearing_NonExistingCode_ReturnsNull()
        {
            var processor = new FhirProcessor();
            processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });
            
            var json = GetBundleWithHearing();
            var result = processor.Process(json);

            var item = result.Flatten?.GetHearing("NON-EXIST");
            
            Assert.IsNull(item);
        }

        [TestMethod]
        public void GetOral_ExistingCode_ReturnsItem()
        {
            var processor = new FhirProcessor();
            processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });
            
            var json = GetBundleWithOral();
            var result = processor.Process(json);

            var item = result.Flatten?.GetOral("OS-001");
            
            Assert.IsNotNull(item);
            Assert.AreEqual("OS-001", item.Question.Code);
        }

        [TestMethod]
        public void GetVision_ExistingCode_ReturnsItem()
        {
            var processor = new FhirProcessor();
            processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });
            
            var json = GetBundleWithVision();
            var result = processor.Process(json);

            var item = result.Flatten?.GetVision("VS-001");
            
            Assert.IsNotNull(item);
            Assert.AreEqual("VS-001", item.Question.Code);
        }

        private string GetBundleWithHearing()
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

        private string GetBundleWithOral()
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
                            ""component"": [
                                {
                                    ""code"": {
                                        ""coding"": [{
                                            ""code"": ""OS-001"",
                                            ""display"": ""Oral Test""
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
                            ""code"": { ""coding"": [{ ""code"": ""VS"" }] },
                            ""component"": []
                        }
                    }
                ]
            }";
        }

        private string GetBundleWithVision()
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
                            ""component"": [
                                {
                                    ""code"": {
                                        ""coding"": [{
                                            ""code"": ""VS-001"",
                                            ""display"": ""Vision Test""
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
