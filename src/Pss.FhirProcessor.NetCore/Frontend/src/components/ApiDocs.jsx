import { Card, Typography, Divider, Tag, Alert } from 'antd';
import { ApiOutlined, CheckCircleOutlined } from '@ant-design/icons';

const { Title, Paragraph, Text } = Typography;

function ApiDocs() {
  const endpoints = [
    {
      method: 'POST',
      path: '/api/fhir/validate',
      description: 'Validate FHIR Bundle without extraction',
      requestBody: {
        fhirJson: 'string',
        logLevel: 'string (optional, default: "info")',
        strictDisplayMatch: 'boolean (optional, default: true)'
      },
      response: {
        success: 'boolean',
        validation: 'ValidationResult',
        logs: 'string[]'
      }
    },
    {
      method: 'POST',
      path: '/api/fhir/extract',
      description: 'Validate and extract data from FHIR Bundle',
      requestBody: {
        fhirJson: 'string',
        logLevel: 'string (optional, default: "info")',
        strictDisplayMatch: 'boolean (optional, default: true)'
      },
      response: {
        success: 'boolean',
        validation: 'ValidationResult',
        flatten: 'object',
        logs: 'string[]'
      }
    },
    {
      method: 'POST',
      path: '/api/fhir/process',
      description: 'Complete processing: validate and extract',
      requestBody: {
        fhirJson: 'string',
        logLevel: 'string (optional, default: "info")',
        strictDisplayMatch: 'boolean (optional, default: true)'
      },
      response: {
        success: 'boolean',
        validation: 'ValidationResult',
        flatten: 'object',
        logs: 'string[]'
      }
    },
    {
      method: 'GET',
      path: '/api/fhir/codes-master',
      description: 'Get Codes Master metadata',
      requestBody: null,
      response: 'JSON string containing question codes and allowed answers'
    },
    {
      method: 'GET',
      path: '/api/fhir/rules',
      description: 'Get validation rules',
      requestBody: null,
      response: 'Dictionary of rule sets by scope'
    },
    {
      method: 'GET',
      path: '/api/fhir/test-cases',
      description: 'Get all test cases',
      requestBody: null,
      response: 'Array of test case objects'
    },
    {
      method: 'GET',
      path: '/api/fhir/test-cases/{name}',
      description: 'Get specific test case by name',
      requestBody: null,
      response: 'Test case object'
    }
  ];

  return (
    <div className="space-y-4">
      <Card title={<><ApiOutlined /> API Documentation</>} className="shadow-md">
        <Alert
          message="Base URL"
          description="http://localhost:5000"
          type="info"
          showIcon
          icon={<CheckCircleOutlined />}
          className="mb-4"
        />

        {endpoints.map((endpoint, index) => (
          <div key={index}>
            <div className="mb-4">
              <Title level={4}>
                <Tag color={endpoint.method === 'GET' ? 'blue' : 'green'}>
                  {endpoint.method}
                </Tag>
                <Text code>{endpoint.path}</Text>
              </Title>
              <Paragraph>{endpoint.description}</Paragraph>

              {endpoint.requestBody && (
                <>
                  <Text strong>Request Body:</Text>
                  <pre className="bg-gray-100 p-3 rounded mt-2 overflow-auto">
                    {JSON.stringify(endpoint.requestBody, null, 2)}
                  </pre>
                </>
              )}

              <Text strong>Response:</Text>
              <pre className="bg-gray-100 p-3 rounded mt-2 overflow-auto">
                {typeof endpoint.response === 'string'
                  ? endpoint.response
                  : JSON.stringify(endpoint.response, null, 2)}
              </pre>
            </div>
            {index < endpoints.length - 1 && <Divider />}
          </div>
        ))}
      </Card>

      <Card title="Log Levels" className="shadow-md">
        <ul className="list-disc list-inside space-y-2">
          <li><Text code>debug</Text> - Most verbose, includes all debug information</li>
          <li><Text code>info</Text> - Standard information messages (default)</li>
          <li><Text code>warn</Text> - Warning messages only</li>
          <li><Text code>error</Text> - Error messages only</li>
        </ul>
      </Card>

      <Card title="Validation Options" className="shadow-md">
        <ul className="list-disc list-inside space-y-2">
          <li>
            <Text strong>strictDisplayMatch</Text> - When true, question displays must
            match exactly. When false, only codes are validated.
          </li>
        </ul>
      </Card>
    </div>
  );
}

export default ApiDocs;
