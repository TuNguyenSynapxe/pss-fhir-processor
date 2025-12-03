using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class DisplayMatchingTests
    {
        private FhirProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new FhirProcessor();
            
            var codesMaster = @"{
                ""Questions"": [
                    {
                        ""QuestionCode"": ""TEST-001"",
                        ""QuestionDisplay"": ""Test Question?"",
                        ""ScreeningType"": ""HS"",
                        ""AllowedAnswers"": [""Yes"", ""No""]
                    }
                ]
            }";
            
            _processor.LoadCodesMaster(codesMaster);
        }

        [TestMethod]
        public void StrictMatch_ExactDisplay_Passes()
        {
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });
            
            var json = GetBundle("TEST-001", "Test Question?", "Yes");
            var result = _processor.Validate(json);

            Assert.IsFalse(result.Errors.Exists(e => e.Code == "QUESTION_DISPLAY_MISMATCH"));
        }

        [TestMethod]
        public void StrictMatch_DifferentCase_ReturnsError()
        {
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });
            
            var json = GetBundle("TEST-001", "test question?", "Yes");
            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "QUESTION_DISPLAY_MISMATCH"));
        }

        [TestMethod]
        public void NormalizedMatch_DifferentCase_Passes()
        {
            _processor.SetValidationOptions(new ValidationOptions 
            { 
                StrictDisplayMatch = false, 
                NormalizeDisplayMatch = true 
            });
            
            var json = GetBundle("TEST-001", "TEST QUESTION?", "Yes");
            var result = _processor.Validate(json);

            Assert.IsFalse(result.Errors.Exists(e => e.Code == "QUESTION_DISPLAY_MISMATCH"));
        }

        private string GetBundle(string code, string display, string answer)
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
