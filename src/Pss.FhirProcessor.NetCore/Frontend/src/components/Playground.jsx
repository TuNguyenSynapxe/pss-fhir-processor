import { useState, useEffect, useMemo, useRef } from 'react';
import { Card, Button, Select, Switch, Tabs, Alert, Spin, message, Tree } from 'antd';
import { PlayCircleOutlined, ClearOutlined, FolderOutlined, FileOutlined, EditOutlined, ExpandOutlined, ShrinkOutlined } from '@ant-design/icons';
import { fhirApi } from '../services/api';
import { useMetadata } from '../contexts/MetadataContext';
import MetadataEditor from './MetadataEditor';
import ValidationHelper from './ValidationHelper';
import JsonEditorModal from './JsonEditorModal';
import LogsPanel from './LogsPanel';
import { generateSampleBundle } from '../utils/sampleDataGenerator';

const { Option } = Select;

// LocalStorage key for persisting left panel width
const STORAGE_KEY = 'pss_left_panel_width';
const DEFAULT_LEFT_WIDTH = '50';

function Playground() {
  const { ruleSets, codesMaster, version, pathSyntax } = useMetadata();
  const [fhirJson, setFhirJson] = useState('');
  const [logLevel, setLogLevel] = useState('verbose');
  const [strictDisplay, setStrictDisplay] = useState(true);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);
  const [sampleFiles, setSampleFiles] = useState([]);
  const [selectedSample, setSelectedSample] = useState(null);
  const [isEditorModalOpen, setIsEditorModalOpen] = useState(false);
  const [expandedKeys, setExpandedKeys] = useState([]);
  
  // Splitter state
  const [leftWidth, setLeftWidth] = useState(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? parseFloat(stored) : parseFloat(DEFAULT_LEFT_WIDTH);
  });
  const dragging = useRef(false);
  const treeContainerRef = useRef(null);

  // Convert JSON to tree data structure
  const jsonToTreeData = (obj, parentKey = 'root') => {
    if (obj === null || obj === undefined) {
      return [];
    }

    const buildNode = (value, key, path) => {
      const nodeKey = `${path}.${key}`;
      
      if (Array.isArray(value)) {
        return {
          title: (
            <span>
              <strong>{key}</strong>: <span className="text-blue-600">[Array({value.length})]</span>
            </span>
          ),
          key: nodeKey,
          icon: <FolderOutlined />,
          children: value.map((item, idx) => buildNode(item, `[${idx}]`, nodeKey))
        };
      } else if (typeof value === 'object' && value !== null) {
        const keys = Object.keys(value);
        return {
          title: (
            <span>
              <strong>{key}</strong>: <span className="text-purple-600">{`{Object}`}</span>
            </span>
          ),
          key: nodeKey,
          icon: <FolderOutlined />,
          children: keys.map(k => buildNode(value[k], k, nodeKey))
        };
      } else {
        const valueStr = String(value);
        const displayValue = valueStr.length > 50 ? valueStr.substring(0, 50) + '...' : valueStr;
        const valueColor = typeof value === 'string' ? 'text-green-600' : 
                          typeof value === 'number' ? 'text-orange-600' : 
                          typeof value === 'boolean' ? 'text-red-600' : 'text-gray-600';
        
        return {
          title: (
            <span>
              <strong>{key}</strong>: <span className={valueColor}>"{displayValue}"</span>
            </span>
          ),
          key: nodeKey,
          icon: <FileOutlined />,
          isLeaf: true
        };
      }
    };

    try {
      const parsed = typeof obj === 'string' ? JSON.parse(obj) : obj;
      const keys = Object.keys(parsed);
      return keys.map(key => buildNode(parsed[key], key, parentKey));
    } catch (error) {
      return [{
        title: <span className="text-red-600">Invalid JSON</span>,
        key: 'error',
        isLeaf: true,
        icon: <FileOutlined />
      }];
    }
  };

  // Memoize tree data to avoid rebuilding on every render
  const treeData = useMemo(() => {
    if (!fhirJson.trim()) return [];
    try {
      return jsonToTreeData(fhirJson);
    } catch {
      return [];
    }
  }, [fhirJson]);

  // Generate keys for default expansion - expand root level and all children under 'entry'
  const defaultExpandedKeys = useMemo(() => {
    const keys = [];
    const collectKeys = (nodes, parentKey = '') => {
      nodes.forEach(node => {
        if (!node.isLeaf) {
          // Always expand root level
          const isRootLevel = node.key.split('.').length === 2;
          // Always expand if we're inside the entry array or it's a root-level node
          const isUnderEntry = node.key.includes('.entry.');
          
          if (isRootLevel || isUnderEntry) {
            keys.push(node.key);
            if (node.children) {
              collectKeys(node.children, node.key);
            }
          }
        }
      });
    };
    collectKeys(treeData);
    return keys;
  }, [treeData]);

  // Collect all keys from tree for expand all functionality
  const allTreeKeys = useMemo(() => {
    const keys = [];
    const collectAllKeys = (nodes) => {
      nodes.forEach(node => {
        if (!node.isLeaf) {
          keys.push(node.key);
          if (node.children) {
            collectAllKeys(node.children);
          }
        }
      });
    };
    collectAllKeys(treeData);
    return keys;
  }, [treeData]);

  // Initialize expanded keys when tree data changes
  useEffect(() => {
    setExpandedKeys(defaultExpandedKeys);
  }, [defaultExpandedKeys]);

  // Handlers for expand/collapse
  const handleExpandAll = () => {
    setExpandedKeys(allTreeKeys);
    message.success('All nodes expanded');
  };

  const handleCollapseAll = () => {
    setExpandedKeys([]);
    message.success('All nodes collapsed');
  };

  const handleTreeExpand = (keys) => {
    setExpandedKeys(keys);
  };

  // Load all happy-*.json files from seed folder
  useEffect(() => {
    const loadSampleFiles = async () => {
      try {
        // Import all files matching happy-*.json pattern
        const modules = import.meta.glob('/src/seed/happy-*.json');
        const files = [];
        
        for (const path in modules) {
          const fileName = path.split('/').pop(); // Get filename from path
          const displayName = fileName
            .replace('happy-', '')
            .replace('.json', '')
            .replace(/-/g, ' ')
            .replace(/\b\w/g, l => l.toUpperCase()); // Capitalize words
          
          files.push({
            name: displayName,
            fileName: fileName,
            path: path,
            loader: modules[path]
          });
        }
        
        setSampleFiles(files);
        
        // Auto-select first file if available
        if (files.length > 0) {
          setSelectedSample(files[0].fileName);
        }
      } catch (error) {
        console.error('Failed to load sample files:', error);
      }
    };
    
    loadSampleFiles();
  }, []);

  const handleProcess = async () => {
    if (!fhirJson.trim()) {
      message.warning('Please enter FHIR JSON');
      return;
    }

    try {
      setLoading(true);
      console.log('=== PROCESSING REQUEST ===');
      console.log('RuleSets count:', ruleSets?.length);
      console.log('CodesMaster questions:', codesMaster?.Questions?.length);
      console.log('FHIR JSON length:', fhirJson.length);
      
      // Build v5 metadata from context
      const validationMetadata = {
        Version: version || '5.0',
        PathSyntax: pathSyntax || 'CPS1',
        RuleSets: ruleSets,
        CodesMaster: codesMaster
      };
      
      console.log('Sending validation metadata:', JSON.stringify(validationMetadata, null, 2));
      
      const data = await fhirApi.process(
        fhirJson, 
        JSON.stringify(validationMetadata),
        logLevel,
        strictDisplay
      );
      
      console.log('=== API RESPONSE ===');
      console.log('Success:', data.success);
      console.log('Validation errors:', data.validation?.errors?.length || 0);
      console.log('Response keys:', Object.keys(data));
      
      // Log detailed error information
      if (data.validation?.errors && data.validation.errors.length > 0) {
        console.log('\n=== DETAILED VALIDATION ERRORS ===');
        data.validation.errors.forEach((error, idx) => {
          console.log(`\nError #${idx + 1}:`);
          console.log(`  Code: ${error.code}`);
          console.log(`  Scope: ${error.scope}`);
          console.log(`  Field: ${error.fieldPath}`);
          console.log(`  Message: ${error.message}`);
        });
      }
      
      setResult(data);
      message.success('Processing completed');
    } catch (error) {
      console.error('=== API ERROR ===');
      console.error('Status:', error.response?.status);
      console.error('Error data:', error.response?.data);
      console.error('Error message:', error.message);
      console.error('Full error:', error);
      
      message.error(error.response?.data?.error || error.message || 'Processing failed');
    } finally {
      setLoading(false);
    }
  };

  const handleClear = () => {
    setFhirJson('');
    setResult(null);
  };

  // Splitter drag handlers
  const startDragging = (e) => {
    e.preventDefault();
    dragging.current = true;
    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';
  };

  const stopDragging = () => {
    if (dragging.current) {
      dragging.current = false;
      document.body.style.cursor = '';
      document.body.style.userSelect = '';
    }
  };

  const handleMouseMove = (e) => {
    if (!dragging.current) return;

    const percentage = (e.clientX / window.innerWidth) * 100;
    const clamped = Math.min(Math.max(percentage, 20), 80);

    setLeftWidth(clamped);
    localStorage.setItem(STORAGE_KEY, clamped.toString());
  };

  // Attach global mouse listeners for dragging
  useEffect(() => {
    window.addEventListener('mousemove', handleMouseMove);
    window.addEventListener('mouseup', stopDragging);

    return () => {
      window.removeEventListener('mousemove', handleMouseMove);
      window.removeEventListener('mouseup', stopDragging);
    };
  }, []);

  // Handle "Go to Resource" navigation
  const goToResource = (resourcePointer) => {
    if (!resourcePointer || resourcePointer.entryIndex === undefined) {
      message.warning('Resource location not available');
      return;
    }

    // Scroll to the tree node if we have a resolved path
    if (resourcePointer.resolvedTreePath && treeContainerRef.current) {
      const targetNode = treeContainerRef.current.querySelector(
        `[data-tree-key="${resourcePointer.resolvedTreePath}"]`
      );
      
      if (targetNode) {
        targetNode.scrollIntoView({ behavior: 'smooth', block: 'center' });
        
        // Add flash highlight
        targetNode.classList.add('flash-highlight');
        setTimeout(() => {
          targetNode.classList.remove('flash-highlight');
        }, 1200);
      }
    }

    message.success(`Navigated to ${resourcePointer.resourceType} (Entry #${resourcePointer.entryIndex})`);
  };

  const loadSample = async () => {
    if (!selectedSample) {
      message.warning('Please select a sample file');
      return;
    }

    try {
      // Find the selected sample file
      const sampleFile = sampleFiles.find(f => f.fileName === selectedSample);
      
      if (!sampleFile) {
        message.error('Sample file not found');
        return;
      }

      // Load the JSON file
      const module = await sampleFile.loader();
      const sampleData = module.default;
      
      setFhirJson(JSON.stringify(sampleData, null, 2));
      message.success(`Loaded: ${sampleFile.name}`);
    } catch (error) {
      console.error('Failed to load sample:', error);
      message.error('Failed to load sample file');
    }
  };

  const handleOpenEditor = () => {
    setIsEditorModalOpen(true);
  };

  const handleCloseEditor = () => {
    setIsEditorModalOpen(false);
  };

  const handleApplyJsonChanges = (newJsonString, parsedJson) => {
    setFhirJson(newJsonString);
    // Trigger re-processing automatically
    setTimeout(() => {
      handleProcess();
    }, 100);
  };

  return (
    <div className="playground-wrapper">
      <Card title="FHIR Processor Playground" className="shadow-md mb-4" extra={<MetadataEditor />}>
        <div className="space-y-4">
          {/* Controls bar */}
          <div className="flex gap-4 items-center flex-wrap">
            <div>
              <label className="block mb-1 font-medium">Log Level:</label>
              <Select value={logLevel} onChange={setLogLevel} style={{ width: 120 }}>
                <Option value="error">Error</Option>
                <Option value="warn">Warning</Option>
                <Option value="info">Info</Option>
                <Option value="debug">Debug</Option>
                <Option value="verbose">Verbose</Option>
              </Select>
            </div>
            <div>
              <label className="block mb-1 font-medium">Strict Display Match:</label>
              <Switch checked={strictDisplay} onChange={setStrictDisplay} />
            </div>
            <div>
              <label className="block mb-1 font-medium">Sample Files:</label>
              <div className="flex gap-2">
                <Select
                  value={selectedSample}
                  onChange={setSelectedSample}
                  style={{ width: 250 }}
                  placeholder="Select a sample file"
                >
                  {sampleFiles.map(file => (
                    <Option key={file.fileName} value={file.fileName}>
                      {file.name}
                    </Option>
                  ))}
                </Select>
                <Button onClick={loadSample} disabled={!selectedSample}>
                  Load Sample
                </Button>
              </div>
            </div>
          </div>

          {/* Action buttons */}
          <div className="flex gap-2">
            <Button
              type="primary"
              icon={<PlayCircleOutlined />}
              onClick={handleProcess}
              loading={loading}
              size="large"
            >
              Process
            </Button>
            <Button
              icon={<ClearOutlined />}
              onClick={handleClear}
              size="large"
            >
              Clear
            </Button>
          </div>

          {/* Split layout: Processing Results (left) + Tree View (right) */}
          <div className="split-container">
            <div className="left-panel" style={{ width: `${leftWidth}%` }}>
              {loading && (
                <div className="text-center py-8 bg-white rounded border">
                  <Spin size="large" tip="Processing FHIR Bundle..." />
                </div>
              )}

              {result && !loading && (
                <div className="processing-results">
                  <div className="font-semibold text-lg mb-3">Processing Results</div>
                  <Tabs 
                    defaultActiveKey="validation"
                    items={[
                      {
                        key: 'validation',
                        label: 'Validation',
                        children: (
                          <>
                            <Alert
                              message={result?.validation?.isValid ? 'Validation Passed' : 'Validation Failed'}
                              type={result?.validation?.isValid ? 'success' : 'error'}
                              showIcon
                              className="mb-4"
                            />
                            {result?.validation?.errors && Array.isArray(result.validation.errors) && result.validation.errors.length > 0 ? (
                              <div className="space-y-2">
                                <h3 className="font-semibold text-base mb-3">Validation Errors ({result.validation.errors.length}):</h3>
                                {result.validation.errors.map((error, idx) => (
                                  <ValidationHelper key={idx} error={error} onGoToResource={goToResource} />
                                ))}
                              </div>
                            ) : result?.validation?.isValid ? (
                              <Alert message="✓ No validation errors found" type="success" showIcon />
                            ) : (
                              <Alert 
                                message="No error details available" 
                                type="warning" 
                              />
                            )}
                          </>
                        )
                      },
                      {
                        key: 'extraction',
                        label: 'Extraction',
                        children: (
                          <pre className="bg-gray-100 p-4 rounded overflow-auto max-h-96">
                            {result?.flatten ? JSON.stringify(result.flatten, null, 2) : 'No extraction data'}
                          </pre>
                        )
                      },
                      {
                        key: 'originalBundle',
                        label: 'Original Bundle',
                        children: (
                          <pre className="bg-gray-100 p-4 rounded overflow-auto max-h-96">
                            {result?.originalBundle ? JSON.stringify(result.originalBundle, null, 2) : 'No original bundle data'}
                          </pre>
                        )
                      },
                      {
                        key: 'logs',
                        label: `Logs (${result?.logs?.length || 0})`,
                        children: (
                          <LogsPanel logs={result?.logs || []} />
                        )
                      }
                    ]}
                  />
                  
                  {/* Edit JSON button at the bottom of results */}
                  <div className="mt-4 pt-4 border-t">
                    <Button
                      icon={<EditOutlined />}
                      onClick={handleOpenEditor}
                      size="large"
                      block
                    >
                      Edit JSON Input
                    </Button>
                  </div>
                </div>
              )}

              {!result && !loading && (
                <div className="flex flex-col items-center justify-center py-12 bg-white rounded border">
                  <div className="text-gray-400 text-center mb-4">
                    <div className="text-lg mb-2">No data processed yet</div>
                    <div className="text-sm">Load a sample file or edit JSON input to begin</div>
                  </div>
                  <Button
                    type="primary"
                    icon={<EditOutlined />}
                    onClick={handleOpenEditor}
                    size="large"
                  >
                    Edit JSON Input
                  </Button>
                </div>
              )}
            </div>
            
            <div 
              className="splitter" 
              onMouseDown={startDragging}
              title="Drag to resize"
            />
            
            <div className="right-panel">
              <div className="sticky-header">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span>Tree View</span>
                  <div style={{ display: 'flex', gap: '8px' }}>
                    <Button 
                      size="small" 
                      icon={<ExpandOutlined />}
                      onClick={handleExpandAll}
                      disabled={treeData.length === 0}
                    >
                      Expand All
                    </Button>
                    <Button 
                      size="small" 
                      icon={<ShrinkOutlined />}
                      onClick={handleCollapseAll}
                      disabled={treeData.length === 0}
                    >
                      Collapse All
                    </Button>
                  </div>
                </div>
              </div>
              <div ref={treeContainerRef} className="tree-content">
                {treeData.length > 0 ? (
                  <Tree
                    treeData={treeData}
                    expandedKeys={expandedKeys}
                    onExpand={handleTreeExpand}
                    showIcon
                    className="text-sm"
                  />
                ) : (
                  <div className="text-gray-400 text-center py-8">
                    No JSON data to display
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </Card>

      {/* JSON Editor Modal */}
      <JsonEditorModal
        isOpen={isEditorModalOpen}
        onClose={handleCloseEditor}
        onApply={handleApplyJsonChanges}
        initialValue={fhirJson}
      />
    </div>
  );
}

export default Playground;
