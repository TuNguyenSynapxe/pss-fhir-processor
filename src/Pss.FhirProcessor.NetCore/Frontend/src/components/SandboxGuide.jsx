import React, { useState } from 'react';
import { Typography, Card, Divider, Space, Tag, Alert, Input, Button, Tabs } from 'antd';
import { ApiOutlined, KeyOutlined, CodeOutlined, CheckCircleOutlined, ExperimentOutlined, PlayCircleOutlined, CopyOutlined } from '@ant-design/icons';

const { Title, Paragraph, Text } = Typography;
const { TextArea } = Input;

export default function SandboxGuide() {
  const [apiKey, setApiKey] = useState('your-api-key-here');

  const copyToClipboard = (text) => {
    navigator.clipboard.writeText(text);
  };

  const CodeBlock = ({ code, language = 'bash', showCopy = true }) => (
    <div className="relative">
      {showCopy && (
        <Button
          size="small"
          icon={<CopyOutlined />}
          className="absolute top-2 right-2 z-10"
          onClick={() => copyToClipboard(code)}
        >
          Copy
        </Button>
      )}
      <pre className="bg-gray-900 text-gray-100 p-4 rounded-lg overflow-x-auto">
        <code>{code}</code>
      </pre>
    </div>
  );

  const examples = {
    curl: {
      process: `curl -X POST https://api.pss-fhir.example.com/api/sandbox/process \\
  -H "Content-Type: application/json" \\
  -H "X-API-Key: ${apiKey}" \\
  -d '{
    "fhirJson": "{ \\"resourceType\\": \\"Bundle\\", \\"type\\": \\"collection\\", ... }",
    "logLevel": "info",
    "strictDisplay": true
  }'`,
      validate: `curl -X POST https://api.pss-fhir.example.com/api/sandbox/validate \\
  -H "Content-Type: application/json" \\
  -H "X-API-Key: ${apiKey}" \\
  -d '{
    "fhirJson": "{ \\"resourceType\\": \\"Bundle\\", ... }",
    "logLevel": "info",
    "strictDisplay": true
  }'`,
      extract: `curl -X POST https://api.pss-fhir.example.com/api/sandbox/extract \\
  -H "Content-Type: application/json" \\
  -H "X-API-Key: ${apiKey}" \\
  -d '{
    "fhirJson": "{ \\"resourceType\\": \\"Bundle\\", ... }",
    "logLevel": "info"
  }'`
    },
    javascript: {
      process: `const response = await fetch('https://api.pss-fhir.example.com/api/sandbox/process', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'X-API-Key': '${apiKey}'
  },
  body: JSON.stringify({
    fhirJson: JSON.stringify(fhirBundle),
    logLevel: 'info',
    strictDisplay: true
  })
});

const result = await response.json();
console.log('Validation:', result.validation);
console.log('Extraction:', result.flatten);`,
      validate: `const response = await fetch('https://api.pss-fhir.example.com/api/sandbox/validate', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'X-API-Key': '${apiKey}'
  },
  body: JSON.stringify({
    fhirJson: JSON.stringify(fhirBundle),
    logLevel: 'info',
    strictDisplay: true
  })
});

const result = await response.json();
if (result.validation.isValid) {
  console.log('✓ Validation passed!');
} else {
  console.log('Errors:', result.validation.errors);
}`,
      extract: `const response = await fetch('https://api.pss-fhir.example.com/api/sandbox/extract', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'X-API-Key': '${apiKey}'
  },
  body: JSON.stringify({
    fhirJson: JSON.stringify(fhirBundle),
    logLevel: 'info'
  })
});

const result = await response.json();
console.log('Event:', result.flatten.event);
console.log('Participant:', result.flatten.participant);`
    },
    python: {
      process: `import requests
import json

url = 'https://api.pss-fhir.example.com/api/sandbox/process'
headers = {
    'Content-Type': 'application/json',
    'X-API-Key': '${apiKey}'
}
payload = {
    'fhirJson': json.dumps(fhir_bundle),
    'logLevel': 'info',
    'strictDisplay': True
}

response = requests.post(url, headers=headers, json=payload)
result = response.json()

print('Validation:', result['validation'])
print('Extraction:', result['flatten'])`,
      validate: `import requests
import json

url = 'https://api.pss-fhir.example.com/api/sandbox/validate'
headers = {
    'Content-Type': 'application/json',
    'X-API-Key': '${apiKey}'
}
payload = {
    'fhirJson': json.dumps(fhir_bundle),
    'logLevel': 'info',
    'strictDisplay': True
}

response = requests.post(url, headers=headers, json=payload)
result = response.json()

if result['validation']['isValid']:
    print('✓ Validation passed!')
else:
    print('Errors:', result['validation']['errors'])`,
      extract: `import requests
import json

url = 'https://api.pss-fhir.example.com/api/sandbox/extract'
headers = {
    'Content-Type': 'application/json',
    'X-API-Key': '${apiKey}'
}
payload = {
    'fhirJson': json.dumps(fhir_bundle),
    'logLevel': 'info'
}

response = requests.post(url, headers=headers, json=payload)
result = response.json()

print('Event:', result['flatten']['event'])
print('Participant:', result['flatten']['participant'])`
    },
    csharp: {
      process: `using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-API-Key", "${apiKey}");

var payload = new {
    fhirJson = JsonConvert.SerializeObject(fhirBundle),
    logLevel = "info",
    strictDisplay = true
};

var response = await client.PostAsync(
    "https://api.pss-fhir.example.com/api/sandbox/process",
    new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
);

var result = await response.Content.ReadAsStringAsync();
Console.WriteLine(result);`,
      validate: `var payload = new {
    fhirJson = JsonConvert.SerializeObject(fhirBundle),
    logLevel = "info",
    strictDisplay = true
};

var response = await client.PostAsync(
    "https://api.pss-fhir.example.com/api/sandbox/validate",
    new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
);

var resultJson = await response.Content.ReadAsStringAsync();
var result = JsonConvert.DeserializeObject<ValidationResponse>(resultJson);

if (result.Validation.IsValid)
    Console.WriteLine("✓ Validation passed!");
else
    Console.WriteLine($"Errors: {result.Validation.Errors.Count}");`,
      extract: `var payload = new {
    fhirJson = JsonConvert.SerializeObject(fhirBundle),
    logLevel = "info"
};

var response = await client.PostAsync(
    "https://api.pss-fhir.example.com/api/sandbox/extract",
    new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
);

var resultJson = await response.Content.ReadAsStringAsync();
var result = JsonConvert.DeserializeObject<ExtractionResponse>(resultJson);

Console.WriteLine($"Event: {result.Flatten.Event.Start}");
Console.WriteLine($"Participant: {result.Flatten.Participant.Name}");`
    }
  };

  const tabItems = [
    {
      key: 'curl',
      label: 'cURL',
      children: (
        <Space direction="vertical" size="large" className="w-full">
          <div>
            <Text strong className="text-base">Process (Validate + Extract)</Text>
            <CodeBlock code={examples.curl.process} />
          </div>
          <div>
            <Text strong className="text-base">Validate Only</Text>
            <CodeBlock code={examples.curl.validate} />
          </div>
          <div>
            <Text strong className="text-base">Extract Only</Text>
            <CodeBlock code={examples.curl.extract} />
          </div>
        </Space>
      )
    },
    {
      key: 'javascript',
      label: 'JavaScript',
      children: (
        <Space direction="vertical" size="large" className="w-full">
          <div>
            <Text strong className="text-base">Process (Validate + Extract)</Text>
            <CodeBlock code={examples.javascript.process} language="javascript" />
          </div>
          <div>
            <Text strong className="text-base">Validate Only</Text>
            <CodeBlock code={examples.javascript.validate} language="javascript" />
          </div>
          <div>
            <Text strong className="text-base">Extract Only</Text>
            <CodeBlock code={examples.javascript.extract} language="javascript" />
          </div>
        </Space>
      )
    },
    {
      key: 'python',
      label: 'Python',
      children: (
        <Space direction="vertical" size="large" className="w-full">
          <div>
            <Text strong className="text-base">Process (Validate + Extract)</Text>
            <CodeBlock code={examples.python.process} language="python" />
          </div>
          <div>
            <Text strong className="text-base">Validate Only</Text>
            <CodeBlock code={examples.python.validate} language="python" />
          </div>
          <div>
            <Text strong className="text-base">Extract Only</Text>
            <CodeBlock code={examples.python.extract} language="python" />
          </div>
        </Space>
      )
    },
    {
      key: 'csharp',
      label: 'C#',
      children: (
        <Space direction="vertical" size="large" className="w-full">
          <div>
            <Text strong className="text-base">Process (Validate + Extract)</Text>
            <CodeBlock code={examples.csharp.process} language="csharp" />
          </div>
          <div>
            <Text strong className="text-base">Validate Only</Text>
            <CodeBlock code={examples.csharp.validate} language="csharp" />
          </div>
          <div>
            <Text strong className="text-base">Extract Only</Text>
            <CodeBlock code={examples.csharp.extract} language="csharp" />
          </div>
        </Space>
      )
    }
  ];

  return (
    <div className="max-w-7xl mx-auto">
      <Space direction="vertical" size="large" className="w-full">
        {/* Header */}
        <Card className="shadow-sm">
          <Title level={2}>
            <ApiOutlined className="mr-3" />
            Sandbox API Guide
          </Title>
          <Paragraph className="text-lg">
            Use the PSS FHIR Processor as a REST API for external integrations. 
            Process, validate, or extract FHIR data programmatically with API key authentication.
          </Paragraph>
        </Card>

        {/* API Key Setup */}
        <Card title={<Title level={3}><KeyOutlined className="mr-2" />API Key Setup</Title>} className="shadow-sm">
          <Alert
            message="API Key Required"
            description="All sandbox API requests require authentication via X-API-Key header. Contact your system administrator to obtain an API key."
            type="info"
            showIcon
            className="mb-4"
          />
          
          <Space direction="vertical" size="middle" className="w-full">
            <div>
              <Text strong>Your API Key:</Text>
              <Input
                value={apiKey}
                onChange={(e) => setApiKey(e.target.value)}
                placeholder="Enter your API key"
                prefix={<KeyOutlined />}
                className="mt-2"
              />
              <Text type="secondary" className="text-sm">
                This key will be used in all code examples below. Keep it secure and never commit it to source control.
              </Text>
            </div>

            <Divider />

            <div>
              <Text strong className="text-base">Rate Limits:</Text>
              <ul className="ml-6 mt-2">
                <li>100 requests per hour per API key</li>
                <li>Maximum payload size: 5MB</li>
                <li>Timeout: 30 seconds per request</li>
              </ul>
            </div>

            <div>
              <Text strong className="text-base">API Key Management:</Text>
              <ul className="ml-6 mt-2">
                <li><Text code>X-API-Key</Text> header is mandatory for all requests</li>
                <li>API keys are organization-specific</li>
                <li>Keys can be rotated by administrators</li>
                <li>Invalid keys return <Tag color="red">401 Unauthorized</Tag></li>
              </ul>
            </div>
          </Space>
        </Card>

        {/* Endpoints */}
        <Card title={<Title level={3}><CodeOutlined className="mr-2" />Available Endpoints</Title>} className="shadow-sm">
          <Space direction="vertical" size="middle" className="w-full">
            <div className="border-l-4 border-blue-500 pl-4">
              <Text strong className="text-base flex items-center gap-2">
                <PlayCircleOutlined className="text-blue-500" />
                POST /api/sandbox/process
              </Text>
              <Paragraph className="mt-2 mb-1">
                Full validation and extraction in one call. Returns both validation results and flattened data.
              </Paragraph>
              <Tag color="blue">Validate + Extract</Tag>
            </div>

            <div className="border-l-4 border-green-500 pl-4">
              <Text strong className="text-base flex items-center gap-2">
                <CheckCircleOutlined className="text-green-500" />
                POST /api/sandbox/validate
              </Text>
              <Paragraph className="mt-2 mb-1">
                Validation only. Checks FHIR Bundle against validation rules without extraction.
              </Paragraph>
              <Tag color="green">Validate Only</Tag>
            </div>

            <div className="border-l-4 border-orange-500 pl-4">
              <Text strong className="text-base flex items-center gap-2">
                <ExperimentOutlined className="text-orange-500" />
                POST /api/sandbox/extract
              </Text>
              <Paragraph className="mt-2 mb-1">
                Extraction only. Returns flattened data without validation checks.
              </Paragraph>
              <Tag color="orange">Extract Only</Tag>
            </div>
          </Space>
        </Card>

        {/* Request Format */}
        <Card title={<Title level={3}>📤 Request Format</Title>} className="shadow-sm">
          <Paragraph>
            <Text strong>Headers:</Text>
          </Paragraph>
          <ul className="ml-6 mb-4">
            <li><Text code>Content-Type: application/json</Text></li>
            <li><Text code>X-API-Key: your-api-key</Text> <Tag color="red">Required</Tag></li>
          </ul>

          <Paragraph>
            <Text strong>Body Parameters:</Text>
          </Paragraph>
          <table className="w-full border-collapse">
            <thead>
              <tr className="bg-gray-100">
                <th className="border p-2 text-left">Parameter</th>
                <th className="border p-2 text-left">Type</th>
                <th className="border p-2 text-left">Required</th>
                <th className="border p-2 text-left">Description</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td className="border p-2"><Text code>fhirJson</Text></td>
                <td className="border p-2">string</td>
                <td className="border p-2"><Tag color="red">Yes</Tag></td>
                <td className="border p-2">FHIR Bundle as JSON string</td>
              </tr>
              <tr>
                <td className="border p-2"><Text code>logLevel</Text></td>
                <td className="border p-2">string</td>
                <td className="border p-2"><Tag color="blue">No</Tag></td>
                <td className="border p-2">verbose, debug, info, warn, error (default: info)</td>
              </tr>
              <tr>
                <td className="border p-2"><Text code>strictDisplay</Text></td>
                <td className="border p-2">boolean</td>
                <td className="border p-2"><Tag color="blue">No</Tag></td>
                <td className="border p-2">Strict display text matching (default: true)</td>
              </tr>
            </tbody>
          </table>
        </Card>

        {/* Response Format */}
        <Card title={<Title level={3}>📥 Response Format</Title>} className="shadow-sm">
          <Tabs
            defaultActiveKey="process"
            items={[
              {
                key: 'process',
                label: 'Process',
                children: (
                  <CodeBlock code={`{
  "success": true,
  "validation": {
    "isValid": false,
    "errors": [
      {
        "code": "MISSING_REQUIRED",
        "fieldPath": "Patient.identifier",
        "message": "Required field is missing",
        "scope": "Patient",
        "resourcePointer": {
          "entryIndex": 0,
          "resourceType": "Patient",
          "resourceId": "patient-123"
        }
      }
    ]
  },
  "flatten": {
    "event": {
      "start": "2024-01-15T09:00:00Z",
      "end": "2024-01-15T17:00:00Z",
      "venueName": "Community Center"
    },
    "participant": {
      "nric": "S1234567A",
      "name": "John Doe"
    },
    "hearingRaw": { "items": [...] },
    "visionRaw": { "items": [...] },
    "oralRaw": { "items": [...] }
  },
  "logs": ["Processing started...", "..."]
}`} language="json" />
                )
              },
              {
                key: 'validate',
                label: 'Validate',
                children: (
                  <CodeBlock code={`{
  "success": true,
  "validation": {
    "isValid": true,
    "errors": []
  },
  "logs": ["Validation completed..."]
}`} language="json" />
                )
              },
              {
                key: 'extract',
                label: 'Extract',
                children: (
                  <CodeBlock code={`{
  "success": true,
  "flatten": {
    "event": {...},
    "participant": {...},
    "hearingRaw": {...},
    "visionRaw": {...},
    "oralRaw": {...}
  },
  "logs": ["Extraction completed..."]
}`} language="json" />
                )
              }
            ]}
          />
        </Card>

        {/* Code Examples */}
        <Card title={<Title level={3}>💻 Code Examples</Title>} className="shadow-sm">
          <Tabs defaultActiveKey="curl" items={tabItems} />
        </Card>

        {/* Error Handling */}
        <Card title={<Title level={3}>⚠️ Error Handling</Title>} className="shadow-sm">
          <Space direction="vertical" size="middle" className="w-full">
            <div>
              <Tag color="red">401 Unauthorized</Tag>
              <Paragraph className="mt-2">
                Invalid or missing API key. Check your <Text code>X-API-Key</Text> header.
              </Paragraph>
            </div>

            <div>
              <Tag color="orange">400 Bad Request</Tag>
              <Paragraph className="mt-2">
                Invalid request format. Check that <Text code>fhirJson</Text> is a valid JSON string.
              </Paragraph>
            </div>

            <div>
              <Tag color="orange">429 Too Many Requests</Tag>
              <Paragraph className="mt-2">
                Rate limit exceeded. Wait before making additional requests.
              </Paragraph>
            </div>

            <div>
              <Tag color="red">500 Internal Server Error</Tag>
              <Paragraph className="mt-2">
                Server error during processing. Check logs or contact support.
              </Paragraph>
            </div>
          </Space>
        </Card>

        {/* Best Practices */}
        <Card title={<Title level={3}>✨ Best Practices</Title>} className="shadow-sm">
          <ul className="ml-6 space-y-2">
            <li><Text strong>Validate before processing:</Text> Use validate endpoint first to check for errors before extraction</li>
            <li><Text strong>Handle rate limits:</Text> Implement exponential backoff for 429 responses</li>
            <li><Text strong>Secure your API key:</Text> Never expose keys in client-side code or public repositories</li>
            <li><Text strong>Use appropriate log levels:</Text> 'info' for production, 'verbose' for debugging</li>
            <li><Text strong>Cache validation results:</Text> Avoid re-validating identical bundles</li>
            <li><Text strong>Monitor usage:</Text> Track API usage to stay within rate limits</li>
            <li><Text strong>Rotate keys regularly:</Text> Update API keys periodically for security</li>
          </ul>
        </Card>

        {/* Support */}
        <Card className="shadow-sm bg-blue-50">
          <Space direction="vertical" size="small">
            <Text strong className="text-base">Need Help?</Text>
            <Paragraph className="mb-0">
              • Test your FHIR bundles in the <Text strong>Playground</Text> tab first<br />
              • Review validation rules in the <Text strong>Validation Rules</Text> tab<br />
              • Check integration examples in the <Text strong>Developer Guide</Text> tab<br />
              • Contact support for API key issues or rate limit increases
            </Paragraph>
          </Space>
        </Card>
      </Space>
    </div>
  );
}
