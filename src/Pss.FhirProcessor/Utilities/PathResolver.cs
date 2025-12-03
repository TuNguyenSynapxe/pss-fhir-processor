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

            var parts = ParsePath(path);
            object current = root;

            foreach (var part in parts)
            {
                if (current == null)
                    return null;

                if (part.IsIndexed)
                {
                    current = ResolveIndexedProperty(current, part.Name, part.Index);
                }
                else
                {
                    current = ResolveProperty(current, part.Name);
                }
            }

            return current;
        }

        /// <summary>
        /// Resolve a path that may contain wildcards [] returning multiple matches
        /// </summary>
        public static List<object> ResolvePathWithWildcard(object root, string path)
        {
            if (root == null || string.IsNullOrEmpty(path))
                return new List<object>();

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
                // Try to get property from JObject (case-insensitive)
                var jprop = jobj.Properties().FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                if (jprop != null)
                    return jprop.Value;
            }

            var type = obj.GetType();
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop != null)
            {
                return prop.GetValue(obj);
            }

            // Check for JsonExtensionData dictionary (for FHIR Resource base class)
            var extDataProp = type.GetProperty("ExtensionData", BindingFlags.Public | BindingFlags.Instance);
            if (extDataProp != null)
            {
                var extData = extDataProp.GetValue(obj) as System.Collections.Generic.Dictionary<string, object>;
                if (extData != null)
                {
                    // Try exact match first
                    if (extData.TryGetValue(propertyName, out var value))
                        return value;
                    
                    // Try case-insensitive match
                    var key = extData.Keys.FirstOrDefault(k => k.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                    if (key != null)
                        return extData[key];
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
