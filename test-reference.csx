#r "src/Pss.FhirProcessor/bin/Debug/net6.0/MOH.HealthierSG.Plugins.PSS.FhirProcessor.dll"
#r "nuget: Newtonsoft.Json, 13.0.3"

using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation;
using System.IO;

var metadataJson = File.ReadAllText("src/Pss.FhirProcessor.NetCore/Frontend/src/seed/validation-metadata.json");
var bundleJson = File.ReadAllText("test-reference-validation.json");

var engine = new ValidationEngine();
engine.LoadMetadataFromJson(metadataJson);

var result = engine.Validate(bundleJson, "verbose");

Console.WriteLine($"Is Valid: {result.IsValid}");
Console.WriteLine($"Error Count: {result.Errors.Count}");

foreach (var error in result.Errors)
{
    Console.WriteLine($"  [{error.Code}] {error.Path}: {error.Message}");
}
