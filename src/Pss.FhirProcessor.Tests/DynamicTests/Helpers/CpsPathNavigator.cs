using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MOH.HealthierSG.PSS.FhirProcessor.Tests.DynamicTests.Helpers
{
    /// <summary>
    /// Navigates and mutates JSON using CPS (Custom Path Syntax) paths.
    /// Mirrors the CpsPathResolver logic from the validation engine for test mutations.
    /// </summary>
    public static class CpsPathNavigator
    {
        /// <summary>
        /// Select all tokens matching a CPS path
        /// </summary>
        public static List<JToken> SelectTokens(JToken root, string cpsPath)
        {
            if (root == null || string.IsNullOrEmpty(cpsPath))
            {
                return new List<JToken>();
            }

            var segments = ParsePath(cpsPath);
            var results = new List<JToken> { root };

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
        /// Remove the property/element at the specified CPS path
        /// </summary>
        public static void RemoveAt(JToken root, string cpsPath)
        {
            if (root == null || string.IsNullOrEmpty(cpsPath))
            {
                return;
            }

            // Special handling for Bundle.entry[ResourceType] - remove matching entries
            if (cpsPath.StartsWith("Bundle.entry[") && cpsPath.EndsWith("]"))
            {
                RemoveBundleEntry(root, cpsPath);
                return;
            }

            // Parse path to get parent and target property
            var segments = ParsePath(cpsPath);
            if (segments.Count == 0)
            {
                return;
            }

            // Navigate to parent tokens (all but last segment)
            var parentSegments = segments.Take(segments.Count - 1).ToList();
            var targetSegment = segments.Last();

            var parentTokens = new List<JToken> { root };
            foreach (var segment in parentSegments)
            {
                parentTokens = ApplySegment(parentTokens, segment);
                if (parentTokens.Count == 0)
                {
                    return;
                }
            }

            // Remove target from each parent
            foreach (var parent in parentTokens)
            {
                RemoveFromParent(parent, targetSegment);
            }
        }

        /// <summary>
        /// Replace the value at the specified CPS path
        /// </summary>
        public static void ReplaceValue(JToken root, string cpsPath, JToken newValue)
        {
            if (root == null || string.IsNullOrEmpty(cpsPath))
            {
                return;
            }

            var targets = SelectTokens(root, cpsPath);
            foreach (var target in targets)
            {
                if (target.Parent != null)
                {
                    target.Replace(newValue);
                }
            }
        }

        #region Path Parsing

        private static List<PathSegment> ParsePath(string cpsPath)
        {
            var segments = new List<PathSegment>();
            var parts = new List<string>();
            
            // Split by '.' but respect brackets
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
                    if (i > start)
                    {
                        parts.Add(cpsPath.Substring(start, i - start));
                    }
                    start = i + 1;
                }
            }
            
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

                var segment = new PathSegment();
                var bracketStart = part.IndexOf('[');

                if (bracketStart == -1)
                {
                    segment.Name = part;
                }
                else
                {
                    segment.Name = part.Substring(0, bracketStart);
                    var bracketEnd = part.IndexOf(']', bracketStart);

                    if (bracketEnd != -1)
                    {
                        var filterContent = part.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
                        segment.Selector = ParseSelector(filterContent);
                    }
                }

                segments.Add(segment);
            }

            return segments;
        }

        private static PathSelector ParseSelector(string filterContent)
        {
            if (filterContent == "*")
            {
                return new PathSelector { Type = SelectorType.Wildcard };
            }

            int colonIndex = filterContent.IndexOf(':');
            if (colonIndex != -1)
            {
                var key = filterContent.Substring(0, colonIndex);
                var value = filterContent.Substring(colonIndex + 1);
                
                // Special handling for QuestionCode selector
                if (key.Equals("QuestionCode", StringComparison.OrdinalIgnoreCase))
                {
                    key = "code.coding[0].code";
                }
                
                return new PathSelector
                {
                    Type = SelectorType.KeyValue,
                    Key = key,
                    Value = value
                };
            }

            // Try to parse as index
            if (int.TryParse(filterContent, out int index))
            {
                return new PathSelector
                {
                    Type = SelectorType.Index,
                    Index = index
                };
            }

            // Bare resource type name (e.g., [Patient], [Observation])
            // These match resource.resourceType in Bundle.entry contexts
            return new PathSelector
            {
                Type = SelectorType.KeyValue,
                Key = "resource.resourceType",
                Value = filterContent
            };
        }

        #endregion

        #region Navigation

        private static List<JToken> ApplySegment(List<JToken> currentTokens, PathSegment segment)
        {
            var results = new List<JToken>();

            foreach (var token in currentTokens)
            {
                var matches = NavigateSegment(token, segment);
                results.AddRange(matches);
            }

            return results;
        }

        private static List<JToken> NavigateSegment(JToken token, PathSegment segment)
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

                if (segment.Selector == null)
                {
                    results.Add(property);
                }
                else
                {
                    results = ApplySelector(property, segment.Selector);
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                var array = (JArray)token;
                foreach (var element in array)
                {
                    var matches = NavigateSegment(element, segment);
                    results.AddRange(matches);
                }
            }

            return results;
        }

        private static List<JToken> ApplySelector(JToken token, PathSelector selector)
        {
            var results = new List<JToken>();

            if (token.Type == JTokenType.Array)
            {
                var array = (JArray)token;

                if (selector.Type == SelectorType.Wildcard)
                {
                    results.AddRange(array);
                }
                else if (selector.Type == SelectorType.Index)
                {
                    if (selector.Index >= 0 && selector.Index < array.Count)
                    {
                        results.Add(array[selector.Index]);
                    }
                }
                else if (selector.Type == SelectorType.KeyValue)
                {
                    foreach (var element in array)
                    {
                        if (element.Type == JTokenType.Object)
                        {
                            var obj = (JObject)element;
                            
                            if (selector.Key.Contains(".") || selector.Key.Contains("["))
                            {
                                // Nested path matching
                                if (MatchesNestedPath(obj, selector.Key, selector.Value))
                                {
                                    results.Add(element);
                                }
                            }
                            else
                            {
                                // Simple property matching
                                var keyProperty = obj[selector.Key];
                                if (keyProperty != null && MatchesValue(keyProperty, selector.Value))
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
                results.Add(token);
            }

            return results;
        }

        private static bool MatchesValue(JToken token, string expectedValue)
        {
            if (token == null)
            {
                return false;
            }

            if (token.Type == JTokenType.String || token.Type == JTokenType.Integer || 
                token.Type == JTokenType.Float || token.Type == JTokenType.Boolean)
            {
                return token.ToString() == expectedValue;
            }

            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;
                
                var code = obj["code"];
                if (code != null && code.ToString() == expectedValue)
                {
                    return true;
                }

                var system = obj["system"];
                if (system != null && system.ToString() == expectedValue)
                {
                    return true;
                }
            }

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

        private static bool MatchesNestedPath(JToken token, string path, string expectedValue)
        {
            if (token == null || string.IsNullOrEmpty(path))
            {
                return false;
            }

            var values = SelectTokens(token, path);
            
            foreach (var value in values)
            {
                if (value != null && value.ToString() == expectedValue)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Mutation Operations

        private static void RemoveFromParent(JToken parent, PathSegment targetSegment)
        {
            if (parent.Type == JTokenType.Object)
            {
                var obj = (JObject)parent;
                var property = obj[targetSegment.Name];

                if (property == null)
                {
                    return;
                }

                if (targetSegment.Selector == null)
                {
                    // Remove the entire property
                    obj.Remove(targetSegment.Name);
                }
                else
                {
                    // Remove from array based on selector
                    RemoveFromArray(property, targetSegment.Selector);
                }
            }
        }

        private static void RemoveFromArray(JToken token, PathSelector selector)
        {
            if (token.Type != JTokenType.Array)
            {
                return;
            }

            var array = (JArray)token;

            if (selector.Type == SelectorType.Index)
            {
                if (selector.Index >= 0 && selector.Index < array.Count)
                {
                    array.RemoveAt(selector.Index);
                }
            }
            else if (selector.Type == SelectorType.KeyValue)
            {
                var toRemove = new List<JToken>();
                
                foreach (var element in array)
                {
                    if (element.Type == JTokenType.Object)
                    {
                        var obj = (JObject)element;
                        
                        if (selector.Key.Contains(".") || selector.Key.Contains("["))
                        {
                            if (MatchesNestedPath(obj, selector.Key, selector.Value))
                            {
                                toRemove.Add(element);
                            }
                        }
                        else
                        {
                            var keyProperty = obj[selector.Key];
                            if (keyProperty != null && MatchesValue(keyProperty, selector.Value))
                            {
                                toRemove.Add(element);
                            }
                        }
                    }
                }

                foreach (var element in toRemove)
                {
                    array.Remove(element);
                }
            }
            else if (selector.Type == SelectorType.Wildcard)
            {
                array.Clear();
            }
        }

        private static void RemoveBundleEntry(JToken root, string cpsPath)
        {
            // Extract resource selector from Bundle.entry[ResourceType]
            var selectorStart = "Bundle.entry[".Length;
            var selectorEnd = cpsPath.Length - 1;
            var resourceSelector = cpsPath.Substring(selectorStart, selectorEnd - selectorStart);

            var entries = root["entry"] as JArray;
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            // Parse resource selector
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

            // Find and remove matching entries
            var toRemove = new List<JToken>();

            foreach (var entry in entries)
            {
                var entryResource = entry["resource"] as JObject;
                if (entryResource == null) continue;

                var resourceType = entryResource["resourceType"]?.ToString();
                if (resourceType != targetResourceType) continue;

                if (string.IsNullOrEmpty(targetScreeningCode))
                {
                    toRemove.Add(entry);
                }
                else
                {
                    // Check type code
                    if (resourceType == "Observation")
                    {
                        var screeningCode = SelectTokens(entryResource, "code.coding[0].code")
                            .FirstOrDefault()?.ToString();
                        if (screeningCode == targetScreeningCode)
                        {
                            toRemove.Add(entry);
                        }
                    }
                    else if (resourceType == "Organization")
                    {
                        var orgTypeCode = SelectTokens(entryResource, "type.coding[0].code")
                            .FirstOrDefault()?.ToString();
                        if (orgTypeCode == targetScreeningCode)
                        {
                            toRemove.Add(entry);
                        }
                    }
                }
            }

            foreach (var entry in toRemove)
            {
                entries.Remove(entry);
            }
        }

        #endregion

        #region Helper Classes

        private class PathSegment
        {
            public string Name { get; set; }
            public PathSelector Selector { get; set; }
        }

        private class PathSelector
        {
            public SelectorType Type { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
            public int Index { get; set; }
        }

        private enum SelectorType
        {
            None,
            Index,
            KeyValue,
            Wildcard
        }

        #endregion
    }
}
