import axios from 'axios';

// Use relative URL - Vite proxy will forward /api requests to localhost:5000
const API_BASE_URL = '/api';

export const fhirApi = {
  validate: async (fhirJson, validationMetadata, logLevel = 'info', strictDisplayMatch = true) => {
    const response = await axios.post(`${API_BASE_URL}/fhir/validate`, {
      fhirJson,
      validationMetadata,
      logLevel,
      strictDisplayMatch
    });
    return response.data;
  },

  extract: async (fhirJson, validationMetadata, logLevel = 'info', strictDisplayMatch = true) => {
    const response = await axios.post(`${API_BASE_URL}/fhir/extract`, {
      fhirJson,
      validationMetadata,
      logLevel,
      strictDisplayMatch
    });
    return response.data;
  },

  process: async (fhirJson, validationMetadata, logLevel = 'info', strictDisplayMatch = true) => {
    const response = await axios.post(`${API_BASE_URL}/fhir/process`, {
      fhirJson,
      validationMetadata,
      logLevel,
      strictDisplayMatch
    });
    return response.data;
  },

  getTestCases: async () => {
    const response = await axios.get(`${API_BASE_URL}/fhir/test-cases`);
    return response.data;
  },

  getTestCase: async (name) => {
    const response = await axios.get(`${API_BASE_URL}/fhir/test-cases/${name}`);
    return response.data;
  }
};
