/**
 * SmartPathNavigator Demo / Test Harness
 * 
 * This file demonstrates how to use the new SmartPathNavigator component
 * with EnhancedPathSegment data from the Phase 1 refactored pathParser.
 * 
 * This is for development/testing only - not part of the production app yet.
 */

import React, { useState } from 'react';
import { Card, Space, Typography, Divider } from 'antd';
import { SmartPathNavigator } from './SmartPathNavigator';
import { parseFieldPath, buildEnhancedPathSegments } from '../utils/pathParser';

const { Title, Paragraph, Text } = Typography;

/**
 * Demo component showing SmartPathNavigator with sample data
 */
export const SmartPathNavigatorDemo: React.FC = () => {
  const [clickedSegment, setClickedSegment] = useState<string | null>(null);

  // Sample FHIR Bundle resource data
  const sampleJson = {
    resourceType: 'Patient',
    id: '12345',
    identifier: [
      {
        system: 'urn:oid:2.16.840.1.113883.2.1.4.1',
        value: 'NHS123456',
      },
    ],
    extension: [
      {
        url: 'https://fhir.synapxe.sg/StructureDefinition/race',
        valueCodeableConcept: {
          coding: [
            {
              system: 'https://fhir.synapxe.sg/CodeSystem/race',
              code: 'CHINESE',
              display: 'Chinese',
            },
          ],
        },
      },
    ],
    name: [
      {
        family: 'Smith',
        given: ['John'],
      },
    ],
  };

  // Test scenarios
  const scenarios = [
    {
      title: 'Scenario 1: All segments exist',
      path: 'identifier[0].system',
      description: 'Simple path where all segments exist',
    },
    {
      title: 'Scenario 2: Discriminator filter match',
      path: 'extension[url:https://fhir.synapxe.sg/StructureDefinition/race].valueCodeableConcept.coding[0].code',
      description: 'Complex path with discriminator that matches',
    },
    {
      title: 'Scenario 3: Missing leaf property',
      path: 'name[0].suffix',
      description: 'Path where leaf property is missing',
    },
    {
      title: 'Scenario 4: Discriminator filter no match',
      path: 'extension[url:https://fhir.synapxe.sg/StructureDefinition/ethnicity].valueCodeableConcept',
      description: 'Discriminator that does not match any array element',
    },
    {
      title: 'Scenario 5: Missing parent structure',
      path: 'address[0].line[0]',
      description: 'Deeply nested path where parent does not exist',
    },
  ];

  return (
    <div style={{ padding: 24, maxWidth: 1200, margin: '0 auto' }}>
      <Title level={2}>SmartPathNavigator Demo</Title>
      <Paragraph>
        This demo shows the new <Text code>SmartPathNavigator</Text> component with{' '}
        <Text code>EnhancedPathSegment</Text> data from Phase 1 refactoring.
      </Paragraph>

      <Card title="Sample JSON" style={{ marginBottom: 24 }}>
        <pre
          style={{
            background: '#f5f5f5',
            padding: 12,
            borderRadius: 4,
            overflow: 'auto',
            maxHeight: 300,
          }}
        >
          <code>{JSON.stringify(sampleJson, null, 2)}</code>
        </pre>
      </Card>

      {clickedSegment && (
        <Card type="inner" style={{ marginBottom: 16, background: '#e6f7ff' }}>
          <Text strong>Last clicked segment: </Text>
          <Text code>{clickedSegment}</Text>
        </Card>
      )}

      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        {scenarios.map((scenario, idx) => {
          const segments = parseFieldPath(scenario.path);
          const enhancedSegments = buildEnhancedPathSegments(sampleJson, segments, '');

          return (
            <Card key={idx} title={scenario.title}>
              <Paragraph type="secondary">{scenario.description}</Paragraph>
              <Paragraph>
                <Text strong>Path: </Text>
                <Text code>{scenario.path}</Text>
              </Paragraph>

              <SmartPathNavigator
                segments={enhancedSegments}
                onSegmentClick={(jumpKey) => {
                  console.log('Clicked segment with jumpKey:', jumpKey);
                  setClickedSegment(jumpKey);
                }}
                defaultExpanded={false}
              />

              <Divider style={{ margin: '16px 0' }} />

              <details style={{ fontSize: 12 }}>
                <summary style={{ cursor: 'pointer', userSelect: 'none' }}>
                  <Text type="secondary">View Enhanced Segment Data (Debug)</Text>
                </summary>
                <pre
                  style={{
                    background: '#fafafa',
                    padding: 8,
                    borderRadius: 4,
                    marginTop: 8,
                    fontSize: 11,
                    overflow: 'auto',
                  }}
                >
                  {JSON.stringify(enhancedSegments, null, 2)}
                </pre>
              </details>
            </Card>
          );
        })}
      </Space>

      <Divider style={{ margin: '32px 0' }} />

      <Card title="Integration Notes" type="inner">
        <Space direction="vertical">
          <Paragraph>
            <Text strong>To integrate SmartPathNavigator into your application:</Text>
          </Paragraph>
          <ol>
            <li>
              Parse your field path using <Text code>parseFieldPath(fieldPath)</Text>
            </li>
            <li>
              Build enhanced segments using{' '}
              <Text code>buildEnhancedPathSegments(jsonRoot, segments, basePath)</Text>
            </li>
            <li>
              Pass the enhanced segments to <Text code>SmartPathNavigator</Text>
            </li>
            <li>
              Handle <Text code>onSegmentClick</Text> callback to implement tree navigation
            </li>
          </ol>
          <Paragraph type="secondary" style={{ marginTop: 16 }}>
            <Text strong>Note:</Text> This component does NOT modify existing ErrorHelperPanel or
            ValidationErrorCard yet. It's a standalone component ready for Phase 2 integration.
          </Paragraph>
        </Space>
      </Card>
    </div>
  );
};

export default SmartPathNavigatorDemo;
