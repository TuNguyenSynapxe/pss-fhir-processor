using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Common;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Flattened;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Utilities;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Extraction
{
    /// <summary>
    /// Core extraction engine that converts validated FHIR Bundle into FlattenResult
    /// </summary>
    public class ExtractionEngine
    {
        private Logger _logger;
        private static readonly Newtonsoft.Json.JsonSerializerSettings JsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
        };

        public void SetLogger(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Main extraction entry point - assumes bundle has been validated
        /// </summary>
        public FlattenResult Extract(Bundle bundle)
        {
            _logger?.Info("Extraction started");

            if (bundle == null || bundle.Entry == null)
            {
                _logger?.Error("✗ Cannot extract from null bundle");
                return null;
            }

            var result = new FlattenResult();

            // Index resources
            var index = IndexResources(bundle);

            // Extract Event
            _logger?.Info("  → Extracting Event data...");
            result.Event = ExtractEvent(index);
            _logger?.Debug($"    Event extracted: {(result.Event != null ? "✓" : "✗")}");

            // Extract Participant
            _logger?.Info("  → Extracting Participant data...");
            result.Participant = ExtractParticipant(index);
            _logger?.Debug($"    Participant extracted: {(result.Participant != null ? "✓" : "✗")}");

            // Extract Screenings
            _logger?.Info("  → Extracting Screening data...");
            result.HearingRaw = ExtractScreening(index, "HS");
            _logger?.Debug($"    Hearing screening: {result.HearingRaw?.Items?.Count ?? 0} items");
            result.OralRaw = ExtractScreening(index, "OS");
            _logger?.Debug($"    Oral screening: {result.OralRaw?.Items?.Count ?? 0} items");
            result.VisionRaw = ExtractScreening(index, "VS");
            _logger?.Debug($"    Vision screening: {result.VisionRaw?.Items?.Count ?? 0} items");

            _logger?.Info("✓ Extraction completed successfully");

            return result;
        }

        private Dictionary<string, List<Resource>> IndexResources(Bundle bundle)
        {
            _logger?.Info("  → Indexing resources for extraction...");
            var index = new Dictionary<string, List<Resource>>();
            int entryIndex = 0;

            foreach (var entry in bundle.Entry)
            {
                entryIndex++;
                if (entry.Resource == null)
                {
                    _logger?.Debug($"    Entry[{entryIndex-1}]: null resource, skipping");
                    continue;
                }

                var resourceType = entry.Resource.ResourceType;

                // Special handling for Observation
                if (resourceType == "Observation")
                {
                    // Need to deserialize to get the Code property
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(entry.Resource, JsonSettings);
                    var obs = Newtonsoft.Json.JsonConvert.DeserializeObject<Observation>(json, JsonSettings);
                    var screeningType = obs?.Code?.Coding?.FirstOrDefault()?.Code;
                    if (!string.IsNullOrEmpty(screeningType))
                    {
                        var key = $"Observation:{screeningType}";
                        if (!index.ContainsKey(key))
                            index[key] = new List<Resource>();
                        index[key].Add(entry.Resource);
                    }
                }

                if (!index.ContainsKey(resourceType))
                    index[resourceType] = new List<Resource>();

                index[resourceType].Add(entry.Resource);
            }

            return index;
        }

        private EventData ExtractEvent(Dictionary<string, List<Resource>> index)
        {
            var eventData = new EventData();

            // Extract from Encounter
            if (index.ContainsKey("Encounter"))
            {
                var resource = index["Encounter"].FirstOrDefault();
                if (resource != null)
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(resource, JsonSettings);
                    var encounter = Newtonsoft.Json.JsonConvert.DeserializeObject<Encounter>(json, JsonSettings);
                    if (encounter != null)
                    {
                        eventData.Start = encounter.ActualPeriod?.Start;
                        eventData.End = encounter.ActualPeriod?.End;
                    }
                }
            }

            // Extract from Location
            if (index.ContainsKey("Location"))
            {
                var resource = index["Location"].FirstOrDefault();
                if (resource != null)
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(resource, JsonSettings);
                    var location = Newtonsoft.Json.JsonConvert.DeserializeObject<Location>(json, JsonSettings);
                    if (location != null)
                    {
                        // Use Name if available, otherwise use first line of address
                        eventData.VenueName = location.Name ?? location.Address?.Line?.FirstOrDefault();
                        eventData.PostalCode = location.Address?.PostalCode;

                        // Extract GRC and Constituency from extensions
                        if (location.Extension != null)
                        {
                            var grcExt = location.Extension.FirstOrDefault(e => e.Url?.Contains("grc") == true);
                            if (grcExt != null)
                            {
                                // Try ValueString first, then ValueCodeableConcept
                                eventData.Grc = grcExt.ValueString ?? 
                                                grcExt.ValueCodeableConcept?.Coding?.FirstOrDefault()?.Code;
                            }

                            var constExt = location.Extension.FirstOrDefault(e => e.Url?.Contains("constituency") == true);
                            if (constExt != null)
                            {
                                // Try ValueString first, then ValueCodeableConcept
                                eventData.Constituency = constExt.ValueString ?? 
                                                        constExt.ValueCodeableConcept?.Coding?.FirstOrDefault()?.Code;
                            }
                        }
                    }
                }
            }

            // Extract Organizations (Provider and Cluster)
            if (index.ContainsKey("Organization"))
            {
                foreach (var resource in index["Organization"])
                {
                    // Try to deserialize as Organization
                    var orgJson = Newtonsoft.Json.JsonConvert.SerializeObject(resource, JsonSettings);
                    var org = Newtonsoft.Json.JsonConvert.DeserializeObject<Organization>(orgJson, JsonSettings);
                    
                    if (org == null) continue;
                    
                    var orgType = org.Type?.FirstOrDefault()?.Coding?.FirstOrDefault()?.Code;

                    if (orgType == "prov" || orgType == "provider")
                    {
                        eventData.ProviderName = org.Name;
                    }
                    else if (orgType == "cluster")
                    {
                        eventData.ClusterName = org.Name;
                    }
                    else if (string.IsNullOrEmpty(eventData.ProviderName))
                    {
                        // Fallback: first org is provider
                        eventData.ProviderName = org.Name;
                    }
                    else if (string.IsNullOrEmpty(eventData.ClusterName))
                    {
                        // Fallback: second org is cluster
                        eventData.ClusterName = org.Name;
                    }
                }
            }

            return eventData;
        }

        private ParticipantData ExtractParticipant(Dictionary<string, List<Resource>> index)
        {
            var participantData = new ParticipantData();

            if (index.ContainsKey("Patient"))
            {
                var resource = index["Patient"].FirstOrDefault();
                if (resource != null)
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(resource, JsonSettings);
                    var patient = Newtonsoft.Json.JsonConvert.DeserializeObject<Patient>(json, JsonSettings);
                    if (patient != null)
                    {
                        // NRIC
                        var nricIdentifier = patient.Identifier?.FirstOrDefault(id => id.System?.Contains("nric") == true);
                        if (nricIdentifier != null)
                            participantData.Nric = nricIdentifier.Value;

                        // Name
                        participantData.Name = patient.Name?.FirstOrDefault()?.Text;

                        // Gender
                        participantData.Gender = patient.Gender;

                        // BirthDate
                        participantData.BirthDate = patient.BirthDate;

                        // Address
                        if (patient.Address != null && patient.Address.Count > 0)
                        {
                            var address = patient.Address.FirstOrDefault();
                            if (address?.Line != null && address.Line.Count > 0)
                            {
                                participantData.Address = string.Join(", ", address.Line);
                            }
                            if (!string.IsNullOrEmpty(address?.PostalCode))
                            {
                                participantData.Address = string.IsNullOrEmpty(participantData.Address)
                                    ? address.PostalCode
                                    : $"{participantData.Address}, {address.PostalCode}";
                            }
                        }
                    }
                }
            }

            return participantData;
        }

        private ScreeningSet ExtractScreening(Dictionary<string, List<Resource>> index, string screeningType)
        {
            var screeningSet = new ScreeningSet
            {
                ScreeningType = screeningType,
                Items = new List<ObservationItem>()
            };

            var key = $"Observation:{screeningType}";
            if (!index.ContainsKey(key))
                return screeningSet;

            var resource = index[key].FirstOrDefault();
            if (resource != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(resource, JsonSettings);
                var observation = Newtonsoft.Json.JsonConvert.DeserializeObject<Observation>(json, JsonSettings);
                
                if (observation?.Component != null)
                {
                    foreach (var component in observation.Component)
                    {
                        var item = ExtractObservationItem(component);
                        if (item != null)
                            screeningSet.Items.Add(item);
                    }
                }
            }

            return screeningSet;
        }

        private ObservationItem ExtractObservationItem(ObservationComponent component)
        {
            if (component == null)
                return null;

            var item = new ObservationItem
            {
                Question = new CodeDisplayValue(),
                Values = new List<string>()
            };

            // Extract question code and display
            var coding = component.Code?.Coding?.FirstOrDefault();
            if (coding != null)
            {
                item.Question.Code = coding.Code;
                item.Question.Display = coding.Display;
            }

            // Extract answer(s)
            if (!string.IsNullOrEmpty(component.ValueString))
            {
                // Check for multi-value (pipe-separated)
                if (component.ValueString.Contains("|"))
                {
                    var values = component.ValueString.Split('|');
                    foreach (var val in values)
                    {
                        var trimmed = val.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                            item.Values.Add(trimmed);
                    }
                }
                else
                {
                    item.Values.Add(component.ValueString);
                }
            }

            return item;
        }
    }
}
