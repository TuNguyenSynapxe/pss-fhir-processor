import { createContext, useContext, useState, useEffect } from 'react';
import { fhirApi } from '../services/api';
import validationMetadata from '../seed/validation-metadata.json';

const MetadataContext = createContext();

export const MetadataProvider = ({ children }) => {
  const [ruleSets, setRuleSets] = useState([]);
  const [codesMaster, setCodesMaster] = useState(null);
  const [loading, setLoading] = useState(true);
  const [version, setVersion] = useState('5.0');
  const [pathSyntax, setPathSyntax] = useState('CPS1');

  useEffect(() => {
    loadMetadata();
  }, []);

  const loadMetadata = async () => {
    try {
      setLoading(true);
      
      // Load from local JSON file - v5 format
      setRuleSets(validationMetadata.RuleSets);
      setCodesMaster(validationMetadata.CodesMaster);
      setVersion(validationMetadata.Version);
      setPathSyntax(validationMetadata.PathSyntax);
      
      console.log('Metadata v5 loaded from frontend seed file');
      console.log('Version:', validationMetadata.Version);
      console.log('Path Syntax:', validationMetadata.PathSyntax);
      console.log('RuleSets count:', validationMetadata.RuleSets.length);
      console.log('Questions count:', validationMetadata.CodesMaster.Questions.length);
      
      // Note: Backend will receive metadata with each request, no need to pre-sync
    } catch (error) {
      console.error('Failed to load metadata:', error);
      // Fallback to empty metadata
      setRuleSets([]);
      setCodesMaster({ Questions: [], CodeSystems: [] });
    } finally {
      setLoading(false);
    }
  };

  const updateRuleSets = async (newRuleSets) => {
    try {
      // newRuleSets should be an array in v5 format
      await fhirApi.updateRules(newRuleSets);
      setRuleSets(newRuleSets);
      return { success: true };
    } catch (error) {
      console.error('Failed to update RuleSets:', error);
      return { success: false, error: error.message };
    }
  };

  const updateCodesMaster = async (newCodesMaster) => {
    try {
      await fhirApi.updateCodesMaster(newCodesMaster);
      setCodesMaster(newCodesMaster);
      return { success: true };
    } catch (error) {
      console.error('Failed to update CodesMaster:', error);
      return { success: false, error: error.message };
    }
  };

  const updateFullMetadata = async (metadata) => {
    try {
      setRuleSets(metadata.RuleSets);
      setCodesMaster(metadata.CodesMaster);
      setVersion(metadata.Version);
      setPathSyntax(metadata.PathSyntax);
      return { success: true };
    } catch (error) {
      console.error('Failed to update metadata:', error);
      return { success: false, error: error.message };
    }
  };

  return (
    <MetadataContext.Provider
      value={{
        ruleSets,
        codesMaster,
        version,
        pathSyntax,
        loading,
        updateRuleSets,
        updateCodesMaster,
        updateFullMetadata,
        refreshMetadata: loadMetadata
      }}
    >
      {children}
    </MetadataContext.Provider>
  );
};

export const useMetadata = () => {
  const context = useContext(MetadataContext);
  if (!context) {
    throw new Error('useMetadata must be used within MetadataProvider');
  }
  return context;
};
