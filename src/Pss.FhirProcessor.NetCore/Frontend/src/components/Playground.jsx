import { useState, useEffect, useMemo } from 'react';
import { Card, Button, Input, Select, Switch, Tabs, Alert, Spin, message, Tree } from 'antd';
import { PlayCircleOutlined, ClearOutlined, FolderOutlined, FileOutlined } from '@ant-design/icons';
import { fhirApi } from '../services/api';
import { useMetadata } from '../contexts/MetadataContext';
import MetadataEditor from './MetadataEditor';
import { generateSampleBundle } from '../utils/sampleDataGenerator';

const { TextArea } = Input;
const { Option } = Select;

function Playground() {
  const { ruleSets, codesMaster, version, pathSyntax } = useMetadata();
  const [fhirJson, setFhirJson] = useState('');
  const [logLevel, setLogLevel] = useState('verbose');
  const [strictDisplay, setStrictDisplay] = useState(true);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);
  const [sampleFiles, setSampleFiles] = useState([]);
  const [selectedSample, setSelectedSample] = useState(null);

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

  return (
    <div className="space-y-4">
      <Card title="FHIR Processor Playground" className="shadow-md" extra={<MetadataEditor />}>
        <div className="space-y-4">
          <div className="flex gap-4 items-center">
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
          </div>

          <div>
            <label className="block mb-2 font-medium">FHIR JSON Input:</label>
            <div className="flex gap-2 mb-2">
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
            
            <div className="grid grid-cols-2 gap-4">
              <div>
                <div className="mb-1 text-sm font-medium text-gray-600">JSON Editor</div>
                <TextArea
                  value={fhirJson}
                  onChange={(e) => setFhirJson(e.target.value)}
                  placeholder="Paste your FHIR Bundle JSON here..."
                  rows={20}
                  className="font-mono text-sm"
                />
              </div>
              
              <div>
                <div className="mb-1 text-sm font-medium text-gray-600">Tree View</div>
                <div className="border border-gray-300 rounded p-2 bg-white overflow-auto" style={{ height: '500px' }}>
                  {treeData.length > 0 ? (
                    <Tree
                      treeData={treeData}
                      defaultExpandedKeys={defaultExpandedKeys}
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
        </div>
      </Card>

      {loading && (
        <Card className="shadow-md">
          <div className="text-center py-8">
            <Spin size="large" tip="Processing FHIR Bundle..." />
          </div>
        </Card>
      )}

      {result && !loading && (
        <Card title="Processing Results" className="shadow-md">
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
                        <h3 className="font-semibold text-lg mb-3">Validation Errors ({result.validation.errors.length}):</h3>
                        {result.validation.errors.map((error, idx) => {
                          // Split message by pipe to separate main message from details
                          const messageParts = error.message.split('|').map(part => part.trim());
                          const mainMessage = messageParts[0];
                          const details = messageParts.slice(1);
                          
                          return (
                            <Alert 
                              key={idx} 
                              message={
                                <div>
                                  <div className="flex items-start gap-2 mb-2">
                                    <strong className="text-red-600 text-base">[{error.code}]</strong>
                                    <span className="font-medium text-base">{error.fieldPath}</span>
                                  </div>
                                  <div className="text-sm mt-2 text-gray-800 font-medium">{mainMessage}</div>
                                  {details.length > 0 && (
                                    <div className="mt-2 pl-4 border-l-2 border-gray-300">
                                      {details.map((detail, detailIdx) => (
                                        <div key={detailIdx} className="text-xs text-gray-600 font-mono bg-gray-50 p-2 rounded mt-1">
                                          {detail}
                                        </div>
                                      ))}
                                    </div>
                                  )}
                                  <div className="text-xs text-gray-500 mt-2">
                                    <span className="font-semibold">Scope:</span> {error.scope}
                                  </div>
                                </div>
                              }
                              type="error"
                              className="mb-2"
                            />
                          );
                        })}
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
                label: 'Logs',
                children: (
                  <div className="bg-gray-900 text-green-400 p-4 rounded font-mono text-sm overflow-auto max-h-96">
                    {result?.logs && result.logs.length > 0 ? (
                      result.logs.map((log, idx) => (
                        <div key={idx}>{log}</div>
                      ))
                    ) : (
                      <div>No logs available</div>
                    )}
                  </div>
                )
              }
            ]}
          />
        </Card>
      )}
    </div>
  );
}

export default Playground;
