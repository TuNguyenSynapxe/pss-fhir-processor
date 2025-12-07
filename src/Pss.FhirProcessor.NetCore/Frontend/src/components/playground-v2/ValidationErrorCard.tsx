import React, { useState } from 'react';
import { Alert, Tag, Button, Breadcrumb, Typography } from 'antd';
import {
  InfoCircleOutlined,
  EnvironmentOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  BulbOutlined,
  ToolOutlined,
  HomeOutlined,
  RightOutlined,
  AimOutlined,
} from '@ant-design/icons';
import { generateHelper } from '../../utils/validationHelper';
import type { ValidationError, Helper } from './types';

const { Text, Paragraph } = Typography;

interface ValidationErrorCardProps {
  error: ValidationError;
  onGoToResource: (resourcePointer: any) => void;
}

export default function ValidationErrorCard({ error, onGoToResource }: ValidationErrorCardProps) {
  const [isExpanded, setIsExpanded] = useState(false);

  if (!error) return null;

  const helper: Helper = generateHelper(error);

  // Handler for navigating to resource in bundle
  const handleNavigateToResource = () => {
    console.log('🖱️ Go to Resource clicked');
    console.log('📍 Helper resourcePointer:', helper.resourcePointer);
    if (helper.resourcePointer && helper.resourcePointer.entryIndex !== undefined) {
      console.log('✅ Calling onGoToResource with:', helper.resourcePointer);
      onGoToResource(helper.resourcePointer);
    } else {
      console.warn('⚠️ No valid resourcePointer:', helper);
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
          <Button
            type={isExpanded ? "default" : "primary"}
            size="small"
            onClick={() => setIsExpanded(!isExpanded)}
            style={{ minWidth: '100px' }}
          >
            {isExpanded ? 'Hide Details' : 'Show Help'}
          </Button>
        </div>
      }
      description={
        isExpanded ? (
          <div className="mt-3 space-y-4">
            {/* Always show error message */}
            <div className="bg-yellow-50 border-l-4 border-yellow-500 p-3 rounded">
              <div className="flex items-start gap-2">
                <InfoCircleOutlined className="text-yellow-600 text-lg mt-0.5" />
                <div className="flex-1">
                  <Text strong className="text-yellow-800">Error Message:</Text>
                  <Paragraph className="mt-1 mb-0 text-gray-800">
                    {error.message || 'No message available'}
                  </Paragraph>
                </div>
              </div>
            </div>

            {/* What This Means */}
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
                    {helper.breadcrumb.map((crumb: string, idx: number) => (
                      <Breadcrumb.Item key={idx}>
                        <span className="font-mono text-sm">{crumb}</span>
                      </Breadcrumb.Item>
                    ))}
                  </Breadcrumb>
                </div>
              </div>
            )}

            {/* Resource Pointer */}
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
                    </div>
                  </div>
                  <Button
                    type="primary"
                    size="small"
                    icon={<AimOutlined />}
                    onClick={handleNavigateToResource}
                  >
                    Go to Resource
                  </Button>
                </div>
              </div>
            )}

            {/* Expected/Actual Values */}
            {helper.expected && (
              <div>
                <Text strong><CheckCircleOutlined className="text-green-600" /> Expected:</Text>
                <div className="ml-6 mt-1 font-mono text-sm bg-green-50 p-2 rounded border border-green-200">
                  {helper.expected}
                </div>
              </div>
            )}

            {helper.actual && (
              <div>
                <Text strong><CloseCircleOutlined className="text-red-600" /> Actual:</Text>
                <div className="ml-6 mt-1 font-mono text-sm bg-red-50 p-2 rounded border border-red-200">
                  {helper.actual}
                </div>
              </div>
            )}

            {/* How to Fix */}
            {helper.howToFix && helper.howToFix.length > 0 && (
              <div className="bg-green-50 border-l-4 border-green-500 p-3 rounded">
                <div className="flex items-start gap-2">
                  <ToolOutlined className="text-green-600 text-lg mt-0.5" />
                  <div className="flex-1">
                    <Text strong className="text-green-800">How to fix this:</Text>
                    <ol className="mt-2 ml-4 space-y-2 list-decimal">
                      {helper.howToFix.map((step: string, idx: number) => (
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
          </div>
        ) : null
      }
      type="error"
      className="mb-2"
      showIcon={!isExpanded}
    />
  );
}
