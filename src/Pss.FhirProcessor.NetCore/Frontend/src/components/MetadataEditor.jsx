import { useState } from 'react';
import { Modal, Button, Input, message, Tabs, Alert } from 'antd';
import { EditOutlined, SaveOutlined } from '@ant-design/icons';
import { useMetadata } from '../contexts/MetadataContext';

const { TextArea } = Input;

function MetadataEditor() {
  const { ruleSets, codesMaster, updateRuleSets, updateCodesMaster } = useMetadata();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editedRuleSets, setEditedRuleSets] = useState('');
  const [editedCodesMaster, setEditedCodesMaster] = useState('');
  const [activeTab, setActiveTab] = useState('ruleSets');
  const [saving, setSaving] = useState(false);

  const showModal = () => {
    // RuleSets is already an array in v5 format
    const formattedRuleSets = JSON.stringify(ruleSets, null, 2);
    const formattedCodesMaster = JSON.stringify(codesMaster, null, 2);
    
    setEditedRuleSets(formattedRuleSets);
    setEditedCodesMaster(formattedCodesMaster);
    setIsModalOpen(true);
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      
      if (activeTab === 'ruleSets') {
        const parsed = JSON.parse(editedRuleSets);
        // Validate it's an array
        if (!Array.isArray(parsed)) {
          message.error('RuleSets must be an array');
          return;
        }
        
        const result = await updateRuleSets(parsed);
        if (result.success) {
          message.success('RuleSets updated successfully');
        } else {
          message.error(result.error || 'Failed to update RuleSets');
          return;
        }
      } else {
        const parsed = JSON.parse(editedCodesMaster);
        const result = await updateCodesMaster(parsed);
        if (result.success) {
          message.success('CodesMaster updated successfully');
        } else {
          message.error(result.error || 'Failed to update CodesMaster');
          return;
        }
      }
      
      setIsModalOpen(false);
    } catch (error) {
      if (error instanceof SyntaxError) {
        message.error('Invalid JSON format. Please check your syntax.');
      } else {
        message.error('Failed to save: ' + error.message);
      }
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    setIsModalOpen(false);
  };

  return (
    <>
      <Button 
        type="primary" 
        icon={<EditOutlined />} 
        onClick={showModal}
        size="large"
      >
        Edit Metadata
      </Button>

      <Modal
        title="Edit Validation Metadata"
        open={isModalOpen}
        onCancel={handleCancel}
        width="80%"
        footer={[
          <Button key="cancel" onClick={handleCancel}>
            Cancel
          </Button>,
          <Button
            key="save"
            type="primary"
            icon={<SaveOutlined />}
            loading={saving}
            onClick={handleSave}
          >
            Save Changes
          </Button>
        ]}
      >
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={[
            {
              key: 'ruleSets',
              label: '📋 Rule Sets',
              children: (
                <div>
                  <Alert
                    message="Edit Rule Sets by Scope"
                    description="Each scope (Event, Participant, HS, OS, VS) contains a Rules array. Modify the rules directly below in clean JSON format."
                    type="info"
                    showIcon
                    className="mb-3"
                  />
                  <TextArea
                    value={editedRuleSets}
                    onChange={(e) => setEditedRuleSets(e.target.value)}
                    rows={20}
                    className="font-mono text-sm"
                    placeholder="Enter RuleSets JSON..."
                  />
                </div>
              )
            },
            {
              key: 'codesMaster',
              label: '📚 Codes Master',
              children: (
                <div>
                  <p className="mb-2 text-gray-600">
                    Edit the codes master containing questions and code systems.
                  </p>
                  <TextArea
                    value={editedCodesMaster}
                    onChange={(e) => setEditedCodesMaster(e.target.value)}
                    rows={20}
                    className="font-mono text-sm"
                    placeholder="Enter CodesMaster JSON..."
                  />
                </div>
              )
            }
          ]}
        />
      </Modal>
    </>
  );
}

export default MetadataEditor;
