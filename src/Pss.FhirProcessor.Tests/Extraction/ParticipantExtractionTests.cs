using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Extraction
{
    [TestClass]
    public class ParticipantExtractionTests
    {
        [TestMethod]
        public void ExtractParticipant_ValidPatient_ExtractsAllFields()
        {
            var processor = new FhirProcessor();
            processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = false });
            
            var json = GetBundleWithPatient();
            var result = processor.Process(json);

            Assert.IsNotNull(result.Flatten?.Participant);
            Assert.AreEqual("S1234567A", result.Flatten.Participant.Nric);
            Assert.AreEqual("John Tan", result.Flatten.Participant.Name);
            Assert.AreEqual("male", result.Flatten.Participant.Gender);
            Assert.AreEqual("1950-01-01", result.Flatten.Participant.BirthDate);
        }

        private string GetBundleWithPatient()
        {
            return @"{
                ""resourceType"": ""Bundle"",
                ""entry"": [
                    {
                        ""resource"": {
                            ""resourceType"": ""Encounter""
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
                            ""resourceType"": ""Location""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""HealthcareService""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Organization""
                        }
                    },
                    {
                        ""resource"": {
                            ""resourceType"": ""Observation"",
                            ""code"": { ""coding"": [{ ""code"": ""HS"" }] },
                            ""component"": []
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
    }
}
