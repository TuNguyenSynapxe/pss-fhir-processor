using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class FixedCodingRuleTests
    {
        private FhirProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new FhirProcessor();
            
            var rules = new Dictionary<string, string>
            {
                { "Test", @"{
                    ""Scope"": ""Test"",
                    ""Rules"": [{
                        ""RuleType"": ""FixedCoding"",
                        ""Path"": ""Entry[0].Resource.Code.Coding[0]"",
                        ""FixedSystem"": ""http://example.com/codes"",
                        ""FixedCode"": ""TEST""
                    }]
                }" }
            };
            
            _processor.LoadRuleSets(rules);
        }

        [TestMethod]
        public void FixedCoding_SystemAndCodeMatch_Passes()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [{
                    ""resource"": {
                        ""resourceType"": ""Observation"",
                        ""code"": {
                            ""coding"": [{
                                ""system"": ""http://example.com/codes"",
                                ""code"": ""TEST""
                            }]
                        }
                    }
                }]
            }";

            var result = _processor.Validate(json);

            Assert.IsFalse(result.Errors.Exists(e => e.Code == "FIXED_CODING_MISMATCH"));
        }

        [TestMethod]
        public void FixedCoding_SystemMismatch_ReturnsError()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [{
                    ""resource"": {
                        ""resourceType"": ""Observation"",
                        ""code"": {
                            ""coding"": [{
                                ""system"": ""http://wrong.com/codes"",
                                ""code"": ""TEST""
                            }]
                        }
                    }
                }]
            }";

            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "FIXED_CODING_MISMATCH"));
        }

        [TestMethod]
        public void FixedCoding_CodeMismatch_ReturnsError()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [{
                    ""resource"": {
                        ""resourceType"": ""Observation"",
                        ""code"": {
                            ""coding"": [{
                                ""system"": ""http://example.com/codes"",
                                ""code"": ""WRONG""
                            }]
                        }
                    }
                }]
            }";

            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "FIXED_CODING_MISMATCH"));
        }
    }
}
