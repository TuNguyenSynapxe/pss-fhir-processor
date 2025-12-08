using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Utilities
{
    /// <summary>
    /// Resolves paths in FHIR objects supporting dot notation, array indexing, and wildcards
    /// </summary>
    public class PathResolver
    {
        /// <summary>
        /// Resolve a path to get a single value
        /// Supports: dot notation, [0] indexing, coding[0] shortcut
        /// </summary>
        public static object ResolvePath(object root, string path)
        {
            if (root == null || string.IsNullOrEmpty(path))
                return null;

            try
            {
                var parts = ParsePath(path);
                object current = root;

                for (int i = 0; i < parts.Count; i++)
                {
                    var part = parts[i];
                    if (current == null)
                    {
                        // Console.WriteLine($"[PathResolver] Part {i} '{part.Name}': current is null, stopping");
                        return null;
                    }

                    // Console.WriteLine($"[PathResolver] Part {i} '{part.Name}': current type = {current.GetType().Name}");
                    
                    if (part.IsIndexed)
                    {
                        current = ResolveIndexedProperty(current, part.Name, part.Index);
                    }
                    else
                    {
                        current = ResolveProperty(current, part.Name);
                    }
                    
                    // Console.WriteLine($"[PathResolver] Part {i} '{part.Name}': resolved to {(current == null ? "null" : current.GetType().Name)}");
                }

                return current;
            }
            catch (Exception ex)
            {
                // Log the error and return null to indicate the path couldn't be resolved
                // This handles cases like accessing JArray with string keys
                Console.WriteLine($"[PathResolver] Error resolving path '{path}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Resolve a path that may contain wildcards [] returning multiple matches
        /// </summary>
        public static List<object> ResolvePathWithWildcard(object root, string path)
        {
            if (root == null || string.IsNullOrEmpty(path))
                return new List<object>();

            try
            {
                var parts = ParsePath(path);
                var results = new List<object> { root };

                foreach (var part in parts)
                {
                    var nextResults = new List<object>();

                    foreach (var current in results)
                    {
                        if (current == null)
                            continue;

                        if (part.IsWildcard)
                        {
                            var items = ResolveWildcard(current, part.Name);
                            nextResults.AddRange(items);
                        }
                        else if (part.IsIndexed)
                        {
                            var item = ResolveIndexedProperty(current, part.Name, part.Index);
                            if (item != null)
                                nextResults.Add(item);
                        }
                        else
                        {
                            var item = ResolveProperty(current, part.Name);
                            if (item != null)
                                nextResults.Add(item);
                        }
                    }

                    results = nextResults;
                }

                return results;
            }
            catch (Exception ex)
            {
                // Log the error and return empty list
                Console.WriteLine($"[PathResolver] Error resolving wildcard path '{path}': {ex.Message}");
                return new List<object>();
            }
        }

        private static List<PathPart> ParsePath(string path)
        {
            var parts = new List<PathPart>();
            var segments = path.Split('.');

            foreach (var segment in segments)
            {
                if (segment.Contains("[") && segment.Contains("]"))
                {
                    var propName = segment.Substring(0, segment.IndexOf('['));
                    var indexPart = segment.Substring(segment.IndexOf('[') + 1, segment.IndexOf(']') - segment.IndexOf('[') - 1);

                    if (string.IsNullOrEmpty(indexPart))
                    {
                        // Wildcard []
                        parts.Add(new PathPart { Name = propName, IsWildcard = true });
                    }
                    else
                    {
                        // Indexed [0]
                        parts.Add(new PathPart { Name = propName, IsIndexed = true, Index = int.Parse(indexPart) });
                    }
                }
                else
                {
                    parts.Add(new PathPart { Name = segment });
                }
            }

            return parts;
        }

        private static object ResolveProperty(object obj, string propertyName)
        {
            if (obj == null)
                return null;

            // Handle JSON.NET types
            if (obj is Newtonsoft.Json.Linq.JObject jobj)
            {
                // Console.WriteLine($"[PathResolver.ResolveProperty] Looking for '{propertyName}' in JObject");
                // Console.WriteLine($"[PathResolver.ResolveProperty] JObject properties: {string.Join(", ", jobj.Properties().Select(p => p.Name))}");
                
                // Try to get property from JObject (case-insensitive)
                var jprop = jobj.Properties().FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                if (jprop != null)
                {
                    // Console.WriteLine($"[PathResolver.ResolveProperty] Found property '{jprop.Name}', value type: {jprop.Value?.Type}");
                    var value = jprop.Value;
                    // If the property is a JArray with elements, return the first element
                    // This handles FHIR's array properties like identifier, name, address
                    if (value is Newtonsoft.Json.Linq.JArray jarray && jarray.Count > 0)
                    {
                        return jarray[0];
                    }
                    return value;
                }
                else
                {
                    // Console.WriteLine($"[PathResolver.ResolveProperty] Property '{propertyName}' NOT FOUND in JObject");
                }
            }

            var type = obj.GetType();
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop != null)
            {
                // Console.WriteLine($"[PathResolver.ResolveProperty] Found property '{propertyName}' via reflection on {type.Name}");
                var value = prop.GetValue(obj);
                // Console.WriteLine($"[PathResolver.ResolveProperty] Reflection returned type: {value?.GetType()?.Name ?? "null"}");
                
                // If the property is a List with elements, return the first element
                // This handles FHIR's array properties like Identifier, Name, Address
                if (value is System.Collections.IList list && list.Count > 0)
                {
                    return list[0];
                }
                
                return value;
            }

            // Check for JsonExtensionData dictionary (for FHIR Resource base class)
            // Console.WriteLine($"[PathResolver.ResolveProperty] No direct property found, checking ExtensionData for '{propertyName}'");
            var extDataProp = type.GetProperty("ExtensionData", BindingFlags.Public | BindingFlags.Instance);
            if (extDataProp != null)
            {
                var extData = extDataProp.GetValue(obj) as System.Collections.Generic.Dictionary<string, object>;
                if (extData != null)
                {
                    // Console.WriteLine($"[PathResolver.ResolveProperty] ExtensionData has {extData.Count} keys: {string.Join(", ", extData.Keys)}");
                    
                    // Try exact match first
                    if (extData.TryGetValue(propertyName, out var value))
                    {
                        // Console.WriteLine($"[PathResolver.ResolveProperty] Found exact match for '{propertyName}', type: {value?.GetType()?.Name ?? "null"}");
                        // Console.WriteLine($"[PathResolver.ResolveProperty] Is JProperty? {value is Newtonsoft.Json.Linq.JProperty}");
                        // Console.WriteLine($"[PathResolver.ResolveProperty] Is JObject? {value is Newtonsoft.Json.Linq.JObject}");
                        
                        // Unwrap JProperty if needed (sometimes ExtensionData stores JProperty instead of the value)
                        if (value is Newtonsoft.Json.Linq.JProperty jprop)
                        {
                            // Console.WriteLine($"[PathResolver.ResolveProperty] Unwrapping JProperty, value type: {jprop.Value?.GetType()?.Name ?? "null"}");
                            value = jprop.Value;
                        }
                        
                        // Don't auto-unwrap JObject (it implements IList but shouldn't be treated as an array)
                        if (value is Newtonsoft.Json.Linq.JObject)
                        {
                            // Console.WriteLine($"[PathResolver.ResolveProperty] Value is JObject, returning as-is");
                            return value;
                        }
                        
                        // Auto-unwrap JArray (return first element for FHIR path convention)
                        if (value is Newtonsoft.Json.Linq.JArray jarray && jarray.Count > 0)
                        {
                            // Console.WriteLine($"[PathResolver.ResolveProperty] Value is JArray, returning first element");
                            return jarray[0];
                        }
                        
                        // Handle other array types in extension data
                        if (value is System.Collections.IList list && list.Count > 0)
                        {
                            // Console.WriteLine($"[PathResolver.ResolveProperty] Value is IList, returning first element");
                            return list[0];
                        }
                        // Console.WriteLine($"[PathResolver.ResolveProperty] Returning value of type: {value?.GetType()?.Name ?? "null"}");
                        return value;
                    }
                    
                    // Try case-insensitive match
                    var key = extData.Keys.FirstOrDefault(k => k.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                    if (key != null)
                    {
                        // Console.WriteLine($"[PathResolver.ResolveProperty] Found case-insensitive match: '{key}'");
                        var val = extData[key];
                        
                        // Unwrap JProperty if needed
                        if (val is Newtonsoft.Json.Linq.JProperty jp)
                        {
                            // Console.WriteLine($"[PathResolver.ResolveProperty] Unwrapping JProperty, value type: {jp.Value?.GetType()?.Name ?? "null"}");
                            val = jp.Value;
                        }
                        
                        // Don't auto-unwrap JObject
                        if (val is Newtonsoft.Json.Linq.JObject)
                        {
                            return val;
                        }
                        
                        // Auto-unwrap JArray (return first element)
                        if (val is Newtonsoft.Json.Linq.JArray jarr && jarr.Count > 0)
                        {
                            return jarr[0];
                        }
                        
                        // Handle other array types in extension data
                        if (val is System.Collections.IList list && list.Count > 0)
                        {
                            return list[0];
                        }
                        return val;
                    }
                }
            }

            return null;
        }

        private static object ResolveIndexedProperty(object obj, string propertyName, int index)
        {
            var collection = ResolveProperty(obj, propertyName);
            if (collection == null)
                return null;

            // Handle JSON.NET JArray
            if (collection is Newtonsoft.Json.Linq.JArray jarray)
            {
                if (index >= 0 && index < jarray.Count)
                    return jarray[index];
                return null;
            }

            if (collection is System.Collections.IList list)
            {
                if (index >= 0 && index < list.Count)
                    return list[index];
            }

            return null;
        }

        private static List<object> ResolveWildcard(object obj, string propertyName)
        {
            var results = new List<object>();
            var collection = ResolveProperty(obj, propertyName);

            if (collection == null)
                return results;

            if (collection is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item != null)
                        results.Add(item);
                }
            }

            return results;
        }

        private class PathPart
        {
            public string Name { get; set; }
            public bool IsIndexed { get; set; }
            public int Index { get; set; }
            public bool IsWildcard { get; set; }
        }
    }
}
