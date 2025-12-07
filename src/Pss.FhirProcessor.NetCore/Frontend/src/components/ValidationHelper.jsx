import React, { useState } from 'react';
import { Alert, Collapse, Tag, Typography, Button, Breadcrumb } from 'antd';
import {
  InfoCircleOutlined,
  EnvironmentOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  QuestionCircleOutlined,
  BulbOutlined,
  ToolOutlined,
  HomeOutlined,
  RightOutlined,
  AimOutlined,
} from '@ant-design/icons';
import { generateHelper, humanizePath } from '../utils/validationHelper';

const { Panel } = Collapse;
const { Text, Paragraph, Title } = Typography;

/**
 * ValidationHelper Component
 * Renders metadata-driven help for validation errors
 */
function ValidationHelper({ error, onGoToResource }) {
  const [isExpanded, setIsExpanded] = useState(false);

  if (!error) return null;

  const helper = generateHelper(error);

  // Debug logging to help troubleshoot blank content issues
  console.log('ValidationHelper render:', {
    isExpanded,
    errorCode: error.code,
    errorRuleType: error.ruleType,
    hasWhatThisMeans: !!helper.whatThisMeans,
    hasDescription: !!helper.description,
    hasLocation: !!helper.location,
    hasHowToFix: !!helper.howToFix?.length,
    helperKeys: Object.keys(helper || {})
  });

  // Handler for navigating to resource in bundle
  const handleNavigateToResource = () => {
    if (helper.resourcePointer && helper.resourcePointer.entryIndex !== undefined) {
      // Emit custom event that Playground can listen to
      window.dispatchEvent(new CustomEvent('navigateToEntry', {
        detail: { entryIndex: helper.resourcePointer.entryIndex }
      }));
    }
  };

  return (
    <Alert
      message={
        <div className="flex items-start gap-2">
          <div className="flex-1">
            <div className="flex items-center gap-2 mb-1">
              <strong className="text-red-600 text-sm">[{error.code}]</strong>
              <Tag color="red">{error.ruleType || 'Validation Error'}</Tag>
              {helper.resourceType && (
                <Tag color="blue" icon={<HomeOutlined />}>{helper.resourceType}</Tag>
              )}
            </div>
            <div className="font-medium text-base">{helper.title}</div>
          </div>
          <button
            onClick={() => setIsExpanded(!isExpanded)}
            className="text-blue-600 hover:text-blue-800 text-sm underline"
          >
            {isExpanded ? 'Hide Details' : 'Show Help'}
          </button>
        </div>
      }
      description={
        isExpanded ? (
          <div className="mt-3 space-y-4">
            {/* Always show at least the error message */}
            <div className="bg-yellow-50 border-l-4 border-yellow-500 p-3 rounded">
              <div className="flex items-start gap-2">
                <InfoCircleOutlined className="text-yellow-600 text-lg mt-0.5" />
                <div className="flex-1">
                  <Text strong className="text-yellow-800">Error Message:</Text>
                  <Paragraph className="mt-1 mb-0 text-gray-800">{error.message || 'No message available'}</Paragraph>
                </div>
              </div>
            </div>

            {/* Check if helper has any content - show fallback if empty */}
            {!helper.whatThisMeans && !helper.description && !helper.location && !helper.howToFix?.length && (
              <div className="bg-gray-50 p-3 rounded border border-gray-300">
                <Text className="text-gray-600">
                  <InfoCircleOutlined className="mr-2" />
                  No additional help information available for this error. Please review the error message and field path above.
                </Text>
              </div>
            )}

            {/* What This Means - Human-friendly explanation */}
            {helper.whatThisMeans && (
              <div className="bg-blue-50 border-l-4 border-blue-500 p-3 rounded">
                <div className="flex items-start gap-2">
                  <BulbOutlined className="text-blue-600 text-lg mt-0.5" />
                  <div className="flex-1">
                    <Text strong className="text-blue-800">What this means:</Text>
                    <Paragraph className="mt-1 mb-0 text-gray-800">{helper.whatThisMeans}</Paragraph>
                  </div>
                </div>
              </div>
            )}

            {/* Breadcrumb Navigation */}
            {helper.breadcrumb && helper.breadcrumb.length > 0 && (
              <div>
                <Text strong><EnvironmentOutlined /> Location in record:</Text>
                <div className="ml-6 mt-1">
                  <Breadcrumb separator={<RightOutlined style={{ fontSize: '10px' }} />}>
                    {helper.breadcrumb.map((crumb, idx) => (
                      <Breadcrumb.Item key={idx}>
                        <span className="font-mono text-sm">{crumb}</span>
                      </Breadcrumb.Item>
                    ))}
                  </Breadcrumb>
                </div>
              </div>
            )}

            {/* Resource Pointer - Navigation to Bundle Entry */}
            {helper.resourcePointer && helper.resourcePointer.entryIndex !== undefined && (
              <div className="bg-gray-50 border border-gray-300 p-3 rounded">
                <div className="flex items-center justify-between">
                  <div>
                    <Text strong>Resource in Bundle:</Text>
                    <div className="ml-6 mt-1 space-y-1">
                      <div className="text-sm">
                        <Tag color="purple">Entry #{helper.resourcePointer.entryIndex}</Tag>
                        {helper.resourcePointer.resourceType && (
                          <Tag color="blue">{helper.resourcePointer.resourceType}</Tag>
                        )}
                      </div>
                      {helper.resourcePointer.fullUrl && (
                        <div className="text-xs text-gray-600 font-mono">
                          {helper.resourcePointer.fullUrl}
                        </div>
                      )}
                      {helper.resourcePointer.resourceId && (
                        <div className="text-xs text-gray-600">
                          ID: <code>{helper.resourcePointer.resourceId}</code>
                        </div>
                      )}
                    </div>
                  </div>
                  <Button 
                    type="primary" 
                    size="small"
                    onClick={handleNavigateToResource}
                  >
                    Go to Resource →
                  </Button>
                </div>
              </div>
            )}

            {/* Description */}
            {helper.description && (
              <div>
                <Text strong><InfoCircleOutlined /> Technical Details:</Text>
                <Paragraph className="ml-6 mt-1 text-gray-700">{helper.description}</Paragraph>
              </div>
            )}

            {/* Location */}
            {helper.location && (
              <div>
                <Text strong><EnvironmentOutlined /> Location:</Text>
                <div className="ml-6 mt-1 font-mono text-xs bg-gray-100 p-2 rounded">
                  {helper.location}
                </div>
              </div>
            )}

            {/* Question Details (CodesMaster) */}
            {helper.questionDisplay && (
              <div>
                <Text strong><QuestionCircleOutlined /> Question:</Text>
                <div className="ml-6 mt-1">
                  <div className="font-medium">{helper.questionDisplay}</div>
                  {helper.questionCode && (
                    <div className="text-xs text-gray-500">Code: {helper.questionCode}</div>
                  )}
                </div>
              </div>
            )}

            {/* Expected Value */}
            {helper.expected && !helper.allowedAnswers && !helper.allowedCodes && (
              <div>
                <Text strong><CheckCircleOutlined className="text-green-600" /> Expected:</Text>
                <div className="ml-6 mt-1 font-mono text-sm bg-green-50 p-2 rounded border border-green-200">
                  {helper.expected}
                </div>
              </div>
            )}

            {/* Actual Value */}
            {helper.actual && (
              <div>
                <Text strong><CloseCircleOutlined className="text-red-600" /> Actual:</Text>
                <div className="ml-6 mt-1 font-mono text-sm bg-red-50 p-2 rounded border border-red-200">
                  {helper.actual}
                </div>
              </div>
            )}

            {/* Example */}
            {helper.example && (
              <div>
                <Text strong>Example:</Text>
                <div className="ml-6 mt-1 font-mono text-sm bg-blue-50 p-2 rounded border border-blue-200">
                  {helper.example}
                </div>
              </div>
            )}

            {/* Allowed Answers (CodesMaster) */}
            {helper.allowedAnswers && helper.allowedAnswers.length > 0 && (
              <div>
                <Text strong><CheckCircleOutlined className="text-green-600" /> Allowed Answers:</Text>
                <div className="ml-6 mt-1">
                  <ul className="list-disc list-inside space-y-1">
                    {helper.allowedAnswers.map((answer, idx) => (
                      <li key={idx} className="text-sm">
                        <Tag color="green">{answer}</Tag>
                      </li>
                    ))}
                  </ul>
                  {helper.isMultiValue && (
                    <div className="mt-2 p-2 bg-blue-50 rounded border border-blue-200 text-xs">
                      <Text strong>Multi-value format:</Text> Separate multiple values with pipe (|). 
                      Example: "500Hz – R|1000Hz – R"
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Allowed Codes (CodeSystem) */}
            {helper.allowedCodes && helper.allowedCodes.length > 0 && (
              <div>
                <Text strong><CheckCircleOutlined className="text-green-600" /> Allowed Codes:</Text>
                <div className="ml-6 mt-1 max-h-48 overflow-y-auto">
                  <table className="min-w-full text-sm border-collapse">
                    <thead>
                      <tr className="bg-gray-100">
                        <th className="border border-gray-300 px-2 py-1 text-left">Code</th>
                        <th className="border border-gray-300 px-2 py-1 text-left">Display</th>
                      </tr>
                    </thead>
                    <tbody>
                      {helper.allowedCodes.map((concept, idx) => (
                        <tr key={idx} className="hover:bg-gray-50">
                          <td className="border border-gray-300 px-2 py-1 font-mono text-xs">
                            <Tag color="blue">{concept.code}</Tag>
                          </td>
                          <td className="border border-gray-300 px-2 py-1">{concept.display}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}

            {/* Allowed Types (Reference) */}
            {helper.allowedTypes && helper.allowedTypes.length > 0 && (
              <div>
                <Text strong>Allowed Reference Types:</Text>
                <div className="ml-6 mt-1">
                  {helper.allowedTypes.map((type, idx) => (
                    <Tag key={idx} color="blue">{type}</Tag>
                  ))}
                </div>
              </div>
            )}

            {/* How to Fix - Step-by-step instructions */}
            {helper.howToFix && helper.howToFix.length > 0 && (
              <div className="bg-green-50 border-l-4 border-green-500 p-3 rounded">
                <div className="flex items-start gap-2">
                  <ToolOutlined className="text-green-600 text-lg mt-0.5" />
                  <div className="flex-1">
                    <Text strong className="text-green-800">How to fix this:</Text>
                    <ol className="mt-2 ml-4 space-y-2 list-decimal">
                      {helper.howToFix.map((step, idx) => (
                        step && (
                          <li key={idx} className="text-sm text-gray-800">
                            {step}
                          </li>
                        )
                      ))}
                    </ol>
                  </div>
                </div>
              </div>
            )}

            {/* Navigation Button */}
            {error.resourcePointer && onGoToResource && (
              <div className="mt-3 pt-3 border-t border-gray-200">
                <Button
                  type="primary"
                  size="small"
                  icon={<AimOutlined />}
                  onClick={() => onGoToResource(error.resourcePointer)}
                >
                  Go to {error.resourcePointer.resourceType} (Entry #{error.resourcePointer.entryIndex})
                </Button>
              </div>
            )}

            {/* Scope Info */}
            {error.scope && (
              <div className="text-xs text-gray-500 mt-3 pt-3 border-t border-gray-200">
                <Text>Scope: {error.scope}</Text>
                {helper.screeningType && <Text> | Screening Type: {helper.screeningType}</Text>}
                {error.context?.resourceType && <Text> | Resource: {error.context.resourceType}</Text>}
              </div>
            )}
          </div>
        ) : null
      }
      type="error"
      className="mb-2"
      showIcon={!isExpanded}
    />
  );
}

export default ValidationHelper;
