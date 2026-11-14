import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
    // guarda o JWT recebido do backend
  const token = ref<string | null>(null)

  // indica se existe um token
  const isAuthenticated = computed(() => token.value !== null)

  // atualiza o token depois do login
  function setToken(newToken: string) {
    token.value = newToken
  }

  // limpa o token
  function logout() {
    token.value = null
  }

  return { token, isAuthenticated, setToken, logout }
})