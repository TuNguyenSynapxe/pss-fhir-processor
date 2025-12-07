using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests.Helpers
{
    /// <summary>
    /// Helper methods for mutating FHIR Bundle JSON to create negative test cases
    /// </summary>
    public static class JsonMutationHelpers
    {
        /// <summary>
        /// Remove all bundle entries with the specified resource type
        /// </summary>
        public static JObject RemoveEntryByResourceType(JObject bundle, string resourceType)
        {
            var clone = (JObject)bundle.DeepClone();
            var entries = clone["entry"] as JArray;
            
            if (entries == null) return clone;

            var toRemove = entries
                .Where(e => e["resource"]?["resourceType"]?.ToString() == resourceType)
                .ToList();

            foreach (var entry in toRemove)
            {
                entries.Remove(entry);
            }

            return clone;
        }

        /// <summary>
        /// Remove entries by filtering on a nested property value
        /// Example: RemoveEntryByProperty(bundle, "resource.type[0].coding[0].code", "PROVIDER")
        /// </summary>
        public static JObject RemoveEntryByProperty(JObject bundle, string propertyPath, string expectedValue)
        {
            var clone = (JObject)bundle.DeepClone();
            var entries = clone["entry"] as JArray;
            
            if (entries == null) return clone;

            var toRemove = entries
                .Where(e =>
                {
                    var token = e.SelectToken(propertyPath);
                    return token?.ToString() == expectedValue;
                })
                .ToList();

            foreach (var entry in toRemove)
            {
                entries.Remove(entry);
            }

            return clone;
        }

        /// <summary>
        /// Remove a property at the specified JSON path
        /// </summary>
        public static JObject RemoveProperty(JObject obj, string jsonPath)
        {
            var clone = (JObject)obj.DeepClone();
            var token = clone.SelectToken(jsonPath);
            
            if (token != null)
            {
                token.Parent.Remove();
            }

            return clone;
        }

        /// <summary>
        /// Replace a string value at the specified JSON path
        /// </summary>
        public static JObject ReplaceString(JObject obj, string jsonPath, string newValue)
        {
            var clone = (JObject)obj.DeepClone();
            var token = clone.SelectToken(jsonPath);
            
            if (token != null)
            {
                token.Replace(new JValue(newValue));
            }

            return clone;
        }

        /// <summary>
        /// Replace any value at the specified JSON path
        /// </summary>
        public static JObject ReplaceValue(JObject obj, string jsonPath, JToken newValue)
        {
            var clone = (JObject)obj.DeepClone();
            var token = clone.SelectToken(jsonPath);
            
            if (token != null)
            {
                token.Replace(newValue);
            }

            return clone;
        }

        /// <summary>
        /// Remove a resource by its resource type
        /// </summary>
        public static JObject RemoveResource(JObject bundle, string resourceType)
        {
            return RemoveEntryByResourceType(bundle, resourceType);
        }

        /// <summary>
        /// Replace a GUID with an invalid value
        /// </summary>
        public static JObject BreakGuid(JObject obj, string jsonPath)
        {
            return ReplaceString(obj, jsonPath, "NOT-A-VALID-GUID");
        }

        /// <summary>
        /// Replace a URN UUID with an invalid value
        /// </summary>
        public static JObject BreakGuidUri(JObject obj, string jsonPath)
        {
            return ReplaceString(obj, jsonPath, "bad-uri-format");
        }

        /// <summary>
        /// Replace a datetime with an invalid value
        /// </summary>
        public static JObject BreakDateTime(JObject obj, string jsonPath)
        {
            return ReplaceString(obj, jsonPath, "not-a-valid-datetime");
        }

        /// <summary>
        /// Replace a date with an invalid value
        /// </summary>
        public static JObject BreakDate(JObject obj, string jsonPath)
        {
            return ReplaceString(obj, jsonPath, "not-a-date");
        }

        /// <summary>
        /// Replace a reference with a non-existent URN UUID
        /// </summary>
        public static JObject BreakReference(JObject obj, string jsonPath)
        {
            return ReplaceString(obj, jsonPath, "urn:uuid:deadbeef-dead-beef-dead-beefdeadbeef");
        }

        /// <summary>
        /// Find the first entry with the specified resource type
        /// </summary>
        public static JToken FindFirstResourceByType(JObject bundle, string resourceType)
        {
            var entries = bundle["entry"] as JArray;
            if (entries == null) return null;

            return entries.FirstOrDefault(e => 
                e["resource"]?["resourceType"]?.ToString() == resourceType);
        }

        /// <summary>
        /// Find the first Observation with the specified screening type code
        /// </summary>
        public static JToken FindObservationByScreeningType(JObject bundle, string screeningTypeCode)
        {
            var entries = bundle["entry"] as JArray;
            if (entries == null) return null;

            return entries.FirstOrDefault(e => 
                e["resource"]?["resourceType"]?.ToString() == "Observation" &&
                e["resource"]?["code"]?["coding"]?[0]?["code"]?.ToString() == screeningTypeCode);
        }

        /// <summary>
        /// Mutate the first component of an Observation to have an invalid question code
        /// </summary>
        public static JObject BreakObservationQuestionCode(JObject bundle, string screeningTypeCode, string invalidCode)
        {
            var clone = (JObject)bundle.DeepClone();
            var observation = FindObservationByScreeningType(clone, screeningTypeCode);
            
            if (observation != null)
            {
                var component = observation["resource"]?["component"]?[0];
                if (component != null)
                {
                    component["code"]["coding"][0]["code"] = invalidCode;
                }
            }

            return clone;
        }

        /// <summary>
        /// Mutate the first component of an Observation to have an invalid display
        /// </summary>
        public static JObject BreakObservationQuestionDisplay(JObject bundle, string screeningTypeCode, string invalidDisplay)
        {
            var clone = (JObject)bundle.DeepClone();
            var observation = FindObservationByScreeningType(clone, screeningTypeCode);
            
            if (observation != null)
            {
                var component = observation["resource"]?["component"]?[0];
                if (component != null)
                {
                    component["code"]["coding"][0]["display"] = invalidDisplay;
                }
            }

            return clone;
        }

        /// <summary>
        /// Mutate the first component of an Observation to have an invalid answer value
        /// </summary>
        public static JObject BreakObservationAnswerValue(JObject bundle, string screeningTypeCode, string invalidAnswer)
        {
            var clone = (JObject)bundle.DeepClone();
            var observation = FindObservationByScreeningType(clone, screeningTypeCode);
            
            if (observation != null)
            {
                var component = observation["resource"]?["component"]?[0];
                if (component != null)
                {
                    component["valueString"] = invalidAnswer;
                }
            }

            return clone;
        }

        /// <summary>
        /// Remove field/element at CPS path
        /// Returns unchanged bundle if path doesn't exist
        /// </summary>
        public static JObject RemoveAtCps(JObject bundle, string cpsPath)
        {
            var clone = (JObject)bundle.DeepClone();
            
            // Check if path exists
            var tokens = CpsPathNavigator.SelectTokens(clone, cpsPath);
            if (tokens.Count == 0)
            {
                // Path doesn't exist - return unchanged (mutation is a no-op)
                return clone;
            }
            
            CpsPathNavigator.RemoveAt(clone, cpsPath);
            return clone;
        }

        /// <summary>
        /// Replace value at CPS path
        /// Returns unchanged bundle if path doesn't exist
        /// </summary>
        public static JObject ReplaceValueCps(JObject bundle, string cpsPath, JToken newValue)
        {
            var clone = (JObject)bundle.DeepClone();
            
            // Check if path exists
            var tokens = CpsPathNavigator.SelectTokens(clone, cpsPath);
            if (tokens.Count == 0)
            {
                // Path doesn't exist - return unchanged (mutation is a no-op)
                return clone;
            }
            
            CpsPathNavigator.ReplaceValue(clone, cpsPath, newValue);
            return clone;
        }

        /// <summary>
        /// Check if a CPS path exists and has values
        /// </summary>
        public static bool ExistsCpsPath(JObject bundle, string cpsPath)
        {
            var tokens = CpsPathNavigator.SelectTokens(bundle, cpsPath);
            return tokens.Any();
        }
    }
}
