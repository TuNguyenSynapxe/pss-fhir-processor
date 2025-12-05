using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Api.Services
{
    public interface IFhirProcessorService
    {
        ProcessResult Process(string fhirJson, string validationMetadata, string logLevel = "info", bool strictDisplay = true);
    }
}
