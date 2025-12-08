using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation
{
    /// <summary>
    /// Individual validation error with rich metadata for helper system
    /// </summary>
    public class ValidationError
    {
        public string Code { get; set; }
        public string FieldPath { get; set; }
        public string Message { get; set; }
        public string Scope { get; set; }
        public string Severity { get; set; } = "error";
        
        // Rich metadata for helper system
        public string RuleType { get; set; }
        public ValidationRuleMetadata Rule { get; set; }
        public ValidationErrorContext Context { get; set; }
        public ResourcePointer ResourcePointer { get; set; }
        public PathAnalysis PathAnalysis { get; set; }

        public ValidationError()
        {
            Rule = new ValidationRuleMetadata();
            Context = new ValidationErrorContext();
            ResourcePointer = new ResourcePointer();
            PathAnalysis = new PathAnalysis();
        }

        public override string ToString()
        {
            return $"[{Code}] {FieldPath}: {Message}" + (Scope != null ? $" (Scope: {Scope})" : "");
        }
    }
    
    /// <summary>
    /// Rule metadata extracted from validation-metadata.json
    /// </summary>
    public class ValidationRuleMetadata
    {
        public string Path { get; set; }
        public string ExpectedType { get; set; }
        public string ExpectedValue { get; set; }
        public string Pattern { get; set; }
        public List<string> TargetTypes { get; set; }
        public string System { get; set; }
        public List<string> AllowedValues { get; set; }
    }
    
    /// <summary>
    /// Context information for error helper rendering
    /// </summary>
    public class ValidationErrorContext
    {
        public string ResourceType { get; set; }
        public string ScreeningType { get; set; }
        public string QuestionCode { get; set; }
        public string QuestionDisplay { get; set; }
        public List<string> AllowedAnswers { get; set; }
        public List<CodeSystemConcept> CodeSystemConcepts { get; set; }
    }
    
    /// <summary>
    /// CodeSystem concept for display mapping
    /// </summary>
    public class CodeSystemConcept
    {
        public string Code { get; set; }
        public string Display { get; set; }
    }
    
    /// <summary>
    /// Resource pointer for navigation in the bundle
    /// </summary>
    public class ResourcePointer
    {
        public int? EntryIndex { get; set; }
        public string FullUrl { get; set; }
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
    }
    
    /// <summary>
    /// Path analysis for determining fix scenarios
    /// </summary>
    public class PathAnalysis
    {
        /// <summary>
        /// Whether all parent path segments exist in the resource
        /// </summary>
        public bool ParentPathExists { get; set; } = true;
        
        /// <summary>
        /// The path segment that couldn't be found (if any)
        /// </summary>
        public string PathMismatchSegment { get; set; }
        
        /// <summary>
        /// The depth at which the mismatch occurred (0-based)
        /// </summary>
        public int? MismatchDepth { get; set; }
    }
}
