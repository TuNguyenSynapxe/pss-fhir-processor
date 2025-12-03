using System;
using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

class Program
{
    static void Main()
    {
        var processor = new FhirProcessor();
        
        var codesMaster = @"{
            ""Questions"": [
                {
                    ""QuestionCode"": ""SQ-L2H9-00000001"",
                    ""QuestionDisplay"": ""Currently wearing hearing aid(s)?"",
                    ""ScreeningType"": ""HS"",
                    ""AllowedAnswers"": [""Yes"", ""No""]
                }
            ]
        }";
        
        var rules = new Dictionary<string, string>
        {
            { "HS", @"{
                ""Scope"": ""HS"",
                ""Rules"": [
                    {
                        ""RuleType"": ""CodesMaster"",
                        ""Path"": ""Entry[].Resource.Component""
                    }
                ]
            }" }
        };
        
        processor.LoadCodesMaster(codesMaster);
        processor.LoadRuleSets(rules);
        processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });
        
        var json = @"{
            ""resourceType"": ""Bundle"",
            ""type"": ""collection"",
            ""entry"": [
                {
                    ""resource"": {
                        ""resourceType"": ""Encounter"",
                        ""status"": ""completed""
                    }
                },
                {
                    ""resource"": {
                        ""resourceType"": ""Patient"",
                        ""identifier"": [{ ""system"": ""https://fhir.synapxe.sg/identifier/nric"", ""value"": ""S1234567A"" }]
                    }
                },
                {
                    ""resource"": {
                        ""resourceType"": ""Observation"",
                        ""code"": { ""coding"": [{ ""code"": ""HS"" }] },
                        ""component"": [
                            {
                                ""code"": {
                                    ""coding"": [{
                                        ""code"": ""SQ-L2H9-00000001"",
                                        ""display"": ""Currently wearing hearing aids?""
                                    }]
                                },
                                ""valueString"": ""No""
                            }
                        ]
                    }
                },
                {
                    ""resource"": {
                        ""resourceType"": ""Observation"",
                        ""code"": { ""coding"": [{ ""code"": ""OS"" }] },
                        ""component"": []
                    }
                },
                {
                    ""resource"": {
                        ""resourceType"": ""Observation"",
                        ""code"": { ""coding"": [{ ""code"": ""VS"" }] },
                        ""component"": []
                    }
                }
            ]
        }";
        
        var result = processor.Process(json);
        
        Console.WriteLine($"Valid: {result.Validation.IsValid}");
        Console.WriteLine($"Error Count: {result.Validation.Errors.Count}");
        foreach (var error in result.Validation.Errors)
        {
            Console.WriteLine($"  - [{error.Code}] {error.Message}");
        }
    }
}
