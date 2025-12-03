using System.Collections.Generic;
using System.IO;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.WebApp.Seed
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
  ""Rules"": [
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Entry[0].Resource.ActualPeriod.Start""
    },
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Entry[0].Resource.ActualPeriod.End""
    }
  ]
}";
        }

        private static string GetParticipantRules()
        {
            return @"{
  ""Scope"": ""Participant"",
  ""Rules"": [
    {
      ""RuleType"": ""Required"",
      ""Path"": ""Entry[].Resource.Identifier[0].Value""
    }
  ]
}";
        }

        private static string GetHsRules()
        {
            return @"{
  ""Scope"": ""HS"",
  ""Rules"": [
    {
      ""RuleType"": ""CodesMaster"",
      ""Path"": ""Entry[].Resource.Component""
    }
  ]
}";
        }

        private static string GetOsRules()
        {
            return @"{
  ""Scope"": ""OS"",
  ""Rules"": [
    {
      ""RuleType"": ""CodesMaster"",
      ""Path"": ""Entry[].Resource.Component""
    }
  ]
}";
        }

        private static string GetVsRules()
        {
            return @"{
  ""Scope"": ""VS"",
  ""Rules"": [
    {
      ""RuleType"": ""CodesMaster"",
      ""Path"": ""Entry[].Resource.Component""
    }
  ]
}";
        }
    }
}
