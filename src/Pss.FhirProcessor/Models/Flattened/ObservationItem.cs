using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Common;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Flattened
{
    /// <summary>
    /// Single observation item (question + answer(s))
    /// </summary>
    public class ObservationItem
    {
        public CodeDisplayValue Question { get; set; }
        public List<string> Values { get; set; }
    }
}
