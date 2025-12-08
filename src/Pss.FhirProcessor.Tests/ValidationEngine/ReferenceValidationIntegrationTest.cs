using Xunit;
using FluentAssertions;
using System.IO;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.ValidationEngine
{
    /// <summary>
    /// Integration test for Reference validation using the full ValidationEngine
    /// </summary>
    public class ReferenceValidationIntegrationTest
    {
        private readonly string _testDataPath;
        private readonly string _metadataPath;

        public ReferenceValidationIntegrationTest()
        {
            // Get project root - go up from bin/Debug/net6.0 to project root
            var testAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var testDir = Path.GetDirectoryName(testAssemblyPath);
            // testDir = .../src/Pss.FhirProcessor.Tests/bin/Debug/net6.0
            // Go up 3 levels to get to src/Pss.FhirProcessor.Tests
            var testProjectDir = Path.GetFullPath(Path.Combine(testDir, "..", "..", ".."));
            // Go up 1 more level to get to src/
            var srcDir = Path.GetFullPath(Path.Combine(testProjectDir, ".."));
            // Go up 1 more level to get to project root
            var projectRoot = Path.GetFullPath(Path.Combine(srcDir, ".."));
            
            _testDataPath = Path.Combine(projectRoot, "test-data");
            _metadataPath = Path.Combine(projectRoot, "src", "Pss.FhirProcessor.NetCore", "Frontend", "src", "seed");
        }

        [Fact]
        public void IntegrationTest_ValidationEngine_ShouldCatchInvalidPatientReference()
        {
            // Arrange - Load test bundle with invalid patient reference
            var bundlePath = Path.Combine(_testDataPath, "bad-reference-sample.json");
            var bundleJson = File.ReadAllText(bundlePath);

            // Load metadata
            var metadataPath = Path.Combine(_metadataPath, "validation-metadata.json");
            
            // Create validation engine
            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            engine.LoadMetadata(metadataPath);

            // Act - Validate the bundle
            var result = engine.Validate(bundleJson);

            // Assert - Should fail with reference error
            result.IsValid.Should().BeFalse("Bundle contains invalid patient reference");
            result.Errors.Should().Contain(e => 
                (e.Code == "REFERENCE_INVALID" || e.Code == "INVALID_PATIENT_REFERENCE") && 
                e.Message.Contains("WRONG-PATIENT-ID-DOES-NOT-EXIST"),
                "Should catch invalid patient reference"
            );
        }

        [Fact]
        public void IntegrationTest_ValidationEngine_ShouldPassValidReferences()
        {
            // Arrange - Load happy sample with valid references
            var bundlePath = Path.Combine(_metadataPath, "happy-sample-full.json");
            var bundleJson = File.ReadAllText(bundlePath);

            // Load metadata
            var metadataPath = Path.Combine(_metadataPath, "validation-metadata.json");

            // Create validation engine
            var engine = new MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine();
            engine.LoadMetadata(metadataPath);

            // Act - Validate the bundle
            var result = engine.Validate(bundleJson);

            // Assert - Should pass (or only have non-reference errors)
            var referenceErrors = result.Errors.FindAll(e => 
                e.Code == "INVALID_PATIENT_REFERENCE" || 
                e.Code == "INVALID_ENCOUNTER_REFERENCE" ||
                e.Code == "REFERENCE_NOT_FOUND" ||
                e.Code == "REFERENCE_TYPE_MISMATCH"
            );

            referenceErrors.Should().BeEmpty("Valid references should not produce errors");
        }
    }
}
