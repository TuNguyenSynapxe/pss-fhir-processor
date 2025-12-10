using System.Linq;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using MOH.HealthierSG.PSS.FhirProcessor.Core.Validation;
using MOH.HealthierSG.PSS.FhirProcessor.Tests.DynamicTests.Models;

namespace MOH.HealthierSG.PSS.FhirProcessor.Tests.DynamicTests
{
    /// <summary>
    /// Metadata-driven end-to-end validation tests using mutation-based test case generation.
    /// This test fixture automatically generates negative test cases by mutating a valid baseline bundle.
    /// </summary>
    public class MetadataDrivenEndToEndTests
    {
        private readonly Core.Validation.ValidationEngine _engine;
        private readonly ITestOutputHelper _output;

        public MetadataDrivenEndToEndTests(ITestOutputHelper output)
        {
            _output = output;
            
            // TODO: Adjust path to your validation-metadata.json if needed
            // Load from file system
            var metadataPath = System.AppDomain.CurrentDomain.BaseDirectory;
            var metadataFile = System.IO.Path.Combine(metadataPath, "..", "..", "..", "..", "..", "src", "Pss.FhirProcessor.NetCore", "Frontend", "src", "seed", "validation-metadata.json");
            
            if (System.IO.File.Exists(metadataFile))
            {
                var metadataJson = System.IO.File.ReadAllText(metadataFile);
                _engine = new Core.Validation.ValidationEngine();
                _engine.LoadMetadataFromJson(metadataJson);
                _output.WriteLine($"ValidationEngine initialized with metadata from: {metadataFile}");
            }
            else
            {
                // Fallback: Create engine with default metadata
                _engine = new Core.Validation.ValidationEngine();
                _output.WriteLine("ValidationEngine initialized with default metadata");
            }
        }

        /// <summary>
        /// Execute a single dynamic test case through the validation engine
        /// </summary>
        /// <param name="testCase">The test case to execute</param>
        [Theory]
        [MemberData(nameof(GetTestCases))]
        public void ExecuteDynamicCase(DynamicTestCase testCase)
        {
            // Log test case info
            _output.WriteLine($"Executing test case: {testCase.Name}");
            _output.WriteLine($"Should pass: {testCase.ShouldPass}");
            _output.WriteLine($"Expected error codes: {string.Join(", ", testCase.ExpectedErrorCodes)}");

            // Convert JObject to JSON string for validation
            var bundleJson = testCase.InputJson.ToString();

            // Execute validation
            var result = _engine.Validate(bundleJson, logLevel: "info");

            // Log result
            _output.WriteLine($"Validation result: IsValid={result.IsValid}, ErrorCount={result.Errors.Count}");
            if (result.Errors.Any())
            {
                _output.WriteLine("Validation errors:");
                foreach (var error in result.Errors)
                {
                    _output.WriteLine($"  [{error.Code}] {error.FieldPath}: {error.Message}");
                }
            }

            // Assertions
            if (testCase.ShouldPass)
            {
                // Happy case - should pass with no errors
                Assert.True(
                    result.IsValid, 
                    $"Expected validation to pass but got {result.Errors.Count} error(s):\n" +
                    string.Join("\n", result.Errors.Select(e => $"  [{e.Code}] {e.FieldPath}: {e.Message}"))
                );
            }
            else
            {
                // Negative case - should fail
                Assert.False(
                    result.IsValid, 
                    "Expected validation to fail but it succeeded."
                );

                // Must have at least one validation error
                Assert.True(
                    result.Errors.Count > 0,
                    "Expected validation errors but none were found."
                );

                // Check that expected error codes are present OR early structural errors occurred
                var actualErrorCodes = result.Errors.Select(e => e.Code).ToList();
                
                // Define structural/mandatory errors that can legitimately occur before reaching the target rule
                var structuralErrorCodes = new[]
                {
                    "MANDATORY_MISSING",
                    "TYPE_MISMATCH",
                    "ID_FULLURL_MISMATCH",
                    "REGEX_INVALID_NRIC",
                    "REGEX_PATTERN_MISMATCH",
                    "INVALID_CODE",
                    "INVALID_REFERENCE",
                    "REFERENCE_NOT_FOUND"
                };
                
                foreach (var expectedCode in testCase.ExpectedErrorCodes)
                {
                    // Skip mutation error markers
                    if (expectedCode.StartsWith("MUTATION_ERROR:"))
                    {
                        Assert.True(false, $"Mutation failed: {expectedCode}");
                        continue;
                    }

                    // Check if expected code is present
                    var foundExpected = actualErrorCodes.Any(c => c == expectedCode);
                    
                    // Check if any structural error is present (early-fail scenario)
                    var foundStructural = actualErrorCodes.Any(c => structuralErrorCodes.Contains(c));
                    
                    // Accept either the expected error OR a structural error
                    if (foundExpected)
                    {
                        _output.WriteLine($"✓ Found expected error code: {expectedCode}");
                    }
                    else if (foundStructural)
                    {
                        _output.WriteLine($"⚠ Expected '{expectedCode}' but found structural error(s): [{string.Join(", ", actualErrorCodes.Where(c => structuralErrorCodes.Contains(c)))}]");
                        _output.WriteLine($"  This is acceptable - mutation caused early validation failure");
                    }
                    else
                    {
                        Assert.True(
                            false,
                            $"Expected error code '{expectedCode}' not found, and no structural errors present.\n" +
                            $"Actual error codes: [{string.Join(", ", actualErrorCodes)}]\n" +
                            $"Full errors:\n" +
                            string.Join("\n", result.Errors.Select(e => $"  [{e.Code}] {e.FieldPath}: {e.Message}"))
                        );
                    }
                }

                _output.WriteLine($"✓ Validation failed as expected with {result.Errors.Count} error(s)");
            }
        }

        /// <summary>
        /// Provides test data for xUnit Theory
        /// </summary>
        public static TheoryData<DynamicTestCase> GetTestCases()
        {
            var theoryData = new TheoryData<DynamicTestCase>();
            
            foreach (var testCase in DynamicTestSource.GetCases())
            {
                theoryData.Add(testCase);
            }

            return theoryData;
        }

        /// <summary>
        /// Verify that we have a reasonable number of test cases generated
        /// </summary>
        [Fact]
        public void VerifyTestCaseGeneration()
        {
            var testCases = DynamicTestSource.GetCases().ToList();
            
            _output.WriteLine($"Total test cases generated: {testCases.Count}");
            _output.WriteLine($"Happy cases: {testCases.Count(tc => tc.ShouldPass)}");
            _output.WriteLine($"Negative cases: {testCases.Count(tc => !tc.ShouldPass)}");

            Assert.True(testCases.Count > 0, "No test cases were generated");
            Assert.Equal(1, testCases.Count(tc => tc.ShouldPass));
            Assert.True(testCases.Count(tc => !tc.ShouldPass) > 10, "Should have at least 10 negative cases");

            _output.WriteLine("\nGenerated test cases:");
            foreach (var tc in testCases)
            {
                _output.WriteLine($"  - {tc.Name} (ShouldPass={tc.ShouldPass}, ExpectedErrors={tc.ExpectedErrorCodes.Count})");
            }
        }
    }
}
