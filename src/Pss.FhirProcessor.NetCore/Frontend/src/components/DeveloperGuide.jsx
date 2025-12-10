import React from 'react';
import { Typography, Card, Divider, Space, Tag } from 'antd';
import { CodeOutlined, CheckCircleOutlined, ExperimentOutlined, PlayCircleOutlined } from '@ant-design/icons';

const { Title, Paragraph, Text } = Typography;

export default function DeveloperGuide() {
  const csharpCode = {
    setup: `using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation;

// Initialize the validation engine
var validationEngine = new ValidationEngine();

// Load validation metadata from JSON string
// This includes RuleSets, CodesMaster, Version, and PathSyntax
string metadataJson = @"{
  ""Version"": ""5.0"",
  ""PathSyntax"": ""CPS1"",
  ""RuleSets"": [
    { ""Scope"": ""Bundle"", ""Rules"": [...] },
    { ""Scope"": ""Appointment"", ""Rules"": [...] },
    { ""Scope"": ""Patient"", ""Rules"": [...] },
    { ""Scope"": ""QuestionnaireResponse.hearing"", ""Rules"": [...] },
    { ""Scope"": ""QuestionnaireResponse.vision"", ""Rules"": [...] },
    { ""Scope"": ""QuestionnaireResponse.oral"", ""Rules"": [...] }
  ],
  ""CodesMaster"": {
    ""Systems"": [...],
    ""Questions"": [...]
  }
}";

validationEngine.LoadMetadataFromJson(metadataJson);

// Or load from file
// validationEngine.LoadMetadata("path/to/validation-metadata.json");

// Configure logging
var logger = new Logger("info"); // "verbose", "debug", "info", "warn", "error"`,

    process: `// ============================================
// OPTION 1: Process (Validate + Extract)
// ============================================
string fhirJson = File.ReadAllText("sample-bundle.json");

// Using ValidationEngine directly
var validationEngine = new ValidationEngine();
validationEngine.LoadMetadataFromJson(metadataJson);

ValidationResult validationResult = validationEngine.Validate(fhirJson, "info");

// Check validation
if (validationResult.IsValid)
{
    Console.WriteLine("✓ Validation passed!");
}
else
{
    Console.WriteLine($"✗ Validation failed with {validationResult.Errors.Count} errors:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"  [{error.Code}] {error.FieldPath}: {error.Message}");
    }
}

// Extract data using ExtractionEngine
var extractionEngine = new ExtractionEngine();
Bundle bundle = JsonHelper.Deserialize<Bundle>(fhirJson);
FlattenResult flatten = extractionEngine.Extract(bundle);

if (flatten != null)
{
    Console.WriteLine($"Event Start: {flatten.Event?.Start}");
    Console.WriteLine($"Participant: {flatten.Participant?.Name}");
    Console.WriteLine($"Hearing Screening: {flatten.HearingRaw?.Items?.Count ?? 0} items");
    Console.WriteLine($"Vision Screening: {flatten.VisionRaw?.Items?.Count ?? 0} items");
    Console.WriteLine($"Oral Screening: {flatten.OralRaw?.Items?.Count ?? 0} items");
}

// Review logs
foreach (var log in validationResult.Logs)
{
    Console.WriteLine(log);
}`,

    validate: `// ============================================
// OPTION 2: Validate Only
// ============================================
string fhirJson = File.ReadAllText("sample-bundle.json");

// Load metadata
var validationEngine = new ValidationEngine();
validationEngine.LoadMetadataFromJson(metadataJson);

// Validate
ValidationResult validation = validationEngine.Validate(fhirJson, "info");

if (validation.IsValid)
{
    Console.WriteLine("✓ FHIR Bundle is valid!");
}
else
{
    Console.WriteLine($"✗ Found {validation.Errors.Count} validation errors:\\n");
    
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"Error: {error.Code}");
        Console.WriteLine($"  Path: {error.FieldPath}");
        Console.WriteLine($"  Message: {error.Message}");
        Console.WriteLine($"  Scope: {error.Scope}");
        
        // Access detailed context
        if (error.Context != null)
        {
            Console.WriteLine($"  Resource: {error.Context.ResourceType}");
            Console.WriteLine($"  Question: {error.Context.QuestionCode} - {error.Context.QuestionDisplay}");
        }
        
        // Navigate to error location
        if (error.ResourcePointer != null)
        {
            Console.WriteLine($"  Entry: #{error.ResourcePointer.EntryIndex}");
            Console.WriteLine($"  Resource: {error.ResourcePointer.ResourceType}/{error.ResourcePointer.ResourceId}");
        }
        
        Console.WriteLine();
    }
}`,

    extract: `// ============================================
// OPTION 3: Extract Only
// ============================================
string fhirJson = File.ReadAllText("sample-bundle.json");

// Deserialize and extract
var extractionEngine = new ExtractionEngine();
Bundle bundle = JsonHelper.Deserialize<Bundle>(fhirJson);
FlattenResult flatten = extractionEngine.Extract(bundle);

if (flatten != null)
{
    // Event Data
    if (flatten.Event != null)
    {
        Console.WriteLine("=== Event Information ===");
        Console.WriteLine($"Start: {flatten.Event.Start}");
        Console.WriteLine($"End: {flatten.Event.End}");
        Console.WriteLine($"Venue: {flatten.Event.VenueName}");
        Console.WriteLine($"Provider: {flatten.Event.ProviderName}");
    }
    
    // Participant Data
    if (flatten.Participant != null)
    {
        Console.WriteLine("\\n=== Participant Information ===");
        Console.WriteLine($"NRIC: {flatten.Participant.Nric}");
        Console.WriteLine($"Name: {flatten.Participant.Name}");
        Console.WriteLine($"Birth Date: {flatten.Participant.BirthDate}");
        Console.WriteLine($"Gender: {flatten.Participant.Gender}");
    }
    
    // Hearing Screening
    if (flatten.HearingRaw?.Items?.Count > 0)
    {
        Console.WriteLine("\\n=== Hearing Screening ===");
        foreach (var item in flatten.HearingRaw.Items)
        {
            Console.WriteLine($"{item.QuestionCode}: {item.AnswerValueString}");
        }
    }
    
    // Vision Screening
    if (flatten.VisionRaw?.Items?.Count > 0)
    {
        Console.WriteLine("\\n=== Vision Screening ===");
        foreach (var item in flatten.VisionRaw.Items)
        {
            Console.WriteLine($"{item.QuestionCode}: {item.AnswerValueString}");
        }
    }
    
    // Oral Screening
    if (flatten.OralRaw?.Items?.Count > 0)
    {
        Console.WriteLine("\\n=== Oral Screening ===");
        foreach (var item in flatten.OralRaw.Items)
        {
            Console.WriteLine($"{item.QuestionCode}: {item.AnswerValueString}");
        }
    }
}
else
{
    Console.WriteLine("✗ Extraction failed - invalid JSON format");
}`,

    apiUsage: `// ============================================
// Web API Usage (ASP.NET Core)
// ============================================
using Microsoft.AspNetCore.Mvc;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;

[ApiController]
[Route("api/[controller]")]
public class FhirController : ControllerBase
{
    private readonly IFhirProcessorService _fhirService;
    
    public FhirController(IFhirProcessorService fhirService)
    {
        _fhirService = fhirService;
    }
    
    // POST /api/fhir/process
    [HttpPost("process")]
    public IActionResult Process([FromBody] ProcessRequest request)
    {
        try
        {
            var result = _fhirService.Process(
                request.FhirJson,
                request.ValidationMetadata,
                request.LogLevel ?? "info",
                request.StrictDisplay ?? true
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    // POST /api/fhir/validate
    [HttpPost("validate")]
    public IActionResult Validate([FromBody] ValidateRequest request)
    {
        try
        {
            var result = _fhirService.ValidateOnly(
                request.FhirJson,
                request.ValidationMetadata,
                request.LogLevel ?? "info",
                request.StrictDisplay ?? true
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    // POST /api/fhir/extract
    [HttpPost("extract")]
    public IActionResult Extract([FromBody] ExtractRequest request)
    {
        try
        {
            var result = _fhirService.ExtractOnly(
                request.FhirJson,
                request.LogLevel ?? "info"
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}`
  };

  const CodeBlock = ({ code, language = 'csharp' }) => (
    <pre className="bg-gray-900 text-gray-100 p-4 rounded-lg overflow-x-auto">
      <code>{code}</code>
    </pre>
  );

  return (
    <div className="max-w-7xl mx-auto">
      <Space direction="vertical" size="large" className="w-full">
        {/* Header */}
        <Card className="shadow-sm">
          <Title level={2}>
            <CodeOutlined className="mr-3" />
            PSS FHIR Processor - Developer Guide
          </Title>
          <Paragraph className="text-lg">
            Comprehensive guide for integrating and using the PSS FHIR Processor in your C# applications.
            This library provides three main operations: <Tag color="blue">Process</Tag>, <Tag color="green">Validate</Tag>, and <Tag color="orange">Extract</Tag>.
          </Paragraph>
        </Card>

        {/* Setup */}
        <Card title={<Title level={3}>📦 Setup & Initialization</Title>} className="shadow-sm">
          <Paragraph>
            Initialize the processor and load validation metadata (rule sets and codes master):
          </Paragraph>
          <CodeBlock code={csharpCode.setup} />
        </Card>

        {/* Option 1: Process */}
        <Card 
          title={
            <Title level={3}>
              <PlayCircleOutlined className="mr-2" />
              Option 1: Process (Validate + Extract)
            </Title>
          } 
          className="shadow-sm"
        >
          <Paragraph>
            <Text strong>Use case:</Text> Full processing pipeline - validates the FHIR Bundle and extracts flattened data in one operation.
            Extraction runs even if validation fails, allowing you to see the data structure.
          </Paragraph>
          <Divider />
          <Paragraph>
            <Text strong>Returns:</Text> <Text code>ProcessResult</Text> containing:
          </Paragraph>
          <ul className="ml-6 mb-4">
            <li><Text code>Validation</Text> - Validation results with detailed error information</li>
            <li><Text code>Flatten</Text> - Extracted data (Event, Participant, Screening items)</li>
            <li><Text code>OriginalBundle</Text> - Original FHIR Bundle</li>
            <li><Text code>Logs</Text> - Processing logs</li>
          </ul>
          <CodeBlock code={csharpCode.process} />
        </Card>

        {/* Option 2: Validate */}
        <Card 
          title={
            <Title level={3}>
              <CheckCircleOutlined className="mr-2" />
              Option 2: Validate Only
            </Title>
          } 
          className="shadow-sm"
        >
          <Paragraph>
            <Text strong>Use case:</Text> Check FHIR Bundle compliance against validation rules without extraction.
            Useful for pre-submission validation or validation-only workflows.
          </Paragraph>
          <Divider />
          <Paragraph>
            <Text strong>Returns:</Text> <Text code>ValidationResult</Text> containing:
          </Paragraph>
          <ul className="ml-6 mb-4">
            <li><Text code>IsValid</Text> - Boolean indicating validation success</li>
            <li><Text code>Errors</Text> - List of validation errors with:
              <ul className="ml-6 mt-2">
                <li><Text code>Code</Text> - Error code (e.g., MISSING_REQUIRED, INVALID_FORMAT)</li>
                <li><Text code>FieldPath</Text> - FHIR path to the error location</li>
                <li><Text code>Message</Text> - Human-readable error message</li>
                <li><Text code>Context</Text> - Additional context (resource type, question codes, etc.)</li>
                <li><Text code>ResourcePointer</Text> - Location in bundle (entry index, resource ID)</li>
              </ul>
            </li>
          </ul>
          <CodeBlock code={csharpCode.validate} />
        </Card>

        {/* Option 3: Extract */}
        <Card 
          title={
            <Title level={3}>
              <ExperimentOutlined className="mr-2" />
              Option 3: Extract Only
            </Title>
          } 
          className="shadow-sm"
        >
          <Paragraph>
            <Text strong>Use case:</Text> Extract and flatten data from FHIR Bundle without validation.
            Fast extraction for trusted or pre-validated data.
          </Paragraph>
          <Divider />
          <Paragraph>
            <Text strong>Returns:</Text> <Text code>FlattenResult</Text> containing:
          </Paragraph>
          <ul className="ml-6 mb-4">
            <li><Text code>Event</Text> - Event information (start, end, venue, provider)</li>
            <li><Text code>Participant</Text> - Participant demographics (NRIC, name, birth date, gender)</li>
            <li><Text code>HearingRaw</Text> - Hearing screening questions and answers</li>
            <li><Text code>VisionRaw</Text> - Vision screening questions and answers</li>
            <li><Text code>OralRaw</Text> - Oral screening questions and answers</li>
          </ul>
          <CodeBlock code={csharpCode.extract} />
        </Card>

        {/* Web API Integration */}
        <Card title={<Title level={3}>🌐 Web API Integration</Title>} className="shadow-sm">
          <Paragraph>
            Example ASP.NET Core controller implementing all three operations as REST endpoints:
          </Paragraph>
          <CodeBlock code={csharpCode.apiUsage} />
          <Divider />
          <Paragraph>
            <Text strong>API Endpoints:</Text>
          </Paragraph>
          <ul className="ml-6">
            <li><Tag color="blue">POST</Tag> <Text code>/api/fhir/process</Text> - Full validation and extraction</li>
            <li><Tag color="green">POST</Tag> <Text code>/api/fhir/validate</Text> - Validation only</li>
            <li><Tag color="orange">POST</Tag> <Text code>/api/fhir/extract</Text> - Extraction only</li>
          </ul>
        </Card>

        {/* Key Features */}
        <Card title={<Title level={3}>✨ Key Features</Title>} className="shadow-sm">
          <ul className="ml-6 space-y-2">
            <li><Text strong>Three flexible operation modes</Text> - Choose the right operation for your use case</li>
            <li><Text strong>Detailed validation errors</Text> - Pinpoint exact location and cause of validation failures</li>
            <li><Text strong>Structured extraction</Text> - Flatten complex FHIR Bundle into easy-to-use data models</li>
            <li><Text strong>Configurable logging</Text> - Control verbosity (verbose, debug, info, warn, error)</li>
            <li><Text strong>Metadata-driven validation</Text> - Load custom rule sets and codes master</li>
            <li><Text strong>Graceful degradation</Text> - Extraction continues even if validation fails</li>
            <li><Text strong>Path syntax support</Text> - Configure FHIR path resolution (CPS1, etc.)</li>
          </ul>
        </Card>

        {/* Best Practices */}
        <Card title={<Title level={3}>💡 Best Practices</Title>} className="shadow-sm">
          <Space direction="vertical" size="middle" className="w-full">
            <div>
              <Text strong className="text-base">1. Choose the Right Operation</Text>
              <ul className="ml-6 mt-2">
                <li>Use <Tag color="blue">Process</Tag> for complete workflow (validation + extraction)</li>
                <li>Use <Tag color="green">Validate</Tag> for pre-submission checks or validation-only flows</li>
                <li>Use <Tag color="orange">Extract</Tag> for trusted data or when validation is done separately</li>
              </ul>
            </div>
            
            <div>
              <Text strong className="text-base">2. Handle Errors Gracefully</Text>
              <ul className="ml-6 mt-2">
                <li>Always check <Text code>result.Validation.IsValid</Text> before processing data</li>
                <li>Use <Text code>ResourcePointer</Text> to help users locate errors in the source</li>
                <li>Display validation errors in a user-friendly format</li>
              </ul>
            </div>
            
            <div>
              <Text strong className="text-base">3. Configure Appropriately</Text>
              <ul className="ml-6 mt-2">
                <li>Use <Text code>verbose</Text> logging during development</li>
                <li>Use <Text code>info</Text> or <Text code>warn</Text> logging in production</li>
                <li>Enable <Text code>StrictDisplayMatch</Text> for higher data quality</li>
              </ul>
            </div>
            
            <div>
              <Text strong className="text-base">4. Optimize Performance</Text>
              <ul className="ml-6 mt-2">
                <li>Load metadata once during initialization, not per-request</li>
                <li>Use <Text code>Extract</Text> only when validation is not needed</li>
                <li>Consider caching validation results for identical bundles</li>
              </ul>
            </div>
          </Space>
        </Card>

        {/* Support */}
        <Card className="shadow-sm bg-blue-50">
          <Space direction="vertical" size="small">
            <Text strong className="text-base">Need Help?</Text>
            <Paragraph className="mb-0">
              • Check the <Text strong>Validation Rules</Text> tab to understand available validation rules<br />
              • Test your FHIR bundles in the <Text strong>Playground</Text> tab<br />
              • Review the source code in <Text code>src/Pss.FhirProcessor/</Text> for detailed implementation
            </Paragraph>
          </Space>
        </Card>
      </Space>
    </div>
  );
}
