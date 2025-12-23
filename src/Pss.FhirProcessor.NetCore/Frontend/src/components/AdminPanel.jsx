import { useState, useEffect } from 'react';
import { Layout, Card, Button, Input, message, Tabs, Space, Typography, Modal, App } from 'antd';
import { LockOutlined, SaveOutlined, ReloadOutlined, SettingOutlined } from '@ant-design/icons';
import Editor from '@monaco-editor/react';
import { seedApi } from '../services/seedApi';

const { Content } = Layout;
const { Title, Text } = Typography;
const { Password } = Input;

const AdminPanel = () => {
  const { modal } = App.useApp();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [files, setFiles] = useState({
    'happy-sample-full.json': '',
    'validation-metadata.json': ''
  });
  const [activeTab, setActiveTab] = useState('happy-sample-full.json');
  const [saving, setSaving] = useState(false);

  const validatePassword = async () => {
    if (!password.trim()) {
      message.error('Please enter password');
      return;
    }

    setLoading(true);
    try {
      const data = await seedApi.validatePassword(password);

      if (data.valid) {
        setIsAuthenticated(true);
        message.success('Authentication successful');
        loadFiles(password);
      } else {
        message.error('Invalid password');
        setPassword('');
      }
    } catch (error) {
      console.error('Authentication error:', error);
      message.error('Authentication failed');
    } finally {
      setLoading(false);
    }
  };

  const loadFiles = async (adminPassword) => {
    setLoading(true);
    try {
      const fileNames = ['happy-sample-full.json', 'validation-metadata.json'];
      const loadedFiles = {};

      for (const fileName of fileNames) {
        try {
          const data = await seedApi.getSeedFile(fileName, adminPassword || password);
          loadedFiles[fileName] = data.content;
        } catch (error) {
          console.error(`Error loading ${fileName}:`, error);
          message.error(`Failed to load ${fileName}`);
          loadedFiles[fileName] = '{}';
        }
      }

      setFiles(loadedFiles);
      message.success('Files loaded successfully');
    } catch (error) {
      console.error('Error loading files:', error);
      message.error('Failed to load files');
    } finally {
      setLoading(false);
    }
  };

  const saveFile = async (fileName) => {
    console.log('saveFile called with:', fileName);
    console.log('Current file content length:', files[fileName]?.length);
    
    modal.confirm({
      title: 'Save File',
      content: `Are you sure you want to save changes to ${fileName}?`,
      okText: 'Save',
      okType: 'primary',
      cancelText: 'Cancel',
      onOk: async () => {
        console.log('Save confirmed, calling API...');
        setSaving(true);
        try {
          const data = await seedApi.updateSeedFile(fileName, files[fileName], password);
          console.log('Save response:', data);
          message.success(`${fileName} saved successfully`);
        } catch (error) {
          console.error('Error saving file:', error);
          const errorMsg = error.response?.data?.message || `Failed to save ${fileName}`;
          message.error(errorMsg);
        } finally {
          setSaving(false);
        }
      }
    });
  };

  const handleEditorChange = (value, fileName) => {
    setFiles(prev => ({
      ...prev,
      [fileName]: value || ''
    }));
  };

  const formatJson = () => {
    try {
      const formatted = JSON.stringify(JSON.parse(files[activeTab]), null, 2);
      setFiles(prev => ({
        ...prev,
        [activeTab]: formatted
      }));
      message.success('JSON formatted');
    } catch (error) {
      message.error('Invalid JSON format');
    }
  };

  if (!isAuthenticated) {
    return (
      <Layout style={{ minHeight: '100vh', background: '#f0f2f5' }}>
        <Content style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', padding: '50px' }}>
          <Card
            title={
              <div style={{ textAlign: 'center' }}>
                <SettingOutlined style={{ fontSize: '32px', color: '#1890ff', marginBottom: '16px' }} />
                <Title level={3} style={{ margin: 0 }}>Admin Panel</Title>
              </div>
            }
            style={{ width: 400, textAlign: 'center' }}
          >
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <Text type="secondary">Enter admin password to manage seed files</Text>
              <Password
                prefix={<LockOutlined />}
                placeholder="Enter admin password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                onPressEnter={validatePassword}
                size="large"
              />
              <Button
                type="primary"
                size="large"
                block
                loading={loading}
                onClick={validatePassword}
                icon={<LockOutlined />}
              >
                Authenticate
              </Button>
            </Space>
          </Card>
        </Content>
      </Layout>
    );
  }

  const tabItems = [
    {
      key: 'happy-sample-full.json',
      label: 'Happy Sample',
      children: (
        <div style={{ height: 'calc(100vh - 280px)' }}>
          <Editor
            height="100%"
            defaultLanguage="json"
            theme="vs-dark"
            value={files['happy-sample-full.json']}
            onChange={(value) => handleEditorChange(value, 'happy-sample-full.json')}
            options={{
              minimap: { enabled: true },
              fontSize: 14,
              wordWrap: 'on',
              formatOnPaste: true,
              formatOnType: true,
            }}
          />
        </div>
      )
    },
    {
      key: 'validation-metadata.json',
      label: 'Validation Metadata',
      children: (
        <div style={{ height: 'calc(100vh - 280px)' }}>
          <Editor
            height="100%"
            defaultLanguage="json"
            theme="vs-dark"
            value={files['validation-metadata.json']}
            onChange={(value) => handleEditorChange(value, 'validation-metadata.json')}
            options={{
              minimap: { enabled: true },
              fontSize: 14,
              wordWrap: 'on',
              formatOnPaste: true,
              formatOnType: true,
            }}
          />
        </div>
      )
    }
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Content style={{ padding: '24px' }}>
        <Card
          title={
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <div>
                <SettingOutlined style={{ marginRight: '8px' }} />
                Seed File Manager
              </div>
              <Space>
                <Button
                  icon={<ReloadOutlined />}
                  onClick={() => loadFiles()}
                  loading={loading}
                >
                  Reload
                </Button>
                <Button
                  type="primary"
                  icon={<SaveOutlined />}
                  onClick={() => saveFile(activeTab)}
                  loading={saving}
                >
                  Save Current File
                </Button>
              </Space>
            </div>
          }
        >
          <Space direction="vertical" style={{ width: '100%' }} size="middle">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Text type="secondary">
                Edit seed files for the FHIR Processor. Changes will be saved to the server.
              </Text>
              <Button size="small" onClick={formatJson}>
                Format JSON
              </Button>
            </div>
            
            <Tabs
              activeKey={activeTab}
              onChange={setActiveTab}
              items={tabItems}
            />
          </Space>
        </Card>
      </Content>
    </Layout>
  );
};

export default AdminPanel;
