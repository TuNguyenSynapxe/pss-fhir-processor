/**
 * Example integration of ErrorHelperPanel into ValidationTab
 * This shows how to use the error helper system in the playground
 */

import React, { useState } from 'react';
import { Card, List, Badge, Tag, Alert, Space, Button } from 'antd';
import { WarningOutlined, CheckCircleOutlined, InfoCircleOutlined } from '@ant-design/icons';
import ErrorHelperPanel from './ErrorHelperPanel';
import { ValidationError } from '../utils/helperGenerator';

interface ValidationTabWithHelperProps {
  validationResult?: {
    isValid: boolean;
    errors: ValidationError[];
    summary?: string;
  };
  jsonInput: any;
  onJumpToLocation?: (entryIndex?: number, path?: string) => void;
}

/**
 * Enhanced Validation Tab with Error Helper Panel
 */
export const ValidationTabWithHelper: React.FC<ValidationTabWithHelperProps> = ({
  validationResult,
  jsonInput,
  onJumpToLocation,
}) => {
  const [expandedErrorIndex, setExpandedErrorIndex] = useState<number | null>(null);

  if (!validationResult) {
    return (
      <Alert
        message="No validation results"
        description="Process a FHIR bundle to see validation results."
        type="info"
        showIcon
      />
    );
  }

  const { isValid, errors, summary } = validationResult;

  // Group errors by severity/code for better organization
  const mandatoryErrors = errors.filter(e => e.code.includes('MANDATORY') || e.code.includes('MISSING'));
  const formatErrors = errors.filter(e => e.code.includes('TYPE') || e.code.includes('REGEX'));
  const valueErrors = errors.filter(e => e.code.includes('MISMATCH') || e.code.includes('INVALID'));
  const referenceErrors = errors.filter(e => e.code.includes('REF'));
  const otherErrors = errors.filter(e => 
    !mandatoryErrors.includes(e) && 
    !formatErrors.includes(e) && 
    !valueErrors.includes(e) && 
    !referenceErrors.includes(e)
  );

  const errorGroups = [
    { title: 'Missing Required Fields', errors: mandatoryErrors, color: 'red' },
    { title: 'Value Mismatches', errors: valueErrors, color: 'orange' },
    { title: 'Format Errors', errors: formatErrors, color: 'gold' },
    { title: 'Reference Errors', errors: referenceErrors, color: 'purple' },
    { title: 'Other Issues', errors: otherErrors, color: 'default' },
  ].filter(group => group.errors.length > 0);

  return (
    <div style={{ padding: 16 }}>
      {/* Summary */}
      {isValid ? (
        <Alert
          message="Validation Passed"
          description={summary || 'All validation rules passed successfully.'}
          type="success"
          icon={<CheckCircleOutlined />}
          showIcon
          style={{ marginBottom: 16 }}
        />
      ) : (
        <Alert
          message={`Validation Failed - ${errors.length} Error(s)`}
          description={summary || `Found ${errors.length} validation error(s). Click on each error below for detailed help.`}
          type="error"
          icon={<WarningOutlined />}
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Error Groups */}
      {errorGroups.map((group, groupIdx) => (
        <Card
          key={groupIdx}
          title={
            <Space>
              <Badge count={group.errors.length} style={{ backgroundColor: group.color }} />
              {group.title}
            </Space>
          }
          style={{ marginBottom: 16 }}
          size="small"
        >
          <List
            dataSource={group.errors}
            renderItem={(error, idx) => {
              const globalIdx = errors.indexOf(error);
              const isExpanded = expandedErrorIndex === globalIdx;

              return (
                <List.Item
                  key={globalIdx}
                  style={{
                    flexDirection: 'column',
                    alignItems: 'stretch',
                    border: isExpanded ? '2px solid #1890ff' : 'none',
                    borderRadius: 4,
                    padding: 12,
                    marginBottom: 8,
                  }}
                >
                  {/* Error Summary */}
                  <div
                    style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      cursor: 'pointer',
                    }}
                    onClick={() => setExpandedErrorIndex(isExpanded ? null : globalIdx)}
                  >
                    <Space direction="vertical" style={{ flex: 1 }}>
                      <Space>
                        <Tag color="error">{error.code}</Tag>
                        {error.scope && <Tag color="blue">{error.scope}</Tag>}
                        {error.resourcePointer?.entryIndex !== undefined && (
                          <Tag>Entry #{error.resourcePointer.entryIndex}</Tag>
                        )}
                      </Space>
                      <div style={{ fontWeight: 500 }}>{error.message}</div>
                      {error.fieldPath && (
                        <code style={{ fontSize: 12, color: '#666' }}>{error.fieldPath}</code>
                      )}
                    </Space>
                    <Button
                      type="link"
                      icon={<InfoCircleOutlined />}
                      onClick={(e) => {
                        e.stopPropagation();
                        setExpandedErrorIndex(isExpanded ? null : globalIdx);
                      }}
                    >
                      {isExpanded ? 'Hide Help' : 'Show Help'}
                    </Button>
                  </div>

                  {/* Helper Panel */}
                  {isExpanded && (
                    <div style={{ marginTop: 16 }}>
                      <ErrorHelperPanel
                        error={error}
                        json={jsonInput}
                        onJumpToLocation={onJumpToLocation}
                      />
                    </div>
                  )}
                </List.Item>
              );
            }}
          />
        </Card>
      ))}

      {/* Quick Actions */}
      {!isValid && (
        <Card size="small" title="Quick Actions">
          <Space>
            <Button
              type="primary"
              onClick={() => {
                // Expand all helpers
                if (expandedErrorIndex !== null) {
                  setExpandedErrorIndex(null);
                } else {
                  setExpandedErrorIndex(0);
                }
              }}
            >
              {expandedErrorIndex !== null ? 'Collapse All' : 'Expand First Error'}
            </Button>
            <Button
              onClick={() => {
                // Export errors as JSON
                const dataStr = JSON.stringify(errors, null, 2);
                const dataBlob = new Blob([dataStr], { type: 'application/json' });
                const url = URL.createObjectURL(dataBlob);
                const link = document.createElement('a');
                link.href = url;
                link.download = 'validation-errors.json';
                link.click();
              }}
            >
              Export Errors
            </Button>
          </Space>
        </Card>
      )}
    </div>
  );
};

export default ValidationTabWithHelper;
