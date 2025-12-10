using System.Collections.Generic;

namespace MOH.HealthierSG.PSS.FhirProcessor.Core.Metadata
{
    /// <summary>
    /// Single validation rule definition
    /// Supports: Required, FixedValue, FixedCoding, AllowedValues, CodesMaster, Type, Regex, Reference, CodeSystem, FullUrlIdMatch, ArrayLength rule types
    /// </summary>
    public class RuleDefinition
    {
        public string RuleType { get; set; }           // Required, FixedValue, FixedCoding, AllowedValues, CodesMaster, Type, Regex, Reference, CodeSystem, FullUrlIdMatch, ArrayLength
        public string PathType { get; set; }           // "CPS1"
        public string Path { get; set; }               // Custom Path Syntax
        public string ExpectedValue { get; set; }
        public string ExpectedSystem { get; set; }
        public string ExpectedCode { get; set; }
        public string ExpectedType { get; set; }       // Type validation: string, integer, decimal, boolean, guid, guid-uri, date, datetime, pipestring[], array, object
        public string Pattern { get; set; }            // Regex validation: regex pattern
        public List<string> TargetTypes { get; set; }  // Reference validation: allowed resource types
        public List<string> AllowedValues { get; set; } // AllowedValues validation: list of permitted values
        public string System { get; set; }             // CodeSystem validation: code system URL
        public int? Min { get; set; }                  // ArrayLength validation: minimum array length
        public int? Max { get; set; }                  // ArrayLength validation: maximum array length
        public string ElementType { get; set; }        // ArrayLength validation: type of array elements (string, any)
        public bool? NonEmptyForStrings { get; set; }  // ArrayLength validation: require non-empty strings
        public string ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
