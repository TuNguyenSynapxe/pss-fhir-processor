import React, { useState, useCallback, useRef, useEffect } from 'react';
import { message } from 'antd';
import TreeViewPanel from './TreeViewPanel';
import RightPanel from './RightPanel';
import Splitter from './Splitter';
import JsonEditorModal from './JsonEditorModal';
import { fhirApi } from '../../services/api';
import { useMetadata } from '../../contexts/MetadataContext';

const STORAGE_KEY = 'pss_playground_left_width';
const DEFAULT_LEFT_WIDTH = 40; // percentage
const MIN_WIDTH = 20;
const MAX_WIDTH = 70;

interface ProcessResult {
  success: boolean;
  validation?: {
    isValid: boolean;
    errors: ValidationError[];
  };
  flatten?: any;  // Extraction result from backend
  logs?: any[];
  originalBundle?: any;
}

interface ValidationError {
  code: string;
  message: string;
  fieldPath: string;
  scope: string;
  ruleType?: string;
  rule?: any;
  context?: any;
  resourcePointer?: {
    entryIndex: number;
    resourceType: string;
    resourceId: string;
    fullUrl: string;
  };
}

export default function PlaygroundLayout() {
  const { ruleSets, codesMaster, version, pathSyntax } = useMetadata();
  
  // State
  const [fhirJson, setFhirJson] = useState('');
  const [logLevel, setLogLevel] = useState('verbose');
  const [strictDisplay, setStrictDisplay] = useState(true);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<ProcessResult | null>(null);
  const [isEditorModalOpen, setIsEditorModalOpen] = useState(false);
  const [scrollTrigger, setScrollTrigger] = useState(0); // Trigger for tree navigation
  const [jsonTree, setJsonTree] = useState<any>(null); // Parsed JSON for error helper
  const [leftWidth, setLeftWidth] = useState(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? parseFloat(stored) : DEFAULT_LEFT_WIDTH;
  });

  // Refs for navigation
  const treeScrollTargetRef = useRef<{ entryIndex: number; fieldPath?: string } | null>(null);

  // Persist width to localStorage
  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, leftWidth.toString());
  }, [leftWidth]);

  // Handle width changes from splitter
  const handleWidthChange = useCallback((newWidth: number) => {
    const clampedWidth = Math.max(MIN_WIDTH, Math.min(MAX_WIDTH, newWidth));
    setLeftWidth(clampedWidth);
  }, []);

  // Process FHIR JSON
  const handleProcess = async () => {
    if (!fhirJson.trim()) {
      message.warning('Please enter FHIR JSON');
      return;
    }

    try {
      setLoading(true);
      
      // Parse JSON for error helper
      let parsedJson = null;
      try {
        parsedJson = JSON.parse(fhirJson);
        setJsonTree(parsedJson);
      } catch (e) {
        console.warn('Failed to parse JSON for error helper:', e);
      }
      
      console.log('=== PROCESSING V2 ===');
      console.log('FHIR JSON length:', fhirJson.length);
      console.log('Log level:', logLevel);
      console.log('Strict display:', strictDisplay);
      
      // Build metadata from context
      const validationMetadata = {
        Version: version || '5.0',
        PathSyntax: pathSyntax || 'CPS1',
        RuleSets: ruleSets,
        CodesMaster: codesMaster
      };
      
      console.log('Metadata:', {
        version: validationMetadata.Version,
        pathSyntax: validationMetadata.PathSyntax,
        ruleSetsCount: ruleSets?.length,
        codesMasterQuestions: codesMaster?.Questions?.length
      });
      
      const data = await fhirApi.process(
        fhirJson,
        JSON.stringify(validationMetadata),
        logLevel,
        strictDisplay
      );
      
      console.log('=== RESPONSE ===');
      console.log('Success:', data.success);
      console.log('Validation errors:', data.validation?.errors?.length || 0);
      
      setResult(data);
      
      if (data.validation?.errors && data.validation.errors.length > 0) {
        message.warning(`Validation found ${data.validation.errors.length} error(s)`);
      } else if (data.success) {
        message.success('Processing completed successfully!');
      }
    } catch (error: any) {
      console.error('=== PROCESSING ERROR ===', error);
      message.error(error.message || 'Processing failed');
    } finally {
      setLoading(false);
    }
  };

  // Clear all
  const handleClear = () => {
    setFhirJson('');
    setResult(null);
    message.info('Cleared');
  };

  // Load sample
  const handleLoadSample = async (sampleName: string) => {
    try {
      // Dynamic import of sample files
      const modules = import.meta.glob('/src/seed/happy-*.json');
      
      for (const path in modules) {
        const fileName = path.split('/').pop();
        if (fileName === sampleName) {
          const module = await modules[path]() as any;
          const jsonString = JSON.stringify(module.default || module, null, 2);
          setFhirJson(jsonString);
          message.success(`Loaded ${sampleName}`);
          return;
        }
      }
      
      message.error('Sample file not found');
    } catch (error) {
      console.error('Failed to load sample:', error);
      message.error('Failed to load sample');
    }
  };

  // Open editor modal
  const handleOpenEditor = () => {
    setIsEditorModalOpen(true);
  };

  // Apply changes from editor modal
  const handleEditorApply = (newJson: string) => {
    setFhirJson(newJson);
    setIsEditorModalOpen(false);
    message.success('JSON updated');
  };

  // Go to resource (triggered from validation error card)
  const handleGoToResource = useCallback((resourcePointer: any) => {
    if (resourcePointer?.entryIndex !== undefined) {
      treeScrollTargetRef.current = resourcePointer;
      setScrollTrigger(prev => prev + 1);
      message.info(`Navigating to entry #${resourcePointer.entryIndex}${resourcePointer.fieldPath ? ` → ${resourcePointer.fieldPath}` : ''}`);
    }
  }, []);

  // Listen for custom navigation events
  useEffect(() => {
    const handleNavigateEvent = (event: any) => {
      if (event.detail?.entryIndex !== undefined) {
        handleGoToResource({ entryIndex: event.detail.entryIndex });
      }
    };

    window.addEventListener('navigateToEntry', handleNavigateEvent);
    return () => {
      window.removeEventListener('navigateToEntry', handleNavigateEvent);
    };
  }, [handleGoToResource]);

  return (
    <div className="h-screen flex flex-col">
      {/* Two-panel layout with splitter */}
      <div className="flex-1 flex overflow-hidden">
        {/* Left Panel - Tree View */}
        <div 
          className="flex flex-col border-r border-gray-300 bg-white"
          style={{ width: `${leftWidth}%` }}
        >
          <TreeViewPanel
            fhirJson={fhirJson}
            scrollTargetRef={treeScrollTargetRef}
            scrollTrigger={scrollTrigger}
            onOpenEditor={handleOpenEditor}
          />
        </div>

        {/* Draggable Splitter */}
        <Splitter
          leftWidth={leftWidth}
          onWidthChange={handleWidthChange}
          minWidth={MIN_WIDTH}
          maxWidth={MAX_WIDTH}
        />

        {/* Right Panel - Processing Controls + Results */}
        <div 
          className="flex-1 flex flex-col bg-gray-50"
          style={{ width: `${100 - leftWidth}%` }}
        >
          <RightPanel
            loading={loading}
            result={result}
            logLevel={logLevel}
            strictDisplay={strictDisplay}
            onProcess={handleProcess}
            onClear={handleClear}
            onLoadSample={handleLoadSample}
            onLogLevelChange={setLogLevel}
            onStrictDisplayChange={setStrictDisplay}
            onGoToResource={handleGoToResource}
            hasJson={!!fhirJson.trim()}
            jsonTree={jsonTree}
          />
        </div>
      </div>

      {/* JSON Editor Modal */}
      <JsonEditorModal
        open={isEditorModalOpen}
        initialValue={fhirJson}
        onClose={() => setIsEditorModalOpen(false)}
        onApply={handleEditorApply}
        onLoadSample={handleLoadSample}
      />
    </div>
  );
}
