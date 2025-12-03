using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Flattened
{
    /// <summary>
    /// Screening set containing all observations for a screening type (HS/OS/VS)
    /// </summary>
    public class ScreeningSet
    {
        public string ScreeningType { get; set; }
        public List<ObservationItem> Items { get; set; }
    }
}
