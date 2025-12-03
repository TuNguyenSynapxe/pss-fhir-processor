using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Extraction;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Utilities;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor
{
    /// <summary>
    /// Main facade for PSS FHIR Processor - coordinates validation and extraction
    /// </summary>
    public class FhirProcessor
    {
        private readonly ValidationEngine _validationEngine;
        private readonly ExtractionEngine _extractionEngine;
        private ValidationOptions _validationOptions;
        private LoggingOptions _loggingOptions;

        public FhirProcessor()
        {
            _validationEngine = new ValidationEngine();
            _extractionEngine = new ExtractionEngine();
            _validationOptions = new ValidationOptions();
            _loggingOptions = new LoggingOptions();
        }

        /// <summary>
        /// Load validation RuleSets from JSON strings keyed by scope
        /// </summary>
        public void LoadRuleSets(Dictionary<string, string> jsonByScope)
        {
            _validationEngine.LoadRuleSets(jsonByScope);
        }

        /// <summary>
        /// Load Codes Master metadata from JSON
        /// </summary>
        public void LoadCodesMaster(string json)
        {
            _validationEngine.LoadCodesMaster(json);
        }

        /// <summary>
        /// Set validation options
        /// </summary>
        public void SetValidationOptions(ValidationOptions options)
        {
            _validationOptions = options ?? new ValidationOptions();
            _validationEngine.SetOptions(_validationOptions);
        }

        /// <summary>
        /// Set logging options
        /// </summary>
        public void SetLoggingOptions(LoggingOptions options)
        {
            _loggingOptions = options ?? new LoggingOptions();
        }

        /// <summary>
        /// Process FHIR JSON: validate and extract if valid
        /// </summary>
        public ProcessResult Process(string fhirJson)
        {
            var logger = new Logger(_loggingOptions.LogLevel);
            var result = new ProcessResult();

            logger.Info("========================================");
            logger.Info("FHIR Processing Started");
            logger.Info("========================================");
            logger.Info($"Timestamp: {System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            logger.Info($"Log Level: {_loggingOptions.LogLevel}");
            logger.Info($"Input JSON length: {fhirJson?.Length ?? 0} characters");

            // Deserialize
            logger.Info("\n--- STEP 1: Deserializing FHIR Bundle ---");
            Bundle bundle = null;
            try
            {
                bundle = JsonHelper.Deserialize<Bundle>(fhirJson);
                logger.Info("✓ Bundle deserialized successfully");
                logger.Info($"  Resource Type: {bundle?.ResourceType}");
                logger.Info($"  Entry Count: {bundle?.Entry?.Count ?? 0}");
            }
            catch (System.Exception ex)
            {
                logger.Error($"Deserialization failed: {ex.Message}");
                result.Validation = new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Code = "INVALID_JSON",
                            FieldPath = "Bundle",
                            Message = $"Failed to deserialize JSON: {ex.Message}",
                            Scope = "Bundle"
                        }
                    }
                };
                result.Logs = logger.GetLogs();
                return result;
            }

            // Validate
            logger.Info("\n--- STEP 2: Validation Phase ---");
            _validationEngine.SetLogger(logger);
            result.Validation = _validationEngine.Validate(bundle);
            logger.Info($"Validation Result: {(result.Validation.IsValid ? "✓ VALID" : "✗ INVALID")}");
            logger.Info($"Total Errors: {result.Validation.Errors.Count}");
            if (result.Validation.Errors.Count > 0)
            {
                logger.Info("Validation Errors:");
                for (int i = 0; i < result.Validation.Errors.Count && i < 10; i++)
                {
                    var err = result.Validation.Errors[i];
                    logger.Info($"  {i+1}. [{err.Code}] {err.FieldPath}: {err.Message}");
                }
                if (result.Validation.Errors.Count > 10)
                {
                    logger.Info($"  ... and {result.Validation.Errors.Count - 10} more errors");
                }
            }

            // Always extract (even if validation fails)
            logger.Info("\n--- STEP 3: Extraction Phase ---");
            _extractionEngine.SetLogger(logger);
            result.Flatten = _extractionEngine.Extract(bundle);
            logger.Info($"Extraction Result: {(result.Flatten != null ? "✓ SUCCESS" : "✗ FAILED")}");
            if (result.Flatten != null)
            {
                logger.Info($"  Event Data: {(result.Flatten.Event != null ? "✓" : "✗")}");
                logger.Info($"  Participant Data: {(result.Flatten.Participant != null ? "✓" : "✗")}");
                logger.Info($"  Hearing Screening: {result.Flatten.HearingRaw?.Items?.Count ?? 0} items");
                logger.Info($"  Oral Screening: {result.Flatten.OralRaw?.Items?.Count ?? 0} items");
                logger.Info($"  Vision Screening: {result.Flatten.VisionRaw?.Items?.Count ?? 0} items");
            }
            
            // Summary
            logger.Info("\n--- STEP 4: Processing Summary ---");
            if (!result.Validation.IsValid)
            {
                logger.Info($"⚠ Validation failed with {result.Validation.Errors.Count} error(s), but extraction completed");
            }
            else
            {
                logger.Info("✓ Validation passed");
            }
            logger.Info($"Total log entries: {logger.GetLogs().Count}");
            logger.Info("========================================");
            logger.Info("FHIR Processing Completed");
            logger.Info("========================================\n");

            result.Logs = logger.GetLogs();

            return result;
        }

        /// <summary>
        /// Validate only (no extraction)
        /// </summary>
        public ValidationResult Validate(string fhirJson)
        {
            var logger = new Logger(_loggingOptions.LogLevel);

            Bundle bundle = null;
            try
            {
                bundle = JsonHelper.Deserialize<Bundle>(fhirJson);
            }
            catch (System.Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Code = "INVALID_JSON",
                            FieldPath = "Bundle",
                            Message = $"Failed to deserialize JSON: {ex.Message}",
                            Scope = "Bundle"
                        }
                    }
                };
            }

            _validationEngine.SetLogger(logger);
            return _validationEngine.Validate(bundle);
        }
    }
}
