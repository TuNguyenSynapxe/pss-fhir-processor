using Newtonsoft.Json;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Utilities
{
    /// <summary>
    /// JSON serialization/deserialization helper
    /// </summary>
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.None  // Security: Prevent deserialization attacks
        };

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);

            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        public static string Serialize(object obj)
        {
            if (obj == null)
                return null;

            return JsonConvert.SerializeObject(obj, Settings);
        }
    }
}
