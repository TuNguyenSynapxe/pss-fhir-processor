namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Path
{
    /// <summary>
    /// Filter types for CPS1 path segments
    /// </summary>
    public enum FilterType
    {
        KeyValue,   // [key:value] - e.g., [url:ext-ethnicity]
        Index,      // [0] - e.g., [0]
        Wildcard    // [*] - select all
    }

    /// <summary>
    /// Filter specification for array/list navigation
    /// </summary>
    public class CpsFilter
    {
        public FilterType Type { get; set; }
        
        // For KeyValue filter
        public string Key { get; set; }
        public string Value { get; set; }
        
        // For Index filter
        public int Index { get; set; }

        public override string ToString()
        {
            switch (Type)
            {
                case FilterType.KeyValue:
                    return $"{Key}:{Value}";
                case FilterType.Index:
                    return Index.ToString();
                case FilterType.Wildcard:
                    return "*";
                default:
                    return "";
            }
        }
    }
}
