import { useState, useEffect } from 'react';
import { Card, Table, Button, Tag, message, Spin } from 'antd';
import { PlayCircleOutlined, CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import { fhirApi } from '../services/api';

function TestRunner() {
  const [testCases, setTestCases] = useState([]);
  const [loading, setLoading] = useState(false);
  const [runningTests, setRunningTests] = useState(false);
  const [results, setResults] = useState({});

  useEffect(() => {
    loadTestCases();
  }, []);

  const loadTestCases = async () => {
    try {
      setLoading(true);
      const data = await fhirApi.getTestCases();
      setTestCases(data);
    } catch (error) {
      message.error('Failed to load test cases');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const runSingleTest = async (testCase) => {
    try {
      const result = await fhirApi.process(testCase.inputJson, 'info', true);
      const passed = result.validation.isValid === testCase.expectedIsValid;
      
      setResults(prev => ({
        ...prev,
        [testCase.name]: { passed, result }
      }));

      return passed;
    } catch (error) {
      console.error(error);
      setResults(prev => ({
        ...prev,
        [testCase.name]: { passed: false, error: error.message }
      }));
      return false;
    }
  };

  const runAllTests = async () => {
    setRunningTests(true);
    setResults({});
    
    let passed = 0;
    let failed = 0;

    for (const testCase of testCases) {
      const result = await runSingleTest(testCase);
      if (result) passed++;
      else failed++;
    }

    message.success(`Tests completed: ${passed} passed, ${failed} failed`);
    setRunningTests(false);
  };

  const columns = [
    {
      title: 'Test Name',
      dataIndex: 'name',
      key: 'name',
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
    },
    {
      title: 'Expected',
      dataIndex: 'expectedIsValid',
      key: 'expectedIsValid',
      render: (expected) => (
        <Tag color={expected ? 'green' : 'red'}>
          {expected ? 'Valid' : 'Invalid'}
        </Tag>
      ),
    },
    {
      title: 'Status',
      key: 'status',
      render: (_, record) => {
        const result = results[record.name];
        if (!result) return <Tag>Not Run</Tag>;
        if (result.passed) {
          return <Tag color="green" icon={<CheckCircleOutlined />}>Passed</Tag>;
        }
        return <Tag color="red" icon={<CloseCircleOutlined />}>Failed</Tag>;
      },
    },
    {
      title: 'Action',
      key: 'action',
      render: (_, record) => (
        <Button
          size="small"
          onClick={() => runSingleTest(record)}
          loading={runningTests}
        >
          Run
        </Button>
      ),
    },
  ];

  if (loading) {
    return (
      <Card className="shadow-md">
        <div className="text-center py-8">
          <Spin size="large" tip="Loading test cases..." />
        </div>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      <Card title="Test Cases" className="shadow-md">
        <div className="mb-4">
          <Button
            type="primary"
            icon={<PlayCircleOutlined />}
            onClick={runAllTests}
            loading={runningTests}
            size="large"
          >
            Run All Tests
          </Button>
        </div>

        <Table
          columns={columns}
          dataSource={testCases}
          rowKey="name"
          pagination={false}
        />
      </Card>

      {Object.keys(results).length > 0 && (
        <Card title="Test Summary" className="shadow-md">
          <div className="space-y-2">
            <div>
              <span className="font-semibold">Total: </span>
              {testCases.length}
            </div>
            <div>
              <span className="font-semibold text-green-600">Passed: </span>
              {Object.values(results).filter(r => r.passed).length}
            </div>
            <div>
              <span className="font-semibold text-red-600">Failed: </span>
              {Object.values(results).filter(r => !r.passed).length}
            </div>
          </div>
        </Card>
      )}
    </div>
  );
}

export default TestRunner;
