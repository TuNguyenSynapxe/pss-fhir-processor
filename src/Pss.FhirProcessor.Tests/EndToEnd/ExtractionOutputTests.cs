using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using System.Linq;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end tests validating extraction output for valid bundles
    /// </summary>
    [TestClass]
    public class ExtractionOutputTests
    {
        private FhirProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new FhirProcessor();
            
            var codesMaster = @"{
                ""Questions"": [
                    {
                        ""QuestionCode"": ""SQ-L2H9-00000001"",
                        ""QuestionDisplay"": ""Currently wearing hearing aid(s)?"",
                        ""ScreeningType"": ""HS"",
                        ""AllowedAnswers"": [""Yes"", ""No""]
                    },
                    {
                        ""QuestionCode"": ""SQ-L2H9-00000002"",
                        ""QuestionDisplay"": ""Pure Tone Audiometry Test Result"",
                        ""ScreeningType"": ""HS"",
                        ""AllowedAnswers"": [""500Hz – R"", ""500Hz – NR"", ""1000Hz – R"", ""1000Hz – NR"", ""2000Hz – R"", ""2000Hz – NR""]
                    },
                    {
                        ""QuestionCode"": ""SQ-L2O9-00000001"",
                        ""QuestionDisplay"": ""Currently wearing dentures?"",
                        ""ScreeningType"": ""OS"",
                        ""AllowedAnswers"": [""Yes"", ""No""]
                    },
                    {
                        ""QuestionCode"": ""SQ-L2V9-00000001"",
                        ""QuestionDisplay"": ""Currently wearing glasses?"",
                        ""ScreeningType"": ""VS"",
                        ""AllowedAnswers"": [""Yes"", ""No""]
                    }
                ]
            }";
            
            _processor.LoadCodesMaster(codesMaster);
            _processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });
        }

        [TestMethod]
        public void ValidBundle_ExtractsEventData()
        {
            var json = GetCompleteValidBundle();
            
            var result = _processor.Process(json);

            Assert.IsTrue(result.Validation.IsValid);
            Assert.IsNotNull(result.Flatten.Event);
            Assert.AreEqual("2025-01-10T09:00:00+08:00", result.Flatten.Event.Start);
            Assert.AreEqual("2025-01-10T09:20:00+08:00", result.Flatten.Event.End);
            Assert.AreEqual("ABC Community Center", result.Flatten.Event.VenueName);
            Assert.AreEqual("123456", result.Flatten.Event.PostalCode);
        }

        [TestMethod]
        public void ValidBundle_ExtractsParticipantData()
        {
            var json = GetCompleteValidBundle();
            
            var result = _processor.Process(json);

            Assert.IsTrue(result.Validation.IsValid);
            Assert.IsNotNull(result.Flatten.Participant);
            Assert.AreEqual("S1234567A", result.Flatten.Participant.Nric);
            Assert.AreEqual("John Tan", result.Flatten.Participant.Name);
            Assert.AreEqual("male", result.Flatten.Participant.Gender);
            Assert.AreEqual("1950-01-01", result.Flatten.Participant.BirthDate);
        }

        [TestMethod]
        public void ValidBundle_ExtractsHearingScreening()
        {
            var json = GetCompleteValidBundle();
            
            var result = _processor.Process(json);

            Assert.IsTrue(result.Validation.IsValid);
            Assert.IsNotNull(result.Flatten.HearingRaw);
            Assert.AreEqual(2, result.Flatten.HearingRaw.Items.Count);

            var hearingAid = result.Flatten.GetHearing("SQ-L2H9-00000001");
            Assert.IsNotNull(hearingAid);
            Assert.AreEqual("No", hearingAid.Values[0]);

            var pureTone = result.Flatten.GetHearing("SQ-L2H9-00000002");
            Assert.IsNotNull(pureTone);
            Assert.IsTrue(pureTone.Values.Count > 0, "PureTone should have values");
        }

        [TestMethod]
        public void ValidBundle_ExtractsOralScreening()
        {
            var json = GetCompleteValidBundle();
            
            var result = _processor.Process(json);

            Assert.IsTrue(result.Validation.IsValid);
            Assert.IsNotNull(result.Flatten.OralRaw);
            Assert.AreEqual(1, result.Flatten.OralRaw.Items.Count);

            var dentures = result.Flatten.GetOral("SQ-L2O9-00000001");
            Assert.IsNotNull(dentures);
            Assert.AreEqual("Yes", dentures.Values[0]);
        }

        [TestMethod]
        public void ValidBundle_ExtractsVisionScreening()
        {
            var json = GetCompleteValidBundle();
            
            var result = _processor.Process(json);

            Assert.IsTrue(result.Validation.IsValid);
            Assert.IsNotNull(result.Flatten.VisionRaw);
            Assert.AreEqual(1, result.Flatten.VisionRaw.Items.Count);

            var glasses = result.Flatten.GetVision("SQ-L2V9-00000001");
            Assert.IsNotNull(glasses);
            Assert.AreEqual("Yes", glasses.Values[0]);
        }

        [TestMethod]
        public void ValidBundle_LookupHelpers_ReturnNull_ForNonExistent()
        {
            var json = GetCompleteValidBundle();
            
            var result = _processor.Process(json);

            var nonExistent = result.Flatten.GetHearing("NONEXISTENT");
            Assert.IsNull(nonExistent, "Non-existent question should return null");
        }

        [TestMethod]
        public void ValidBundle_ExtractsAllFields()
        {
            var json = GetCompleteValidBundle();
            
            var result = _processor.Process(json);

            Assert.IsTrue(result.Validation.IsValid);
            
            // Event
            Assert.IsNotNull(result.Flatten.Event.Start);
            Assert.IsNotNull(result.Flatten.Event.End);
            Assert.IsNotNull(result.Flatten.Event.VenueName);
            Assert.IsNotNull(result.Flatten.Event.PostalCode);
            Assert.IsNotNull(result.Flatten.Event.Grc);
            Assert.IsNotNull(result.Flatten.Event.Constituency);
            Assert.IsNotNull(result.Flatten.Event.ProviderName);
            Assert.IsNotNull(result.Flatten.Event.ClusterName);

            // Participant
            Assert.IsNotNull(result.Flatten.Participant.Nric);
            Assert.IsNotNull(result.Flatten.Participant.Name);
            Assert.IsNotNull(result.Flatten.Participant.Gender);
            Assert.IsNotNull(result.Flatten.Participant.BirthDate);

            // Screenings
            Assert.IsTrue(result.Flatten.HearingRaw != null && result.Flatten.HearingRaw.Items.Count > 0);
            Assert.IsTrue(result.Flatten.OralRaw != null && result.Flatten.OralRaw.Items.Count > 0);
            Assert.IsTrue(result.Flatten.VisionRaw != null && result.Flatten.VisionRaw.Items.Count > 0);
        }

        private string GetCompleteValidBundle()
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
                            ""address"": { ""postalCode"": ""123456"" },
                            ""extension"": [
                                { ""url"": ""grc"", ""valueString"": ""Tanjong Pagar GRC"" },
                                { ""url"": ""constituency"", ""valueString"": ""Tanjong Pagar"" }
                            ]
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
                            ""name"": ""XYZ Provider""
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
                                },
                                {
                                    ""code"": {
                                        ""coding"": [{
                                            ""code"": ""SQ-L2H9-00000002"",
                                            ""display"": ""Pure Tone Audiometry Test Result""
                                        }]
                                    },
                                    ""valueString"": ""500Hz – R|1000Hz – R|2000Hz – NR""
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
                                    ""valueString"": ""Yes""
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
    }
}
