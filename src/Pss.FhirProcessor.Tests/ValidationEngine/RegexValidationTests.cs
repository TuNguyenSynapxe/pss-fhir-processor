using FluentAssertions;
using MOH.HealthierSG.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.PSS.FhirProcessor.Core.Validation;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MOH.HealthierSG.PSS.FhirProcessor.Tests.ValidationEngine
{
    /// <summary>
    /// Tests for Regex rule type validation
    /// </summary>
    public class RegexValidationTests
    {
        private ValidationResult CreateValidationResult()
        {
            return new ValidationResult();
        }

        private JObject CreateResource(string field, string value)
        {
            return JObject.Parse($@"{{
                ""resourceType"": ""Patient"",
                ""{field}"": ""{value}""
            }}");
        }

        #region NRIC Pattern Tests

        [Theory]
        [InlineData("S1234567A")]
        [InlineData("T9876543Z")]
        [InlineData("F0000000X")]
        [InlineData("G5555555B")]
        public void RegexRule_NRIC_ShouldPass_WhenValueMatchesPattern(string nric)
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "nric",
                Pattern = @"^[STFG]\d{7}[A-Z]$",
                ErrorCode = "INVALID_NRIC",
                Message = "NRIC format invalid"
            };
            var resource = CreateResource("nric", nric);
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("A1234567B")] // Wrong first letter
        [InlineData("S123456A")] // Too few digits
        [InlineData("S12345678A")] // Too many digits
        [InlineData("S1234567a")] // Lowercase last letter
        [InlineData("s1234567A")] // Lowercase first letter
        [InlineData("S12345671")] // Number as last char
        [InlineData("1234567A")] // Missing first letter
        [InlineData("S1234567")] // Missing last letter
        [InlineData("S1234567A ")] // Trailing space
        [InlineData(" S1234567A")] // Leading space
        public void RegexRule_NRIC_ShouldFail_WhenValueDoesNotMatchPattern(string nric)
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "nric",
                Pattern = @"^[STFG]\d{7}[A-Z]$",
                ErrorCode = "INVALID_NRIC",
                Message = "NRIC format invalid"
            };
            var resource = CreateResource("nric", nric);
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("INVALID_NRIC");
            result.Errors[0].Message.Should().Contain("NRIC format invalid");
            result.Errors[0].Message.Should().Contain(nric);
        }

        #endregion

        #region Email Pattern Tests

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("test.user@example.com")]
        [InlineData("user+tag@example.co.uk")]
        public void RegexRule_Email_ShouldPass_WhenValueMatchesPattern(string email)
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "email",
                Pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
                ErrorCode = "INVALID_EMAIL",
                Message = "Email format invalid"
            };
            var resource = CreateResource("email", email);
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("@example.com")]
        [InlineData("user@")]
        [InlineData("user@.com")]
        public void RegexRule_Email_ShouldFail_WhenValueDoesNotMatchPattern(string email)
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "email",
                Pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
                ErrorCode = "INVALID_EMAIL",
                Message = "Email format invalid"
            };
            var resource = CreateResource("email", email);
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
        }

        #endregion

        #region Null and Empty Value Tests

        [Fact]
        public void RegexRule_ShouldPass_WhenValueIsNull()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "nonexistent",
                Pattern = @"^\d+$",
                ErrorCode = "REGEX_MISMATCH",
                Message = "Field is required"
            };
            var resource = CreateResource("other", "value");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert - Should skip validation for null values
            result.IsValid.Should().BeTrue();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void RegexRule_ShouldPass_WhenValueIsEmptyString()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "field",
                Pattern = @"^\d+$",
                ErrorCode = "REGEX_MISMATCH",
                Message = "Must be numeric"
            };
            var resource = CreateResource("field", "");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert - Should skip validation for empty strings
            result.IsValid.Should().BeTrue();
            result.Errors.Should().HaveCount(0);
        }

        #endregion

        #region Missing Pattern Tests

        [Fact]
        public void RegexRule_ShouldFail_WhenPatternIsMissing()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "field",
                Pattern = null, // Missing pattern
                ErrorCode = "REGEX_VALIDATION_ERROR",
                Message = "Regex validation error"
            };
            var resource = CreateResource("field", "value");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("REGEX_VALIDATION_ERROR");
            result.Errors[0].Message.Should().Contain("Regex rule missing Pattern");
        }

        [Fact]
        public void RegexRule_ShouldFail_WhenPatternIsEmpty()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "field",
                Pattern = "", // Empty pattern
                ErrorCode = "REGEX_VALIDATION_ERROR",
                Message = "Regex validation error"
            };
            var resource = CreateResource("field", "value");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
        }

        #endregion

        #region Invalid Pattern Tests

        [Fact]
        public void RegexRule_ShouldFail_WhenPatternIsInvalid()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "field",
                Pattern = "[invalid(", // Invalid regex pattern
                Message = "Pattern validation failed"
            };
            var resource = CreateResource("field", "value");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("REGEX_PATTERN_ERROR");
            result.Errors[0].Message.Should().Contain("Invalid regex pattern");
        }

        #endregion

        #region Unicode Tests

        [Theory]
        [InlineData("测试123")]
        [InlineData("テスト456")]
        [InlineData("한글789")]
        public void RegexRule_Unicode_ShouldPass_WhenPatternAllowsUnicode(string value)
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "text",
                Pattern = @"^.+\d+$", // Any chars followed by digits
                ErrorCode = "INVALID_TEXT",
                Message = "Text format invalid"
            };
            var resource = CreateResource("text", value);
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region Pipe-Separated Values Tests

        [Fact]
        public void RegexRule_ShouldValidate_FirstValueInPipeSeparatedList()
        {
            // Arrange - CpsPathResolver.GetValueAsString returns the raw string
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "codes",
                Pattern = @"^[A-Z]\d{3}\|[A-Z]\d{3}$", // Pattern for pipe-separated codes
                ErrorCode = "INVALID_CODES",
                Message = "Codes format invalid"
            };
            var resource = JObject.Parse(@"{
                ""resourceType"": ""Patient"",
                ""codes"": ""A123|B456""
            }");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region Complex Pattern Tests

        [Theory]
        [InlineData("2024-01-01")]
        [InlineData("1900-12-31")]
        [InlineData("2099-06-15")]
        public void RegexRule_DatePattern_ShouldPass_WhenValueMatchesYYYYMMDD(string date)
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "date",
                Pattern = @"^\d{4}-\d{2}-\d{2}$",
                ErrorCode = "INVALID_DATE",
                Message = "Date format must be YYYY-MM-DD"
            };
            var resource = CreateResource("date", date);
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void RegexRule_PostalCode_ShouldPass_WhenValueMatchesSingaporeFormat()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "postalCode",
                Pattern = @"^\d{6}$",
                ErrorCode = "INVALID_POSTAL_CODE",
                Message = "Postal code must be 6 digits"
            };
            var resource = CreateResource("postalCode", "123456");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region Error Message Tests

        [Fact]
        public void RegexRule_ShouldInclude_PatternInErrorMessage()
        {
            // Arrange
            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                PathType = "CPS1",
                Path = "field",
                Pattern = @"^\d{3}$",
                ErrorCode = "INVALID_FORMAT",
                Message = "Must be 3 digits"
            };
            var resource = CreateResource("field", "abc");
            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors[0].Message.Should().Contain("Must be 3 digits");
            result.Errors[0].Message.Should().Contain(@"^\d{3}$");
            result.Errors[0].Message.Should().Contain("abc");
        }

        #endregion
    }
}
