using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using System.Linq;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Extraction
{
    [TestClass]
    public class MultiValueSplitTests
    {
        [TestMethod]
        public void MultiValue_PipeSeparated_SplitsCorrectly()
        {
            var processor = new FhirProcessor();
            processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });
            
            var json = GetBundleWithMultiValue();
            var result = processor.Process(json);

            var item = result.Flatten?.HearingRaw?.Items?.FirstOrDefault();
            Assert.IsNotNull(item);
            Assert.AreEqual(3, item.Values.Count);
            Assert.AreEqual("500Hz – R", item.Values[0]);
            Assert.AreEqual("1000Hz – R", item.Values[1]);
            Assert.AreEqual("2000Hz – NR", item.Values[2]);
        }

        [TestMethod]
        public void SingleValue_NoSplit_ReturnsSingleItem()
        {
            var processor = new FhirProcessor();
            processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });
            
            var json = GetBundleWithSingleValue();
            var result = processor.Process(json);

            var item = result.Flatten?.HearingRaw?.Items?.FirstOrDefault();
            Assert.IsNotNull(item);
            Assert.AreEqual(1, item.Values.Count);
            Assert.AreEqual("Yes", item.Values[0]);
        }

        private string GetBundleWithMultiValue()
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
                                            ""code"": ""PT-001"",
                                            ""display"": ""PureTone""
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
                    }
                ]
            }";
        }

        private string GetBundleWithSingleValue()
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
                                            ""display"": ""Test""
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
    }
}
