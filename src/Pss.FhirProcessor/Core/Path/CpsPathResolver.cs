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
        ///   "QuestionCode:SQ-xxx" → KeyValue filter (maps to code.coding[0].code)
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
                var key = filterContent.Substring(0, colonIndex);
                var value = filterContent.Substring(colonIndex + 1);
                
                // Special handling for QuestionCode selector
                // component[QuestionCode:SQ-xxx] maps to component where code.coding[0].code == SQ-xxx
                if (key.Equals("QuestionCode", StringComparison.OrdinalIgnoreCase))
                {
                    key = "code.coding[0].code";
                }
                
                return new CpsFilter
                {
                    Type = FilterType.KeyValue,
                    Key = key,
                    Value = value
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
                            
                            // Check if filter.Key contains nested path (e.g., "code.coding[0].code")
                            if (filter.Key.Contains(".") || filter.Key.Contains("["))
                            {
                                // Use nested path matching for complex selectors
                                if (MatchesNestedPath(obj, filter.Key, filter.Value))
                                {
                                    results.Add(element);
                                }
                            }
                            else
                            {
                                // Simple property matching
                                var keyProperty = obj[filter.Key];
                                if (keyProperty != null && MatchesValue(keyProperty, filter.Value))
                                {
                                    results.Add(element);
                                }
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
        /// Handles simple values, nested objects (for "code", "system"), nested paths (for "code.coding[0].code"), and arrays
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
                
                // Check simple code property
                var code = obj["code"];
                if (code != null && code.ToString() == expectedValue)
                {
                    return true;
                }

                // Check simple system property
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
        /// Check if a token matches a nested path selector (like "code.coding[0].code")
        /// Used for QuestionCode filters: component[QuestionCode:SQ-xxx]
        /// </summary>
        private static bool MatchesNestedPath(JToken token, string path, string expectedValue)
        {
            if (token == null || string.IsNullOrEmpty(path))
            {
                return false;
            }

            // Resolve the nested path
            var values = Resolve(token, path);
            
            // Check if any resolved value matches
            foreach (var value in values)
            {
                if (value != null && value.ToString() == expectedValue)
                {
                    return true;
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

        /// <summary>
        /// Check if a resource type exists in Bundle.entry[] array
        /// Supports paths like: Bundle.entry[Patient], Bundle.entry[Observation:HS], Bundle.entry[Organization:PROVIDER]
        /// </summary>
        /// <param name="bundleRoot">The Bundle JObject</param>
        /// <param name="cps1Path">Path like "Bundle.entry[Patient]", "Bundle.entry[Observation:HS]", or "Bundle.entry[Organization:PROVIDER]"</param>
        /// <returns>True if at least one matching entry exists</returns>
        public static bool ExistsResourceByType(JToken bundleRoot, string cps1Path)
        {
            if (bundleRoot == null || string.IsNullOrEmpty(cps1Path))
            {
                return false;
            }

            // Parse the path - expect format: Bundle.entry[ResourceType] or Bundle.entry[Observation:HS]
            if (!cps1Path.StartsWith("Bundle.entry[") || !cps1Path.EndsWith("]"))
            {
                // Not a Bundle.entry[Type] selector - use regular resolution
                return PathExists(bundleRoot, cps1Path);
            }

            // Extract resource type selector
            var selectorStart = "Bundle.entry[".Length;
            var selectorEnd = cps1Path.Length - 1;
            var resourceSelector = cps1Path.Substring(selectorStart, selectorEnd - selectorStart);

            // Get bundle entries
            var entries = bundleRoot["entry"] as JArray;
            if (entries == null || entries.Count == 0)
            {
                return false;
            }

            // Check if selector contains screening type (Observation:HS format)
            string targetResourceType = null;
            string targetScreeningCode = null;

            if (resourceSelector.Contains(":"))
            {
                var parts = resourceSelector.Split(':');
                targetResourceType = parts[0];
                targetScreeningCode = parts.Length > 1 ? parts[1] : null;
            }
            else
            {
                targetResourceType = resourceSelector;
            }

            // Search for matching resource
            foreach (var entry in entries)
            {
                var entryResource = entry["resource"] as JObject;
                if (entryResource == null) continue;

                var resourceType = entryResource["resourceType"]?.ToString();
                if (resourceType != targetResourceType) continue;

                // If no type code specified, we found a match
                if (string.IsNullOrEmpty(targetScreeningCode))
                {
                    return true;
                }

                // Check if type code matches based on resource type
                if (resourceType == "Observation")
                {
                    // For Observation:HS, Observation:OS, Observation:VS
                    // Check code.coding[0].code
                    var screeningCode = GetValueAsString(entryResource, "code.coding[0].code");
                    if (screeningCode == targetScreeningCode)
                    {
                        return true;
                    }
                }
                else if (resourceType == "Organization")
                {
                    // For Organization:PROVIDER, Organization:CLUSTER
                    // Check type.coding[0].code
                    var orgTypeCode = GetValueAsString(entryResource, "type.coding[0].code");
                    if (orgTypeCode == targetScreeningCode)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Convert a CPS path with filters to a path with numeric indices
        /// Example: "extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-constituency].valueCodeableConcept.coding[0].system"
        /// Returns: "extension[1].valueCodeableConcept.coding[0].system" (if the url filter matches index 1)
        /// </summary>
        public static string ResolveFiltersToIndices(JToken resource, string cpsPath)
        {
            if (resource == null || string.IsNullOrEmpty(cpsPath))
            {
                return cpsPath;
            }

            try
            {
                var segments = ParsePath(cpsPath);
                var resolvedSegments = new List<string>();
                var currentContext = resource;

                foreach (var segment in segments)
                {
                    var fieldName = segment.Name;

                    if (segment.Filter != null)
                    {
                        if (segment.Filter.Type == FilterType.Index)
                        {
                            // Already numeric index, keep as-is
                            resolvedSegments.Add($"{fieldName}[{segment.Filter.Index}]");
                            
                            // Navigate to this element
                            if (currentContext != null && currentContext[fieldName] is JArray array)
                            {
                                if (segment.Filter.Index < array.Count)
                                {
                                    currentContext = array[segment.Filter.Index];
                                }
                                else
                                {
                                    currentContext = null;
                                }
                            }
                            else
                            {
                                currentContext = null;
                            }
                        }
                        else if (segment.Filter.Type == FilterType.KeyValue)
                        {
                            // Filter notation - need to find matching index
                            if (currentContext != null && currentContext[fieldName] is JArray array)
                            {
                                int matchingIndex = -1;
                                for (int i = 0; i < array.Count; i++)
                                {
                                    var element = array[i];
                                    var filterValue = element[segment.Filter.Key]?.ToString();
                                    
                                    if (filterValue == segment.Filter.Value)
                                    {
                                        matchingIndex = i;
                                        currentContext = element;
                                        break;
                                    }
                                }

                                if (matchingIndex >= 0)
                                {
                                    resolvedSegments.Add($"{fieldName}[{matchingIndex}]");
                                }
                                else
                                {
                                    // No match found, keep filter notation (frontend will handle)
                                    resolvedSegments.Add($"{fieldName}[{segment.Filter.Key}:{segment.Filter.Value}]");
                                    currentContext = null;
                                }
                            }
                            else
                            {
                                // Not an array or context lost, keep filter notation
                                resolvedSegments.Add($"{fieldName}[{segment.Filter.Key}:{segment.Filter.Value}]");
                                currentContext = null;
                            }
                        }
                        else
                        {
                            // Wildcard or other - keep as-is
                            resolvedSegments.Add(fieldName);
                            currentContext = currentContext?[fieldName];
                        }
                    }
                    else
                    {
                        // No filter
                        resolvedSegments.Add(fieldName);
                        currentContext = currentContext?[fieldName];
                    }
                }

                return string.Join(".", resolvedSegments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CpsPathResolver] Error resolving path '{cpsPath}': {ex.Message}");
                return cpsPath;
            }
        }
    }
}

