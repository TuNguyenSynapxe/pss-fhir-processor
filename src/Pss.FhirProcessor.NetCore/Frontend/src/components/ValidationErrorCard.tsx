import React from 'react';
import { Card, Tag, Button, Typography, Space, Divider, Tooltip } from 'antd';
import {
  WarningOutlined,
  InfoCircleOutlined,
  CopyOutlined,
  EnvironmentOutlined,
} from '@ant-design/icons';
import type { ValidationError } from '../types/fhir';
import './ValidationErrorCard.css';

const { Text, Paragraph } = Typography;

interface ValidationErrorCardProps {
  error: ValidationError;
  index: number;
  onGoToResource?: (resourcePointer: any) => void;
}

/**
 * Enhanced validation error card with human-readable explanations
 */
export const ValidationErrorCard: React.FC<ValidationErrorCardProps> = ({
  error,
  index,
  onGoToResource,
}) => {
  const ruleType = error.ruleType || 'Unknown';
  const resourceType = error.resourcePointer?.resourceType || 'Unknown';
  
  // Generate human-readable explanation
  const explanation = generateExplanation(error);
  
  // Generate breadcrumb path
  const breadcrumb = generateBreadcrumb(error.fieldPath);
  
  // Generate JSONPath
  const jsonPath = generateJSONPath(error.fieldPath);
  
  // Generate fix instructions
  const fixInstructions = generateFixInstructions(error);

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
  };

  const handleGoToResource = () => {
    if (onGoToResource && error.resourcePointer) {
      onGoToResource(error.resourcePointer);
    }
  };

  return (
    <Card
      className="validation-error-card"
      size="small"
      title={
        <div className="error-card-header">
          <Space>
            <WarningOutlined style={{ color: '#ff4d4f', fontSize: '16px' }} />
            <Text strong>[{error.code}]</Text>
            <Tag color="red">{ruleType}</Tag>
            <Tag color="blue">{resourceType}</Tag>
          </Space>
        </div>
      }
      extra={
        error.resourcePointer && error.resourcePointer.entryIndex !== undefined && (
          <Button
            type="primary"
            size="small"
            icon={<EnvironmentOutlined />}
            onClick={handleGoToResource}
          >
            Go to Resource
          </Button>
        )
      }
    >
      {/* What This Means */}
      <div className="error-section">
        <div className="section-title">
          <InfoCircleOutlined /> What This Means
        </div>
        <Paragraph className="explanation-text">{explanation}</Paragraph>
      </div>

      <Divider className="section-divider" />

      {/* Location in Record */}
      <div className="error-section">
        <div className="section-title">Location in Record</div>
        <div className="breadcrumb-path">{breadcrumb}</div>
        <Space className="jsonpath-row">
          <Text type="secondary" code className="jsonpath-text">
            {jsonPath}
          </Text>
          <Tooltip title="Copy JSONPath">
            <Button
              type="text"
              size="small"
              icon={<CopyOutlined />}
              onClick={() => copyToClipboard(jsonPath)}
            />
          </Tooltip>
        </Space>
      </div>

      {/* Resource in Bundle */}
      {error.resourcePointer && (
        <>
          <Divider className="section-divider" />
          <div className="error-section">
            <div className="section-title">Resource in Bundle</div>
            <Space direction="vertical" size="small">
              {error.resourcePointer.entryIndex !== undefined && (
                <Text>
                  <Text strong>Entry:</Text> #{error.resourcePointer.entryIndex}
                </Text>
              )}
              {error.resourcePointer.fullUrl && (
                <Text>
                  <Text strong>Full URL:</Text> {error.resourcePointer.fullUrl}
                </Text>
              )}
              {error.resourcePointer.resourceType && (
                <Text>
                  <Text strong>Type:</Text> {error.resourcePointer.resourceType}
                </Text>
              )}
            </Space>
          </div>
        </>
      )}

      {/* Technical Details */}
      {(error.rule || error.context) && (
        <>
          <Divider className="section-divider" />
          <div className="error-section">
            <div className="section-title">Technical Details</div>
            <Space direction="vertical" size="small" style={{ width: '100%' }}>
              {error.rule?.pattern && (
                <Text>
                  <Text strong>Expected Pattern:</Text>{' '}
                  <Text code>{error.rule.pattern}</Text>
                </Text>
              )}
              {error.rule?.expectedType && (
                <Text>
                  <Text strong>Expected Type:</Text>{' '}
                  <Tag color="blue">{error.rule.expectedType}</Tag>
                </Text>
              )}
              {error.rule?.expectedValue && (
                <Text>
                  <Text strong>Expected Value:</Text>{' '}
                  <Text code>{error.rule.expectedValue}</Text>
                </Text>
              )}
              {error.context?.allowedAnswers && error.context.allowedAnswers.length > 0 && (
                <div>
                  <Text strong>Allowed Values:</Text>
                  <div style={{ marginTop: 4 }}>
                    {error.context.allowedAnswers.map((answer, idx) => (
                      <Tag key={idx} color="green">
                        {answer}
                      </Tag>
                    ))}
                  </div>
                </div>
              )}
              {error.context?.codeSystemConcepts && error.context.codeSystemConcepts.length > 0 && (
                <div>
                  <Text strong>Valid Codes:</Text>
                  <div style={{ marginTop: 4 }}>
                    {error.context.codeSystemConcepts.slice(0, 5).map((concept, idx) => (
                      <Tag key={idx} color="cyan">
                        {concept.code}
                        {concept.display && ` - ${concept.display}`}
                      </Tag>
                    ))}
                    {error.context.codeSystemConcepts.length > 5 && (
                      <Text type="secondary"> +{error.context.codeSystemConcepts.length - 5} more</Text>
                    )}
                  </div>
                </div>
              )}
            </Space>
          </div>
        </>
      )}

      {/* How to Fix */}
      <Divider className="section-divider" />
      <div className="error-section">
        <div className="section-title">How to Fix</div>
        <ol className="fix-instructions">
          {fixInstructions.map((instruction, idx) => (
            <li key={idx}>{instruction}</li>
          ))}
        </ol>
      </div>

      {/* Original Error Message (Collapsed) */}
      {error.message && (
        <>
          <Divider className="section-divider" />
          <div className="error-section">
            <details>
              <summary className="technical-summary">Technical Error Message</summary>
              <Text type="secondary" className="technical-message">
                {error.message}
              </Text>
            </details>
          </div>
        </>
      )}
    </Card>
  );
};

// Helper Functions

function generateExplanation(error: ValidationError): string {
  const ruleType = error.ruleType?.toLowerCase();
  const fieldPath = error.fieldPath || 'this field';

  switch (ruleType) {
    case 'required':
      return `A required piece of information is missing. The system needs "${fieldPath}" to be filled in, but it's currently empty or not provided.`;
    
    case 'regex':
    case 'pattern':
      return `The format of the information doesn't match what's expected. The value in "${fieldPath}" needs to follow a specific pattern or format.`;
    
    case 'type':
      return `The type of data provided is incorrect. The system expected a different kind of value (like a date, number, or text) for "${fieldPath}".`;
    
    case 'fixedvalue':
      return `This field must have a specific, exact value. The value in "${fieldPath}" doesn't match the required fixed value.`;
    
    case 'reference':
      return `This field is supposed to point to another resource, but it's either pointing to the wrong type of resource or the reference is invalid.`;
    
    case 'codesystem':
      return `The code provided must come from a specific list of valid codes. The value in "${fieldPath}" is not in the approved code system.`;
    
    case 'codesmaster':
      return `This answer doesn't match any of the allowed responses for this screening question. Please select one of the valid answer options.`;
    
    case 'fullurlidmatch':
      return `The resource ID in the URL doesn't match the ID in the resource itself. These should be the same.`;
    
    default:
      return `There's a validation issue with "${fieldPath}". Please review the technical details below for more information.`;
  }
}

function generateBreadcrumb(fieldPath: string): string {
  if (!fieldPath) return 'Unknown location';
  
  // Convert CPS1 path to human-readable breadcrumb
  return fieldPath
    .split('.')
    .map(part => {
      // Handle array indices
      if (part.match(/\[\d+\]/)) {
        return part.replace(/\[(\d+)\]/, ' → Item #$1');
      }
      // Handle special FHIR paths
      if (part.includes(':')) {
        const [key, value] = part.split(':');
        return `${key} (${value})`;
      }
      return part;
    })
    .join(' → ');
}

function generateJSONPath(fieldPath: string): string {
  if (!fieldPath) return '$';
  
  // Convert to JSONPath notation
  return '$.' + fieldPath.replace(/\[/g, '[').replace(/\]/g, ']');
}

function generateFixInstructions(error: ValidationError): string[] {
  const ruleType = error.ruleType?.toLowerCase();
  const instructions: string[] = [];

  switch (ruleType) {
    case 'required':
      instructions.push('Add the missing required field to your JSON');
      instructions.push('Ensure the field is not null or empty');
      instructions.push('Check that the field name is spelled correctly');
      break;
    
    case 'regex':
    case 'pattern':
      instructions.push('Check the current value against the expected pattern');
      if (error.rule?.pattern) {
        instructions.push(`Ensure the value matches the pattern: ${error.rule.pattern}`);
      }
      instructions.push('Remove any extra spaces or special characters');
      instructions.push('Verify the format (e.g., date format, ID format)');
      break;
    
    case 'type':
      if (error.rule?.expectedType) {
        instructions.push(`Change the value to type: ${error.rule.expectedType}`);
      }
      instructions.push('Remove quotes if the value should be a number or boolean');
      instructions.push('Add quotes if the value should be a string');
      instructions.push('Use proper date/time format if required');
      break;
    
    case 'fixedvalue':
      if (error.rule?.expectedValue) {
        instructions.push(`Set the value to exactly: ${error.rule.expectedValue}`);
      }
      instructions.push('This field has a fixed value that cannot be changed');
      instructions.push('Copy the exact value from the specification');
      break;
    
    case 'reference':
      if (error.rule?.targetTypes) {
        instructions.push(`Reference must point to: ${error.rule.targetTypes.join(' or ')}`);
      }
      instructions.push('Check that the reference format is correct (e.g., "Patient/123")');
      instructions.push('Verify the referenced resource exists in the bundle');
      instructions.push('Ensure the resource type in the reference matches the target');
      break;
    
    case 'codesystem':
      if (error.context?.codeSystemConcepts && error.context.codeSystemConcepts.length > 0) {
        instructions.push('Choose one of the valid codes listed above');
      }
      instructions.push('Check the code system URL is correct');
      instructions.push('Verify the code exists in the specified code system');
      instructions.push('Ensure both code and system fields are provided');
      break;
    
    case 'codesmaster':
      if (error.context?.allowedAnswers && error.context.allowedAnswers.length > 0) {
        instructions.push('Select one of the allowed answers listed above');
      }
      instructions.push('Check the screening type matches the question');
      instructions.push('Verify the answer code is spelled correctly');
      break;
    
    case 'fullurlidmatch':
      instructions.push('Ensure the ID in the fullUrl matches the resource.id');
      instructions.push('Check for typos in either the URL or the resource ID');
      instructions.push('Remove any extra characters from the IDs');
      break;
    
    default:
      instructions.push('Review the technical details section');
      instructions.push('Check the FHIR specification for this field');
      instructions.push('Verify the JSON structure is correct');
      instructions.push('Consult with a technical team member if needed');
  }

  return instructions;
}
