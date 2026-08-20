import axios from 'axios';

const api = axios.create({
  // Ako je aplikacija na AWS-u, koristiće javni IP. Ako je na tvom laptopu, koristiće localhost.
  baseURL: window.location.hostname === 'localhost' 
    ? 'http://localhost:5110/api/v1' 
    : 'http://35.159.18.203:8080/api/v1',
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});

export default api;