using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class AllowedValuesRuleTests
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
                        ""RuleType"": ""AllowedValues"",
                        ""Path"": ""Entry[0].Resource.Status"",
                        ""AllowedValues"": [""active"", ""completed"", ""cancelled""]
                    }]
                }" }
            };
            
            _processor.LoadRuleSets(rules);
        }

        [TestMethod]
        public void AllowedValue_InList_Passes()
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

            Assert.IsFalse(result.Errors.Exists(e => e.Code == "INVALID_ANSWER_VALUE"));
        }

        [TestMethod]
        public void AllowedValue_NotInList_ReturnsError()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [{
                    ""resource"": {
                        ""resourceType"": ""Encounter"",
                        ""status"": ""invalid-status""
                    }
                }]
            }";

            var result = _processor.Validate(json);

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "INVALID_ANSWER_VALUE"));
        }
    }
}
