namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.WebApp.Models
{
    public class TestCaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string InputJson { get; set; }
        public bool ExpectedIsValid { get; set; }
    }
}
