using Xunit;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for AllowedValues validation rule
    /// </summary>
    public class AllowedValuesRuleTests
    {
        [Fact]
        public void AllowedValues_ValidValue_PassesValidation()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "11.0",
                PathSyntax = "CPS1",
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Patient",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "AllowedValues",
                                Path = "gender",
                                AllowedValues = new List<string> { "male", "female", "other", "unknown" },
                                ErrorCode = "INVALID_GENDER",
                                Message = "Gender must be one of the allowed values"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Patient"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"",
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""id"": ""a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"",
                            ""gender"": ""male""
                        }
                    }
                ]
            }";

            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            var metadataJson = JsonConvert.SerializeObject(metadata);
            engine.LoadMetadataFromJson(metadataJson);

            // Act
            var result = engine.Validate(bundleJson, "error");

            // Assert
            Assert.True(result.IsValid, "Validation should pass for valid gender value");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void AllowedValues_InvalidValue_FailsValidation()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "11.0",
                PathSyntax = "CPS1",
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Patient",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "AllowedValues",
                                Path = "gender",
                                AllowedValues = new List<string> { "male", "female", "other", "unknown" },
                                ErrorCode = "INVALID_GENDER",
                                Message = "Gender must be one of the allowed values"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Patient"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"",
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""id"": ""a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"",
                            ""gender"": ""invalid-gender""
                        }
                    }
                ]
            }";

            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            var metadataJson = JsonConvert.SerializeObject(metadata);
            engine.LoadMetadataFromJson(metadataJson);

            // Act
            var result = engine.Validate(bundleJson, "verbose");

            // Assert
            if (!result.IsValid)
            {
                // Good - validation failed as expected
                Assert.Single(result.Errors);
                Assert.Equal("INVALID_GENDER", result.Errors[0].Code);
                Assert.Contains("invalid-gender", result.Errors[0].Message);
                Assert.Contains("male", result.Errors[0].Message);
            }
            else
            {
                // Fail with diagnostic message including logs
                var logsText = result.Logs != null ? string.Join("\n", result.Logs) : "No logs";
                var diagnosticMessage = $"Expected validation to fail but it passed.\n" +
                    $"IsValid={result.IsValid}, Errors={result.Errors.Count}\n" +
                    $"Logs:\n{logsText}";
                Assert.True(false, diagnosticMessage);
            }
            Assert.Single(result.Errors);
            Assert.Equal("INVALID_GENDER", result.Errors[0].Code);
            Assert.Contains("invalid-gender", result.Errors[0].Message);
            Assert.Contains("male", result.Errors[0].Message);
        }

        [Fact]
        public void AllowedValues_MissingField_SkipsValidation()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "11.0",
                PathSyntax = "CPS1",
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Patient",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "AllowedValues",
                                Path = "gender",
                                AllowedValues = new List<string> { "male", "female", "other", "unknown" },
                                ErrorCode = "INVALID_GENDER",
                                Message = "Gender must be one of the allowed values"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Patient"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"",
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""id"": ""a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91""
                        }
                    }
                ]
            }";

            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            var metadataJson = JsonConvert.SerializeObject(metadata);
            engine.LoadMetadataFromJson(metadataJson);

            // Act
            var result = engine.Validate(bundleJson, "error");

            // Assert
            Assert.True(result.IsValid, "Validation should skip when field is missing (use Required rule for mandatory fields)");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void AllowedValues_EmptyValue_SkipsValidation()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "11.0",
                PathSyntax = "CPS1",
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Patient",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "AllowedValues",
                                Path = "gender",
                                AllowedValues = new List<string> { "male", "female", "other", "unknown" },
                                ErrorCode = "INVALID_GENDER",
                                Message = "Gender must be one of the allowed values"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Patient"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"",
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""id"": ""a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"",
                            ""gender"": """"
                        }
                    }
                ]
            }";

            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            var metadataJson = JsonConvert.SerializeObject(metadata);
            engine.LoadMetadataFromJson(metadataJson);

            // Act
            var result = engine.Validate(bundleJson, "error");

            // Assert
            Assert.True(result.IsValid, "Validation should skip when field is empty");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void AllowedValues_MultipleValues_ValidatesEach()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "11.0",
                PathSyntax = "CPS1",
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Patient",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "AllowedValues",
                                Path = "communication[*].language.coding[*].code",
                                AllowedValues = new List<string> { "en", "zh", "ms", "ta" },
                                ErrorCode = "INVALID_LANGUAGE",
                                Message = "Language code must be one of the allowed values"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Patient"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"",
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""id"": ""a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"",
                            ""communication"": [
                                {
                                    ""language"": {
                                        ""coding"": [
                                            { ""code"": ""en"" }
                                        ]
                                    }
                                },
                                {
                                    ""language"": {
                                        ""coding"": [
                                            { ""code"": ""fr"" }
                                        ]
                                    }
                                }
                            ]
                        }
                    }
                ]
            }";

            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            var metadataJson = JsonConvert.SerializeObject(metadata);
            engine.LoadMetadataFromJson(metadataJson);

            // Act
            var result = engine.Validate(bundleJson, "error");

            // Assert
            Assert.False(result.IsValid, "Validation should fail when one of multiple values is invalid");
            Assert.Single(result.Errors);
            Assert.Equal("INVALID_LANGUAGE", result.Errors[0].Code);
            Assert.Contains("fr", result.Errors[0].Message);
        }
    }
}
