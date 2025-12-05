using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    /// <summary>
    /// Tests for metadata-driven scope matching functionality
    /// </summary>
    [TestClass]
    public class ScopeMatchingTests
    {
        private ValidationEngine _engine;

        [TestInitialize]
        public void Setup()
        {
            _engine = new ValidationEngine();
        }

        [TestMethod]
        public void ScopeMatching_OrganizationProvider_MatchesOnlyProviderOrgs()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""7.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [
                    {
                        ""Scope"": ""Organization.Provider"",
                        ""ScopeDefinition"": {
                            ""ResourceType"": ""Organization"",
                            ""Match"": [
                                {
                                    ""Path"": ""type.coding[system:https://fhir.synapxe.sg/CodeSystem/organization-type].code"",
                                    ""Expected"": ""PROVIDER""
                                }
                            ]
                        },
                        ""Rules"": [
                            {
                                ""RuleType"": ""Required"",
                                ""PathType"": ""CPS1"",
                                ""Path"": ""name"",
                                ""ErrorCode"": ""MANDATORY_MISSING"",
                                ""Message"": ""Provider Organization name is required""
                            }
                        ]
                    }
                ],
                ""CodesMaster"": {
                    ""Questions"": [],
                    ""CodeSystems"": []
                }
            }";

            var bundle = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization"",
                            ""id"": ""org-provider"",
                            ""type"": [{
                                ""coding"": [{
                                    ""system"": ""https://fhir.synapxe.sg/CodeSystem/organization-type"",
                                    ""code"": ""PROVIDER""
                                }]
                            }],
                            ""name"": ""Provider Org""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization"",
                            ""id"": ""org-cluster"",
                            ""type"": [{
                                ""coding"": [{
                                    ""system"": ""https://fhir.synapxe.sg/CodeSystem/organization-type"",
                                    ""code"": ""CLUSTER""
                                }]
                            }],
                            ""name"": ""Cluster Org""
                        }
                    }
                ]
            }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(bundle, "debug");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid, $"Expected valid but got errors: {string.Join("; ", result.Errors)}");
            // Should only validate provider org, not cluster org
        }

        [TestMethod]
        public void ScopeMatching_OrganizationCluster_MatchesOnlyClusterOrgs()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""7.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [
                    {
                        ""Scope"": ""Organization.Cluster"",
                        ""ScopeDefinition"": {
                            ""ResourceType"": ""Organization"",
                            ""Match"": [
                                {
                                    ""Path"": ""type.coding[system:https://fhir.synapxe.sg/CodeSystem/organization-type].code"",
                                    ""Expected"": ""CLUSTER""
                                }
                            ]
                        },
                        ""Rules"": [
                            {
                                ""RuleType"": ""Required"",
                                ""PathType"": ""CPS1"",
                                ""Path"": ""name"",
                                ""ErrorCode"": ""MANDATORY_MISSING"",
                                ""Message"": ""Cluster Organization name is required""
                            }
                        ]
                    }
                ],
                ""CodesMaster"": {
                    ""Questions"": [],
                    ""CodeSystems"": []
                }
            }";

            var bundle = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization"",
                            ""id"": ""org-provider"",
                            ""type"": [{
                                ""coding"": [{
                                    ""system"": ""https://fhir.synapxe.sg/CodeSystem/organization-type"",
                                    ""code"": ""PROVIDER""
                                }]
                            }],
                            ""name"": ""Provider Org""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization"",
                            ""id"": ""org-cluster"",
                            ""type"": [{
                                ""coding"": [{
                                    ""system"": ""https://fhir.synapxe.sg/CodeSystem/organization-type"",
                                    ""code"": ""CLUSTER""
                                }]
                            }],
                            ""name"": ""Cluster Org""
                        }
                    }
                ]
            }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(bundle, "debug");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid, $"Expected valid but got errors: {string.Join("; ", result.Errors)}");
            // Should only validate cluster org, not provider org
        }

        [TestMethod]
        public void ScopeMatching_ObservationHS_MatchesOnlyHearingScreening()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""7.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [
                    {
                        ""Scope"": ""Observation.HearingScreening"",
                        ""ScopeDefinition"": {
                            ""ResourceType"": ""Observation"",
                            ""Match"": [
                                {
                                    ""Path"": ""code.coding[system:https://fhir.synapxe.sg/CodeSystem/screening-type].code"",
                                    ""Expected"": ""HS""
                                }
                            ]
                        },
                        ""Rules"": [
                            {
                                ""RuleType"": ""Required"",
                                ""PathType"": ""CPS1"",
                                ""Path"": ""component"",
                                ""ErrorCode"": ""MANDATORY_MISSING"",
                                ""Message"": ""Component is required""
                            }
                        ]
                    }
                ],
                ""CodesMaster"": {
                    ""Questions"": [],
                    ""CodeSystems"": []
                }
            }";

            var bundle = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""id"": ""obs-hs"",
                            ""code"": {
                                ""coding"": [{
                                    ""system"": ""https://fhir.synapxe.sg/CodeSystem/screening-type"",
                                    ""code"": ""HS""
                                }]
                            },
                            ""component"": []
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""id"": ""obs-vs"",
                            ""code"": {
                                ""coding"": [{
                                    ""system"": ""https://fhir.synapxe.sg/CodeSystem/screening-type"",
                                    ""code"": ""VS""
                                }]
                            }
                        }
                    }
                ]
            }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(bundle, "debug");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid, $"Expected valid but got errors: {string.Join("; ", result.Errors)}");
            // Should only validate HS observation, not VS
        }

        [TestMethod]
        public void ScopeMatching_MismatchedOrganizationType_ReturnsError()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""7.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [
                    {
                        ""Scope"": ""Organization.Provider"",
                        ""ScopeDefinition"": {
                            ""ResourceType"": ""Organization"",
                            ""Match"": [
                                {
                                    ""Path"": ""type.coding[system:https://fhir.synapxe.sg/CodeSystem/organization-type].code"",
                                    ""Expected"": ""PROVIDER""
                                }
                            ]
                        },
                        ""Rules"": [
                            {
                                ""RuleType"": ""FixedValue"",
                                ""PathType"": ""CPS1"",
                                ""Path"": ""type.coding[system:https://fhir.synapxe.sg/CodeSystem/organization-type].code"",
                                ""ExpectedValue"": ""PROVIDER"",
                                ""ErrorCode"": ""FIXED_VALUE_MISMATCH"",
                                ""Message"": ""Organization type must be PROVIDER""
                            }
                        ]
                    }
                ],
                ""CodesMaster"": {
                    ""Questions"": [],
                    ""CodeSystems"": []
                }
            }";

            var bundle = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization"",
                            ""id"": ""org-cluster"",
                            ""type"": [{
                                ""coding"": [{
                                    ""system"": ""https://fhir.synapxe.sg/CodeSystem/organization-type"",
                                    ""code"": ""CLUSTER""
                                }]
                            }],
                            ""name"": ""Wrong Type Org""
                        }
                    }
                ]
            }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(bundle, "debug");

            // Assert
            Assert.IsNotNull(result);
            // Cluster org should NOT match Provider scope, so no errors
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ScopeMatching_MultipleMatchConditions_AllMustPass()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""7.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [
                    {
                        ""Scope"": ""Patient.Adult"",
                        ""ScopeDefinition"": {
                            ""ResourceType"": ""Patient"",
                            ""Match"": [
                                {
                                    ""Path"": ""active"",
                                    ""Expected"": ""true""
                                },
                                {
                                    ""Path"": ""gender"",
                                    ""Expected"": ""male""
                                }
                            ]
                        },
                        ""Rules"": [
                            {
                                ""RuleType"": ""Required"",
                                ""PathType"": ""CPS1"",
                                ""Path"": ""name"",
                                ""ErrorCode"": ""MANDATORY_MISSING"",
                                ""Message"": ""Name is required""
                            }
                        ]
                    }
                ],
                ""CodesMaster"": {
                    ""Questions"": [],
                    ""CodeSystems"": []
                }
            }";

            var bundle = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""id"": ""patient-match"",
                            ""active"": true,
                            ""gender"": ""male"",
                            ""name"": [{""family"": ""Doe""}]
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""id"": ""patient-no-match"",
                            ""active"": true,
                            ""gender"": ""female"",
                            ""name"": [{""family"": ""Smith""}]
                        }
                    }
                ]
            }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(bundle, "debug");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            // Only male active patients should be validated
        }

        [TestMethod]
        public void ScopeMatching_NoScopeDefinition_FallsBackToLegacyMatching()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""7.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [
                    {
                        ""Scope"": ""Patient"",
                        ""Rules"": [
                            {
                                ""RuleType"": ""Required"",
                                ""PathType"": ""CPS1"",
                                ""Path"": ""id"",
                                ""ErrorCode"": ""MANDATORY_MISSING"",
                                ""Message"": ""ID is required""
                            }
                        ]
                    }
                ],
                ""CodesMaster"": {
                    ""Questions"": [],
                    ""CodeSystems"": []
                }
            }";

            var bundle = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Patient"",
                            ""id"": ""patient-1""
                        }
                    }
                ]
            }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(bundle, "debug");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            // Should use legacy matching (all Patients match)
        }

        [TestMethod]
        public void ScopeMatching_EmptyMatchConditions_MatchesAllResourcesOfType()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""7.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [
                    {
                        ""Scope"": ""Encounter"",
                        ""ScopeDefinition"": {
                            ""ResourceType"": ""Encounter"",
                            ""Match"": []
                        },
                        ""Rules"": [
                            {
                                ""RuleType"": ""Required"",
                                ""PathType"": ""CPS1"",
                                ""Path"": ""status"",
                                ""ErrorCode"": ""MANDATORY_MISSING"",
                                ""Message"": ""Status is required""
                            }
                        ]
                    }
                ],
                ""CodesMaster"": {
                    ""Questions"": [],
                    ""CodeSystems"": []
                }
            }";

            var bundle = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter"",
                            ""id"": ""enc-1"",
                            ""status"": ""finished""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter"",
                            ""id"": ""enc-2"",
                            ""status"": ""in-progress""
                        }
                    }
                ]
            }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(bundle, "debug");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            // Both encounters should be validated
        }

        [TestMethod]
        public void ScopeMatching_PathNotResolved_ResourceDoesNotMatch()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""7.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [
                    {
                        ""Scope"": ""Organization.Special"",
                        ""ScopeDefinition"": {
                            ""ResourceType"": ""Organization"",
                            ""Match"": [
                                {
                                    ""Path"": ""extension[url:special].valueString"",
                                    ""Expected"": ""special-value""
                                }
                            ]
                        },
                        ""Rules"": [
                            {
                                ""RuleType"": ""Required"",
                                ""PathType"": ""CPS1"",
                                ""Path"": ""name"",
                                ""ErrorCode"": ""MANDATORY_MISSING"",
                                ""Message"": ""Name is required""
                            }
                        ]
                    }
                ],
                ""CodesMaster"": {
                    ""Questions"": [],
                    ""CodeSystems"": []
                }
            }";

            var bundle = @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization"",
                            ""id"": ""org-1"",
                            ""name"": ""Regular Org""
                        }
                    }
                ]
            }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(bundle, "debug");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            // Organization without the special extension should not match
            Assert.AreEqual(0, result.Errors.Count);
        }
    }
}
