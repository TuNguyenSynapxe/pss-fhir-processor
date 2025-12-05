using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class RuleEvaluatorTests
    {
        [TestMethod]
        public void EvaluateRequired_MissingField_AddsError()
        {
            // Arrange
            var resource = JObject.Parse(@"{""resourceType"": ""Patient""}");
            var rule = new RuleDefinition
            {
                RuleType = "Required",
                Path = "identifier",
                ErrorCode = "MANDATORY_MISSING",
                Message = "Identifier is required"
            };
            var result = new ValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, result);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("MANDATORY_MISSING", result.Errors[0].Code);
        }

        [TestMethod]
        public void EvaluateRequired_FieldPresent_NoError()
        {
            // Arrange
            var resource = JObject.Parse(@"{
                ""resourceType"": ""Patient"",
                ""identifier"": [{""value"": ""S1234567D""}]
            }");
            var rule = new RuleDefinition
            {
                RuleType = "Required",
                Path = "identifier",
                ErrorCode = "MANDATORY_MISSING",
                Message = "Identifier is required"
            };
            var result = new ValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, result);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void EvaluateFixedValue_WrongValue_AddsError()
        {
            // Arrange
            var resource = JObject.Parse(@"{
                ""resourceType"": ""Patient"",
                ""status"": ""inactive""
            }");
            var rule = new RuleDefinition
            {
                RuleType = "FixedValue",
                Path = "status",
                ExpectedValue = "active",
                ErrorCode = "FIXED_VALUE_MISMATCH",
                Message = "Status must be active"
            };
            var result = new ValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, result);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("FIXED_VALUE_MISMATCH", result.Errors[0].Code);
        }

        [TestMethod]
        public void EvaluateFixedValue_CorrectValue_NoError()
        {
            // Arrange
            var resource = JObject.Parse(@"{
                ""resourceType"": ""Patient"",
                ""status"": ""active""
            }");
            var rule = new RuleDefinition
            {
                RuleType = "FixedValue",
                Path = "status",
                ExpectedValue = "active",
                ErrorCode = "FIXED_VALUE_MISMATCH",
                Message = "Status must be active"
            };
            var result = new ValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, result);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void EvaluateFixedCoding_WrongCoding_AddsError()
        {
            // Arrange
            var resource = JObject.Parse(@"{
                ""code"": {
                    ""coding"": [{
                        ""system"": ""http://wrong.system"",
                        ""code"": ""wrong""
                    }]
                }
            }");
            var rule = new RuleDefinition
            {
                RuleType = "FixedCoding",
                Path = "code.coding[0]",
                ExpectedSystem = "http://correct.system",
                ExpectedCode = "correct",
                ErrorCode = "FIXED_CODING_MISMATCH",
                Message = "Coding must match expected system and code"
            };
            var result = new ValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Observation", null, result);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("FIXED_CODING_MISMATCH", result.Errors[0].Code);
        }

        [TestMethod]
        public void EvaluateFixedCoding_CorrectCoding_NoError()
        {
            // Arrange
            var resource = JObject.Parse(@"{
                ""code"": {
                    ""coding"": [{
                        ""system"": ""http://correct.system"",
                        ""code"": ""correct""
                    }]
                }
            }");
            var rule = new RuleDefinition
            {
                RuleType = "FixedCoding",
                Path = "code.coding[0]",
                ExpectedSystem = "http://correct.system",
                ExpectedCode = "correct",
                ErrorCode = "FIXED_CODING_MISMATCH",
                Message = "Coding must match expected system and code"
            };
            var result = new ValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Observation", null, result);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void EvaluateCodesMaster_InvalidAnswer_AddsError()
        {
            // Arrange
            var resource = JObject.Parse(@"{
                ""component"": [{
                    ""code"": ""SQ-R5W8-00000003"",
                    ""valueString"": ""Maybe""
                }]
            }");
            var rule = new RuleDefinition
            {
                RuleType = "CodesMaster",
                Path = "component[code:SQ-R5W8-00000003].valueString",
                ExpectedValue = "SQ-R5W8-00000003",
                ErrorCode = "INVALID_ANSWER_VALUE",
                Message = "Invalid answer for question"
            };
            var codesMaster = new CodesMaster
            {
                Questions = new System.Collections.Generic.List<CodesMasterQuestion>
                {
                    new CodesMasterQuestion
                    {
                        QuestionCode = "SQ-R5W8-00000003",
                        QuestionDisplay = "Test Question",
                        ScreeningType = "HS",
                        AllowedAnswers = new System.Collections.Generic.List<string> { "Yes", "No" },
                        IsMultiValue = false
                    }
                }
            };
            var result = new ValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Observation.HS", codesMaster, result);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("INVALID_ANSWER_VALUE", result.Errors[0].Code);
        }

        [TestMethod]
        public void EvaluateCodesMaster_ValidAnswer_NoError()
        {
            // Arrange
            var resource = JObject.Parse(@"{
                ""component"": [{
                    ""code"": ""SQ-R5W8-00000003"",
                    ""valueString"": ""Yes""
                }]
            }");
            var rule = new RuleDefinition
            {
                RuleType = "CodesMaster",
                Path = "component[code:SQ-R5W8-00000003].valueString",
                ExpectedValue = "SQ-R5W8-00000003",
                ErrorCode = "INVALID_ANSWER_VALUE",
                Message = "Invalid answer for question"
            };
            var codesMaster = new CodesMaster
            {
                Questions = new System.Collections.Generic.List<CodesMasterQuestion>
                {
                    new CodesMasterQuestion
                    {
                        QuestionCode = "SQ-R5W8-00000003",
                        QuestionDisplay = "Test Question",
                        ScreeningType = "HS",
                        AllowedAnswers = new System.Collections.Generic.List<string> { "Yes", "No" },
                        IsMultiValue = false
                    }
                }
            };
            var result = new ValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Observation.HS", codesMaster, result);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }
    }
}
