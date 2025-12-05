using FluentAssertions;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation;
using Xunit;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.UnitTests
{
    /// <summary>
    /// Tests for guid-uri type validation in TypeChecker
    /// Format: urn:uuid:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}
    /// </summary>
    public class TypeGuidUriTests
    {
        [Theory]
        [InlineData("urn:uuid:a1b2c3d4-e5f6-7890-abcd-ef1234567890")]
        [InlineData("urn:uuid:A1B2C3D4-E5F6-7890-ABCD-EF1234567890")]
        [InlineData("urn:uuid:00000000-0000-0000-0000-000000000000")]
        [InlineData("urn:uuid:123e4567-e89b-12d3-a456-426614174000")]
        [InlineData("urn:uuid:ffffffff-ffff-ffff-ffff-ffffffffffff")]
        [InlineData("urn:uuid:FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
        public void IsValid_GuidUri_ShouldReturnTrue_WhenValueIsValidGuidUri(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "guid-uri").Should().BeTrue();
        }

        [Theory]
        [InlineData("a1b2c3d4-e5f6-7890-abcd-ef1234567890")] // Missing prefix
        [InlineData("uuid:a1b2c3d4-e5f6-7890-abcd-ef1234567890")] // Missing urn:
        [InlineData("urn:a1b2c3d4-e5f6-7890-abcd-ef1234567890")] // Missing uuid:
        [InlineData("urn:uuid:a1b2c3d4-e5f6-7890-abcd-ef123456789")] // Too short
        [InlineData("urn:uuid:a1b2c3d4-e5f6-7890-abcd-ef12345678901")] // Too long
        [InlineData("urn:uuid:a1b2c3d4-e5f6-7890-abcd-ef123456789g")] // Invalid char
        [InlineData("urn:uuid:a1b2c3d4e5f678901234ef1234567890")] // Missing hyphens
        [InlineData("urn:uuid:a1b2c3d4-e5f6-7890-abcd")] // Incomplete
        [InlineData("URN:UUID:a1b2c3d4-e5f6-7890-abcd-ef1234567890")] // Wrong case prefix
        [InlineData("")] // Empty
        [InlineData(" ")] // Whitespace
        [InlineData("not-a-guid-uri")] // Random string
        public void IsValid_GuidUri_ShouldReturnFalse_WhenValueIsNotValidGuidUri(string value)
        {
            // Act & Assert
            TypeChecker.IsValid(value, "guid-uri").Should().BeFalse();
        }

        [Fact]
        public void IsValid_GuidUri_ShouldReturnFalse_WhenValueIsNull()
        {
            // Act & Assert
            TypeChecker.IsValid(null, "guid-uri").Should().BeFalse();
        }

        [Theory]
        [InlineData("GUID-URI")]
        [InlineData("Guid-Uri")]
        [InlineData("guid-URI")]
        public void IsValid_GuidUri_ShouldBeCaseInsensitive_ForExpectedType(string expectedType)
        {
            // Arrange
            var validValue = "urn:uuid:a1b2c3d4-e5f6-7890-abcd-ef1234567890";

            // Act & Assert
            TypeChecker.IsValid(validValue, expectedType).Should().BeTrue();
        }

        [Fact]
        public void IsValid_GuidUri_ShouldAccept_MixedCaseGuid()
        {
            // Arrange
            var mixedCase = "urn:uuid:A1b2C3d4-E5f6-7890-AbCd-Ef1234567890";

            // Act & Assert
            TypeChecker.IsValid(mixedCase, "guid-uri").Should().BeTrue();
        }

        [Fact]
        public void IsValid_GuidUri_ShouldReject_SpacesInValue()
        {
            // Arrange
            var withSpaces = "urn:uuid: a1b2c3d4-e5f6-7890-abcd-ef1234567890";

            // Act & Assert
            TypeChecker.IsValid(withSpaces, "guid-uri").Should().BeFalse();
        }

        [Fact]
        public void IsValid_GuidUri_ShouldReject_TrailingCharacters()
        {
            // Arrange
            var withTrailing = "urn:uuid:a1b2c3d4-e5f6-7890-abcd-ef1234567890extra";

            // Act & Assert
            TypeChecker.IsValid(withTrailing, "guid-uri").Should().BeFalse();
        }

        [Fact]
        public void IsValid_GuidUri_ShouldReject_LeadingCharacters()
        {
            // Arrange
            var withLeading = "extraurn:uuid:a1b2c3d4-e5f6-7890-abcd-ef1234567890";

            // Act & Assert
            TypeChecker.IsValid(withLeading, "guid-uri").Should().BeFalse();
        }
    }
}
