using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class FixedValueRuleTests
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
                        ""RuleType"": ""FixedValue"",
                        ""Path"": ""Entry[0].Resource.Status"",
                        ""FixedValue"": ""completed""
                    }]
                }" }
            };
            
            _processor.LoadRuleSets(rules);
        }

        [TestMethod]
        public void FixedValue_Matches_Passes()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [{
                    ""resource"": {
                        ""resourceType"": ""Encounter"",
                        ""status"": ""completed""
                    }
                }]
            }";

            var result = _processor.Validate(json);

            Assert.IsFalse(result.Errors.Exists(e => e.Code == "FIXED_VALUE_MISMATCH"));
        }

        [TestMethod]
        public void FixedValue_Mismatch_ReturnsError()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [{
                    ""resource"": {
                        ""resourceType"": ""Encounter"",
                        ""status"": ""in-progress""
                    }
                }]
            }";

            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "FIXED_VALUE_MISMATCH"));
        }
    }
}
