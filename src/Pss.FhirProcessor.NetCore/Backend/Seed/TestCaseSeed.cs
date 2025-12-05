using System.Collections.Generic;
using System.Linq;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Api.Seed
{
    public static class TestCaseSeed
    {
        public static List<TestCaseModel> GetAllTestCases()
        {
            return new List<TestCaseModel>
            {
                new TestCaseModel
                {
                    Name = "Valid_Complete_Bundle",
                    Description = "Valid bundle with all screening types",
                    ExpectedIsValid = true,
                    InputJson = GetValidCompleteBundle()
                },
                new TestCaseModel
                {
                    Name = "Missing_HS_Screening",
                    Description = "Missing HS Observation",
                    ExpectedIsValid = false,
                    InputJson = GetMissingHsBundle()
                },
                new TestCaseModel
                {
                    Name = "Invalid_Question_Display",
                    Description = "Question display mismatch",
                    ExpectedIsValid = false,
                    InputJson = GetInvalidDisplayBundle()
                },
                new TestCaseModel
                {
                    Name = "Invalid_Answer_Value",
                    Description = "Answer not in allowed values",
                    ExpectedIsValid = false,
                    InputJson = GetInvalidAnswerBundle()
                },
                new TestCaseModel
                {
                    Name = "Unknown_Question_Code",
                    Description = "Question code not in Codes Master",
                    ExpectedIsValid = false,
                    InputJson = GetUnknownCodeBundle()
                }
            };
        }

        public static TestCaseModel? GetTestCase(string name)
        {
            return GetAllTestCases().FirstOrDefault(t => t.Name == name);
        }

        private static string GetValidCompleteBundle()
        {
            return @"{
  ""resourceType"": ""Bundle"",
  ""type"": ""collection"",
  ""entry"": [
    {
      ""resource"": {
        ""resourceType"": ""Encounter"",
        ""status"": ""completed"",
        ""actualPeriod"": {
          ""start"": ""2025-01-10T09:00:00+08:00"",
          ""end"": ""2025-01-10T09:20:00+08:00""
        }
      }
    },
    {
      ""resource"": {
        ""resourceType"": ""Patient"",
        ""identifier"": [
          { ""system"": ""https://fhir.synapxe.sg/identifier/nric"", ""value"": ""S1234567A"" }
        ],
        ""name"": [{ ""text"": ""John Tan"" }],
        ""gender"": ""male"",
        ""birthDate"": ""1950-01-01""
      }
    },
    {
      ""resource"": {
        ""resourceType"": ""Location"",
        ""name"": ""ABC Community Center"",
        ""address"": { ""postalCode"": ""123456"" }
      }
    },
    {
      ""resource"": {
        ""resourceType"": ""HealthcareService"",
        ""name"": ""Mobile Screening Team""
      }
    },
    {
      ""resource"": {
        ""resourceType"": ""Organization"",
        ""name"": ""XYZ Provider"",
        ""type"": [{ ""coding"": [{ ""code"": ""prov"" }] }]
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
                ""display"": ""Currently wearing hearing aid(s)?""
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
        ""component"": [
          {
            ""code"": {
              ""coding"": [{
                ""code"": ""SQ-L2O9-00000001"",
                ""display"": ""Currently wearing dentures?""
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
        ""code"": { ""coding"": [{ ""code"": ""VS"" }] },
        ""component"": [
          {
            ""code"": {
              ""coding"": [{
                ""code"": ""SQ-L2V9-00000001"",
                ""display"": ""Currently wearing glasses?""
              }]
            },
            ""valueString"": ""Yes""
          }
        ]
      }
    }
  ]
}";
        }

        private static string GetMissingHsBundle()
        {
            return @"{
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
        ""identifier"": [{ ""value"": ""S1234567A"" }]
      }
    },
    {
      ""resource"": {
        ""resourceType"": ""Location"",
        ""name"": ""ABC CC""
      }
    },
    {
      ""resource"": {
        ""resourceType"": ""HealthcareService"",
        ""name"": ""Team""
      }
    },
    {
      ""resource"": {
        ""resourceType"": ""Organization"",
        ""name"": ""Provider""
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
        }

        private static string GetInvalidDisplayBundle()
        {
            return @"{
  ""resourceType"": ""Bundle"",
  ""type"": ""collection"",
  ""entry"": [
    {
      ""resource"": {
        ""resourceType"": ""Observation"",
        ""code"": { ""coding"": [{ ""code"": ""HS"" }] },
        ""component"": [
          {
            ""code"": {
              ""coding"": [{
                ""code"": ""SQ-L2H9-00000001"",
                ""display"": ""Wrong Display Text""
              }]
            },
            ""valueString"": ""No""
          }
        ]
      }
    }
  ]
}";
        }

        private static string GetInvalidAnswerBundle()
        {
            return @"{
  ""resourceType"": ""Bundle"",
  ""type"": ""collection"",
  ""entry"": [
    {
      ""resource"": {
        ""resourceType"": ""Observation"",
        ""code"": { ""coding"": [{ ""code"": ""HS"" }] },
        ""component"": [
          {
            ""code"": {
              ""coding"": [{
                ""code"": ""SQ-L2H9-00000001"",
                ""display"": ""Currently wearing hearing aid(s)?""
              }]
            },
            ""valueString"": ""Maybe""
          }
        ]
      }
    }
  ]
}";
        }

        private static string GetUnknownCodeBundle()
        {
            return @"{
  ""resourceType"": ""Bundle"",
  ""type"": ""collection"",
  ""entry"": [
    {
      ""resource"": {
        ""resourceType"": ""Observation"",
        ""code"": { ""coding"": [{ ""code"": ""HS"" }] },
        ""component"": [
          {
            ""code"": {
              ""coding"": [{
                ""code"": ""UNKNOWN-CODE"",
                ""display"": ""Unknown Question""
              }]
            },
            ""valueString"": ""Yes""
          }
        ]
      }
    }
  ]
}";
        }
    }

    public class TestCaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool ExpectedIsValid { get; set; }
        public string InputJson { get; set; } = string.Empty;
    }
}
