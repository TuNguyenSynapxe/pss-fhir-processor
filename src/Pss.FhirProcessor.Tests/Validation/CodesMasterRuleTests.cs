using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class CodesMasterRuleTests
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
        public void CodesMaster_ValidQuestion_Passes()
        {
            var json = GetBundleWithQuestion("TEST-001", "Test Question?", "Yes", "HS");
            
            var result = _processor.Validate(json);

            Assert.IsFalse(result.Errors.Exists(e => e.Code == "UNKNOWN_QUESTION_CODE"));
            Assert.IsFalse(result.Errors.Exists(e => e.Code == "QUESTION_DISPLAY_MISMATCH"));
            Assert.IsFalse(result.Errors.Exists(e => e.Code == "INVALID_ANSWER_VALUE"));
        }

        [TestMethod]
        public void CodesMaster_UnknownCode_ReturnsError()
        {
            var processor2 = new FhirProcessor();
            processor2.SetLoggingOptions(new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation.LoggingOptions { LogLevel = "info" });
            
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
            
            processor2.LoadCodesMaster(codesMaster);
            
            var json = GetBundleWithQuestion("UNKNOWN-CODE", "Unknown Question?", "Yes", "HS");
            
            var result2 = processor2.Process(json);
            
            System.Console.WriteLine($"\n=== DEBUG: CodesMaster_UnknownCode ===");
            System.Console.WriteLine($"Errors: {result2.Validation.Errors.Count}");
            foreach (var err in result2.Validation.Errors)
            {
                System.Console.WriteLine($"  [{err.Code}] {err.Message}");
            }
            System.Console.WriteLine("Relevant logs:");
            foreach (var log in result2.Logs)
            {
                if (log.Contains("CodesMaster") || log.Contains("Observation") || log.Contains("Component"))
                    System.Console.WriteLine($"  {log}");
            }
            System.Console.WriteLine("=== END DEBUG ===\n");
            
            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "UNKNOWN_QUESTION_CODE"));
        }

        [TestMethod]
        public void CodesMaster_DisplayMismatch_ReturnsError()
        {
            var json = GetBundleWithQuestion("TEST-001", "Wrong Display Text", "Yes", "HS");
            
            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "QUESTION_DISPLAY_MISMATCH"));
        }

        [TestMethod]
        public void CodesMaster_InvalidAnswer_ReturnsError()
        {
            var json = GetBundleWithQuestion("TEST-001", "Test Question?", "Maybe", "HS");
            
            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "INVALID_ANSWER_VALUE"));
        }

        [TestMethod]
        public void CodesMaster_WrongScreeningType_ReturnsError()
        {
            var json = GetBundleWithQuestion("TEST-001", "Test Question?", "Yes", "OS");
            
            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "INVALID_SCREENING_TYPE_FOR_QUESTION"));
        }

        private string GetBundleWithQuestion(string code, string display, string answer, string screeningType)
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
                                    ""code"": ""{screeningType}""
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
