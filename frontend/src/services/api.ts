import axios from 'axios'
import { useAuthStore } from '../stores/auth'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 10000,
})

api.interceptors.request.use((config) => {
  const auth = useAuthStore()

  if (auth.token) {
    config.headers.set('Authorization', `Bearer ${auth.token}`)
  }

  return config
})