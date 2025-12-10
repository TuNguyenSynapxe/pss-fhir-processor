using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.PSS.FhirProcessor.Core.Validation;
using System.Collections.Generic;

namespace MOH.HealthierSG.PSS.FhirProcessor.Tests.ValidationEngine
{
    public class ReferenceValidationTests
    {
        private static JObject CreateBundle(params JObject[] resources)
        {
            var entries = new JArray();
            foreach (var resource in resources)
            {
                var resourceType = resource["resourceType"]?.ToString();
                var id = resource["id"]?.ToString();
                
                entries.Add(new JObject
                {
                    ["fullUrl"] = $"urn:uuid:{id}",
                    ["resource"] = resource
                });
            }

            return new JObject
            {
                ["resourceType"] = "Bundle",
                ["type"] = "collection",
                ["entry"] = entries
            };
        }

        private static JObject CreateResource(string resourceType, string id, params (string key, object value)[] properties)
        {
            var resource = new JObject
            {
                ["resourceType"] = resourceType,
                ["id"] = id
            };

            foreach (var (key, value) in properties)
            {
                resource[key] = JToken.FromObject(value);
            }

            return resource;
        }

        private static ValidationResult CreateValidationResult()
        {
            return new ValidationResult();
        }

        #region Happy Path Tests

        [Fact]
        public void ReferenceRule_ShouldPass_WhenReferencedResourceExists_FhirFormat()
        {
            // Arrange
            var encounter = CreateResource("Encounter", "enc-123");
            var encounterRef = new JObject();
            encounterRef["reference"] = "Encounter/enc-123";
            var patient = CreateResource("Patient", "pat-456", 
                ("encounter", encounterRef));
            
            var bundle = CreateBundle(encounter, patient);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "encounter.reference",
                TargetTypes = new List<string> { "Encounter" },
                ErrorCode = "INVALID_ENCOUNTER_REF",
                Message = "Invalid encounter reference"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(patient, rule, "Patient", null, bundle, result, null);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ReferenceRule_ShouldPass_WhenReferencedResourceExists_UrnUuidFormat()
        {
            // Arrange
            var encounter = CreateResource("Encounter", "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d");
            var encounterRef2 = new JObject();
            encounterRef2["reference"] = "urn:uuid:a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d";
            var patient = CreateResource("Patient", "pat-456",
                ("encounter", encounterRef2));

            var bundle = CreateBundle(encounter, patient);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "encounter.reference",
                TargetTypes = new List<string> { "Encounter" },
                ErrorCode = "INVALID_ENCOUNTER_REF",
                Message = "Invalid encounter reference"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(patient, rule, "Patient", null, bundle, result, null);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ReferenceRule_ShouldPass_WhenMultipleTargetTypes_AndOneMatches()
        {
            // Arrange
            var practitioner = CreateResource("Practitioner", "prac-123");
            var performerRef = new JObject();
            performerRef["reference"] = "Practitioner/prac-123";
            var observation = CreateResource("Observation", "obs-456",
                ("performer", new JArray { performerRef }));

            var bundle = CreateBundle(practitioner, observation);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "performer[0].reference",
                TargetTypes = new List<string> { "Practitioner", "Organization", "Patient" },
                ErrorCode = "INVALID_PERFORMER_REF",
                Message = "Invalid performer reference"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(observation, rule, "Observation", null, bundle, result, null);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region Reference Not Found Tests

        [Fact]
        public void ReferenceRule_ShouldFail_WhenReferencedResourceDoesNotExist()
        {
            // Arrange
            var encounterRef3 = new JObject();
            encounterRef3["reference"] = "Encounter/enc-999";
            var patient = CreateResource("Patient", "pat-456",
                ("encounter", encounterRef3));

            var bundle = CreateBundle(patient);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "encounter.reference",
                TargetTypes = new List<string> { "Encounter" },
                ErrorCode = "ENCOUNTER_NOT_FOUND",
                Message = "Encounter not found"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(patient, rule, "Patient", null, bundle, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("ENCOUNTER_NOT_FOUND");
            result.Errors[0].Message.Should().Contain("Encounter not found");
            result.Errors[0].Message.Should().Contain("Encounter/enc-999");
        }

        [Fact]
        public void ReferenceRule_ShouldFail_WhenReferencedResourceDoesNotExist_UrnFormat()
        {
            // Arrange
            var encounterRef4 = new JObject();
            encounterRef4["reference"] = "urn:uuid:99999999-9999-9999-9999-999999999999";
            var patient = CreateResource("Patient", "pat-456",
                ("encounter", encounterRef4));

            var bundle = CreateBundle(patient);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "encounter.reference",
                TargetTypes = new List<string> { "Encounter" },
                ErrorCode = "ENCOUNTER_NOT_FOUND",
                Message = "Encounter not found"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(patient, rule, "Patient", null, bundle, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("ENCOUNTER_NOT_FOUND");
        }

        #endregion

        #region Type Mismatch Tests

        [Fact]
        public void ReferenceRule_ShouldFail_WhenResourceTypeDoesNotMatch()
        {
            // Arrange
            var observation = CreateResource("Observation", "obs-123");
            var encounterRef5 = new JObject();
            encounterRef5["reference"] = "Observation/obs-123";
            var patient = CreateResource("Patient", "pat-456",
                ("encounter", encounterRef5));

            var bundle = CreateBundle(observation, patient);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "encounter.reference",
                TargetTypes = new List<string> { "Encounter" },
                ErrorCode = "WRONG_REFERENCE_TYPE",
                Message = "Must reference an Encounter"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(patient, rule, "Patient", null, bundle, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("WRONG_REFERENCE_TYPE");
            result.Errors[0].Message.Should().Contain("Must reference an Encounter");
            result.Errors[0].Message.Should().Contain("Expected types: Encounter");
            result.Errors[0].Message.Should().Contain("Found: Observation");
        }

        [Fact]
        public void ReferenceRule_ShouldFail_WhenResourceTypeNotInTargetTypes()
        {
            // Arrange
            var location = CreateResource("Location", "loc-123");
            var subjectRef = new JObject();
            subjectRef["reference"] = "Location/loc-123";
            var observation = CreateResource("Observation", "obs-456",
                ("subject", subjectRef));

            var bundle = CreateBundle(location, observation);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "subject.reference",
                TargetTypes = new List<string> { "Patient", "Group" },
                ErrorCode = "INVALID_SUBJECT_TYPE",
                Message = "Subject must be Patient or Group"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(observation, rule, "Observation", null, bundle, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("INVALID_SUBJECT_TYPE");
            result.Errors[0].Message.Should().Contain("Expected types: Patient, Group");
            result.Errors[0].Message.Should().Contain("Found: Location");
        }

        #endregion

        #region Invalid Format Tests

        [Fact]
        public void ReferenceRule_ShouldFail_WhenReferenceFormatIsInvalid()
        {
            // Arrange
            var encounterRef6 = new JObject();
            encounterRef6["reference"] = "just-some-text";
            var patient = CreateResource("Patient", "pat-456",
                ("encounter", encounterRef6));

            var bundle = CreateBundle(patient);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "encounter.reference",
                TargetTypes = new List<string> { "Encounter" },
                ErrorCode = "INVALID_REFERENCE_FORMAT",
                Message = "Invalid reference format"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(patient, rule, "Patient", null, bundle, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("INVALID_REFERENCE_FORMAT");
            result.Errors[0].Message.Should().Contain("just-some-text");
        }

        [Fact]
        public void ReferenceRule_ShouldFail_WhenReferenceHasTooManySlashes()
        {
            // Arrange
            var encounterRef7 = new JObject();
            encounterRef7["reference"] = "Encounter/enc-123/extra";
            var patient = CreateResource("Patient", "pat-456",
                ("encounter", encounterRef7));

            var bundle = CreateBundle(patient);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "encounter.reference",
                TargetTypes = new List<string> { "Encounter" },
                ErrorCode = "INVALID_REFERENCE_FORMAT",
                Message = "Invalid reference format"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(patient, rule, "Patient", null, bundle, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("INVALID_REFERENCE_FORMAT");
        }

        #endregion

        #region Null/Empty Tests

        [Fact]
        public void ReferenceRule_ShouldPass_WhenReferenceIsNull()
        {
            // Arrange
            var patient = CreateResource("Patient", "pat-456");

            var bundle = CreateBundle(patient);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "encounter.reference",
                TargetTypes = new List<string> { "Encounter" },
                ErrorCode = "INVALID_ENCOUNTER_REF",
                Message = "Invalid encounter reference"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(patient, rule, "Patient", null, bundle, result, null);

            // Assert - Should skip validation for null values
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ReferenceRule_ShouldPass_WhenReferenceIsEmptyString()
        {
            // Arrange
            var encounterRef8 = new JObject();
            encounterRef8["reference"] = "";
            var patient = CreateResource("Patient", "pat-456",
                ("encounter", encounterRef8));

            var bundle = CreateBundle(patient);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "encounter.reference",
                TargetTypes = new List<string> { "Encounter" },
                ErrorCode = "INVALID_ENCOUNTER_REF",
                Message = "Invalid encounter reference"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(patient, rule, "Patient", null, bundle, result, null);

            // Assert - Should skip validation for empty values
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region Missing Configuration Tests

        [Fact]
        public void ReferenceRule_ShouldFail_WhenTargetTypesIsMissing()
        {
            // Arrange
            var encounterRef9 = new JObject();
            encounterRef9["reference"] = "Encounter/enc-123";
            var patient = CreateResource("Patient", "pat-456",
                ("encounter", encounterRef9));

            var bundle = CreateBundle(patient);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "encounter.reference",
                TargetTypes = null, // Missing
                ErrorCode = "INVALID_ENCOUNTER_REF",
                Message = "Invalid encounter reference"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(patient, rule, "Patient", null, bundle, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("REFERENCE_VALIDATION_ERROR");
            result.Errors[0].Message.Should().Contain("Reference rule missing TargetTypes");
        }

        [Fact]
        public void ReferenceRule_ShouldFail_WhenTargetTypesIsEmpty()
        {
            // Arrange
            var encounterRef10 = new JObject();
            encounterRef10["reference"] = "Encounter/enc-123";
            var patient = CreateResource("Patient", "pat-456",
                ("encounter", encounterRef10));

            var bundle = CreateBundle(patient);

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "encounter.reference",
                TargetTypes = new List<string>(), // Empty
                ErrorCode = "INVALID_ENCOUNTER_REF",
                Message = "Invalid encounter reference"
            };

            var result = CreateValidationResult();

            // Act
            RuleEvaluator.ApplyRule(patient, rule, "Patient", null, bundle, result, null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("REFERENCE_VALIDATION_ERROR");
            result.Errors[0].Message.Should().Contain("Reference rule missing TargetTypes");
        }

        #endregion
    }
}
