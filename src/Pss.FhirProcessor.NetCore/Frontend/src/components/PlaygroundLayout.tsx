import React, { useState, useEffect, useRef, useMemo, useCallback } from 'react';
import { Card, Button, Select, Switch, Tabs, Alert, Spin, message, Space } from 'antd';
import {
  PlayCircleOutlined,
  ClearOutlined,
  EditOutlined,
  FolderOpenOutlined,
} from '@ant-design/icons';
import { fhirApi } from '../services/api';
import { useMetadata } from '../contexts/MetadataContext';
import { useLocalStorageWidth } from '../hooks/useLocalStorageWidth';
import { useScrollToTreeNode } from '../hooks/useScrollToTreeNode';
import { Splitter } from './Splitter';
import { TreeView } from './TreeView';
import { ValidationErrorCard } from './ValidationErrorCard';
import { JsonEditorModal } from './JsonEditorModal';
import MetadataEditor from './MetadataEditor';
import type { ProcessingResult, SampleFile, ResourcePointer } from '../types/fhir';
import './PlaygroundLayout.css';

const { Option } = Select;

const STORAGE_KEY = 'pss_playground_panel_width';
const DEFAULT_WIDTH = 50;

/**
 * Main Playground Layout Component
 * Provides a clean 2-panel interface for FHIR bundle processing
 */
export const PlaygroundLayout: React.FC = () => {
  // Metadata context
  const { ruleSets, codesMaster, version, pathSyntax } = useMetadata();

  // State management
  const [fhirJson, setFhirJson] = useState('');
  const [logLevel, setLogLevel] = useState('verbose');
  const [strictDisplay, setStrictDisplay] = useState(true);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<ProcessingResult | null>(null);
  const [sampleFiles, setSampleFiles] = useState<SampleFile[]>([]);
  const [selectedSample, setSelectedSample] = useState<string | null>(null);
  const [isEditorModalOpen, setIsEditorModalOpen] = useState(false);

  // Panel width management
  const [leftWidth, setLeftWidth] = useLocalStorageWidth({
    key: STORAGE_KEY,
    defaultValue: DEFAULT_WIDTH,
    min: 30,
    max: 70,
  });

  // Refs
  const treeContainerRef = useRef<HTMLDivElement>(null);
  const scrollToTreeNode = useScrollToTreeNode(treeContainerRef);

  // Load sample files on mount
  useEffect(() => {
    loadSampleFiles();
  }, []);

  const loadSampleFiles = async () => {
    try {
      const modules = (import.meta as any).glob('/src/seed/happy-*.json');
      const files: SampleFile[] = [];

      for (const path in modules) {
        const fileName = path.split('/').pop() || '';
        const displayName = fileName
          .replace('happy-', '')
          .replace('.json', '')
          .replace(/-/g, ' ')
          .replace(/\b\w/g, (l) => l.toUpperCase());

        files.push({
          name: displayName,
          fileName,
          path,
          loader: modules[path] as () => Promise<any>,
        });
      }

      setSampleFiles(files);
      if (files.length > 0) {
        setSelectedSample(files[0].fileName);
      }
    } catch (error) {
      console.error('Failed to load sample files:', error);
      message.error('Failed to load sample files');
    }
  };

  const handleLoadSample = async () => {
    if (!selectedSample) {
      message.warning('Please select a sample file');
      return;
    }

    try {
      const sampleFile = sampleFiles.find((f) => f.fileName === selectedSample);
      if (!sampleFile) {
        message.error('Sample file not found');
        return;
      }

      const module = await sampleFile.loader();
      const sampleData = module.default;
      setFhirJson(JSON.stringify(sampleData, null, 2));
      message.success(`Loaded: ${sampleFile.name}`);
    } catch (error) {
      console.error('Failed to load sample:', error);
      message.error('Failed to load sample file');
    }
  };

  const handleProcess = async () => {
    if (!fhirJson.trim()) {
      message.warning('Please enter FHIR JSON');
      return;
    }

    try {
      setLoading(true);

      const validationMetadata = {
        Version: version || '5.0',
        PathSyntax: pathSyntax || 'CPS1',
        RuleSets: ruleSets,
        CodesMaster: codesMaster,
      };

      const data = await fhirApi.process(
        fhirJson,
        JSON.stringify(validationMetadata),
        logLevel,
        strictDisplay
      );

      setResult(data);
      message.success('Processing completed');
    } catch (error: any) {
      console.error('Processing error:', error);
      message.error(error.response?.data?.error || error.message || 'Processing failed');
    } finally {
      setLoading(false);
    }
  };

  const handleClear = () => {
    setFhirJson('');
    setResult(null);
    message.info('Cleared all data');
  };

  const handleOpenEditor = () => {
    setIsEditorModalOpen(true);
  };

  const handleCloseEditor = () => {
    setIsEditorModalOpen(false);
  };

  const handleApplyJsonChanges = useCallback((newJsonString: string, parsedJson: any) => {
    setFhirJson(newJsonString);
    
    // Auto-process after applying changes
    setTimeout(() => {
      handleProcess();
    }, 100);
  }, []);

  const handleGoToResource = useCallback((resourcePointer: ResourcePointer) => {
    if (!resourcePointer || resourcePointer.entryIndex === undefined) {
      message.warning('Resource location not available');
      return;
    }

    if (resourcePointer.resolvedTreePath) {
      scrollToTreeNode(resourcePointer.resolvedTreePath);
    }

    message.success(
      `Navigated to ${resourcePointer.resourceType} (Entry #${resourcePointer.entryIndex})`
    );
  }, [scrollToTreeNode]);

  return (
    <div className="playground-layout">
      <Card
        title="FHIR Processor Playground"
        className="playground-card"
        extra={<MetadataEditor />}
      >
        {/* Control Bar */}
        <div className="control-bar">
          <Space wrap size="middle">
            <div className="control-item">
              <label className="control-label">Log Level:</label>
              <Select value={logLevel} onChange={setLogLevel} style={{ width: 120 }}>
                <Option value="error">Error</Option>
                <Option value="warn">Warning</Option>
                <Option value="info">Info</Option>
                <Option value="debug">Debug</Option>
                <Option value="verbose">Verbose</Option>
              </Select>
            </div>

            <div className="control-item">
              <label className="control-label">Strict Display:</label>
              <Switch checked={strictDisplay} onChange={setStrictDisplay} />
            </div>

            <div className="control-item">
              <label className="control-label">Sample File:</label>
              <Space.Compact>
                <Select
                  value={selectedSample}
                  onChange={setSelectedSample}
                  style={{ width: 200 }}
                  placeholder="Select a sample"
                >
                  {sampleFiles.map((file) => (
                    <Option key={file.fileName} value={file.fileName}>
                      {file.name}
                    </Option>
                  ))}
                </Select>
                <Button
                  icon={<FolderOpenOutlined />}
                  onClick={handleLoadSample}
                  disabled={!selectedSample}
                >
                  Load
                </Button>
              </Space.Compact>
            </div>
          </Space>
        </div>

        {/* Action Buttons */}
        <div className="action-bar">
          <Space>
            <Button
              type="primary"
              size="large"
              icon={<PlayCircleOutlined />}
              onClick={handleProcess}
              loading={loading}
            >
              Process
            </Button>
            <Button size="large" icon={<ClearOutlined />} onClick={handleClear}>
              Clear
            </Button>
            <Button
              size="large"
              icon={<EditOutlined />}
              onClick={handleOpenEditor}
            >
              Edit JSON Input
            </Button>
          </Space>
        </div>

        {/* Split Panel Layout */}
        <div className="split-layout">
          {/* Left Panel: Processing Results */}
          <div className="left-panel" style={{ width: `${leftWidth}%` }}>
            {loading && (
              <div className="loading-container">
                <Spin size="large" tip="Processing FHIR Bundle..." />
              </div>
            )}

            {result && !loading && (
              <div className="results-container">
                <Tabs
                  defaultActiveKey="validation"
                  className="results-tabs"
                  items={[
                    {
                      key: 'validation',
                      label: `Validation ${
                        result.validation?.errors?.length
                          ? `(${result.validation.errors.length})`
                          : ''
                      }`,
                      children: (
                        <div className="tab-content">
                          <Alert
                            message={
                              result.validation?.isValid
                                ? 'Validation Passed'
                                : 'Validation Failed'
                            }
                            type={result.validation?.isValid ? 'success' : 'error'}
                            showIcon
                            className="validation-status"
                          />
                          {result.validation?.errors &&
                          result.validation.errors.length > 0 ? (
                            <div className="errors-list">
                              {result.validation.errors.map((error, idx) => (
                                <ValidationErrorCard
                                  key={idx}
                                  error={error}
                                  index={idx}
                                  onGoToResource={handleGoToResource}
                                />
                              ))}
                            </div>
                          ) : result.validation?.isValid ? (
                            <Alert
                              message="✓ No validation errors found"
                              type="success"
                              showIcon
                              className="success-message"
                            />
                          ) : (
                            <Alert message="No error details available" type="warning" />
                          )}
                        </div>
                      ),
                    },
                    {
                      key: 'extraction',
                      label: 'Extraction',
                      children: (
                        <div className="tab-content">
                          <pre className="json-display">
                            {result.flatten
                              ? JSON.stringify(result.flatten, null, 2)
                              : 'No extraction data'}
                          </pre>
                        </div>
                      ),
                    },
                    {
                      key: 'originalBundle',
                      label: 'Original Bundle',
                      children: (
                        <div className="tab-content">
                          <pre className="json-display">
                            {result.originalBundle
                              ? JSON.stringify(result.originalBundle, null, 2)
                              : 'No original bundle data'}
                          </pre>
                        </div>
                      ),
                    },
                    {
                      key: 'logs',
                      label: 'Logs',
                      children: (
                        <div className="tab-content">
                          <div className="logs-display">
                            {result.logs && result.logs.length > 0 ? (
                              result.logs.map((log, idx) => <div key={idx}>{log}</div>)
                            ) : (
                              <div>No logs available</div>
                            )}
                          </div>
                        </div>
                      ),
                    },
                  ]}
                />
              </div>
            )}

            {!result && !loading && (
              <div className="empty-state">
                <EditOutlined style={{ fontSize: 64, color: '#d9d9d9' }} />
                <h3>No Data Processed Yet</h3>
                <p>Load a sample file or edit JSON input to begin processing</p>
                <Button
                  type="primary"
                  size="large"
                  icon={<EditOutlined />}
                  onClick={handleOpenEditor}
                >
                  Edit JSON Input
                </Button>
              </div>
            )}
          </div>

          {/* Splitter */}
          <Splitter onWidthChange={setLeftWidth} minWidth={30} maxWidth={70} />

          {/* Right Panel: Tree View */}
          <div className="right-panel" ref={treeContainerRef}>
            <TreeView jsonData={fhirJson} />
          </div>
        </div>
      </Card>

      {/* JSON Editor Modal */}
      <JsonEditorModal
        isOpen={isEditorModalOpen}
        initialValue={fhirJson}
        onClose={handleCloseEditor}
        onApply={handleApplyJsonChanges}
      />
    </div>
  );
};
