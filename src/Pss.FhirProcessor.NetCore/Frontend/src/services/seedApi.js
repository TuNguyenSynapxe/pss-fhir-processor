import axios from 'axios';

// Use relative URL - Vite proxy will forward /api requests to backend
const API_BASE_URL = '/api/seed';

export const seedApi = {
  validatePassword: async (password) => {
    const response = await axios.post(`${API_BASE_URL}/validate-password`, {
      password
    });
    return response.data;
  },

  getSeedFile: async (fileName, password) => {
    const response = await axios.get(`${API_BASE_URL}/${fileName}`, {
      headers: {
        'X-Admin-Password': password
      }
    });
    return response.data;
  },

  updateSeedFile: async (fileName, content, password) => {
    const response = await axios.post(`${API_BASE_URL}/${fileName}`, {
      password,
      content
    });
    return response.data;
  },

  getAvailableFiles: async (password) => {
    const response = await axios.get(`${API_BASE_URL}/files`, {
      headers: {
        'X-Admin-Password': password
      }
    });
    return response.data;
  }
};
