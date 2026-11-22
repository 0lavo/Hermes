import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import JoinRoomModal from '@/components/rooms/JoinRoomModal.vue'
import { joinRoom } from '@/services/roomService'

const { logoutSpy, replaceSpy } = vi.hoisted(() => ({
  logoutSpy: vi.fn<() => void>(),
  replaceSpy: vi.fn<(path: string) => void>(),
}))

vi.mock('@/services/roomService', () => ({
  joinRoom: vi.fn<typeof joinRoom>(),
}))

vi.mock('@/stores/auth', () => ({
  useAuthStore: () => ({
    logout: logoutSpy,
  }),
}))

vi.mock('vue-router', () => ({
  useRouter: () => ({
    replace: replaceSpy,
  }),
}))

const mockedJoinRoom = vi.mocked(joinRoom)

describe('JoinRoomModal - US09', () => {
  let wrapper: ReturnType<typeof mount>

  beforeEach(() => {
    vi.clearAllMocks()

    wrapper = mount(JoinRoomModal, {
      props: { open: true },
      global: {
        stubs: {
          teleport: true,
        },
      },
    })
  })

  afterEach(() => {
    wrapper.unmount()
  })

  it('rejeita códigos inválidos', async () => {
    await wrapper.get('#join-room-code').setValue('123')

    await wrapper.get('form').trigger('submit')

    expect(wrapper.text()).toContain('Introduz um código válido de 6 dígitos.')
    expect(mockedJoinRoom).not.toHaveBeenCalled()
  })

  it('exige a escolha de anonimato', async () => {
    await wrapper.get('#join-room-code').setValue('482913')

    await wrapper.get('form').trigger('submit')

    expect(wrapper.text()).toContain('Escolhe como queres participar.')
    expect(mockedJoinRoom).not.toHaveBeenCalled()
  })

  it('envia o código e a escolha de anonimato', async () => {
    mockedJoinRoom.mockResolvedValue({ anonymous: true })

    await wrapper.get('#join-room-code').setValue('482913')

    await wrapper.findAll('input[type="radio"]')[1]!.setValue()

    await wrapper.get('form').trigger('submit')
    await flushPromises()

    expect(mockedJoinRoom).toHaveBeenCalledWith('482913', true)

    expect(wrapper.emitted('joined')?.[0]).toEqual([{ code: '482913', anonymous: true }])
  })

  it('respeita a identidade guardada no servidor', async () => {
    mockedJoinRoom.mockResolvedValue({ anonymous: false })

    await wrapper.get('#join-room-code').setValue('482913')

    await wrapper.findAll('input[type="radio"]')[1]!.setValue()

    await wrapper.get('form').trigger('submit')
    await flushPromises()

    expect(wrapper.emitted('joined')?.[0]).toEqual([{ code: '482913', anonymous: false }])
  })

  it.each([
    [400, 'Os dados enviados são inválidos.'],
    [403, 'O dono da sala não pode entrar como participante.'],
    [404, 'Sala não encontrada. Verifica o código.'],
    [409, 'Esta sala não tem uma sessão ativa.'],
  ])('apresenta a mensagem correta para o erro %i', async (status, message) => {
    mockedJoinRoom.mockRejectedValue({
      isAxiosError: true,
      response: { status },
    })

    await wrapper.get('#join-room-code').setValue('482913')

    await wrapper.findAll('input[type="radio"]')[1]!.setValue()

    await wrapper.get('form').trigger('submit')
    await flushPromises()

    expect(wrapper.text()).toContain(message)
    expect(wrapper.emitted('joined')).toBeUndefined()
  })

  it('impede duas submissões enquanto o pedido está pendente', async () => {
    let resolveRequest!: (value: { anonymous: boolean }) => void

    mockedJoinRoom.mockImplementation(
      () =>
        new Promise((resolve) => {
          resolveRequest = resolve
        }),
    )

    await wrapper.get('#join-room-code').setValue('482913')
    await wrapper.findAll('input[type="radio"]')[1]!.setValue()

    await wrapper.get('form').trigger('submit')
    await wrapper.get('form').trigger('submit')

    expect(mockedJoinRoom).toHaveBeenCalledTimes(1)

    resolveRequest({ anonymous: true })
    await flushPromises()

    expect(wrapper.emitted('joined')).toHaveLength(1)
  })

  it('termina a sessão e redireciona para login quando recebe 401', async () => {
    mockedJoinRoom.mockRejectedValue({
      isAxiosError: true,
      response: { status: 401 },
    })

    await wrapper.get('#join-room-code').setValue('482913')
    await wrapper.findAll('input[type="radio"]')[1]!.setValue()

    await wrapper.get('form').trigger('submit')
    await flushPromises()

    expect(logoutSpy).toHaveBeenCalledOnce()
    expect(replaceSpy).toHaveBeenCalledWith('/login')
    expect(wrapper.emitted('joined')).toBeUndefined()
  })

  it('permite fechar o modal pelo botão de fechar', async () => {
    await wrapper.get('button[aria-label="Fechar modal"]').trigger('click')

    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})
