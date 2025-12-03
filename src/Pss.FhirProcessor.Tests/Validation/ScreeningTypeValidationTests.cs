using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class ScreeningTypeValidationTests
    {
        private FhirProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new FhirProcessor();
        }

        [TestMethod]
        public void AllScreeningTypes_Present_Passes()
        {
            var json = GetBundleWithScreeningTypes(true, true, true);
            
            var result = _processor.Validate(json);

            Assert.IsTrue(result.IsValid || !result.Errors.Exists(e => e.Code == "MISSING_SCREENING_TYPE"));
        }

        [TestMethod]
        public void MissingHS_ReturnsError()
        {
            var json = GetBundleWithScreeningTypes(false, true, true);
            
            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "MISSING_SCREENING_TYPE" && e.Message.Contains("HS")));
        }

        [TestMethod]
        public void MissingOS_ReturnsError()
        {
            var json = GetBundleWithScreeningTypes(true, false, true);
            
            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "MISSING_SCREENING_TYPE" && e.Message.Contains("OS")));
        }

        [TestMethod]
        public void MissingVS_ReturnsError()
        {
            var json = GetBundleWithScreeningTypes(true, true, false);
            
            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "MISSING_SCREENING_TYPE" && e.Message.Contains("VS")));
        }

        [TestMethod]
        public void AllScreeningTypes_Missing_ReturnsMultipleErrors()
        {
            var json = GetBundleWithScreeningTypes(false, false, false);
            
            var result = _processor.Validate(json);

            var screeningErrors = result.Errors.FindAll(e => e.Code == "MISSING_SCREENING_TYPE");
            Assert.IsTrue(screeningErrors.Count >= 3);
        }

        private string GetBundleWithScreeningTypes(bool hasHS, bool hasOS, bool hasVS)
        {
            var observations = "";
            
            if (hasHS)
            {
                observations += @",
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": {
                                ""coding"": [{
                                    ""code"": ""HS""
                                }]
                            },
                            ""component"": []
                        }
                    }";
            }
            
            if (hasOS)
            {
                observations += @",
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": {
                                ""coding"": [{
                                    ""code"": ""OS""
                                }]
                            },
                            ""component"": []
                        }
                    }";
            }
            
            if (hasVS)
            {
                observations += @",
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": {
                                ""coding"": [{
                                    ""code"": ""VS""
                                }]
                            },
                            ""component"": []
                        }
                    }";
            }
            
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
                    }}
                    {observations}
                ]
            }}";
        }
    }
}
