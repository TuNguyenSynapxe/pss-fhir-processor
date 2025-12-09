/**
 * Error Helper Panel Component
 * Displays comprehensive, contextual help for validation errors
 */

import React, { useMemo, useState } from 'react';
import { Collapse, Typography, Button, Space, Table, Tag, message, Alert } from 'antd';
import {
  InfoCircleOutlined,
  EnvironmentOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  CopyOutlined,
  CodeOutlined,
} from '@ant-design/icons';
import { generateHelper, ValidationError, HelperMessage } from '../utils/helperGenerator';
import { SegmentStatus } from '../utils/pathParser';
import { SmartPathNavigator } from './SmartPathNavigator'; // NEW: Phase 2 Integration

const { Panel } = Collapse;
const { Title, Text, Paragraph } = Typography;

interface ErrorHelperPanelProps {
  error: ValidationError;
  json: any;
  onJumpToLocation?: (entryIndex?: number, path?: string) => void;
}

export const ErrorHelperPanel: React.FC<ErrorHelperPanelProps> = ({
  error,
  json,
  onJumpToLocation,
}) => {
  // Generate helper message
  const helper: HelperMessage = useMemo(() => {
    return generateHelper(error, json);
  }, [error, json]);

  // NEW: Highlight state for synchronized hover between SmartPathNavigator and fix instructions
  const [highlightSegment, setHighlightSegment] = useState<string | null>(null);

  // Copy to clipboard handler
  const handleCopySnippet = (text: string) => {
    navigator.clipboard.writeText(text);
    message.success('Copied to clipboard!');
  };

  // Jump to location handler (original - for legacy button)
  const handleJumpToLocation = () => {
    if (onJumpToLocation) {
      console.log('Jumping to location:', helper.location);
      onJumpToLocation(helper.location.entryIndex, helper.location.fullPath);
    }
  };

  // Handle navigation to entry node (Bundle.entry[X])
  const handleJumpToEntry = () => {
    if (onJumpToLocation && helper.location.entryIndex !== undefined) {
      const entryPath = `entry[${helper.location.entryIndex}]`;
      console.log('Jumping to entry:', entryPath);
      onJumpToLocation(helper.location.entryIndex, entryPath);
    }
  };

  // Handle navigation to resource node (Bundle.entry[X].resource)
  const handleJumpToResource = () => {
    if (onJumpToLocation && helper.location.entryIndex !== undefined) {
      const resourcePath = `entry[${helper.location.entryIndex}].resource`;
      console.log('Jumping to resource:', resourcePath);
      onJumpToLocation(helper.location.entryIndex, resourcePath);
    }
  };

  // NEW: Phase 2 - Jump handler for SmartPathNavigator segments
  const handleNavigateToSegment = (jumpKey: string) => {
    if (onJumpToLocation) {
      console.log('Navigating to segment with jumpKey:', jumpKey);
      // Extract path from jumpKey (remove basePath prefix if present)
      // jumpKey format: "entry[X].resource.path.to.field" or "path.to.field"
      const path = jumpKey.includes('.resource.')
        ? jumpKey.split('.resource.')[1] || jumpKey
        : jumpKey;
      onJumpToLocation(helper.location.entryIndex, path);
    }
  };

  return (
    <div style={{ marginBottom: 16 }}>
      <Collapse
        defaultActiveKey={['1', '2', '3', '4']}
        ghost
        expandIconPosition="end"
      >
        {/* 1. What This Means */}
        <Panel
          header={
            <Space>
              <InfoCircleOutlined style={{ color: '#1890ff' }} />
              <Text strong>What This Means</Text>
            </Space>
          }
          key="1"
        >
          <Paragraph>{helper.whatThisMeans.whatThisMeans}</Paragraph>
          
          {helper.whatThisMeans.commonCauses.length > 0 && (
            <>
              <Text strong>Common Causes:</Text>
              <ul style={{ marginTop: 8 }}>
                {helper.whatThisMeans.commonCauses.map((cause, idx) => (
                  <li key={idx}>
                    <Text type="secondary">{cause}</Text>
                  </li>
                ))}
              </ul>
            </>
          )}
        </Panel>

        {/* 2. Location in Record */}
        <Panel
          header={
            <Space>
              <EnvironmentOutlined style={{ color: '#52c41a' }} />
              <Text strong>Location in Record</Text>
            </Space>
          }
          key="2"
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            {helper.location.entryIndex !== undefined && (
              <div>
                <Text type="secondary">Entry Index: </Text>
                <Tag 
                  color="blue" 
                  style={{ cursor: 'pointer' }}
                  onClick={handleJumpToEntry}
                  title="Click to jump to this entry in JSON viewer"
                >
                  #{helper.location.entryIndex}
                </Tag>
              </div>
            )}
            
            {helper.location.resourceType && (
              <div>
                <Text type="secondary">Resource Type: </Text>
                <Tag 
                  color="green"
                  style={{ cursor: 'pointer' }}
                  onClick={handleJumpToResource}
                  title="Click to jump to this resource in JSON viewer"
                >
                  {helper.location.resourceType}
                </Tag>
              </div>
            )}

            {/* NEW: Phase 2 Integration - SmartPathNavigator replaces breadcrumb + path breakdown */}
            {helper.enhancedSegments && helper.enhancedSegments.length > 0 ? (
              <>
                <SmartPathNavigator
                  segments={helper.enhancedSegments}
                  onSegmentClick={handleNavigateToSegment}
                  defaultExpanded={true}
                  title="Field Path"
                  highlightSegment={highlightSegment}
                  onSegmentHover={setHighlightSegment}
                />
                
                {/* Full path for reference */}
                <div style={{ marginTop: 8 }}>
                  <Text type="secondary">Full Path: </Text>
                  <Text code>{helper.location.fullPath}</Text>
                </div>
              </>
            ) : (
              /* FALLBACK: Legacy breadcrumb if enhancedSegments not available */
              <>
                <div>
                  <Text type="secondary">Path Breadcrumb:</Text>
                  <div style={{ marginTop: 8 }}>
                    {helper.location.breadcrumb.map((crumb, idx) => (
                      <React.Fragment key={idx}>
                        <Tag color="default">{crumb}</Tag>
                        {idx < helper.location.breadcrumb.length - 1 && (
                          <Text type="secondary"> → </Text>
                        )}
                      </React.Fragment>
                    ))}
                  </div>
                </div>

                <div>
                  <Text type="secondary">Full Path: </Text>
                  <Text code>{helper.location.fullPath}</Text>
                </div>
              </>
            )}
          </Space>
        </Panel>

        {/* 3. Expected */}
        <Panel
          header={
            <Space>
              <CheckCircleOutlined style={{ color: '#faad14' }} />
              <Text strong>Expected</Text>
            </Space>
          }
          key="3"
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            <Paragraph>{helper.expected.description}</Paragraph>

            {helper.expected.type === 'value' && helper.expected.value && (
              <div>
                <Text strong>Required Value: </Text>
                <Text code style={{ fontSize: 14 }}>{helper.expected.value}</Text>
              </div>
            )}

            {helper.expected.type === 'pattern' && helper.expected.pattern && (
              <div>
                <Text strong>Pattern (Regex): </Text>
                <Text code style={{ fontSize: 14 }}>{helper.expected.pattern}</Text>
              </div>
            )}

            {helper.expected.type === 'codes' && helper.expected.allowedValues && (
              <Table
                size="small"
                dataSource={helper.expected.allowedValues}
                columns={[
                  {
                    title: 'Allowed Value',
                    dataIndex: 'code',
                    key: 'code',
                    render: (text) => <Text code>{text}</Text>,
                  },
                  ...(helper.expected.allowedValues.some((v) => v.display)
                    ? [
                        {
                          title: 'Display',
                          dataIndex: 'display',
                          key: 'display',
                        },
                      ]
                    : []),
                ]}
                pagination={false}
                style={{ marginTop: 8 }}
              />
            )}

            {helper.expected.type === 'reference' && helper.expected.targetTypes && (
              <div>
                <Text strong>Target Resource Types: </Text>
                {helper.expected.targetTypes.map((type) => (
                  <Tag key={type} color="blue">
                    {type}
                  </Tag>
                ))}
              </div>
            )}

            {helper.expected.type === 'format' && helper.expected.value && (
              <div>
                <Text strong>Expected Type: </Text>
                <Tag color="purple">{helper.expected.value}</Tag>
              </div>
            )}
          </Space>
        </Panel>

        {/* 4. How to Fix This */}
        <Panel
          header={
            <Space>
              <CodeOutlined style={{ color: '#722ed1' }} />
              <Text strong>How to Fix This</Text>
            </Space>
          }
          key="4"
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            {helper.howToFix.needsParentCreation && (
              <Tag color="warning">
                Parent structure missing - {helper.howToFix.missingSegments.length} level(s)
              </Tag>
            )}

            <ol style={{ marginLeft: 16, marginBottom: 0 }}>
              {helper.howToFix.steps.map((step, idx) => (
                <li 
                  key={idx} 
                  style={{ 
                    marginBottom: 8,
                    cursor: 'default',
                    padding: '4px 0',
                    transition: 'background 0.2s ease',
                  }}
                  onMouseEnter={() => {
                    // Highlight corresponding segment if segmentKey is available
                    setHighlightSegment(step.segmentKey || null);
                  }}
                  onMouseLeave={() => setHighlightSegment(null)}
                >
                  <Text>{step.text}</Text>
                </li>
              ))}
            </ol>
          </Space>
        </Panel>

        {/* 5. Example JSON Snippet - Panel renumbered (Path Breakdown removed) */}
        {/* TODO: Next Release - Implement accurate example generation based on validation context */}
        <Panel
          header={
            <Space>
              <CopyOutlined style={{ color: '#13c2c2' }} />
              <Text strong>Example JSON Snippet</Text>
              <Tag color="orange">Placeholder</Tag>
            </Space>
          }
          key="5"
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            <Alert
              message="Placeholder Data"
              description="This example snippet is not yet context-aware. Accurate examples based on validation rules will be implemented in the next release."
              type="warning"
              showIcon
              style={{ marginBottom: 12 }}
            />
            <Text type="secondary">
              Copy this snippet and insert it at the correct location in your JSON:
            </Text>
            
            <div style={{ position: 'relative' }}>
              <pre
                style={{
                  background: '#f5f5f5',
                  padding: 12,
                  borderRadius: 4,
                  overflow: 'auto',
                  maxHeight: 400,
                }}
              >
                <code>{helper.exampleSnippet}</code>
              </pre>
              <Button
                size="small"
                icon={<CopyOutlined />}
                style={{ position: 'absolute', top: 8, right: 8 }}
                onClick={() => handleCopySnippet(helper.exampleSnippet)}
              >
                Copy
              </Button>
            </div>

            {helper.completeExample && (
              <>
                <Text strong style={{ marginTop: 16 }}>Complete Example:</Text>
                <div style={{ position: 'relative' }}>
                  <pre
                    style={{
                      background: '#f5f5f5',
                      padding: 12,
                      borderRadius: 4,
                      overflow: 'auto',
                      maxHeight: 400,
                    }}
                  >
                    <code>{helper.completeExample}</code>
                  </pre>
                  <Button
                    size="small"
                    icon={<CopyOutlined />}
                    style={{ position: 'absolute', top: 8, right: 8 }}
                    onClick={() => handleCopySnippet(helper.completeExample!)}
                  >
                    Copy
                  </Button>
                </div>
              </>
            )}
          </Space>
        </Panel>
      </Collapse>
    </div>
  );
};

/**
 * LEGACY: Path Breakdown Visual Component
 * Kept for backward compatibility / fallback scenarios
 * Now replaced by SmartPathNavigator in Phase 2 integration
 */
/*
interface PathBreakdownViewProps {
  pathStatuses: SegmentStatus[];
}

const PathBreakdownView: React.FC<PathBreakdownViewProps> = ({ pathStatuses }) => {
  return (
    <div style={{ fontFamily: 'monospace' }}>
      {pathStatuses.map((segStatus, idx) => {
        const isSuccess = segStatus.status === 'EXISTS';
        const isFilterNoMatch = segStatus.status === 'FILTER_NO_MATCH';
        const isIndexOutOfRange = segStatus.status === 'INDEX_OUT_OF_RANGE';
        const isMissingArray = segStatus.status === 'MISSING_ARRAY';
        const isMissingProperty = segStatus.status === 'MISSING_PROPERTY';

        // Determine icon and color based on status
        let icon: React.ReactNode;
        let color: string;
        let statusTag: React.ReactNode | null = null;

        if (isSuccess) {
          icon = <CheckCircleOutlined style={{ color: '#52c41a', marginRight: 8 }} />;
          color = '#000';
        } else if (isFilterNoMatch) {
          icon = <CloseCircleOutlined style={{ color: '#faad14', marginRight: 8 }} />;
          color = '#faad14';
          statusTag = <Tag color="warning" style={{ marginLeft: 8, fontSize: '11px' }}>filter mismatch</Tag>;
        } else if (isIndexOutOfRange) {
          icon = <CloseCircleOutlined style={{ color: '#faad14', marginRight: 8 }} />;
          color = '#faad14';
          statusTag = <Tag color="warning" style={{ marginLeft: 8, fontSize: '11px' }}>index out of range</Tag>;
        } else if (isMissingArray) {
          icon = <CloseCircleOutlined style={{ color: '#ff4d4f', marginRight: 8 }} />;
          color = '#ff4d4f';
          statusTag = <Tag color="error" style={{ marginLeft: 8, fontSize: '11px' }}>array missing</Tag>;
        } else {
          // MISSING_PROPERTY
          icon = <CloseCircleOutlined style={{ color: '#ff4d4f', marginRight: 8 }} />;
          color = '#ff4d4f';
          statusTag = <Tag color="error" style={{ marginLeft: 8, fontSize: '11px' }}>property missing</Tag>;
        }

        return (
          <div
            key={idx}
            style={{
              paddingLeft: idx * 20,
              marginBottom: 4,
              display: 'flex',
              alignItems: 'center',
            }}
          >
            {icon}
            
            <Text
              code
              style={{
                color,
                fontWeight: isSuccess ? 'normal' : 'bold',
              }}
            >
              {segStatus.segment.raw}
            </Text>

            {statusTag}

            {isSuccess && segStatus.node !== undefined && (
              <Text type="secondary" style={{ marginLeft: 8, fontSize: 12 }}>
                {typeof segStatus.node === 'object'
                  ? Array.isArray(segStatus.node)
                    ? `[array: ${segStatus.node.length} items]`
                    : '[object]'
                  : `= ${JSON.stringify(segStatus.node)}`}
              </Text>
            )}
          </div>
        );
      ))}
    </div>
  );
};
*/

export default ErrorHelperPanel;
