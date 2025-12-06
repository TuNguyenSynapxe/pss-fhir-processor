using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests.Models
{
    /// <summary>
    /// Represents a single dynamic test case with input JSON and expected validation outcome
    /// </summary>
    public class DynamicTestCase
    {
        /// <summary>
        /// Human-readable name for the test case
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The FHIR Bundle JSON to validate
        /// </summary>
        public JObject InputJson { get; set; }

        /// <summary>
        /// Whether this bundle should pass validation (true) or fail (false)
        /// </summary>
        public bool ShouldPass { get; set; }

        /// <summary>
        /// List of error codes expected to appear when ShouldPass = false
        /// </summary>
        public List<string> ExpectedErrorCodes { get; set; } = new List<string>();

        /// <summary>
        /// Override ToString for better test output in NUnit
        /// </summary>
        public override string ToString()
        {
            return Name ?? "UnnamedTestCase";
        }
    }
}
