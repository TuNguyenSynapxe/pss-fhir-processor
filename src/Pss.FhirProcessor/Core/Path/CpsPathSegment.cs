namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Path
{
    /// <summary>
    /// Represents a single segment in a CPS1 path
    /// </summary>
    public class CpsPathSegment
    {
        public string Name { get; set; }
        public CpsFilter Filter { get; set; }

        public override string ToString()
        {
            if (Filter == null)
            {
                return Name;
            }
            return $"{Name}[{Filter}]";
        }
    }
}
