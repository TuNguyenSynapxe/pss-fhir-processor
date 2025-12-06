using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests.Helpers;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests.Models;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests
{
    /// <summary>
    /// Registry of all mutation templates for generating negative test cases
    /// </summary>
    public static class MutationTemplates
    {
        /// <summary>
        /// Get all mutation templates
        /// Add new templates here to automatically include them in the test suite
        /// </summary>
        public static List<MutationTemplate> GetAll()
        {
            return new List<MutationTemplate>
            {
                // ========== MISSING RESOURCE TESTS ==========
                new MutationTemplate
                {
                    Name = "MissingPatient",
                    Apply = bundle => JsonMutationHelpers.RemoveEntryByResourceType(bundle, "Patient"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.MISSING_PATIENT }
                },

                new MutationTemplate
                {
                    Name = "MissingEncounter",
                    Apply = bundle => JsonMutationHelpers.RemoveEntryByResourceType(bundle, "Encounter"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.MISSING_ENCOUNTER }
                },

                new MutationTemplate
                {
                    Name = "MissingLocation",
                    Apply = bundle => JsonMutationHelpers.RemoveEntryByResourceType(bundle, "Location"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.MISSING_LOCATION }
                },

                new MutationTemplate
                {
                    Name = "MissingHealthcareService",
                    Apply = bundle => JsonMutationHelpers.RemoveEntryByResourceType(bundle, "HealthcareService"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.MISSING_HEALTHCARESERVICE }
                },

                new MutationTemplate
                {
                    Name = "MissingProviderOrganization",
                    Apply = bundle => JsonMutationHelpers.RemoveEntryByProperty(
                        bundle, 
                        "resource.type[0].coding[0].code", 
                        "PROVIDER"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.MISSING_PROVIDER_ORG }
                },

                new MutationTemplate
                {
                    Name = "MissingClusterOrganization",
                    Apply = bundle => JsonMutationHelpers.RemoveEntryByProperty(
                        bundle, 
                        "resource.type[0].coding[0].code", 
                        "CLUSTER"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.MISSING_CLUSTER_ORG }
                },

                // ========== MISSING SCREENING TYPE TESTS ==========
                new MutationTemplate
                {
                    Name = "MissingHearingScreening",
                    Apply = bundle => JsonMutationHelpers.RemoveEntryByProperty(
                        bundle,
                        "resource.code.coding[0].code",
                        "HS"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.MISSING_SCREENING_TYPE }
                },

                new MutationTemplate
                {
                    Name = "MissingOralScreening",
                    Apply = bundle => JsonMutationHelpers.RemoveEntryByProperty(
                        bundle,
                        "resource.code.coding[0].code",
                        "OS"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.MISSING_SCREENING_TYPE }
                },

                new MutationTemplate
                {
                    Name = "MissingVisionScreening",
                    Apply = bundle => JsonMutationHelpers.RemoveEntryByProperty(
                        bundle,
                        "resource.code.coding[0].code",
                        "VS"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.MISSING_SCREENING_TYPE }
                },

                // ========== INVALID FULLURL / GUID TESTS ==========
                new MutationTemplate
                {
                    Name = "InvalidFullUrlFormat",
                    Apply = bundle => JsonMutationHelpers.ReplaceString(
                        bundle,
                        "entry[0].fullUrl",
                        "not-a-valid-urn-uuid"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.TYPE_INVALID_FULLURL }
                },

                new MutationTemplate
                {
                    Name = "InvalidPatientIdGuid",
                    Apply = bundle => JsonMutationHelpers.BreakGuid(
                        bundle,
                        "entry[?(@.resource.resourceType=='Patient')].resource.id"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.TYPE_MISMATCH, ErrorCodes.ID_FULLURL_MISMATCH }
                },

                new MutationTemplate
                {
                    Name = "MismatchedFullUrlAndId",
                    Apply = bundle => {
                        var clone = JsonMutationHelpers.ReplaceString(
                            bundle,
                            "entry[0].resource.id",
                            "a1111111-1111-1111-1111-111111111111");
                        return clone;
                    },
                    ExpectedErrorCodes = new List<string> { ErrorCodes.ID_FULLURL_MISMATCH }
                },

                // ========== INVALID DATETIME / DATE TESTS ==========
                new MutationTemplate
                {
                    Name = "InvalidEncounterStartDateTime",
                    Apply = bundle => JsonMutationHelpers.BreakDateTime(
                        bundle,
                        "entry[?(@.resource.resourceType=='Encounter')].resource.actualPeriod.start"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.TYPE_MISMATCH }
                },

                new MutationTemplate
                {
                    Name = "InvalidPatientBirthDate",
                    Apply = bundle => JsonMutationHelpers.BreakDate(
                        bundle,
                        "entry[?(@.resource.resourceType=='Patient')].resource.birthDate"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.TYPE_MISMATCH }
                },

                // ========== INVALID SCREENING CODE TESTS ==========
                new MutationTemplate
                {
                    Name = "InvalidHearingScreeningCode",
                    Apply = bundle => {
                        var obs = JsonMutationHelpers.FindObservationByScreeningType(bundle, "HS");
                        if (obs != null)
                        {
                            return JsonMutationHelpers.ReplaceString(
                                bundle,
                                $"entry[{((Newtonsoft.Json.Linq.JArray)bundle["entry"]).IndexOf(obs)}].resource.code.coding[0].code",
                                "INVALID_SCREENING");
                        }
                        return bundle;
                    },
                    ExpectedErrorCodes = new List<string> { ErrorCodes.FIXED_VALUE_MISMATCH }
                },

                // ========== CODESMASTER VALIDATION TESTS ==========
                new MutationTemplate
                {
                    Name = "InvalidQuestionCode_HS",
                    Apply = bundle => JsonMutationHelpers.BreakObservationQuestionCode(
                        bundle,
                        "HS",
                        "SQ-INVALID-00000000"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.INVALID_ANSWER_VALUE }
                },

                new MutationTemplate
                {
                    Name = "InvalidQuestionDisplay_OS",
                    Apply = bundle => JsonMutationHelpers.BreakObservationQuestionDisplay(
                        bundle,
                        "OS",
                        "This is completely wrong display text"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.INVALID_ANSWER_VALUE }
                },

                new MutationTemplate
                {
                    Name = "InvalidAnswerValue_VS",
                    Apply = bundle => JsonMutationHelpers.BreakObservationAnswerValue(
                        bundle,
                        "VS",
                        "This answer is not in allowed list"),
                    ExpectedErrorCodes = new List<string> { ErrorCodes.INVALID_ANSWER_VALUE }
                },

                new MutationTemplate
                {
                    Name = "InvalidPureToneValue_HS",
                    Apply = bundle => {
                        // Find pure tone component (contains pipe-separated values)
                        var clone = (Newtonsoft.Json.Linq.JObject)bundle.DeepClone();
                        var obs = JsonMutationHelpers.FindObservationByScreeningType(clone, "HS");
                        if (obs != null)
                        {
                            // Find component with pipe-separated values (pure tone test)
                            var components = obs["resource"]?["component"];
                            if (components != null)
                            {
                                foreach (var component in components)
                                {
                                    var valueString = component["valueString"]?.ToString();
                                    if (valueString != null && valueString.Contains("|"))
                                    {
                                        // Add invalid value to pipe list
                                        component["valueString"] = valueString + "|INVALID_TONE_VALUE";
                                        break;
                                    }
                                }
                            }
                        }
                        return clone;
                    },
                    ExpectedErrorCodes = new List<string> { ErrorCodes.INVALID_ANSWER_VALUE }
                },

                // ========== REFERENCE VALIDATION TESTS ==========
                new MutationTemplate
                {
                    Name = "BrokenSubjectReference_HS",
                    Apply = bundle => {
                        var obs = JsonMutationHelpers.FindObservationByScreeningType(bundle, "HS");
                        if (obs != null)
                        {
                            var entries = (Newtonsoft.Json.Linq.JArray)bundle["entry"];
                            var index = entries.IndexOf(obs);
                            return JsonMutationHelpers.BreakReference(
                                bundle,
                                $"entry[{index}].resource.subject.reference");
                        }
                        return bundle;
                    },
                    ExpectedErrorCodes = new List<string> { ErrorCodes.REF_OBS_SUBJECT_INVALID }
                },

                new MutationTemplate
                {
                    Name = "BrokenEncounterReference_OS",
                    Apply = bundle => {
                        var obs = JsonMutationHelpers.FindObservationByScreeningType(bundle, "OS");
                        if (obs != null)
                        {
                            var entries = (Newtonsoft.Json.Linq.JArray)bundle["entry"];
                            var index = entries.IndexOf(obs);
                            return JsonMutationHelpers.BreakReference(
                                bundle,
                                $"entry[{index}].resource.encounter.reference");
                        }
                        return bundle;
                    },
                    ExpectedErrorCodes = new List<string> { ErrorCodes.REF_OBS_ENCOUNTER_INVALID }
                },

                new MutationTemplate
                {
                    Name = "BrokenPerformerReference_VS",
                    Apply = bundle => {
                        var obs = JsonMutationHelpers.FindObservationByScreeningType(bundle, "VS");
                        if (obs != null)
                        {
                            var entries = (Newtonsoft.Json.Linq.JArray)bundle["entry"];
                            var index = entries.IndexOf(obs);
                            return JsonMutationHelpers.BreakReference(
                                bundle,
                                $"entry[{index}].resource.performer.reference");
                        }
                        return bundle;
                    },
                    ExpectedErrorCodes = new List<string> { ErrorCodes.REF_OBS_PERFORMER_INVALID }
                },

                // ========== CODE SYSTEM VALIDATION TESTS ==========
                new MutationTemplate
                {
                    Name = "InvalidGRCCode",
                    Apply = bundle => {
                        var location = JsonMutationHelpers.FindFirstResourceByType(bundle, "Location");
                        if (location != null)
                        {
                            var entries = (Newtonsoft.Json.Linq.JArray)bundle["entry"];
                            var index = entries.IndexOf(location);
                            return JsonMutationHelpers.ReplaceString(
                                bundle,
                                $"entry[{index}].resource.extension[?(@.url=='https://fhir.synapxe.sg/StructureDefinition/ext-grc')].valueCodeableConcept.coding[0].code",
                                "ZZ");
                        }
                        return bundle;
                    },
                    ExpectedErrorCodes = new List<string> { ErrorCodes.FIXED_VALUE_MISMATCH }
                },

                new MutationTemplate
                {
                    Name = "InvalidConstituencyCode",
                    Apply = bundle => {
                        var location = JsonMutationHelpers.FindFirstResourceByType(bundle, "Location");
                        if (location != null)
                        {
                            var entries = (Newtonsoft.Json.Linq.JArray)bundle["entry"];
                            var index = entries.IndexOf(location);
                            return JsonMutationHelpers.ReplaceString(
                                bundle,
                                $"entry[{index}].resource.extension[?(@.url=='https://fhir.synapxe.sg/StructureDefinition/ext-constituency')].valueCodeableConcept.coding[0].code",
                                "ZZ");
                        }
                        return bundle;
                    },
                    ExpectedErrorCodes = new List<string> { ErrorCodes.FIXED_VALUE_MISMATCH }
                }

                // TODO: Add more mutation templates as needed
                // Easy to extend: just add new MutationTemplate { ... } to this list
            };
        }
    }
}
