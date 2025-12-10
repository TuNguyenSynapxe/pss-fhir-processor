using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace MOH.HealthierSG.PSS.FhirProcessor.Tests.DynamicTests.Models
{
    /// <summary>
    /// Template for generating a negative test case by mutating a valid baseline bundle
    /// </summary>
    public class MutationTemplate
    {
        /// <summary>
        /// Descriptive name for this mutation (used as test case name)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Function that takes a baseline bundle and returns a mutated clone
        /// IMPORTANT: The function should clone the input to avoid modifying the original
        /// </summary>
        public Func<JObject, JObject> Apply { get; set; }

        /// <summary>
        /// List of error codes expected to be raised by this mutation
        /// </summary>
        public List<string> ExpectedErrorCodes { get; set; } = new List<string>();
    }
}
