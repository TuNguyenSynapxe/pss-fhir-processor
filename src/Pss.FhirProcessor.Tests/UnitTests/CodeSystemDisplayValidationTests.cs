using Xunit;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace Pss.FhirProcessor.Tests.UnitTests
{
    /// <summary>
    /// Tests for CodeSystem validation with display checking
    /// </summary>
    public class CodeSystemDisplayValidationTests
    {
        [Fact]
        public void CodeSystem_ValidCodeAndDisplay_PassesValidation()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "5.0",
                PathSyntax = "CPS1",
                CodesMaster = new CodesMaster
                {
                    CodeSystems = new List<CodesMasterCodeSystem>
                    {
                        new CodesMasterCodeSystem
                        {
                            System = "http://example.org/test-system",
                            Concepts = new List<CodesMasterConcept>
                            {
                                new CodesMasterConcept { Code = "CODE1", Display = "Test Display 1" },
                                new CodesMasterConcept { Code = "CODE2", Display = "Test Display 2" }
                            }
                        }
                    }
                },
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Observation",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "CodeSystem",
                                Path = "code.coding",
                                System = "http://example.org/test-system"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Observation"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:obs-1"",
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""id"": ""obs-1"",
                            ""code"": {
                                ""coding"": [{
                                    ""system"": ""http://example.org/test-system"",
                                    ""code"": ""CODE1"",
                                    ""display"": ""Test Display 1""
                                }]
                            },
                            ""status"": ""final""
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
            Assert.True(result.IsValid, "Validation should pass for valid code and display");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CodeSystem_ValidCodeButInvalidDisplay_FailsValidation()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "5.0",
                PathSyntax = "CPS1",
                CodesMaster = new CodesMaster
                {
                    CodeSystems = new List<CodesMasterCodeSystem>
                    {
                        new CodesMasterCodeSystem
                        {
                            System = "http://example.org/test-system",
                            Concepts = new List<CodesMasterConcept>
                            {
                                new CodesMasterConcept { Code = "CODE1", Display = "Test Display 1" }
                            }
                        }
                    }
                },
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Observation",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "CodeSystem",
                                Path = "code.coding",
                                System = "http://example.org/test-system"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Observation"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:obs-1"",
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""id"": ""obs-1"",
                            ""code"": {
                                ""coding"": [{
                                    ""system"": ""http://example.org/test-system"",
                                    ""code"": ""CODE1"",
                                    ""display"": ""Wrong Display""
                                }]
                            },
                            ""status"": ""final""
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
            Assert.False(result.IsValid, "Validation should fail for invalid display");
            Assert.Single(result.Errors);
            Assert.Contains("INVALID_DISPLAY", result.Errors[0].Code);
            Assert.Contains("Wrong Display", result.Errors[0].Message);
            Assert.Contains("Test Display 1", result.Errors[0].Message);
        }

        [Fact]
        public void CodeSystem_ValidCodeNoDisplayInCoding_SkipsDisplayValidation()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "5.0",
                PathSyntax = "CPS1",
                CodesMaster = new CodesMaster
                {
                    CodeSystems = new List<CodesMasterCodeSystem>
                    {
                        new CodesMasterCodeSystem
                        {
                            System = "http://example.org/test-system",
                            Concepts = new List<CodesMasterConcept>
                            {
                                new CodesMasterConcept { Code = "CODE1", Display = "Test Display 1" }
                            }
                        }
                    }
                },
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Observation",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "CodeSystem",
                                Path = "code.coding",
                                System = "http://example.org/test-system"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Observation"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:obs-1"",
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""id"": ""obs-1"",
                            ""code"": {
                                ""coding"": [{
                                    ""system"": ""http://example.org/test-system"",
                                    ""code"": ""CODE1""
                                }]
                            },
                            ""status"": ""final""
                        }
                    }
                ]
            }";

            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            var metadataJson = JsonConvert.SerializeObject(metadata);
            engine.LoadMetadataFromJson(metadataJson);

            // Act
            var result = engine.Validate(bundleJson, "error");

            // Assert - Should pass because display is optional
            Assert.True(result.IsValid, "Validation should pass when display is not provided");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CodeSystem_ValidCodeNoDisplayInMetadata_SkipsDisplayValidation()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "5.0",
                PathSyntax = "CPS1",
                CodesMaster = new CodesMaster
                {
                    CodeSystems = new List<CodesMasterCodeSystem>
                    {
                        new CodesMasterCodeSystem
                        {
                            System = "http://example.org/test-system",
                            Concepts = new List<CodesMasterConcept>
                            {
                                new CodesMasterConcept { Code = "CODE1", Display = null }
                            }
                        }
                    }
                },
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Observation",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "CodeSystem",
                                Path = "code.coding",
                                System = "http://example.org/test-system"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Observation"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:obs-1"",
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""id"": ""obs-1"",
                            ""code"": {
                                ""coding"": [{
                                    ""system"": ""http://example.org/test-system"",
                                    ""code"": ""CODE1"",
                                    ""display"": ""Some Display""
                                }]
                            },
                            ""status"": ""final""
                        }
                    }
                ]
            }";

            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            var metadataJson = JsonConvert.SerializeObject(metadata);
            engine.LoadMetadataFromJson(metadataJson);

            // Act
            var result = engine.Validate(bundleJson, "error");

            // Assert - Should pass because metadata has no display to validate against
            Assert.True(result.IsValid, "Validation should pass when metadata has no display");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CodeSystem_DisplayCaseSensitive_FailsOnCaseMismatch()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "5.0",
                PathSyntax = "CPS1",
                CodesMaster = new CodesMaster
                {
                    CodeSystems = new List<CodesMasterCodeSystem>
                    {
                        new CodesMasterCodeSystem
                        {
                            System = "http://example.org/test-system",
                            Concepts = new List<CodesMasterConcept>
                            {
                                new CodesMasterConcept { Code = "CODE1", Display = "Test Display 1" }
                            }
                        }
                    }
                },
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Observation",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "CodeSystem",
                                Path = "code.coding",
                                System = "http://example.org/test-system"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Observation"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:obs-1"",
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""id"": ""obs-1"",
                            ""code"": {
                                ""coding"": [{
                                    ""system"": ""http://example.org/test-system"",
                                    ""code"": ""CODE1"",
                                    ""display"": ""test display 1""
                                }]
                            },
                            ""status"": ""final""
                        }
                    }
                ]
            }";

            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            var metadataJson = JsonConvert.SerializeObject(metadata);
            engine.LoadMetadataFromJson(metadataJson);

            // Act
            var result = engine.Validate(bundleJson, "error");

            // Assert - Display comparison is case-sensitive
            Assert.False(result.IsValid, "Validation should fail for case mismatch");
            Assert.Single(result.Errors);
            Assert.Contains("INVALID_DISPLAY", result.Errors[0].Code);
        }

        [Fact]
        public void CodeSystem_MultipleCodingsWithMixedDisplays_ValidatesEach()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "5.0",
                PathSyntax = "CPS1",
                CodesMaster = new CodesMaster
                {
                    CodeSystems = new List<CodesMasterCodeSystem>
                    {
                        new CodesMasterCodeSystem
                        {
                            System = "http://example.org/test-system",
                            Concepts = new List<CodesMasterConcept>
                            {
                                new CodesMasterConcept { Code = "CODE1", Display = "Test Display 1" },
                                new CodesMasterConcept { Code = "CODE2", Display = "Test Display 2" }
                            }
                        }
                    }
                },
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Observation",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "CodeSystem",
                                Path = "code.coding",
                                System = "http://example.org/test-system"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Observation"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:obs-1"",
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""id"": ""obs-1"",
                            ""code"": {
                                ""coding"": [
                                    {
                                        ""system"": ""http://example.org/test-system"",
                                        ""code"": ""CODE1"",
                                        ""display"": ""Test Display 1""
                                    },
                                    {
                                        ""system"": ""http://example.org/test-system"",
                                        ""code"": ""CODE2"",
                                        ""display"": ""Wrong Display""
                                    }
                                ]
                            },
                            ""status"": ""final""
                        }
                    }
                ]
            }";

            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            var metadataJson = JsonConvert.SerializeObject(metadata);
            engine.LoadMetadataFromJson(metadataJson);

            // Act
            var result = engine.Validate(bundleJson, "error");

            // Assert - First coding passes, second fails
            Assert.False(result.IsValid, "Validation should fail for second coding");
            Assert.Single(result.Errors);
            Assert.Contains("Wrong Display", result.Errors[0].Message);
        }

        [Fact]
        public void CodeSystem_InvalidCodeAndInvalidDisplay_ReportsCodeError()
        {
            // Arrange
            var metadata = new ValidationMetadata
            {
                Version = "5.0",
                PathSyntax = "CPS1",
                CodesMaster = new CodesMaster
                {
                    CodeSystems = new List<CodesMasterCodeSystem>
                    {
                        new CodesMasterCodeSystem
                        {
                            System = "http://example.org/test-system",
                            Concepts = new List<CodesMasterConcept>
                            {
                                new CodesMasterConcept { Code = "CODE1", Display = "Test Display 1" }
                            }
                        }
                    }
                },
                RuleSets = new List<RuleSet>
                {
                    new RuleSet
                    {
                        Scope = "Observation",
                        Rules = new List<RuleDefinition>
                        {
                            new RuleDefinition
                            {
                                RuleType = "CodeSystem",
                                Path = "code.coding",
                                System = "http://example.org/test-system"
                            }
                        },
                        ScopeDefinition = new ScopeDefinition
                        {
                            ResourceType = "Observation"
                        }
                    }
                }
            };

            var bundleJson = @"{
                ""resourceType"": ""Bundle"",
                ""type"": ""collection"",
                ""entry"": [
                    {
                        ""fullUrl"": ""urn:uuid:obs-1"",
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""id"": ""obs-1"",
                            ""code"": {
                                ""coding"": [{
                                    ""system"": ""http://example.org/test-system"",
                                    ""code"": ""INVALID_CODE"",
                                    ""display"": ""Invalid Display""
                                }]
                            },
                            ""status"": ""final""
                        }
                    }
                ]
            }";

            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            var metadataJson = JsonConvert.SerializeObject(metadata);
            engine.LoadMetadataFromJson(metadataJson);

            // Act
            var result = engine.Validate(bundleJson, "error");

            // Assert - Should report invalid code, not display (since display validation only happens for valid codes)
            Assert.False(result.IsValid, "Validation should fail for invalid code");
            Assert.Single(result.Errors);
            Assert.Contains("INVALID_CODE", result.Errors[0].Code);
        }
    }
}
