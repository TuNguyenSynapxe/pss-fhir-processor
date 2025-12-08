/**
 * Error Helper Demo Component
 * Demonstrates all error types with interactive examples
 */

import React, { useState } from 'react';
import { Card, Select, Typography, Space, Divider, Alert } from 'antd';
import { BugOutlined } from '@ant-design/icons';
import ErrorHelperPanel from '../components/ErrorHelperPanel';
import {
  EXAMPLE_VALIDATION_ERRORS,
  EXAMPLE_FHIR_BUNDLE,
} from '../examples/errorExamples';
import { ValidationError } from '../utils/helperGenerator';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;

interface ErrorExample {
  name: string;
  description: string;
  error: ValidationError;
}

const ERROR_EXAMPLES: ErrorExample[] = [
  {
    name: 'Missing Leaf Node',
    description: 'Parent structure exists, but the final field (system) is missing.',
    error: EXAMPLE_VALIDATION_ERRORS.missingLeafNode,
  },
  {
    name: 'Missing Middle Parent',
    description: 'Multiple levels of parent structure are missing (communication.language.coding).',
    error: EXAMPLE_VALIDATION_ERRORS.missingMiddleParent,
  },
  {
    name: 'Missing Top-Level Parent',
    description: 'A top-level field (address) is completely missing.',
    error: EXAMPLE_VALIDATION_ERRORS.missingTopLevelParent,
  },
  {
    name: 'Wrong Array Index',
    description: 'Array index is out of bounds (trying to access coding[1] when only coding[0] exists).',
    error: EXAMPLE_VALIDATION_ERRORS.wrongArrayIndex,
  },
  {
    name: 'Invalid Code Value',
    description: 'Code value "ZZ" is not in the allowed list [CN, MY, IN, XX].',
    error: EXAMPLE_VALIDATION_ERRORS.invalidCodeMismatch,
  },
  {
    name: 'Fixed Value Mismatch',
    description: 'Field exists but has wrong value (must exactly match expected CodeSystem URL).',
    error: EXAMPLE_VALIDATION_ERRORS.fixedValueMismatch,
  },
  {
    name: 'ID-FullUrl Mismatch',
    description: 'resource.id does not match the GUID portion of entry.fullUrl.',
    error: EXAMPLE_VALIDATION_ERRORS.idFullUrlMismatch,
  },
  {
    name: 'Date Format Error',
    description: 'birthDate has wrong format (2024/01/15 instead of 2024-01-15).',
    error: EXAMPLE_VALIDATION_ERRORS.typeMismatchDate,
  },
  {
    name: 'GUID Format Error',
    description: 'Patient.id is not a valid GUID format.',
    error: EXAMPLE_VALIDATION_ERRORS.typeMismatchGuid,
  },
  {
    name: 'NRIC Format Error',
    description: 'NRIC missing checksum letter (S1234567 instead of S1234567A).',
    error: EXAMPLE_VALIDATION_ERRORS.regexInvalidNric,
  },
  {
    name: 'Postal Code Format Error',
    description: 'Postal code is 5 digits instead of 6.',
    error: EXAMPLE_VALIDATION_ERRORS.regexInvalidPostal,
  },
  {
    name: 'Reference Not Found',
    description: 'Encounter.subject references a Patient that does not exist in the Bundle.',
    error: EXAMPLE_VALIDATION_ERRORS.referenceNotFound,
  },
  {
    name: 'Invalid Answer (CodesMaster)',
    description: 'Observation component answer "Maybe" is not in allowed list.',
    error: EXAMPLE_VALIDATION_ERRORS.invalidAnswerValue,
  },
  {
    name: 'Missing Resource in Bundle',
    description: 'Bundle does not contain a required Patient resource.',
    error: EXAMPLE_VALIDATION_ERRORS.missingResourceInBundle,
  },
  {
    name: 'Invalid FullUrl Format',
    description: 'entry.fullUrl is not in urn:uuid:<GUID> format.',
    error: EXAMPLE_VALIDATION_ERRORS.invalidFullUrlFormat,
  },
  {
    name: 'Component Display Mismatch',
    description: 'Question display text does not match expected value.',
    error: EXAMPLE_VALIDATION_ERRORS.componentDisplayMismatch,
  },
  {
    name: 'Multiple Answers Not Allowed',
    description: 'Multiple pipe-separated answers provided for single-answer question.',
    error: EXAMPLE_VALIDATION_ERRORS.multipleAnswersNotAllowed,
  },
];

export const ErrorHelperDemo: React.FC = () => {
  const [selectedExample, setSelectedExample] = useState<ErrorExample>(ERROR_EXAMPLES[0]);

  // Get the correct JSON context for the error
  const getJsonContext = () => {
    const entryIndex = selectedExample.error.resourcePointer?.entryIndex;
    if (entryIndex !== undefined && EXAMPLE_FHIR_BUNDLE.entry[entryIndex]) {
      return EXAMPLE_FHIR_BUNDLE.entry[entryIndex].resource;
    }
    return EXAMPLE_FHIR_BUNDLE;
  };

  return (
    <div style={{ padding: 24, maxWidth: 1400, margin: '0 auto' }}>
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        {/* Header */}
        <Card>
          <Space align="start" size="large">
            <BugOutlined style={{ fontSize: 48, color: '#1890ff' }} />
            <div>
              <Title level={2} style={{ marginBottom: 8 }}>
                Error Helper Panel Demo
              </Title>
              <Paragraph>
                Interactive demonstration of the Error Helper Panel system. Select different error
                types to see how the system generates contextual help, code examples, and fix
                instructions.
              </Paragraph>
            </div>
          </Space>
        </Card>

        {/* Example Selector */}
        <Card title="Select Error Type">
          <Space direction="vertical" style={{ width: '100%' }}>
            <Select
              style={{ width: '100%' }}
              value={selectedExample.name}
              onChange={(value) => {
                const example = ERROR_EXAMPLES.find((ex) => ex.name === value);
                if (example) setSelectedExample(example);
              }}
              size="large"
            >
              {ERROR_EXAMPLES.map((example) => (
                <Option key={example.name} value={example.name}>
                  {example.name}
                </Option>
              ))}
            </Select>

            <Alert
              message={selectedExample.description}
              type="info"
              showIcon
              style={{ marginTop: 8 }}
            />
          </Space>
        </Card>

        <Divider>Error Details from Backend</Divider>

        {/* Backend Error Object */}
        <Card title="Backend Error Object" size="small">
          <pre
            style={{
              background: '#f5f5f5',
              padding: 12,
              borderRadius: 4,
              overflow: 'auto',
              fontSize: 12,
            }}
          >
            <code>{JSON.stringify(selectedExample.error, null, 2)}</code>
          </pre>
        </Card>

        <Divider>Generated Helper Panel</Divider>

        {/* Error Helper Panel */}
        <Card>
          <ErrorHelperPanel
            error={selectedExample.error}
            json={getJsonContext()}
            onJumpToLocation={(entryIndex, path) => {
              console.log('Jump to location:', { entryIndex, path });
              alert(`Would navigate to:\nEntry: ${entryIndex}\nPath: ${path}`);
            }}
          />
        </Card>

        {/* Footer */}
        <Card>
          <Space direction="vertical">
            <Text strong>Key Features Demonstrated:</Text>
            <ul style={{ marginBottom: 0 }}>
              <li>
                <Text>✅ Intelligent path parsing and analysis</Text>
              </li>
              <li>
                <Text>✅ Context-aware error explanations</Text>
              </li>
              <li>
                <Text>✅ Step-by-step fix instructions</Text>
              </li>
              <li>
                <Text>✅ Minimal valid JSON snippets with proper parent context</Text>
              </li>
              <li>
                <Text>✅ Visual path breakdown showing exactly what's missing</Text>
              </li>
              <li>
                <Text>✅ Copy-to-clipboard for quick fixes</Text>
              </li>
              <li>
                <Text>✅ Jump-to-location navigation</Text>
              </li>
            </ul>
          </Space>
        </Card>
      </Space>
    </div>
  );
};

export default ErrorHelperDemo;
