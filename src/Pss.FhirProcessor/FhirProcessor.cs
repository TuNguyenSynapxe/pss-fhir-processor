using System.Collections.Generic;
using System.Linq;
using MOH.HealthierSG.PSS.FhirProcessor.Extraction;
using MOH.HealthierSG.PSS.FhirProcessor.Models.Fhir;
using MOH.HealthierSG.PSS.FhirProcessor.Models.Flattened;
using MOH.HealthierSG.PSS.FhirProcessor.Models.Validation;
using MOH.HealthierSG.PSS.FhirProcessor.Utilities;
using MOH.HealthierSG.PSS.FhirProcessor.Validation;

namespace MOH.HealthierSG.PSS.FhirProcessor
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
            logger.Debug($"Input JSON length: {fhirJson?.Length ?? 0} characters");
            logger.Verbose($"Input JSON preview: {(fhirJson?.Length > 200 ? fhirJson.Substring(0, 200) + "..." : fhirJson)}");

            // Deserialize
            logger.Info("\n--- STEP 1: Deserializing FHIR Bundle ---");
            logger.Debug("Attempting to parse JSON into Bundle object...");
            Bundle bundle = null;
            try
            {
                bundle = JsonHelper.Deserialize<Bundle>(fhirJson);
                logger.Info("✓ Bundle deserialized successfully");
                logger.Debug($"  Resource Type: {bundle?.ResourceType}");
                logger.Debug($"  Entry Count: {bundle?.Entry?.Count ?? 0}");
                
                if (bundle?.Entry != null)
                {
                    logger.Verbose("  Entry breakdown:");
                    var resourceTypes = bundle.Entry
                        .Where(e => e.Resource?.ResourceType != null)
                        .GroupBy(e => e.Resource.ResourceType)
                        .ToDictionary(g => g.Key, g => g.Count());
                    
                    foreach (var rt in resourceTypes)
                    {
                        logger.Verbose($"    - {rt.Key}: {rt.Value}");
                    }
                }
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

            // Store original bundle
            result.OriginalBundle = bundle;

            // Validate
            logger.Info("\n--- STEP 2: Validation Phase ---");
            logger.Debug("Starting validation engine...");
            _validationEngine.SetLogger(logger);
            result.Validation = _validationEngine.Validate(bundle);
            logger.Info($"Validation Result: {(result.Validation.IsValid ? "✓ VALID" : "✗ INVALID")}");
            logger.Info($"Total Errors: {result.Validation.Errors.Count}");
            
            if (result.Validation.Errors.Count > 0)
            {
                logger.Debug("Validation Errors:");
                int displayCount = result.Validation.Errors.Count > 10 ? 10 : result.Validation.Errors.Count;
                
                for (int i = 0; i < displayCount; i++)
                {
                    var err = result.Validation.Errors[i];
                    logger.Debug($"  {i+1}. [{err.Code}] {err.FieldPath}: {err.Message}");
                    logger.Verbose($"      Scope: {err.Scope}");
                }
                
                if (result.Validation.Errors.Count > 10)
                {
                    logger.Debug($"  ... and {result.Validation.Errors.Count - 10} more errors");
                }
            }

            // Always extract (even if validation fails)
            logger.Info("\n--- STEP 3: Extraction Phase ---");
            logger.Debug("Starting extraction engine...");
            _extractionEngine.SetLogger(logger);
            result.Flatten = _extractionEngine.Extract(bundle);
            logger.Info($"Extraction Result: {(result.Flatten != null ? "✓ SUCCESS" : "✗ FAILED")}");
            
            if (result.Flatten != null)
            {
                logger.Debug($"  Event Data: {(result.Flatten.Event != null ? "✓" : "✗")}");
                logger.Debug($"  Participant Data: {(result.Flatten.Participant != null ? "✓" : "✗")}");
                logger.Debug($"  Hearing Screening: {result.Flatten.HearingRaw?.Items?.Count ?? 0} items");
                logger.Debug($"  Oral Screening: {result.Flatten.OralRaw?.Items?.Count ?? 0} items");
                logger.Debug($"  Vision Screening: {result.Flatten.VisionRaw?.Items?.Count ?? 0} items");
                
                logger.Verbose("Extraction details:");
                if (result.Flatten.Event != null)
                {
                    logger.Verbose($"  Event Start: {result.Flatten.Event.Start}");
                    logger.Verbose($"  Event End: {result.Flatten.Event.End}");
                    logger.Verbose($"  Venue: {result.Flatten.Event.VenueName}");
                    logger.Verbose($"  Provider: {result.Flatten.Event.ProviderName}");
                }
                if (result.Flatten.Participant != null)
                {
                    logger.Verbose($"  Participant NRIC: {result.Flatten.Participant.Nric}");
                    logger.Verbose($"  Participant Name: {result.Flatten.Participant.Name}");
                }
            }
            
            // Summary
            logger.Info("\n--- STEP 4: Processing Summary ---");
            if (!result.Validation.IsValid)
            {
                logger.Warn($"⚠ Validation failed with {result.Validation.Errors.Count} error(s), but extraction completed");
            }
            else
            {
                logger.Info("✓ Validation passed");
            }
            logger.Debug($"Total log entries: {logger.GetLogs().Count}");
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

        /// <summary>
        /// Extract only (no validation)
        /// </summary>
        public FlattenResult Extract(string fhirJson)
        {
            var logger = new Logger(_loggingOptions.LogLevel);

            Bundle bundle = null;
            try
            {
                bundle = JsonHelper.Deserialize<Bundle>(fhirJson);
            }
            catch (System.Exception ex)
            {
                logger.Error($"Deserialization failed: {ex.Message}");
                return null;
            }

            _extractionEngine.SetLogger(logger);
            return _extractionEngine.Extract(bundle);
        }
    }
}
