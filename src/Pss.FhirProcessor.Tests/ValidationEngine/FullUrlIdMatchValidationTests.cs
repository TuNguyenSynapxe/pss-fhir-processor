using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation;
using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.ValidationEngine
{
    /// <summary>
    /// Unit tests for FullUrlIdMatch validation rule
    /// Tests that resource.id matches the GUID portion of entry.fullUrl (urn:uuid:GUID)
    /// </summary>
    public class FullUrlIdMatchValidationTests
    {
        private ValidationResult CreateValidationResult()
        {
            return new ValidationResult();
        }

        private JObject CreateResource(string resourceType, string id)
        {
            return new JObject
            {
                ["resourceType"] = resourceType,
                ["id"] = id
            };
        }

        [Fact]
        public void FullUrlIdMatch_ShouldPass_WhenIdMatchesFullUrlGuid()
        {
            // Arrange
            var guid = "a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91";
            var resource = CreateResource("Patient", guid);
            var fullUrl = $"urn:uuid:{guid}";

            var rule = new RuleDefinition
            {
                RuleType = "FullUrlIdMatch",
                ErrorCode = "ID_FULLURL_MISMATCH",
                Message = "Resource.id must match GUID portion of entry.fullUrl."
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null, fullUrl);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void FullUrlIdMatch_ShouldPass_WhenIdMatchesFullUrlGuid_CaseInsensitive()
        {
            // Arrange
            var resource = CreateResource("Patient", "a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91");
            var fullUrl = "urn:uuid:A3F9C2D1-2B40-4AB9-8133-5B1F2D4E9F91"; // Uppercase

            var rule = new RuleDefinition
            {
                RuleType = "FullUrlIdMatch",
                ErrorCode = "ID_FULLURL_MISMATCH",
                Message = "Resource.id must match GUID portion of entry.fullUrl."
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null, fullUrl);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void FullUrlIdMatch_ShouldFail_WhenIdDoesNotMatchFullUrlGuid()
        {
            // Arrange
            var resource = CreateResource("Patient", "a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91");
            var fullUrl = "urn:uuid:different-guid-0000-0000-0000-000000000000";

            var rule = new RuleDefinition
            {
                RuleType = "FullUrlIdMatch",
                ErrorCode = "ID_FULLURL_MISMATCH",
                Message = "Resource.id must match GUID portion of entry.fullUrl."
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null, fullUrl);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("ID_FULLURL_MISMATCH");
            result.Errors[0].Message.Should().Contain("a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91");
            result.Errors[0].Message.Should().Contain("different-guid-0000-0000-0000-000000000000");
        }

        [Fact]
        public void FullUrlIdMatch_ShouldSkip_WhenIdIsMissing()
        {
            // Arrange - resource without id
            var resource = new JObject
            {
                ["resourceType"] = "Patient"
                // No id field
            };
            var fullUrl = "urn:uuid:a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91";

            var rule = new RuleDefinition
            {
                RuleType = "FullUrlIdMatch",
                ErrorCode = "ID_FULLURL_MISMATCH",
                Message = "Resource.id must match GUID portion of entry.fullUrl."
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null, fullUrl);

            // Assert - Should skip validation (Required rule will catch missing id)
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void FullUrlIdMatch_ShouldSkip_WhenIdIsEmpty()
        {
            // Arrange
            var resource = CreateResource("Patient", "");
            var fullUrl = "urn:uuid:a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91";

            var rule = new RuleDefinition
            {
                RuleType = "FullUrlIdMatch",
                ErrorCode = "ID_FULLURL_MISMATCH",
                Message = "Resource.id must match GUID portion of entry.fullUrl."
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null, fullUrl);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void FullUrlIdMatch_ShouldSkip_WhenFullUrlIsMissing()
        {
            // Arrange
            var resource = CreateResource("Patient", "a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91");
            string fullUrl = null; // No fullUrl

            var rule = new RuleDefinition
            {
                RuleType = "FullUrlIdMatch",
                ErrorCode = "ID_FULLURL_MISMATCH",
                Message = "Resource.id must match GUID portion of entry.fullUrl."
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null, fullUrl);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void FullUrlIdMatch_ShouldSkip_WhenFullUrlIsEmpty()
        {
            // Arrange
            var resource = CreateResource("Patient", "a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91");
            string fullUrl = ""; // Empty fullUrl

            var rule = new RuleDefinition
            {
                RuleType = "FullUrlIdMatch",
                ErrorCode = "ID_FULLURL_MISMATCH",
                Message = "Resource.id must match GUID portion of entry.fullUrl."
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null, fullUrl);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void FullUrlIdMatch_ShouldSkip_WhenFullUrlIsNotUrnUuidFormat()
        {
            // Arrange
            var resource = CreateResource("Patient", "a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91");
            var fullUrl = "http://example.com/Patient/123"; // Not urn:uuid format

            var rule = new RuleDefinition
            {
                RuleType = "FullUrlIdMatch",
                ErrorCode = "ID_FULLURL_MISMATCH",
                Message = "Resource.id must match GUID portion of entry.fullUrl."
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null, fullUrl);

            // Assert - Should skip (Type rule will handle non-urn:uuid format)
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void FullUrlIdMatch_ShouldSkip_WhenFullUrlHasWrongUrnPrefix()
        {
            // Arrange
            var resource = CreateResource("Patient", "a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91");
            var fullUrl = "urn:oid:a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"; // urn:oid instead of urn:uuid

            var rule = new RuleDefinition
            {
                RuleType = "FullUrlIdMatch",
                ErrorCode = "ID_FULLURL_MISMATCH",
                Message = "Resource.id must match GUID portion of entry.fullUrl."
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null, fullUrl);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void FullUrlIdMatch_ShouldWorkWithDifferentResourceTypes()
        {
            // Arrange
            var guid = "bb7c8a63-0c49-40cf-9b0c-66e8b64d9254";
            var resource = CreateResource("Encounter", guid);
            var fullUrl = $"urn:uuid:{guid}";

            var rule = new RuleDefinition
            {
                RuleType = "FullUrlIdMatch",
                ErrorCode = "ID_FULLURL_MISMATCH",
                Message = "Resource.id must match GUID portion of entry.fullUrl."
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Encounter", null, null, result, null, fullUrl);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void FullUrlIdMatch_ShouldUseCustomErrorCode()
        {
            // Arrange
            var resource = CreateResource("Patient", "wrong-id");
            var fullUrl = "urn:uuid:correct-id";

            var rule = new RuleDefinition
            {
                RuleType = "FullUrlIdMatch",
                ErrorCode = "CUSTOM_ERROR_CODE",
                Message = "Custom error message."
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Patient", null, null, result, null, fullUrl);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("CUSTOM_ERROR_CODE");
            result.Errors[0].Message.Should().Contain("Custom error message");
        }
    }
}
