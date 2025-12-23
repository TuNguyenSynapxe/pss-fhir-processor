import { createContext, useContext, useState, useEffect } from 'react';
import { fhirApi } from '../services/api';

const MetadataContext = createContext();

export const MetadataProvider = ({ children }) => {
  const [ruleSets, setRuleSets] = useState([]);
  const [codesMaster, setCodesMaster] = useState(null);
  const [loading, setLoading] = useState(true);
  const [version, setVersion] = useState('5.0');
  const [pathSyntax, setPathSyntax] = useState('CPS1');
  const [metadataSource, setMetadataSource] = useState('none'); // 'backend' or 'error'
  const [error, setError] = useState(null);

  useEffect(() => {
    loadMetadata();
  }, []);

  const loadMetadata = async () => {
    try {
      setLoading(true);
      setError(null);
      
      console.log('Fetching metadata from backend API (reads from seed file)...');
      const response = await fetch('/api/metadata/validation');
      
      if (!response.ok) {
        throw new Error(`Failed to fetch metadata: ${response.statusText}`);
      }
      
      const metadata = await response.json();
      
      setRuleSets(metadata.RuleSets);
      setCodesMaster(metadata.CodesMaster);
      setVersion(metadata.Version);
      setPathSyntax(metadata.PathSyntax);
      setMetadataSource('backend');
      
      console.log('✓ Metadata loaded from BACKEND API (dynamic seed file)');
      console.log('  Version:', metadata.Version);
      console.log('  Path Syntax:', metadata.PathSyntax);
      console.log('  RuleSets:', metadata.RuleSets.length);
      console.log('  Questions:', metadata.CodesMaster.Questions.length);
      
    } catch (error) {
      console.error('Failed to load metadata:', error);
      setError(error.message);
      setMetadataSource('error');
      // Set empty metadata on error
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
        error,
        metadataSource, // 'backend' or 'error'
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
