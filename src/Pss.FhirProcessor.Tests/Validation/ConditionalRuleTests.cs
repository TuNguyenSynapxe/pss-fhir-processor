using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class ConditionalRuleTests
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
                        ""RuleType"": ""Conditional"",
                        ""If"": ""Entry[0].Resource.Status"",
                        ""Then"": ""Entry[0].Resource.ActualPeriod""
                    }]
                }" }
            };
            
            _processor.LoadRuleSets(rules);
        }

        [TestMethod]
        public void Conditional_IfExistsThenExists_Passes()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [{
                    ""resource"": {
                        ""resourceType"": ""Encounter"",
                        ""status"": ""completed"",
                        ""actualPeriod"": {
                            ""start"": ""2025-01-01""
                        }
                    }
                }]
            }";

            var result = _processor.Validate(json);

            Assert.IsFalse(result.Errors.Exists(e => e.Code == "CONDITIONAL_FAILED"));
        }

        [TestMethod]
        public void Conditional_IfExistsThenMissing_ReturnsError()
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

            Assert.IsTrue(result.Errors.Exists(e => e.Code == "CONDITIONAL_FAILED"));
        }

        [TestMethod]
        public void Conditional_IfNotExist_NoValidation()
        {
            var json = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [{
                    ""resource"": {
                        ""resourceType"": ""Encounter""
                    }
                }]
            }";

            var result = _processor.Validate(json);

            Assert.IsFalse(result.Errors.Exists(e => e.Code == "CONDITIONAL_FAILED"));
        }
    }
}
