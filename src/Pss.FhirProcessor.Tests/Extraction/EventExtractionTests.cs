using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Extraction
{
    [TestClass]
    public class EventExtractionTests
    {
        [TestMethod]
        public void ExtractEvent_ValidBundle_ExtractsAllFields()
        {
            var processor = new FhirProcessor();
            processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });
            
            var json = GetValidBundle();
            var result = processor.Process(json);

            Assert.IsNotNull(result.Flatten);
            Assert.IsNotNull(result.Flatten.Event);
            Assert.IsNotNull(result.Flatten.Event.Start);
            Assert.IsNotNull(result.Flatten.Event.VenueName);
        }

        [TestMethod]
        public void ExtractEvent_WithGrcAndConstituency_ExtractsCorrectly()
        {
            var processor = new FhirProcessor();
            processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });
            
            var json = GetBundleWithLocation();
            var result = processor.Process(json);

            Assert.IsNotNull(result.Flatten?.Event);
            Assert.AreEqual("ABC CC", result.Flatten.Event.VenueName);
            Assert.AreEqual("123456", result.Flatten.Event.PostalCode);
        }

        private string GetValidBundle()
        {
            return @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter"",
                            ""actualPeriod"": {
                                ""start"": ""2025-01-01T09:00:00+08:00"",
                                ""end"": ""2025-01-01T10:00:00+08:00""
                            }
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""identifier"": [{ ""value"": ""S1234567A"" }]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Location"",
                            ""name"": ""Test Venue""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""HealthcareService""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization"",
                            ""name"": ""Test Provider""
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

        private string GetBundleWithLocation()
        {
            return @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""identifier"": [{ ""value"": ""S1234567A"" }]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Location"",
                            ""name"": ""ABC CC"",
                            ""address"": { ""postalCode"": ""123456"" }
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
