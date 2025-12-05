using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation
{
    /// <summary>
    /// Validates data types for field values
    /// Supports: string, integer, decimal, boolean, guid, guid-uri, date, datetime, pipestring[], array, object
    /// </summary>
    public static class TypeChecker
    {
        /// <summary>
        /// Validate if a raw string value matches the expected type
        /// </summary>
        /// <param name="rawValue">The raw string value to validate</param>
        /// <param name="expectedType">The expected type (case-insensitive)</param>
        /// <returns>True if the value matches the expected type, false otherwise</returns>
        public static bool IsValid(string rawValue, string expectedType)
        {
            if (rawValue == null) 
                return false;

            if (string.IsNullOrWhiteSpace(expectedType))
                return true; // No type constraint

            expectedType = expectedType.Trim().ToLowerInvariant();

            switch (expectedType)
            {
                case "string":
                    // Any non-null string is valid
                    return true;

                case "integer":
                    // Must parse as 32-bit integer
                    return int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);

                case "decimal":
                    // Must parse as decimal number
                    return decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out _);

                case "boolean":
                    // Must be exactly "true" or "false" (case-insensitive)
                    var lower = rawValue.ToLowerInvariant();
                    return lower == "true" || lower == "false";

                case "guid":
                    // Must be valid RFC4122 GUID format
                    return Guid.TryParse(rawValue, out _);

                case "date":
                    // Must be YYYY-MM-DD format
                    return DateTime.TryParseExact(
                        rawValue,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out _
                    );

                case "datetime":
                    // Must be valid ISO-8601 datetime format
                    return DateTimeOffset.TryParse(
                        rawValue,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out _
                    );

                case "pipestring[]":
                    // Pipe-separated list - all parts must be non-empty
                    var parts = rawValue.Split('|');
                    return parts.Length > 0 && parts.All(x => !string.IsNullOrWhiteSpace(x));

                case "array":
                    // Must start with '[' and end with ']' (simple JSON array check)
                    var trimmed = rawValue.Trim();
                    return trimmed.StartsWith("[") && trimmed.EndsWith("]");

                case "object":
                    // Must start with '{' and end with '}' (simple JSON object check)
                    var trimmedObj = rawValue.Trim();
                    return trimmedObj.StartsWith("{") && trimmedObj.EndsWith("}");

                case "guid-uri":
                    // Must be urn:uuid:<GUID> format
                    return Regex.IsMatch(rawValue ?? "", 
                        @"^urn:uuid:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$");

                default:
                    // Unknown type - default to valid
                    return true;
            }
        }
    }
}
