using FluentAssertions;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.ValidationEngine
{
    /// <summary>
    /// Integration tests for Type validation rule in RuleEvaluator
    /// Tests the full flow from rule definition to validation result
    /// </summary>
    public class TypeValidationIntegrationTests
    {
        private ValidationResult CreateValidationResult()
        {
            return new ValidationResult();
        }

        private JObject CreateResource(string field, string value)
        {
            return JObject.Parse($@"{{
                ""resourceType"": ""Test"",
                ""{field}"": ""{value}""
            }}");
        }

        [Fact]
        public void TypeRule_Date_ShouldPass_WhenValueIsValidYYYYMMDD()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Type",
                PathType = "CPS1",
                Path = "birthDate",
                ExpectedType = "date",
                ErrorCode = "TYPE_MISMATCH",
                Message = "birthDate must be in YYYY-MM-DD format"
            };
            var resource = CreateResource("birthDate", "1990-05-15");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, result);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void TypeRule_Date_ShouldFail_WhenValueIsInvalidFormat()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Type",
                PathType = "CPS1",
                Path = "birthDate",
                ExpectedType = "date",
                ErrorCode = "TYPE_MISMATCH",
                Message = "birthDate must be in YYYY-MM-DD format"
            };
            var resource = CreateResource("birthDate", "15-05-1990");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, result);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("TYPE_MISMATCH");
            result.Errors[0].Message.Should().Contain("birthDate must be in YYYY-MM-DD format");
            result.Errors[0].Message.Should().Contain("Expected type: 'date'");
        }

        [Fact]
        public void TypeRule_Integer_ShouldPass_WhenValueIsValidInteger()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Type",
                PathType = "CPS1",
                Path = "age",
                ExpectedType = "integer",
                ErrorCode = "TYPE_MISMATCH",
                Message = "Age must be an integer"
            };
            var resource = CreateResource("age", "30");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, result);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void TypeRule_Integer_ShouldFail_WhenValueIsDecimal()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Type",
                PathType = "CPS1",
                Path = "age",
                ExpectedType = "integer",
                ErrorCode = "TYPE_MISMATCH",
                Message = "Age must be an integer"
            };
            var resource = CreateResource("age", "30.5");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, result);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
        }

        [Fact]
        public void TypeRule_Boolean_ShouldPass_WhenValueIsTrue()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Type",
                PathType = "CPS1",
                Path = "active",
                ExpectedType = "boolean",
                ErrorCode = "TYPE_MISMATCH",
                Message = "Active must be true or false"
            };
            var resource = CreateResource("active", "true");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, result);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void TypeRule_PipeStringArray_ShouldPass_WhenValueIsValidPipeSeparatedList()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Type",
                PathType = "CPS1",
                Path = "tones",
                ExpectedType = "pipestring[]",
                ErrorCode = "TYPE_MISMATCH",
                Message = "Tones must be pipe-separated list"
            };
            var resource = JObject.Parse(@"{
                ""resourceType"": ""Observation"",
                ""tones"": ""pure tone 250Hz|pure tone 500Hz|pure tone 1kHz""
            }");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Observation", null, result);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void TypeRule_PipeStringArray_ShouldFail_WhenValueHasEmptyParts()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Type",
                PathType = "CPS1",
                Path = "tones",
                ExpectedType = "pipestring[]",
                ErrorCode = "TYPE_MISMATCH",
                Message = "Tones must be pipe-separated list with no empty parts"
            };
            var resource = JObject.Parse(@"{
                ""resourceType"": ""Observation"",
                ""tones"": ""pure tone 250Hz||pure tone 1kHz""
            }");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Observation", null, result);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
        }

        [Fact]
        public void TypeRule_ShouldFail_WhenPathNotFound()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Type",
                PathType = "CPS1",
                Path = "nonexistent",
                ExpectedType = "string",
                ErrorCode = "TYPE_MISMATCH",
                Message = "Field is required"
            };
            var resource = CreateResource("other", "value");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Test", null, result);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Message.Should().Contain("not found or value is null");
        }

        [Fact]
        public void TypeRule_ShouldFail_WhenExpectedTypeIsMissing()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Type",
                PathType = "CPS1",
                Path = "field",
                ExpectedType = null,
                ErrorCode = "TYPE_VALIDATION_ERROR",
                Message = "Type validation error"
            };
            var resource = CreateResource("field", "value");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Test", null, result);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Message.Should().Contain("Type rule missing ExpectedType");
        }
    }
}
