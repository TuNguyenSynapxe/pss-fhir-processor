using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.EndToEnd
{
    [TestClass]
    public class ValidationEngineTests
    {
        private ValidationEngine _engine;
        private string _testDataPath;

        [TestInitialize]
        public void Setup()
        {
            _engine = new ValidationEngine();
            _testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
        }

        [TestMethod]
        public void Validate_ValidBundle_ReturnsNoErrors()
        {
            // Arrange
            var metadataPath = Path.Combine(_testDataPath, "validation-metadata-v5.json");
            var bundlePath = Path.Combine(_testDataPath, "happy-case-01.json");

            if (!File.Exists(metadataPath) || !File.Exists(bundlePath))
            {
                Assert.Inconclusive("Test data files not found");
                return;
            }

            _engine.LoadMetadata(metadataPath);
            var bundleJson = File.ReadAllText(bundlePath);

            // Act
            var result = _engine.Validate(bundleJson);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid, $"Expected valid bundle but got errors: {string.Join("; ", result.Errors)}");
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_MissingRequiredField_ReturnsError()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""5.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [
                    {
                        ""Scope"": ""Patient"",
                        ""Rules"": [
                            {
                                ""RuleType"": ""Required"",
                                ""PathType"": ""CPS1"",
                                ""Path"": ""identifier[system:https://fhir.synapxe.sg/id/nric-fin].value"",
                                ""ErrorCode"": ""MANDATORY_MISSING"",
                                ""Message"": ""NRIC is required""
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
                            ""identifier"": []
                        }
                    }
                ]
            }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(bundle);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("MANDATORY_MISSING", result.Errors[0].Code);
        }

        [TestMethod]
        public void Validate_FixedValueMismatch_ReturnsError()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""5.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [
                    {
                        ""Scope"": ""Encounter"",
                        ""Rules"": [
                            {
                                ""RuleType"": ""FixedValue"",
                                ""PathType"": ""CPS1"",
                                ""Path"": ""status"",
                                ""ExpectedValue"": ""finished"",
                                ""ErrorCode"": ""FIXED_VALUE_MISMATCH"",
                                ""Message"": ""Encounter status must be finished""
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
                            ""status"": ""in-progress""
                        }
                    }
                ]
            }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(bundle);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("FIXED_VALUE_MISMATCH", result.Errors[0].Code);
        }

        [TestMethod]
        public void Validate_ObservationWithScreeningType_FiltersCorrectly()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""5.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [
                    {
                        ""Scope"": ""Observation.HS"",
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
                            ""code"": {
                                ""coding"": [{
                                    ""system"": ""https://fhir.synapxe.sg/CodeSystem/screening-type"",
                                    ""code"": ""OS""
                                }]
                            }
                        }
                    }
                ]
            }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(bundle);

            // Assert
            Assert.IsNotNull(result);
            // Should only validate HS observation, not OS
            // HS observation has empty component array, so should pass Required check (empty array is present)
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_InvalidJson_ReturnsError()
        {
            // Arrange
            var metadata = @"{
                ""Version"": ""5.0"",
                ""PathSyntax"": ""CPS1"",
                ""RuleSets"": [],
                ""CodesMaster"": {
                    ""Questions"": [],
                    ""CodeSystems"": []
                }
            }";

            var invalidBundle = @"{ invalid json }";

            _engine.LoadMetadataFromJson(metadata);

            // Act
            var result = _engine.Validate(invalidBundle);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Count > 0);
            Assert.AreEqual("INVALID_JSON", result.Errors[0].Code);
        }
    }
}
