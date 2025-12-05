import { useState, useEffect } from 'react';
import { Card, Tabs, Table, Tag, Alert, Spin, Collapse } from 'antd';
import { CheckCircleOutlined, InfoCircleOutlined } from '@ant-design/icons';
import { useMetadata } from '../contexts/MetadataContext';
import MetadataEditor from './MetadataEditor';

const { Panel } = Collapse;

function ValidationRules() {
  const { ruleSets, codesMaster, loading, version, pathSyntax } = useMetadata();

  const ruleColumns = [
    {
      title: 'Rule Type',
      dataIndex: 'RuleType',
      key: 'ruleType',
      render: (type) => {
        const colors = {
          Required: 'blue',
          CodesMaster: 'green',
          FixedCoding: 'orange',
          Display: 'purple'
        };
        return <Tag color={colors[type] || 'default'}>{type}</Tag>;
      }
    },
    {
      title: 'Path',
      dataIndex: 'Path',
      key: 'path',
      render: (path) => <code className="bg-gray-100 px-2 py-1 rounded">{path}</code>
    },
    {
      title: 'Error Code',
      dataIndex: 'ErrorCode',
      key: 'errorCode',
      render: (code) => code ? <Tag color="red">{code}</Tag> : '-'
    },
    {
      title: 'Message',
      dataIndex: 'Message',
      key: 'message',
      render: (msg) => msg || '-'
    }
  ];

  const questionColumns = [
    {
      title: 'Question Code',
      dataIndex: 'QuestionCode',
      key: 'code',
      render: (code) => <code className="bg-blue-50 px-2 py-1 rounded text-blue-700">{code}</code>
    },
    {
      title: 'Question Display',
      dataIndex: 'QuestionDisplay',
      key: 'display',
      width: '40%'
    },
    {
      title: 'Type',
      dataIndex: 'ScreeningType',
      key: 'type',
      render: (type) => {
        const colors = { HS: 'blue', OS: 'green', VS: 'orange' };
        return <Tag color={colors[type]}>{type}</Tag>;
      }
    },
    {
      title: 'Multi-Value',
      dataIndex: 'IsMultiValue',
      key: 'multiValue',
      render: (isMulti) => (
        <Tag color={isMulti ? 'green' : 'default'}>
          {isMulti ? 'Yes' : 'No'}
        </Tag>
      )
    },
    {
      title: 'Allowed Answers',
      dataIndex: 'AllowedAnswers',
      key: 'answers',
      render: (answers) => (
        <div className="space-y-1">
          {answers.map((answer, idx) => (
            <div key={idx} className="text-xs bg-gray-50 px-2 py-1 rounded">
              {answer}
            </div>
          ))}
        </div>
      )
    }
  ];

  if (loading) {
    return (
      <Card className="shadow-md">
        <div className="text-center py-8">
          <Spin size="large" tip="Loading validation rules..." />
        </div>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      <Alert
        message={`Validation Rules Overview (v${version} - ${pathSyntax})`}
        description="This page displays all validation rules configured for FHIR Bundle processing, including required fields, codes master validation, and screening-specific rules."
        type="info"
        showIcon
        icon={<InfoCircleOutlined />}
      />

      <Card title="📋 Validation Rule Sets" className="shadow-md" extra={<MetadataEditor />}>
        <Tabs 
          defaultActiveKey={ruleSets[0]?.Scope || "Patient"}
          items={ruleSets.map((ruleSet) => {
            return {
              key: ruleSet.Scope,
              label: (
                <span>
                  <CheckCircleOutlined /> {ruleSet.Scope}
                </span>
              ),
              children: (
                <>
                  <Alert
                    message={`${ruleSet.Scope} Scope`}
                    description={`Total Rules: ${ruleSet.Rules.length}`}
                    type="success"
                    showIcon
                    className="mb-4"
                  />
                  <Table
                    columns={ruleColumns}
                    dataSource={ruleSet.Rules}
                    rowKey={(record, index) => `${ruleSet.Scope}-${index}`}
                    pagination={false}
                    size="small"
                  />
                </>
              )
            };
          })}
        />
      </Card>

      {codesMaster && (
        <Card title="📚 Codes Master - Question Repository" className="shadow-md">
          <Alert
            message="Question Codes & Allowed Answers"
            description={`Total Questions: ${codesMaster.Questions?.length || 0}`}
            type="info"
            showIcon
            className="mb-4"
          />
          
          <Collapse defaultActiveKey={['HS']}>
            <Panel header={<span><Tag color="blue">HS</Tag> Hearing Screening Questions</span>} key="HS">
              <Table
                columns={questionColumns}
                dataSource={codesMaster.Questions?.filter(q => q.ScreeningType === 'HS') || []}
                rowKey="QuestionCode"
                pagination={false}
                size="small"
              />
            </Panel>
            <Panel header={<span><Tag color="green">OS</Tag> Oral Screening Questions</span>} key="OS">
              <Table
                columns={questionColumns}
                dataSource={codesMaster.Questions?.filter(q => q.ScreeningType === 'OS') || []}
                rowKey="QuestionCode"
                pagination={false}
                size="small"
              />
            </Panel>
            <Panel header={<span><Tag color="orange">VS</Tag> Vision Screening Questions</span>} key="VS">
              <Table
                columns={questionColumns}
                dataSource={codesMaster.Questions?.filter(q => q.ScreeningType === 'VS') || []}
                rowKey="QuestionCode"
                pagination={false}
                size="small"
              />
            </Panel>
          </Collapse>
        </Card>
      )}

      {codesMaster?.CodeSystems && (
        <Card title="🏷️ Code Systems" className="shadow-md">
          <Alert
            message="FHIR Code Systems"
            description={`Total Code Systems: ${codesMaster.CodeSystems.length}`}
            type="info"
            showIcon
            className="mb-4"
          />
          
          {codesMaster.CodeSystems.map((codeSystem) => (
            <Card 
              key={codeSystem.Id} 
              type="inner" 
              title={
                <span>
                  <Tag color="purple">{codeSystem.Id}</Tag>
                  {codeSystem.Description}
                </span>
              }
              className="mb-4"
            >
              <div className="mb-2">
                <strong>System URI:</strong>{' '}
                <code className="bg-gray-100 px-2 py-1 rounded text-sm">{codeSystem.System}</code>
              </div>
              <div>
                <strong>Concepts:</strong>
                <Table
                  columns={[
                    {
                      title: 'Code',
                      dataIndex: 'Code',
                      key: 'code',
                      render: (code) => <Tag color="blue">{code}</Tag>
                    },
                    {
                      title: 'Display',
                      dataIndex: 'Display',
                      key: 'display'
                    }
                  ]}
                  dataSource={codeSystem.Concepts || []}
                  rowKey="Code"
                  pagination={false}
                  size="small"
                />
              </div>
            </Card>
          ))}
        </Card>
      )}

      <Card title="📖 Rule Types Explained" className="shadow-md">
        <div className="space-y-3">
          <div>
            <Tag color="blue">Required</Tag>
            <span className="ml-2">Validates that mandatory fields are present in the FHIR Bundle</span>
          </div>
          <div>
            <Tag color="green">CodesMaster</Tag>
            <span className="ml-2">Ensures question codes and answers match the Codes Master definitions</span>
          </div>
          <div>
            <Tag color="orange">FixedCoding</Tag>
            <span className="ml-2">Verifies that specific coding systems and codes are used correctly</span>
          </div>
          <div>
            <Tag color="purple">Display</Tag>
            <span className="ml-2">Validates that display text matches expected values (when strict mode is enabled)</span>
          </div>
        </div>
      </Card>

      <Card title="ℹ️ Validation Process" className="shadow-md">
        <ol className="list-decimal list-inside space-y-2">
          <li><strong>Event Validation:</strong> Checks encounter details, location, and screening date</li>
          <li><strong>Participant Validation:</strong> Validates patient demographics and required identifiers</li>
          <li><strong>Screening-Specific Validation:</strong> Applies HS/OS/VS rules based on observation type</li>
          <li><strong>Codes Master Validation:</strong> Ensures all question codes and answers are valid</li>
          <li><strong>Fixed Coding Validation:</strong> Verifies correct coding systems are used</li>
        </ol>
      </Card>
    </div>
  );
}

export default ValidationRules;
