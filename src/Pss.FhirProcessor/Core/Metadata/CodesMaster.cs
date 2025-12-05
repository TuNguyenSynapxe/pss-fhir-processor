using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata
{
    /// <summary>
    /// Master data for screening questions and code systems
    /// </summary>
    public class CodesMaster
    {
        public List<CodesMasterQuestion> Questions { get; set; }
        public List<CodesMasterCodeSystem> CodeSystems { get; set; }

        public CodesMaster()
        {
            Questions = new List<CodesMasterQuestion>();
            CodeSystems = new List<CodesMasterCodeSystem>();
        }
    }

    /// <summary>
    /// Screening questionnaire definition with allowed answers
    /// </summary>
    public class CodesMasterQuestion
    {
        public string QuestionCode { get; set; }
        public string QuestionDisplay { get; set; }
        public string ScreeningType { get; set; } // HS / OS / VS
        public List<string> AllowedAnswers { get; set; }
        public bool IsMultiValue { get; set; }

        public CodesMasterQuestion()
        {
            AllowedAnswers = new List<string>();
        }
    }

    /// <summary>
    /// Code system definition with concepts
    /// </summary>
    public class CodesMasterCodeSystem
    {
        public string Id { get; set; }
        public string System { get; set; }
        public string Description { get; set; }
        public List<CodesMasterConcept> Concepts { get; set; }

        public CodesMasterCodeSystem()
        {
            Concepts = new List<CodesMasterConcept>();
        }
    }

    /// <summary>
    /// Individual code concept within a code system
    /// </summary>
    public class CodesMasterConcept
    {
        public string Code { get; set; }
        public string Display { get; set; }
    }
}
