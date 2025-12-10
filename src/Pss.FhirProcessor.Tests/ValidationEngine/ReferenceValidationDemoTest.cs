using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.PSS.FhirProcessor.Core.Validation;
using System.Collections.Generic;

namespace MOH.HealthierSG.PSS.FhirProcessor.Tests.ValidationEngine
{
    /// <summary>
    /// Demo test showing Reference validation catching invalid patient references
    /// </summary>
    public class ReferenceValidationDemoTest
    {
        [Fact]
        public void Demo_ReferenceValidation_ShouldCatchWrongPatientReference()
        {
            // Arrange - Create a bundle with Patient ID that doesn't match Observation.subject reference
            var bundle = new JObject
            {
                ["resourceType"] = "Bundle",
                ["type"] = "collection",
                ["entry"] = new JArray
                {
                    // Patient with CORRECT ID
                    new JObject
                    {
                        ["fullUrl"] = "urn:uuid:CORRECT-PATIENT-ID",
                        ["resource"] = new JObject
                        {
                            ["resourceType"] = "Patient",
                            ["id"] = "CORRECT-PATIENT-ID"
                        }
                    },
                    // Observation referencing WRONG Patient ID
                    new JObject
                    {
                        ["fullUrl"] = "urn:uuid:obs-123",
                        ["resource"] = new JObject
                        {
                            ["resourceType"] = "Observation",
                            ["id"] = "obs-123",
                            ["subject"] = new JObject
                            {
                                ["reference"] = "urn:uuid:WRONG-PATIENT-ID"  // This doesn't exist!
                            }
                        }
                    }
                }
            };

            var observation = (JObject)bundle["entry"][1]["resource"];

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "subject.reference",  // Path relative to the Observation resource
                TargetTypes = new List<string> { "Patient" },
                ErrorCode = "INVALID_PATIENT_REFERENCE",
                Message = "Observation.subject must reference a valid Patient."
            };

            var result = new ValidationResult();

            // Act - Apply the Reference validation rule
            RuleEvaluator.ApplyRule(observation, rule, "Observation", null, bundle, result, null);

            // Assert - Should FAIL because referenced Patient doesn't exist
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("INVALID_PATIENT_REFERENCE");
            result.Errors[0].Message.Should().Contain("WRONG-PATIENT-ID");
        }

        [Fact]
        public void Demo_ReferenceValidation_ShouldPassWithCorrectReference()
        {
            // Arrange - Create a bundle where references match correctly
            var bundle = new JObject
            {
                ["resourceType"] = "Bundle",
                ["type"] = "collection",
                ["entry"] = new JArray
                {
                    // Patient
                    new JObject
                    {
                        ["fullUrl"] = "urn:uuid:CORRECT-PATIENT-ID",
                        ["resource"] = new JObject
                        {
                            ["resourceType"] = "Patient",
                            ["id"] = "CORRECT-PATIENT-ID"
                        }
                    },
                    // Observation referencing the CORRECT Patient ID
                    new JObject
                    {
                        ["fullUrl"] = "urn:uuid:obs-123",
                        ["resource"] = new JObject
                        {
                            ["resourceType"] = "Observation",
                            ["id"] = "obs-123",
                            ["subject"] = new JObject
                            {
                                ["reference"] = "urn:uuid:CORRECT-PATIENT-ID"  // This exists!
                            }
                        }
                    }
                }
            };

            var observation = (JObject)bundle["entry"][1]["resource"];

            var rule = new RuleDefinition
            {
                RuleType = "Reference",
                PathType = "CPS1",
                Path = "subject.reference",  // Path relative to the Observation resource
                TargetTypes = new List<string> { "Patient" },
                ErrorCode = "INVALID_PATIENT_REFERENCE",
                Message = "Observation.subject must reference a valid Patient."
            };

            var result = new ValidationResult();

            // Act
            RuleEvaluator.ApplyRule(observation, rule, "Observation", null, bundle, result, null);

            // Assert - Should PASS because referenced Patient exists
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
