using Xunit;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for ArrayLength validation rule
    /// Tests Min/Max constraints and NonEmptyForStrings validation
    /// </summary>
    public class ArrayLengthRuleTests
    {
        [Fact]
        public void ArrayLength_ValidLength_PassesValidation()
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
                                RuleType = "ArrayLength",
                                Path = "address[*].line",
                                Min = 4,
                                Max = 5,
                                ElementType = "string",
                                NonEmptyForStrings = true,
                                ErrorCode = "ARRAY_LENGTH_INVALID",
                                Message = "Patient.address[*].line must contain 4 to 5 non-empty address lines."
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
                            ""address"": [
                                {
                                    ""use"": ""home"",
                                    ""line"": [""Line 1"", ""Line 2"", ""Line 3"", ""Line 4""]
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
            Assert.True(result.IsValid, "Validation should pass for array with 4 non-empty strings (within Min=4, Max=5)");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ArrayLength_BelowMinimum_FailsValidation()
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
                                RuleType = "ArrayLength",
                                Path = "address[*].line",
                                Min = 4,
                                Max = 5,
                                ElementType = "string",
                                NonEmptyForStrings = true,
                                ErrorCode = "ARRAY_LENGTH_INVALID",
                                Message = "Patient.address[*].line must contain 4 to 5 non-empty address lines."
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
                            ""address"": [
                                {
                                    ""use"": ""home"",
                                    ""line"": [""Line 1"", ""Line 2"", ""Line 3""]
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
            Assert.False(result.IsValid, "Validation should fail for array with only 3 strings (below Min=4)");
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "ARRAY_LENGTH_INVALID");
            Assert.Contains(result.Errors, e => e.Message.Contains("Actual: 3"));
        }

        [Fact]
        public void ArrayLength_AboveMaximum_FailsValidation()
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
                                RuleType = "ArrayLength",
                                Path = "address[*].line",
                                Min = 4,
                                Max = 5,
                                ElementType = "string",
                                NonEmptyForStrings = true,
                                ErrorCode = "ARRAY_LENGTH_INVALID",
                                Message = "Patient.address[*].line must contain 4 to 5 non-empty address lines."
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
                            ""address"": [
                                {
                                    ""use"": ""home"",
                                    ""line"": [""Line 1"", ""Line 2"", ""Line 3"", ""Line 4"", ""Line 5"", ""Line 6""]
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
            Assert.False(result.IsValid, "Validation should fail for array with 6 strings (above Max=5)");
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "ARRAY_LENGTH_INVALID");
            Assert.Contains(result.Errors, e => e.Message.Contains("Actual: 6"));
        }

        [Fact]
        public void ArrayLength_ContainsEmptyString_FailsValidation()
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
                                RuleType = "ArrayLength",
                                Path = "address[*].line",
                                Min = 4,
                                Max = 5,
                                ElementType = "string",
                                NonEmptyForStrings = true,
                                ErrorCode = "ARRAY_LENGTH_INVALID",
                                Message = "Patient.address[*].line must contain 4 to 5 non-empty address lines."
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
                            ""address"": [
                                {
                                    ""use"": ""home"",
                                    ""line"": [""Line 1"", ""Line 2"", """", ""Line 4""]
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
            Assert.False(result.IsValid, "Validation should fail when NonEmptyForStrings=true and array contains empty string");
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "ARRAY_LENGTH_INVALID");
            Assert.Contains(result.Errors, e => e.Message.Contains("Element[2] is empty"));
        }

        [Fact]
        public void ArrayLength_WithoutNonEmptyCheck_AllowsEmptyStrings()
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
                                RuleType = "ArrayLength",
                                Path = "address[*].line",
                                Min = 4,
                                Max = 5,
                                ErrorCode = "ARRAY_LENGTH_INVALID",
                                Message = "Patient.address[*].line must contain 4 to 5 address lines."
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
                            ""address"": [
                                {
                                    ""use"": ""home"",
                                    ""line"": [""Line 1"", ""Line 2"", """", ""Line 4""]
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
            Assert.True(result.IsValid, "Validation should pass when NonEmptyForStrings is not set and array length is within bounds");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ArrayLength_MultipleAddresses_ValidatesEach()
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
                                RuleType = "ArrayLength",
                                Path = "address[*].line",
                                Min = 2,
                                Max = 5,
                                ErrorCode = "ARRAY_LENGTH_INVALID",
                                Message = "Patient.address[*].line must contain 2 to 5 address lines."
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
                            ""address"": [
                                {
                                    ""use"": ""home"",
                                    ""line"": [""Home Line 1"", ""Home Line 2""]
                                },
                                {
                                    ""use"": ""work"",
                                    ""line"": [""Work Line 1""]
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
            Assert.False(result.IsValid, "Validation should fail when second address has only 1 line (below Min=2)");
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "ARRAY_LENGTH_INVALID" && e.Message.Contains("Actual: 1"));
        }

        [Fact]
        public void ArrayLength_PathNotAnArray_FailsValidation()
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
                                RuleType = "ArrayLength",
                                Path = "gender",
                                Min = 1,
                                Max = 5,
                                ErrorCode = "ARRAY_LENGTH_INVALID",
                                Message = "Expected array at path."
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
            Assert.False(result.IsValid, "Validation should fail when path points to non-array value");
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "ARRAY_LENGTH_INVALID" && e.Message.Contains("Expected array"));
        }
    }
}
