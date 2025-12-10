using MOH.HealthierSG.PSS.FhirProcessor.Models.Validation;

namespace MOH.HealthierSG.PSS.FhirProcessor.Api.Services
{
    public interface IFhirProcessorService
    {
        ProcessResult Process(string fhirJson, string validationMetadata, string logLevel = "info", bool strictDisplay = true);
        ProcessResult ValidateOnly(string fhirJson, string validationMetadata, string logLevel = "info", bool strictDisplay = true);
        ProcessResult ExtractOnly(string fhirJson, string logLevel = "info");
    }
}
