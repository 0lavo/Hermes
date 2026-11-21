import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import axios from 'axios'
import { getCurrentUser, type AuthResponse, type AuthUser } from '@/services/authService'

export const useAuthStore = defineStore('auth', () => {
  // Estado da autenticação
  const token = ref<string | null>(sessionStorage.getItem('hermes_token'))
  const user = ref<AuthUser | null>(null)

  // Verifica se existe um token
  const isAuthenticated = computed(() => token.value !== null && user.value !== null)

  // Guarda os dados recebidos após Login/Registo
  function setSession(data: AuthResponse) {
    token.value = data.token
    user.value = data.user

    sessionStorage.setItem('hermes_token', data.token)
  }

  async function restoreSession() {
    if (!token.value) {
      user.value = null
      return
    }

    if (user.value) return

    try {
      user.value = await getCurrentUser()
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 401) {
        // JWT rejeitado pelo backend
        logout()
      }

      // Outros erros: manter o token para tentar novamente.
      // Como user continua null, a rota privada fica bloqueada.
    }
  }


  // Termina a sessão localmente
  function logout() {
    token.value = null
    user.value = null

    sessionStorage.removeItem('hermes_token')
  }

  return {
    token,
    user,
    isAuthenticated,
    setSession,
    logout,
    restoreSession,
  }
})
