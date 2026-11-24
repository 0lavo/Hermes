<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

import BaseModal from '@/components/ui/BaseModal.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import { joinRoom } from '@/services/roomService'
import { useAuthStore } from '@/stores/auth'

const props = defineProps<{ open: boolean }>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'joined', data: { code: string; anonymous: boolean }): void
}>()

const router = useRouter()
const authStore = useAuthStore()

const code = ref('')
const anonymous = ref<boolean | null>(null)
const isSubmitting = ref(false)
const codeError = ref('')
const choiceError = ref('')
const serverError = ref('')

watch(
  () => props.open,
  (open) => {
    if (open) {
      code.value = ''
      anonymous.value = null
      codeError.value = ''
      choiceError.value = ''
      serverError.value = ''
    }
  },
)

function handleClose() {
  if (!isSubmitting.value) emit('close')
}

async function handleSubmit() {
  if (isSubmitting.value) return

  codeError.value = ''
  choiceError.value = ''
  serverError.value = ''

  const normalizedCode = code.value.trim()
  const selectedAnonymous = anonymous.value

  if (!/^[1-9][0-9]{5}$/.test(normalizedCode)) {
    codeError.value = 'Introduz um código válido de 6 dígitos.'
  }

  if (anonymous.value === null) {
    choiceError.value = 'Escolhe como queres participar.'
  }

  if (codeError.value || choiceError.value || selectedAnonymous === null) return

  isSubmitting.value = true

  try {
    const result = await joinRoom(normalizedCode, selectedAnonymous)
    // A resposta do servidor prevalece sobre a escolha enviada.
    emit('joined', {
      code: normalizedCode,
      anonymous: result.anonymous,
    })
    emit('close')
  } catch (error) {
    if (!axios.isAxiosError(error)) {
      serverError.value = 'Ocorreu um erro inesperado.'
      return
    }

    switch (error.response?.status) {
      case 400:
        serverError.value = 'Os dados enviados são inválidos.'
        break
      case 401:
        authStore.logout()
        await router.replace('/login')
        break
      case 403:
        serverError.value = 'O dono da sala não pode entrar como participante.'
        break
      case 404:
        serverError.value = 'Sala não encontrada. Verifica o código.'
        break
      case 409:
        serverError.value = 'Esta sala não tem uma sessão ativa.'
        break
      default:
        serverError.value = 'Não foi possível entrar na sala. Tenta novamente.'
    }
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal
    :open="open"
    title="Entrar numa sala"
    :close-on-backdrop="!isSubmitting"
    :close-on-escape="!isSubmitting"
    @close="handleClose"
  >
    <form class="flex flex-col gap-6" @submit.prevent="handleSubmit">
      <p class="text-sm text-slate-600 dark:text-slate-300">
        Introduz o código de 6 dígitos fornecido pelo professor.
      </p>

      <BaseInput
        id="join-room-code"
        v-model="code"
        label="Código da sala"
        type="text"
        inputmode="numeric"
        :maxlength="6"
        autocomplete="off"
        placeholder="Ex.: 482913"
        :disabled="isSubmitting"
        :error="codeError"
        required
      />

      <fieldset class="flex flex-col gap-3" :disabled="isSubmitting">
        <legend class="mb-3 text-sm font-semibold text-slate-900 dark:text-white">
          Como queres participar?
        </legend>

        <label
          class="flex min-h-16 cursor-pointer items-center gap-3 rounded-xl border border-slate-200 bg-white p-4 focus-within:ring-2 focus-within:ring-indigo-500 dark:border-slate-700 dark:bg-slate-800"
          :class="{ 'border-indigo-500! bg-indigo-50! dark:bg-indigo-950!': anonymous === false }"
        >
          <input
            v-model="anonymous"
            type="radio"
            name="join-identity"
            :value="false"
            class="accent-indigo-600"
          />
          <span class="flex flex-col gap-1">
            <span class="text-sm font-semibold">Mostrar o meu nome</span>
            <span class="text-xs text-slate-500 dark:text-slate-400">
              Participar com a minha identidade visível.
            </span>
          </span>
        </label>

        <label
          class="flex min-h-16 cursor-pointer items-center gap-3 rounded-xl border border-slate-200 bg-white p-4 focus-within:ring-2 focus-within:ring-indigo-500 dark:border-slate-700 dark:bg-slate-800"
          :class="{ 'border-indigo-500! bg-indigo-50! dark:bg-indigo-950!': anonymous === true }"
        >
          <input
            v-model="anonymous"
            type="radio"
            name="join-identity"
            :value="true"
            class="accent-indigo-600"
          />
          <span class="flex flex-col gap-1">
            <span class="text-sm font-semibold">Participar anonimamente</span>
            <span class="text-xs text-slate-500 dark:text-slate-400">
              A minha participação será registada como anónima.
            </span>
          </span>
        </label>

        <p v-if="choiceError" role="alert" class="text-xs text-red-500">
          {{ choiceError }}
        </p>
      </fieldset>

      <p class="text-xs text-slate-500 dark:text-slate-400">
        Podes alterar esta escolha voltando a entrar na sala.
      </p>

      <p
        v-if="serverError"
        role="alert"
        class="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700 dark:border-red-900 dark:bg-red-950 dark:text-red-300"
      >
        {{ serverError }}
      </p>

      <div class="flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
        <BaseButton variant="secondary" :disabled="isSubmitting" @click="handleClose">
          Cancelar
        </BaseButton>

        <BaseButton type="submit" :disabled="isSubmitting">
          {{ isSubmitting ? 'A entrar...' : 'Entrar na sala' }}
        </BaseButton>
      </div>
    </form>
  </BaseModal>
</template>
