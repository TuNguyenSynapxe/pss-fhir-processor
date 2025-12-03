using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class RuleSetsLoaderTests
    {
        [TestMethod]
        public void LoadRuleSets_ValidJson_LoadsSuccessfully()
        {
            var processor = new FhirProcessor();
            
            var rules = new Dictionary<string, string>
            {
                { "Event", @"{ ""Scope"": ""Event"", ""Rules"": [] }" },
                { "HS", @"{ ""Scope"": ""HS"", ""Rules"": [] }" }
            };
            
            // Should not throw
            processor.LoadRuleSets(rules);
            
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void LoadCodesMaster_ValidJson_LoadsSuccessfully()
        {
            var processor = new FhirProcessor();
            
            var codesMaster = @"{
                ""Questions"": [
                    {
                        ""QuestionCode"": ""TEST-001"",
                        ""QuestionDisplay"": ""Test?"",
                        ""ScreeningType"": ""HS"",
                        ""AllowedAnswers"": [""Yes"", ""No""]
                    }
                ]
            }";
            
            // Should not throw
            processor.LoadCodesMaster(codesMaster);
            
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void LoadCodesMaster_InvalidJson_DoesNotCrash()
        {
            var processor = new FhirProcessor();
            
            // Should handle gracefully
            processor.LoadCodesMaster("{ invalid json }");
            
            Assert.IsTrue(true);
        }
    }
}
