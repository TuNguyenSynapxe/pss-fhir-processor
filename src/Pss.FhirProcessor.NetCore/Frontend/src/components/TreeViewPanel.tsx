import React, { useState, useEffect, useMemo, useRef } from 'react';
import { Tree, Button, message } from 'antd';
import { 
  FolderOutlined, 
  FileOutlined, 
  EditOutlined,
  ExpandOutlined,
  ShrinkOutlined
} from '@ant-design/icons';
import type { DataNode } from 'antd/es/tree';
import happySampleData from '../seed/happy-sample-full.json';

interface TreeViewPanelProps {
  fhirJson: string;
  setFhirJson: (json: string) => void;
  scrollTargetRef: React.MutableRefObject<{ entryIndex: number; fieldPath?: string } | null>;
  scrollTrigger: number; // Added to force re-render
  onOpenEditor: () => void;
}

// Virtualization threshold
const VIRTUALIZATION_THRESHOLD = 500;

export default function TreeViewPanel({ fhirJson, setFhirJson, scrollTargetRef, scrollTrigger, onOpenEditor }: TreeViewPanelProps) {
  const [expandedKeys, setExpandedKeys] = useState<React.Key[]>([]);
  const [selectedKeys, setSelectedKeys] = useState<React.Key[]>([]);
  const treeRef = useRef<any>(null);
  const scrollContainerRef = useRef<HTMLDivElement>(null);

  // Handle loading sample data
  const handleLoadSample = () => {
    try {
      const sampleJson = JSON.stringify(happySampleData, null, 2);
      setFhirJson(sampleJson);
      message.success('Sample FHIR data loaded successfully');
    } catch (error) {
      console.error('Failed to load sample data:', error);
      message.error('Failed to load sample data');
    }
  };

  // Convert JSON to tree data structure
  const jsonToTreeData = (obj: any, parentKey = 'root'): DataNode[] => {
    if (obj === null || obj === undefined) {
      return [];
    }

    const buildNode = (value: any, key: string, path: string): DataNode => {
      const nodeKey = `${path}.${key}`;
      
      if (Array.isArray(value)) {
        return {
          title: (
            <span>
              <strong>{key}</strong>: <span className="text-blue-600">[{value.length}]</span>
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
              <strong>{key}</strong>: <span className="text-purple-600">{'{...}'}</span>
            </span>
          ),
          key: nodeKey,
          icon: <FolderOutlined />,
          children: keys.map(k => buildNode(value[k], k, nodeKey))
        };
      } else {
        const valueStr = String(value);
        const displayValue = valueStr.length > 50 ? valueStr.substring(0, 50) + '...' : valueStr;
        const valueColor = 
          typeof value === 'string' ? 'text-green-600' : 
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

  // Memoize tree data
  const treeData = useMemo(() => {
    if (!fhirJson.trim()) return [];
    try {
      return jsonToTreeData(fhirJson);
    } catch {
      return [];
    }
  }, [fhirJson]);

  // Count total nodes for virtualization decision
  const totalNodes = useMemo(() => {
    let count = 0;
    const countNodes = (nodes: DataNode[]) => {
      nodes.forEach(node => {
        count++;
        if (node.children) {
          countNodes(node.children);
        }
      });
    };
    countNodes(treeData);
    return count;
  }, [treeData]);

  // Generate default expanded keys
  const defaultExpandedKeys = useMemo(() => {
    const keys: React.Key[] = [];
    const collectKeys = (nodes: DataNode[]) => {
      nodes.forEach(node => {
        if (!node.isLeaf) {
          const isRootLevel = String(node.key).split('.').length === 2;
          const isUnderEntry = String(node.key).includes('.entry.');
          
          if (isRootLevel || isUnderEntry) {
            keys.push(node.key);
            if (node.children) {
              collectKeys(node.children);
            }
          }
        }
      });
    };
    collectKeys(treeData);
    return keys;
  }, [treeData]);

  // Collect all keys for expand all
  const allTreeKeys = useMemo(() => {
    const keys: React.Key[] = [];
    const collectAllKeys = (nodes: DataNode[]) => {
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

  // Handle expand/collapse
  const handleExpandAll = () => {
    setExpandedKeys(allTreeKeys);
    message.success('All nodes expanded');
  };

  const handleCollapseAll = () => {
    setExpandedKeys([]);
    message.success('All nodes collapsed');
  };

  const handleTreeExpand = (keys: React.Key[]) => {
    setExpandedKeys(keys);
  };

  // Handle navigation to specific entry
  useEffect(() => {
    console.log('🔄 useEffect triggered:', { 
      hasTarget: scrollTargetRef.current !== null, 
      targetValue: scrollTargetRef.current,
      scrollTrigger,
      treeDataLength: treeData.length 
    });
    
    if (scrollTargetRef.current !== null && treeData.length > 0) {
      const targetIndex = scrollTargetRef.current.entryIndex;
      const fieldPath = scrollTargetRef.current.fieldPath;
      const baseKey = `root.entry.[${targetIndex}]`;
      
      console.log('🎯 Navigation triggered:', { targetIndex, fieldPath, baseKey });
      
      // Build the full path to the target field
      let targetKey = baseKey;
      const keysToExpand = ['root.entry', baseKey];
      
      if (fieldPath) {
        // Parse fieldPath to build tree keys
        // Handle FHIR filter syntax like: identifier[system:https://fhir.synapxe.sg/NamingSystem/nric].value
        // We need to find the actual array element, not use the filter
        const pathParts = fieldPath.split('.');
        let currentPath = `${baseKey}.resource`;
        keysToExpand.push(currentPath);
        
        for (const part of pathParts) {
          // Handle array notation with filters like "identifier[system:...]" or simple "identifier[0]"
          if (part.includes('[')) {
            const match = part.match(/^(.+?)\[(.+?)\]$/);
            if (match) {
              const [, fieldName, filter] = match;
              currentPath += `.${fieldName}`;
              keysToExpand.push(currentPath);
              
              // If it's a numeric index like [0], use it directly
              // Otherwise (like [system:...]), we'll expand all array elements (or just the array)
              if (/^\d+$/.test(filter)) {
                currentPath += `.[${filter}]`;
                keysToExpand.push(currentPath);
              } else {
                // For filters like [system:...], we can't determine the exact index
                // So we just expand the array and let the user see all elements
                // The selection will be on the array itself
                console.log('⚠️ Filter notation detected, expanding array:', filter);
              }
            }
          } else {
            currentPath += `.${part}`;
            keysToExpand.push(currentPath);
          }
        }
        
        targetKey = currentPath;
      }
      
      console.log('🎯 Target key:', targetKey);
      console.log('📂 Path to expand:', keysToExpand);
      
      // First, collapse all nodes (like clicking collapse button)
      console.log('📁 Collapsing all nodes first...');
      setExpandedKeys([]);
      
      // Wait for collapse to take effect, then expand only the path to target
      requestAnimationFrame(() => {
        console.log('📂 Expanding path to target:', keysToExpand);
        setExpandedKeys(keysToExpand);
        setSelectedKeys([targetKey]);
      
        // Use requestAnimationFrame to wait for React to flush DOM updates
        requestAnimationFrame(() => {
          requestAnimationFrame(() => {
            console.log('⏰ RAF fired, searching for node:', targetKey);
          
          // Find the selected node using Ant Design's selected class
          // The selected class is on the parent treenode, we need the content wrapper
          const selectedTreeNode = document.querySelector('.ant-tree-treenode-selected');
          const treeNode = selectedTreeNode?.querySelector('.ant-tree-node-content-wrapper') || 
                          document.querySelector('.ant-tree-node-selected .ant-tree-node-content-wrapper');
          
          console.log('🔍 Tree node search result:', treeNode ? 'FOUND' : 'NOT FOUND');
        
          if (treeNode) {
              console.log('✅ Found tree node, scrolling...');
          
            // Scroll the tree container to make the node visible
            if (scrollContainerRef.current) {
            const container = scrollContainerRef.current;
            const nodeRect = treeNode.getBoundingClientRect();
            const containerRect = container.getBoundingClientRect();
            
            // Calculate scroll position to center the node
            const nodeTop = nodeRect.top - containerRect.top + container.scrollTop;
            const centerOffset = (container.clientHeight / 2) - (nodeRect.height / 2);
            const targetScroll = nodeTop - centerOffset;
            
            console.log('📏 Scroll calculation:', {
              nodeTop,
              centerOffset,
              targetScroll: Math.max(0, targetScroll),
              containerHeight: container.clientHeight
            });
            
            // Smooth scroll
            container.scrollTo({
              top: Math.max(0, targetScroll),
              behavior: 'smooth'
            });
          } else {
            console.warn('⚠️ scrollContainerRef.current is null');
          }
          
          // Add highlight effect
          const titleElement = treeNode.querySelector('.ant-tree-title');
          if (titleElement) {
            console.log('🎨 Adding highlight to title element');
              titleElement.classList.add('bg-yellow-200', 'transition-colors', 'duration-500');
              setTimeout(() => {
                titleElement.classList.remove('bg-yellow-200');
                setTimeout(() => {
                  titleElement.classList.remove('transition-colors', 'duration-500');
                }, 500);
              }, 2000);
            }
          }
          });
        });
      });
      
      // Clear the target
      scrollTargetRef.current = null;
    }
  }, [scrollTrigger, treeData]);

  if (!fhirJson.trim()) {
    return (
      <div className="flex-1 flex items-center justify-center p-8">
        <div className="text-center text-gray-400">
          <FolderOutlined style={{ fontSize: 48 }} />
          <p className="mt-4">No FHIR data loaded</p>
          <div className="flex gap-2 justify-center mt-2">
            <Button type="link" onClick={onOpenEditor}>
              Open Editor to Add JSON
            </Button>
            <Button type="primary" onClick={handleLoadSample}>
              Load Sample
            </Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full">
      {/* Sticky Header */}
      <div className="sticky top-0 z-10 bg-white border-b border-gray-200 p-3">
        <div className="flex gap-2">
          <Button 
            style={{ flex: 1 }}
            icon={<EditOutlined />}
            onClick={onOpenEditor}
            title="Edit / Load Sample JSON"
          >
            Edit / Load Sample
          </Button>
          <Button 
            icon={<ExpandOutlined />}
            onClick={handleExpandAll}
            title="Expand All"
          />
          <Button 
            icon={<ShrinkOutlined />}
            onClick={handleCollapseAll}
            title="Collapse All"
          />
        </div>
      </div>

      {/* Tree Container */}
      <div 
        ref={scrollContainerRef}
        className="flex-1 overflow-auto p-4 bg-gray-50"
      >
        <Tree
          ref={treeRef}
          showIcon
          showLine
          expandedKeys={expandedKeys}
          selectedKeys={selectedKeys}
          onExpand={handleTreeExpand}
          onSelect={(keys) => setSelectedKeys(keys)}
          treeData={treeData}
          virtual={totalNodes > VIRTUALIZATION_THRESHOLD}
          height={totalNodes > VIRTUALIZATION_THRESHOLD ? 600 : undefined}
        />
      </div>
    </div>
  );
}
