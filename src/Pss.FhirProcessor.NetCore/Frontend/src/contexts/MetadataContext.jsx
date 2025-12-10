import { createContext, useContext, useState, useEffect } from 'react';
import { fhirApi } from '../services/api';
// Keep as fallback for development
import validationMetadata from '../seed/validation-metadata.json';

const MetadataContext = createContext();

export const MetadataProvider = ({ children }) => {
  const [ruleSets, setRuleSets] = useState([]);
  const [codesMaster, setCodesMaster] = useState(null);
  const [loading, setLoading] = useState(true);
  const [version, setVersion] = useState('5.0');
  const [pathSyntax, setPathSyntax] = useState('CPS1');
  const [metadataSource, setMetadataSource] = useState('none'); // 'backend' or 'local'

  useEffect(() => {
    loadMetadata();
  }, []);

  const loadMetadata = async () => {
    try {
      setLoading(true);
      
      // Try to fetch from backend API first (single source of truth)
      try {
        console.log('Fetching metadata from backend API...');
        const response = await fetch('/api/metadata/validation');
        
        if (response.ok) {
          const metadata = await response.json();
          
          setRuleSets(metadata.RuleSets);
          setCodesMaster(metadata.CodesMaster);
          setVersion(metadata.Version);
          setPathSyntax(metadata.PathSyntax);
          setMetadataSource('backend');
          
          console.log('✓ Metadata loaded from BACKEND API (single source of truth)');
          console.log('  Version:', metadata.Version);
          console.log('  Path Syntax:', metadata.PathSyntax);
          console.log('  RuleSets:', metadata.RuleSets.length);
          console.log('  Questions:', metadata.CodesMaster.Questions.length);
          
          setLoading(false);
          return;
        }
      } catch (apiError) {
        console.warn('Backend API not available, falling back to local metadata:', apiError.message);
      }
      
      // Fallback: Load from local JSON file
      console.log('Loading metadata from LOCAL fallback...');
      setRuleSets(validationMetadata.RuleSets);
      setCodesMaster(validationMetadata.CodesMaster);
      setVersion(validationMetadata.Version);
      setPathSyntax(validationMetadata.PathSyntax);
      setMetadataSource('local');
      
      console.log('⚠ Metadata loaded from LOCAL fallback (development mode)');
      console.log('  Version:', validationMetadata.Version);
      console.log('  Path Syntax:', validationMetadata.PathSyntax);
      console.log('  RuleSets:', validationMetadata.RuleSets.length);
      console.log('  Questions:', validationMetadata.CodesMaster.Questions.length);
      
    } catch (error) {
      console.error('Failed to load metadata:', error);
      // Ultimate fallback to empty metadata
      setRuleSets([]);
      setCodesMaster({ Questions: [], CodeSystems: [] });
      setMetadataSource('none');
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
        metadataSource, // 'backend', 'local', or 'none'
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
