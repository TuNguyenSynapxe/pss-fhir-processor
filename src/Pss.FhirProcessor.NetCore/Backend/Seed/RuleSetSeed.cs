using System.Collections.Generic;

namespace MOH.HealthierSG.PSS.FhirProcessor.Api.Seed
{
    public static class RuleSetSeed
    {
        public static Dictionary<string, string> GetRuleSets()
        {
            return new Dictionary<string, string>
            {
                { "Event", GetEventRules() },
                { "Participant", GetParticipantRules() },
                { "HS", GetHsRules() },
                { "OS", GetOsRules() },
                { "VS", GetVsRules() }
            };
        }

        private static string GetEventRules()
        {
            return @"{
  ""Scope"": ""Event"",
  ""ResourceType"": ""Encounter"",
  ""Rules"": [
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Encounter.actualPeriod.start"",
      ""ErrorCode"": ""MANDATORY_MISSING"",
      ""Message"": ""SCREENING_DATE is mandatory.""
    }
  ]
}";
        }

        private static string GetParticipantRules()
        {
            return @"{
  ""Scope"": ""Participant"",
  ""ResourceType"": ""Patient"",
  ""Rules"": [
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Patient.identifier.value"",
      ""ErrorCode"": ""MANDATORY_MISSING"",
      ""Message"": ""NRIC is mandatory.""
    },
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Patient.name.text"",
      ""ErrorCode"": ""MANDATORY_MISSING"",
      ""Message"": ""NAME is mandatory.""
    },
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Patient.gender"",
      ""ErrorCode"": ""MANDATORY_MISSING"",
      ""Message"": ""GENDER is mandatory.""
    },
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Patient.birthDate"",
      ""ErrorCode"": ""MANDATORY_MISSING"",
      ""Message"": ""DATE_OF_BIRTH is mandatory.""
    },
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Patient.address.line"",
      ""ErrorCode"": ""MANDATORY_MISSING"",
      ""Message"": ""ADDRESS_BLOCK_NUMBER, ADDRESS_STREET, ADDRESS_FLOOR, ADDRESS_UNIT_NUMBER is mandatory.""
    },
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Patient.address.postalCode"",
      ""ErrorCode"": ""MANDATORY_MISSING"",
      ""Message"": ""ADDRESS_POSTAL_CODE is mandatory.""
    },
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Patient.extension.valueBoolean"",
      ""ErrorCode"": ""MANDATORY_MISSING"",
      ""Message"": ""CONSENT_GIVEN_BY_SENIOR is mandatory.""
    }
  ]
}";
        }

        private static string GetHsRules()
        {
            return @"{
  ""Scope"": ""HS"",
  ""ResourceType"": ""Observation:HS"",
  ""Rules"": [
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Observation.component[]"",
      ""ErrorCode"": ""MANDATORY_MISSING"",
      ""Message"": ""HS Observation must have at least one component.""
    },
    {
      ""RuleType"": ""CodesMaster"",
      ""Path"": ""Observation.component"",
      ""ErrorCode"": ""INVALID_ANSWER_VALUE"",
      ""Message"": ""HS Observation components must conform to Codes Master.""
    },
    {
      ""RuleType"": ""FixedCoding"",
      ""Path"": ""Observation.code.coding"",
      ""ExpectedSystem"": ""https://fhir.synapxe.sg/CodeSystem/screening-type"",
      ""ExpectedCode"": ""HS"",
      ""ErrorCode"": ""FIXED_CODING_MISMATCH"",
      ""Message"": ""Observation.code.coding must use screening-type 'HS'.""
    }
  ]
}";
        }

        private static string GetOsRules()
        {
            return @"{
  ""Scope"": ""OS"",
  ""ResourceType"": ""Observation:OS"",
  ""Rules"": [
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Observation.component[]"",
      ""ErrorCode"": ""MANDATORY_MISSING"",
      ""Message"": ""OS Observation must have at least one component.""
    },
    {
      ""RuleType"": ""CodesMaster"",
      ""Path"": ""Observation.component"",
      ""ErrorCode"": ""INVALID_ANSWER_VALUE"",
      ""Message"": ""OS Observation components must conform to Codes Master.""
    },
    {
      ""RuleType"": ""FixedCoding"",
      ""Path"": ""Observation.code.coding"",
      ""ExpectedSystem"": ""https://fhir.synapxe.sg/CodeSystem/screening-type"",
      ""ExpectedCode"": ""OS"",
      ""ErrorCode"": ""FIXED_CODING_MISMATCH"",
      ""Message"": ""Observation.code.coding must use screening-type 'OS'.""
    }
  ]
}";
        }

        private static string GetVsRules()
        {
            return @"{
  ""Scope"": ""VS"",
  ""ResourceType"": ""Observation:VS"",
  ""Rules"": [
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Observation.component[]"",
      ""ErrorCode"": ""MANDATORY_MISSING"",
      ""Message"": ""VS Observation must have at least one component.""
    },
    {
      ""RuleType"": ""CodesMaster"",
      ""Path"": ""Observation.component"",
      ""ErrorCode"": ""INVALID_ANSWER_VALUE"",
      ""Message"": ""VS Observation components must conform to Codes Master.""
    },
    {
      ""RuleType"": ""FixedCoding"",
      ""Path"": ""Observation.code.coding"",
      ""ExpectedSystem"": ""https://fhir.synapxe.sg/CodeSystem/screening-type"",
      ""ExpectedCode"": ""VS"",
      ""ErrorCode"": ""FIXED_CODING_MISMATCH"",
      ""Message"": ""Observation.code.coding must use screening-type 'VS'.""
    }
  ]
}";
        }
    }
}
