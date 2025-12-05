using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Path
{
    /// <summary>
    /// Parses and resolves CPS1 (Custom Path Syntax 1) paths in FHIR resources
    /// Supports: segment[key:value], segment[*], segment[index], and dot notation
    /// </summary>
    public class CpsPathResolver
    {
        /// <summary>
        /// Resolve a CPS1 path against a JToken resource
        /// </summary>
        /// <param name="resource">The JToken resource to navigate</param>
        /// <param name="cpsPath">The CPS1 path string (e.g., "extension[url:ext-ethnicity].valueCodeableConcept")</param>
        /// <returns>List of matching JTokens (never null, empty if no matches)</returns>
        public static List<JToken> Resolve(JToken resource, string cpsPath)
        {
            if (resource == null || string.IsNullOrEmpty(cpsPath))
            {
                return new List<JToken>();
            }

            var segments = ParsePath(cpsPath);
            var results = new List<JToken> { resource };

            foreach (var segment in segments)
            {
                results = ApplySegment(results, segment);
                if (results.Count == 0)
                {
                    break;
                }
            }

            return results;
        }

        /// <summary>
        /// Parse CPS1 path into segments
        /// Example: "extension[url:ext-ethnicity].valueCodeableConcept.coding[0].code"
        /// Returns: [
        ///   { Name: "extension", Filter: { Type: KeyValue, Key: "url", Value: "ext-ethnicity" } },
        ///   { Name: "valueCodeableConcept" },
        ///   { Name: "coding", Filter: { Type: Index, Index: 0 } },
        ///   { Name: "code" }
        /// ]
        /// </summary>
        private static List<CpsPathSegment> ParsePath(string cpsPath)
        {
            var segments = new List<CpsPathSegment>();
            var parts = new List<string>();
            
            // Split by '.' but respect brackets (don't split on dots inside brackets)
            int start = 0;
            int bracketDepth = 0;
            
            for (int i = 0; i < cpsPath.Length; i++)
            {
                if (cpsPath[i] == '[')
                {
                    bracketDepth++;
                }
                else if (cpsPath[i] == ']')
                {
                    bracketDepth--;
                }
                else if (cpsPath[i] == '.' && bracketDepth == 0)
                {
                    // Found a dot outside brackets - this is a segment separator
                    if (i > start)
                    {
                        parts.Add(cpsPath.Substring(start, i - start));
                    }
                    start = i + 1;
                }
            }
            
            // Add the last segment
            if (start < cpsPath.Length)
            {
                parts.Add(cpsPath.Substring(start));
            }

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                var segment = new CpsPathSegment();
                var bracketStart = part.IndexOf('[');

                if (bracketStart == -1)
                {
                    // Simple segment: "identifier"
                    segment.Name = part;
                }
                else
                {
                    // Segment with filter: "extension[url:ext-ethnicity]"
                    segment.Name = part.Substring(0, bracketStart);
                    var bracketEnd = part.IndexOf(']', bracketStart);

                    if (bracketEnd != -1)
                    {
                        var filterContent = part.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
                        segment.Filter = ParseFilter(filterContent);
                    }
                }

                segments.Add(segment);
            }

            return segments;
        }

        /// <summary>
        /// Parse filter content inside brackets
        /// Examples:
        ///   "url:ext-ethnicity" → KeyValue filter
        ///   "system:https://..." → KeyValue filter
        ///   "code:HS" → KeyValue filter
        ///   "0" → Index filter
        ///   "*" → Wildcard filter
        /// </summary>
        private static CpsFilter ParseFilter(string filterContent)
        {
            if (filterContent == "*")
            {
                return new CpsFilter { Type = FilterType.Wildcard };
            }

            int colonIndex = filterContent.IndexOf(':');
            if (colonIndex != -1)
            {
                // KeyValue filter
                return new CpsFilter
                {
                    Type = FilterType.KeyValue,
                    Key = filterContent.Substring(0, colonIndex),
                    Value = filterContent.Substring(colonIndex + 1)
                };
            }

            // Try to parse as index
            int index;
            if (int.TryParse(filterContent, out index))
            {
                return new CpsFilter
                {
                    Type = FilterType.Index,
                    Index = index
                };
            }

            // Default to KeyValue with empty key (shouldn't happen in valid CPS1)
            return new CpsFilter
            {
                Type = FilterType.KeyValue,
                Key = filterContent,
                Value = ""
            };
        }

        /// <summary>
        /// Apply a single segment to a list of current tokens
        /// </summary>
        private static List<JToken> ApplySegment(List<JToken> currentTokens, CpsPathSegment segment)
        {
            var results = new List<JToken>();

            foreach (var token in currentTokens)
            {
                var matches = NavigateSegment(token, segment);
                results.AddRange(matches);
            }

            return results;
        }

        /// <summary>
        /// Navigate a single token through a segment
        /// </summary>
        private static List<JToken> NavigateSegment(JToken token, CpsPathSegment segment)
        {
            var results = new List<JToken>();

            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;
                var property = obj[segment.Name];

                if (property == null)
                {
                    return results;
                }

                if (segment.Filter == null)
                {
                    // No filter, return the property value
                    results.Add(property);
                }
                else
                {
                    // Apply filter to the property
                    results = ApplyFilter(property, segment.Filter);
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                // If the current token is an array, navigate each element
                var array = (JArray)token;
                foreach (var element in array)
                {
                    var matches = NavigateSegment(element, segment);
                    results.AddRange(matches);
                }
            }

            return results;
        }

        /// <summary>
        /// Apply filter to a token
        /// </summary>
        private static List<JToken> ApplyFilter(JToken token, CpsFilter filter)
        {
            var results = new List<JToken>();

            if (token.Type == JTokenType.Array)
            {
                var array = (JArray)token;

                if (filter.Type == FilterType.Wildcard)
                {
                    // Return all elements
                    results.AddRange(array);
                }
                else if (filter.Type == FilterType.Index)
                {
                    // Return specific index
                    if (filter.Index >= 0 && filter.Index < array.Count)
                    {
                        results.Add(array[filter.Index]);
                    }
                }
                else if (filter.Type == FilterType.KeyValue)
                {
                    // Filter by key:value match
                    foreach (var element in array)
                    {
                        if (element.Type == JTokenType.Object)
                        {
                            var obj = (JObject)element;
                            var keyProperty = obj[filter.Key];

                            if (keyProperty != null && MatchesValue(keyProperty, filter.Value))
                            {
                                results.Add(element);
                            }
                        }
                    }
                }
            }
            else
            {
                // Not an array, return as-is if filter allows (shouldn't happen in valid CPS1)
                results.Add(token);
            }

            return results;
        }

        /// <summary>
        /// Check if a token's value matches the expected value
        /// Handles simple values, nested objects (for "code", "system"), and arrays
        /// </summary>
        private static bool MatchesValue(JToken token, string expectedValue)
        {
            if (token == null)
            {
                return false;
            }

            // Direct value comparison
            if (token.Type == JTokenType.String || token.Type == JTokenType.Integer || 
                token.Type == JTokenType.Float || token.Type == JTokenType.Boolean)
            {
                return token.ToString() == expectedValue;
            }

            // For objects with "code" or "system" properties (Coding/CodeableConcept patterns)
            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;
                
                // Check code property
                var code = obj["code"];
                if (code != null && code.ToString() == expectedValue)
                {
                    return true;
                }

                // Check system property
                var system = obj["system"];
                if (system != null && system.ToString() == expectedValue)
                {
                    return true;
                }
            }

            // For arrays, check if any element matches
            if (token.Type == JTokenType.Array)
            {
                foreach (var element in (JArray)token)
                {
                    if (MatchesValue(element, expectedValue))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a path exists (has at least one result)
        /// </summary>
        public static bool PathExists(JToken resource, string cpsPath)
        {
            return Resolve(resource, cpsPath).Count > 0;
        }

        /// <summary>
        /// Get first matching value as string, or null if not found
        /// </summary>
        public static string GetValueAsString(JToken resource, string cpsPath)
        {
            var results = Resolve(resource, cpsPath);
            return results.Count > 0 ? results[0].ToString() : null;
        }

        /// <summary>
        /// Get all matching values as strings
        /// </summary>
        public static List<string> GetValuesAsStrings(JToken resource, string cpsPath)
        {
            var results = Resolve(resource, cpsPath);
            var values = new List<string>();

            foreach (var result in results)
            {
                values.Add(result.ToString());
            }

            return values;
        }
    }
}
