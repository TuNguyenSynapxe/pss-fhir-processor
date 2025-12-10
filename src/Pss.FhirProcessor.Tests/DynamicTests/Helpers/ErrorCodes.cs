namespace MOH.HealthierSG.PSS.FhirProcessor.Tests.DynamicTests.Helpers
{
    /// <summary>
    /// Central registry of all validation error codes used in metadata.
    /// TODO: Sync these with your actual validation-metadata-v10.json error codes
    /// </summary>
    public static class ErrorCodes
    {
        // Resource existence errors
        public const string MISSING_PATIENT = "MISSING_PATIENT";
        public const string MISSING_ENCOUNTER = "MISSING_ENCOUNTER";
        public const string MISSING_LOCATION = "MISSING_LOCATION";
        public const string MISSING_PROVIDER_ORG = "MISSING_PROVIDER_ORG";
        public const string MISSING_CLUSTER_ORG = "MISSING_CLUSTER_ORG";
        public const string MISSING_HEALTHCARESERVICE = "MISSING_HEALTHCARESERVICE";
        public const string MISSING_SCREENING_TYPE = "MISSING_SCREENING_TYPE";

        // Type validation errors
        public const string TYPE_MISMATCH = "TYPE_MISMATCH";
        public const string TYPE_INVALID_FULLURL = "TYPE_INVALID_FULLURL";

        // Fixed value/coding errors
        public const string FIXED_VALUE_MISMATCH = "FIXED_VALUE_MISMATCH";

        // CodesMaster errors
        public const string INVALID_ANSWER_VALUE = "INVALID_ANSWER_VALUE";

        // Reference validation errors
        public const string REFERENCE_INVALID = "REFERENCE_INVALID";

        // Required field errors
        public const string MANDATORY_MISSING = "MANDATORY_MISSING";
        public const string REQUIRED_FIELD_MISSING = "REQUIRED_FIELD_MISSING";

        // FullUrl/ID matching
        public const string ID_FULLURL_MISMATCH = "ID_FULLURL_MISMATCH";
    }
}
