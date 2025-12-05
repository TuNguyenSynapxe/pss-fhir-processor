using FluentAssertions;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation;
using Xunit;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.UnitTests
{
    public class TypeCheckerTests
    {
        #region String Type Tests

        [Fact]
        public void IsValid_String_ShouldReturnTrue_WhenValueIsAnyString()
        {
            // Arrange & Act & Assert
            TypeChecker.IsValid("hello", "string").Should().BeTrue();
            TypeChecker.IsValid("123", "string").Should().BeTrue();
            TypeChecker.IsValid("", "string").Should().BeTrue();
            TypeChecker.IsValid(" ", "string").Should().BeTrue();
        }

        [Fact]
        public void IsValid_String_ShouldReturnFalse_WhenValueIsNull()
        {
            // Act & Assert
            TypeChecker.IsValid(null, "string").Should().BeFalse();
        }

        #endregion

        #region Integer Type Tests

        [Theory]
        [InlineData("0")]
        [InlineData("123")]
        [InlineData("-456")]
        [InlineData("2147483647")] // Max int32
        [InlineData("-2147483648")] // Min int32
        public void IsValid_Integer_ShouldReturnTrue_WhenValueIsValidInteger(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "integer").Should().BeTrue();
        }

        [Theory]
        [InlineData("123.45")]
        [InlineData("abc")]
        [InlineData("12.0")]
        [InlineData("1e10")]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_Integer_ShouldReturnFalse_WhenValueIsNotInteger(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "integer").Should().BeFalse();
        }

        #endregion

        #region Decimal Type Tests

        [Theory]
        [InlineData("0")]
        [InlineData("123")]
        [InlineData("123.45")]
        [InlineData("-456.78")]
        [InlineData("0.001")]
        [InlineData("999999.999999")]
        public void IsValid_Decimal_ShouldReturnTrue_WhenValueIsValidDecimal(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "decimal").Should().BeTrue();
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12.34.56")]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_Decimal_ShouldReturnFalse_WhenValueIsNotDecimal(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "decimal").Should().BeFalse();
        }

        #endregion

        #region Boolean Type Tests

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        [InlineData("True")]
        [InlineData("False")]
        [InlineData("TRUE")]
        [InlineData("FALSE")]
        public void IsValid_Boolean_ShouldReturnTrue_WhenValueIsTrueOrFalse(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "boolean").Should().BeTrue();
        }

        [Theory]
        [InlineData("1")]
        [InlineData("0")]
        [InlineData("yes")]
        [InlineData("no")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("truee")]
        public void IsValid_Boolean_ShouldReturnFalse_WhenValueIsNotBoolean(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "boolean").Should().BeFalse();
        }

        #endregion

        #region GUID Type Tests

        [Theory]
        [InlineData("a1b2c3d4-e5f6-7890-abcd-ef1234567890")]
        [InlineData("A1B2C3D4-E5F6-7890-ABCD-EF1234567890")]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        [InlineData("123e4567-e89b-12d3-a456-426614174000")]
        public void IsValid_Guid_ShouldReturnTrue_WhenValueIsValidGuid(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "guid").Should().BeTrue();
        }

        [Theory]
        [InlineData("not-a-guid")]
        [InlineData("12345678-1234-1234-1234-12345678901")] // Too short
        [InlineData("12345678-1234-1234-1234-1234567890123")] // Too long
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_Guid_ShouldReturnFalse_WhenValueIsNotGuid(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "guid").Should().BeFalse();
        }

        #endregion

        #region Date Type Tests

        [Theory]
        [InlineData("2024-01-01")]
        [InlineData("2024-12-31")]
        [InlineData("1900-01-01")]
        [InlineData("2099-12-31")]
        public void IsValid_Date_ShouldReturnTrue_WhenValueIsValidYYYYMMDD(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "date").Should().BeTrue();
        }

        [Theory]
        [InlineData("01-01-2024")] // Wrong format
        [InlineData("2024/01/01")] // Wrong separator
        [InlineData("2024-1-1")] // Missing leading zeros
        [InlineData("2024-13-01")] // Invalid month
        [InlineData("2024-01-32")] // Invalid day
        [InlineData("2024-01-01T00:00:00")] // With time
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_Date_ShouldReturnFalse_WhenValueIsNotValidYYYYMMDD(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "date").Should().BeFalse();
        }

        #endregion

        #region DateTime Type Tests

        [Theory]
        [InlineData("2024-01-01T00:00:00")]
        [InlineData("2024-12-31T23:59:59")]
        [InlineData("2024-01-01T00:00:00Z")]
        [InlineData("2024-01-01T00:00:00+08:00")]
        [InlineData("2024-01-01T00:00:00.000Z")]
        public void IsValid_DateTime_ShouldReturnTrue_WhenValueIsValidISO8601(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "datetime").Should().BeTrue();
        }

        [Theory]
        [InlineData("not-a-datetime")]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_DateTime_ShouldReturnFalse_WhenValueIsNotValidISO8601(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "datetime").Should().BeFalse();
        }

        #endregion

        #region PipeString Array Type Tests

        [Theory]
        [InlineData("value1|value2|value3")]
        [InlineData("single")]
        [InlineData("A|B")]
        [InlineData("pure tone 250Hz|pure tone 500Hz|pure tone 1kHz")]
        public void IsValid_PipeStringArray_ShouldReturnTrue_WhenAllPartsAreNonEmpty(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "pipestring[]").Should().BeTrue();
        }

        [Theory]
        [InlineData("|value2")] // Empty first part
        [InlineData("value1|")] // Empty last part
        [InlineData("value1||value3")] // Empty middle part
        [InlineData("|")] // Only separator
        [InlineData("")] // Empty string
        [InlineData(" ")] // Whitespace only
        public void IsValid_PipeStringArray_ShouldReturnFalse_WhenAnyPartIsEmpty(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "pipestring[]").Should().BeFalse();
        }

        #endregion

        #region Array Type Tests

        [Theory]
        [InlineData("[]")]
        [InlineData("[1, 2, 3]")]
        [InlineData("[\"a\", \"b\"]")]
        [InlineData("[{\"key\": \"value\"}]")]
        public void IsValid_Array_ShouldReturnTrue_WhenValueLooksLikeJsonArray(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "array").Should().BeTrue();
        }

        [Theory]
        [InlineData("not an array")]
        [InlineData("{\"key\": \"value\"}")]
        [InlineData("[1, 2, 3")] // Missing closing bracket
        [InlineData("1, 2, 3]")] // Missing opening bracket
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_Array_ShouldReturnFalse_WhenValueDoesNotLookLikeJsonArray(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "array").Should().BeFalse();
        }

        #endregion

        #region Object Type Tests

        [Theory]
        [InlineData("{}")]
        [InlineData("{\"key\": \"value\"}")]
        [InlineData("{\"nested\": {\"key\": \"value\"}}")]
        public void IsValid_Object_ShouldReturnTrue_WhenValueLooksLikeJsonObject(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "object").Should().BeTrue();
        }

        [Theory]
        [InlineData("not an object")]
        [InlineData("[1, 2, 3]")]
        [InlineData("{\"key\": \"value\"")] // Missing closing brace
        [InlineData("\"key\": \"value\"}")] // Missing opening brace
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_Object_ShouldReturnFalse_WhenValueDoesNotLookLikeJsonObject(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "object").Should().BeFalse();
        }

        #endregion

        #region Case Insensitivity Tests

        [Theory]
        [InlineData("STRING")]
        [InlineData("Integer")]
        [InlineData("DECIMAL")]
        [InlineData("Boolean")]
        [InlineData("GUID")]
        [InlineData("Date")]
        [InlineData("DATETIME")]
        [InlineData("PipeString[]")]
        [InlineData("ARRAY")]
        [InlineData("Object")]
        public void IsValid_ShouldBeCaseInsensitive_ForExpectedType(string expectedType)
        {
            // Arrange
            var validValue = expectedType.ToLowerInvariant() switch
            {
                "string" => "test",
                "integer" => "123",
                "decimal" => "123.45",
                "boolean" => "true",
                "guid" => "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                "date" => "2024-01-01",
                "datetime" => "2024-01-01T00:00:00",
                "pipestring[]" => "A|B",
                "array" => "[]",
                "object" => "{}",
                _ => "test"
            };

            // Act & Assert
            TypeChecker.IsValid(validValue, expectedType).Should().BeTrue();
        }

        #endregion

        #region Null and Unknown Type Tests

        [Fact]
        public void IsValid_ShouldReturnFalse_WhenValueIsNull()
        {
            // Act & Assert
            TypeChecker.IsValid(null, "string").Should().BeFalse();
            TypeChecker.IsValid(null, "integer").Should().BeFalse();
            TypeChecker.IsValid(null, "date").Should().BeFalse();
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_WhenExpectedTypeIsNull()
        {
            // Act & Assert
            TypeChecker.IsValid("any value", null).Should().BeTrue();
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_WhenExpectedTypeIsEmpty()
        {
            // Act & Assert
            TypeChecker.IsValid("any value", "").Should().BeTrue();
            TypeChecker.IsValid("any value", "   ").Should().BeTrue();
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_WhenExpectedTypeIsUnknown()
        {
            // Act & Assert
            TypeChecker.IsValid("any value", "unknown-type").Should().BeTrue();
            TypeChecker.IsValid("any value", "custom").Should().BeTrue();
        }

        #endregion
    }
}
