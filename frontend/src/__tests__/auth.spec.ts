
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { AxiosError } from 'axios'

import { useAuthStore } from '@/stores/auth'
import { getCurrentUser } from '@/services/authService'

// Simulamos a resposta da API, sem precisar do backend
vi.mock('@/services/authService', () => ({
  getCurrentUser: vi.fn<typeof getCurrentUser>(),
}))

const testUser = {
  id: 1,
  name: 'Utilizador Teste',
  email: 'teste@hermes.pt',
}

describe('Store de autenticação', () => {
  beforeEach(() => {
    sessionStorage.clear()
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('guarda a sessão após o login', () => {
    const auth = useAuthStore()

    auth.setSession({
      user: testUser,
      token: 'token-teste',
    })

    expect(auth.isAuthenticated).toBe(true)
    expect(auth.user?.name).toBe('Utilizador Teste')
    expect(sessionStorage.getItem('hermes_token')).toBe('token-teste')
  })

  it('remove a sessão ao fazer logout', () => {
    const auth = useAuthStore()

    auth.setSession({
      user: testUser,
      token: 'token-teste',
    })

    auth.logout()

    expect(auth.isAuthenticated).toBe(false)
    expect(auth.token).toBeNull()
    expect(auth.user).toBeNull()
    expect(sessionStorage.getItem('hermes_token')).toBeNull()
  })

  it('recupera uma sessão válida através da API', async () => {
    sessionStorage.setItem('hermes_token', 'token-guardado')

    const auth = useAuthStore()

    vi.mocked(getCurrentUser).mockResolvedValue(testUser)

    await auth.restoreSession()

    expect(getCurrentUser).toHaveBeenCalledOnce()
    expect(auth.user).toEqual(testUser)
    expect(auth.isAuthenticated).toBe(true)
  })

  it('termina a sessão quando o backend devolve 401', async () => {
    sessionStorage.setItem('hermes_token', 'token-invalido')

    const auth = useAuthStore()

    const error = Object.assign(new AxiosError('Unauthorized'), {
      response: { status: 401 },
    })

    vi.mocked(getCurrentUser).mockRejectedValue(error)

    await auth.restoreSession()

    expect(auth.isAuthenticated).toBe(false)
    expect(auth.token).toBeNull()
    expect(sessionStorage.getItem('hermes_token')).toBeNull()
  })

  it('mantém o token quando existe um erro de rede', async () => {
    sessionStorage.setItem('hermes_token', 'token-guardado')

    const auth = useAuthStore()

    vi.mocked(getCurrentUser).mockRejectedValue(
      new Error('Network Error'),
    )

    await auth.restoreSession()

    expect(auth.isAuthenticated).toBe(false)
    expect(auth.token).toBe('token-guardado')
    expect(sessionStorage.getItem('hermes_token')).toBe('token-guardado')
  })
})
