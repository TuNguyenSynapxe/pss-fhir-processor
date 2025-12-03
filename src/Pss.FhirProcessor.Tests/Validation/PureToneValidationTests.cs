using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class PureToneValidationTests
    {
        private FhirProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new FhirProcessor();
            
            var codesMaster = @"{
                ""Questions"": [
                    {
                        ""QuestionCode"": ""PT-001"",
                        ""QuestionDisplay"": ""PureTone Test"",
                        ""ScreeningType"": ""HS"",
                        ""AllowedAnswers"": [""500Hz – R"", ""500Hz – NR"", ""1000Hz – R"", ""1000Hz – NR""]
                    }
                ]
            }";
            
            _processor.LoadCodesMaster(codesMaster);
        }

        [TestMethod]
        public void PureTone_ValidMultiValue_Passes()
        {
            var json = GetBundleWithPureTone("PT-001", "PureTone Test", "500Hz – R|1000Hz – R");
            
            var result = _processor.Validate(json);

            Assert.IsFalse(result.Errors.Exists(e => e.Code == "INVALID_MULTI_VALUE"));
        }

        [TestMethod]
        public void PureTone_InvalidMultiValue_ReturnsError()
        {
            var json = GetBundleWithPureTone("PT-001", "PureTone Test", "500Hz – R|INVALID – VALUE");
            
            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "INVALID_MULTI_VALUE"));
        }

        [TestMethod]
        public void PureTone_SingleValue_Passes()
        {
            var json = GetBundleWithPureTone("PT-001", "PureTone Test", "500Hz – R");
            
            var result = _processor.Validate(json);

            Assert.IsFalse(result.Errors.Exists(e => e.Code == "INVALID_ANSWER_VALUE"));
        }

        private string GetBundleWithPureTone(string code, string display, string answer)
        {
            return $@"{{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {{
                        ""resource"": {{
                            ""resourceType"": ""Patient"",
                            ""identifier"": [{{ ""value"": ""S1234567A"" }}]
                        }}
                    }},
                    {{
                        ""resource"": {{
                            ""resourceType"": ""Encounter""
                        }}
                    }},
                    {{
                        ""resource"": {{
                            ""resourceType"": ""Location""
                        }}
                    }},
                    {{
                        ""resource"": {{
                            ""resourceType"": ""HealthcareService""
                        }}
                    }},
                    {{
                        ""resource"": {{
                            ""resourceType"": ""Organization""
                        }}
                    }},
                    {{
                        ""resource"": {{
                            ""resourceType"": ""Observation"",
                            ""code"": {{
                                ""coding"": [{{
                                    ""code"": ""HS""
                                }}]
                            }},
                            ""component"": [{{
                                ""code"": {{
                                    ""coding"": [{{
                                        ""code"": ""{code}"",
                                        ""display"": ""{display}""
                                    }}]
                                }},
                                ""valueString"": ""{answer}""
                            }}]
                        }}
                    }},
                    {{
                        ""resource"": {{
                            ""resourceType"": ""Observation"",
                            ""code"": {{
                                ""coding"": [{{
                                    ""code"": ""OS""
                                }}]
                            }},
                            ""component"": []
                        }}
                    }},
                    {{
                        ""resource"": {{
                            ""resourceType"": ""Observation"",
                            ""code"": {{
                                ""coding"": [{{
                                    ""code"": ""VS""
                                }}]
                            }},
                            ""component"": []
                        }}
                    }}
                ]
            }}";
        }
    }
}
