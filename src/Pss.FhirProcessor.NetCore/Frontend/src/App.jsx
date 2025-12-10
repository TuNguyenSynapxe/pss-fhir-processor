import { useState } from 'react';
import { Layout, Menu } from 'antd';
import { ExperimentOutlined, FileTextOutlined, CodeOutlined, ApiOutlined } from '@ant-design/icons';
import Playground from './components/PlaygroundLayout';
import ValidationRules from './components/ValidationRules';
import DeveloperGuide from './components/DeveloperGuide';
import SandboxGuide from './components/SandboxGuide';
import { MetadataProvider } from './contexts/MetadataContext';
import './App.css';

const { Header, Content, Footer } = Layout;

function App() {
  const [currentPage, setCurrentPage] = useState('playground');

  const menuItems = [
    {
      key: 'playground',
      icon: <ExperimentOutlined />,
      label: 'Playground',
    },
    {
      key: 'rules',
      icon: <FileTextOutlined />,
      label: 'Validation Rules',
    },
    {
      key: 'guide',
      icon: <CodeOutlined />,
      label: 'Developer Guide',
    },
    {
      key: 'sandbox',
      icon: <ApiOutlined />,
      label: 'Sandbox API',
    },
  ];

  const renderContent = () => {
    switch (currentPage) {
      case 'playground':
        return <Playground />;
      case 'rules':
        return <ValidationRules />;
      case 'guide':
        return <DeveloperGuide />;
      case 'sandbox':
        return <SandboxGuide />;
      default:
        return <Playground />;
    }
  };

  return (
    <MetadataProvider>
      <Layout className="min-h-screen">
        <Header className="flex items-center bg-blue-600">
          <div className="text-white text-xl font-bold mr-8">
            PSS FHIR Processor
          </div>
          <Menu
            theme="dark"
            mode="horizontal"
            selectedKeys={[currentPage]}
            items={menuItems}
            onClick={({ key }) => setCurrentPage(key)}
            className="flex-1 bg-blue-600"
          />
        </Header>
        <Content className={currentPage === 'playground' ? '' : 'p-6 bg-gray-100'}>
          {currentPage === 'playground' ? (
            renderContent()
          ) : (
            <div className="max-w-7xl mx-auto">
              {renderContent()}
            </div>
          )}
        </Content>
        <Footer className="text-center bg-gray-200">
          PSS FHIR Processor ©2025 - MOH HealthierSG
        </Footer>
      </Layout>
    </MetadataProvider>
  );
}

export default App;
