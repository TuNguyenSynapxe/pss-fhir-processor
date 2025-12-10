using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xunit;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.PSS.FhirProcessor.Core.Validation;
using MOH.HealthierSG.PSS.FhirProcessor.Utilities;

namespace MOH.HealthierSG.PSS.FhirProcessor.Tests.UnitTests
{
    /// <summary>
    /// Tests to ensure regex timeout protection doesn't break legitimate validation patterns
    /// </summary>
    public class RegexTimeoutSafetyTests
    {
        [Fact]
        public void AllProductionPatterns_CompleteFastWithTimeout()
        {
            // Arrange: All actual patterns from validation-metadata.json
            var productionPatterns = new[]
            {
                ("NRIC/FIN", @"^[STFG]\d{7}[A-Z]$", "S1234567A"),
                ("Postal Code", @"^\d{6}$", "123456"),
                ("UUID", @"^urn:uuid:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", 
                    "urn:uuid:a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d")
            };

            // Act & Assert: All patterns should complete in < 10ms (well under 2 second timeout)
            foreach (var (name, pattern, testInput) in productionPatterns)
            {
                var sw = Stopwatch.StartNew();
                
                var result = Regex.IsMatch(testInput, pattern, RegexOptions.None, TimeSpan.FromSeconds(2));
                
                sw.Stop();
                
                Assert.True(sw.ElapsedMilliseconds < 10, 
                    $"Pattern '{name}' took {sw.ElapsedMilliseconds}ms (expected < 10ms). Pattern: {pattern}");
                Assert.True(result, $"Pattern '{name}' should match valid input: {testInput}");
            }
        }

        [Fact]
        public void RuleEvaluator_LegitimatePattern_PassesValidation()
        {
            // Arrange
            var resource = JObject.Parse(@"{
                ""nric"": ""S1234567A""
            }");

            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                Path = "nric",
                Pattern = @"^[STFG]\d{7}[A-Z]$",
                ErrorCode = "INVALID_NRIC"
            };

            var result = new ValidationResult();
            var logger = new Logger("error"); // Quiet mode

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Test", null, null, result, logger);

            // Assert
            Assert.True(result.IsValid, "Valid NRIC should pass regex validation");
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void RuleEvaluator_MaliciousReDoSPattern_TimesOutGracefully()
        {
            // Arrange: Known ReDoS pattern
            var resource = JObject.Parse(@"{
                ""field"": ""aaaaaaaaaaaaaaaaaaaaaaaaaaX""
            }");

            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                Path = "field",
                Pattern = @"^(a+)+$",  // Malicious pattern
                ErrorCode = "TEST_ERROR"
            };

            var result = new ValidationResult();
            var logger = new Logger("verbose");

            // Configure short timeout for test
            var originalTimeout = RuleEvaluator.RegexTimeout;
            RuleEvaluator.RegexTimeout = TimeSpan.FromMilliseconds(100);

            var sw = Stopwatch.StartNew();
            
            try
            {
                // Act
                RuleEvaluator.ApplyRule(resource, rule, "Test", null, null, result, logger);
                
                sw.Stop();

                // Assert
                Assert.False(result.IsValid, "ReDoS pattern should fail validation");
                Assert.Single(result.Errors);
                Assert.Equal("REGEX_TIMEOUT", result.Errors[0].Code);
                Assert.True(sw.ElapsedMilliseconds < 500, 
                    $"Timeout should stop pattern quickly (took {sw.ElapsedMilliseconds}ms)");
            }
            finally
            {
                // Restore original timeout
                RuleEvaluator.RegexTimeout = originalTimeout;
            }
        }

        [Fact]
        public void RuleEvaluator_InvalidPattern_ReportsError()
        {
            // Arrange
            var resource = JObject.Parse(@"{
                ""field"": ""test""
            }");

            var rule = new RuleDefinition
            {
                RuleType = "Regex",
                Path = "field",
                Pattern = @"[invalid(regex"  // Invalid syntax - no ErrorCode so default will be used
            };

            var result = new ValidationResult();
            var logger = new Logger("error");

            // Act
            RuleEvaluator.ApplyRule(resource, rule, "Test", null, null, result, logger);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("REGEX_PATTERN_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public void RegexTimeout_IsConfigurable()
        {
            // Arrange
            var originalTimeout = RuleEvaluator.RegexTimeout;

            try
            {
                // Act
                RuleEvaluator.RegexTimeout = TimeSpan.FromMilliseconds(500);

                // Assert
                Assert.Equal(500, RuleEvaluator.RegexTimeout.TotalMilliseconds);
            }
            finally
            {
                // Restore
                RuleEvaluator.RegexTimeout = originalTimeout;
            }
        }

        [Theory]
        [InlineData("S1234567A", true)]   // Valid NRIC
        [InlineData("T9876543B", true)]   // Valid FIN
        [InlineData("G1111111Z", true)]   // Valid
        [InlineData("F2222222C", true)]   // Valid
        [InlineData("A1234567B", false)]  // Invalid - wrong letter
        [InlineData("S123456A", false)]   // Invalid - too short
        [InlineData("S12345678A", false)] // Invalid - too long
        public void NricPattern_WithTimeout_WorksCorrectly(string input, bool shouldMatch)
        {
            // Arrange
            var pattern = @"^[STFG]\d{7}[A-Z]$";

            // Act
            var result = Regex.IsMatch(input, pattern, RegexOptions.None, TimeSpan.FromSeconds(2));

            // Assert
            Assert.Equal(shouldMatch, result);
        }

        [Theory]
        [InlineData("123456", true)]
        [InlineData("000000", true)]
        [InlineData("999999", true)]
        [InlineData("12345", false)]   // Too short
        [InlineData("1234567", false)] // Too long
        [InlineData("12345A", false)]  // Contains letter
        public void PostalCodePattern_WithTimeout_WorksCorrectly(string input, bool shouldMatch)
        {
            // Arrange
            var pattern = @"^\d{6}$";

            // Act
            var result = Regex.IsMatch(input, pattern, RegexOptions.None, TimeSpan.FromSeconds(2));

            // Assert
            Assert.Equal(shouldMatch, result);
        }

        [Fact]
        public void PerformanceBenchmark_ProductionPatterns_AllUnder1ms()
        {
            // This test documents the actual performance of production patterns
            var testCases = new[]
            {
                ("NRIC", @"^[STFG]\d{7}[A-Z]$", "S1234567A", 100),
                ("Postal", @"^\d{6}$", "123456", 100),
                ("UUID", @"^urn:uuid:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", 
                    "urn:uuid:12345678-1234-1234-1234-123456789abc", 100)
            };

            foreach (var (name, pattern, input, iterations) in testCases)
            {
                var sw = Stopwatch.StartNew();
                
                for (int i = 0; i < iterations; i++)
                {
                    Regex.IsMatch(input, pattern, RegexOptions.None, TimeSpan.FromSeconds(2));
                }
                
                sw.Stop();
                var avgMs = sw.ElapsedMilliseconds / (double)iterations;
                
                // All patterns should average < 0.1ms per match
                Assert.True(avgMs < 0.1, 
                    $"{name}: Average {avgMs:F3}ms per match (expected < 0.1ms)");
            }
        }
    }
}
