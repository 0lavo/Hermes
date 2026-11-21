
<script setup lang="ts">
import { ref } from 'vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import axios from 'axios'
import { useRouter } from 'vue-router'
import { register, loginWithGoogle } from '@/services/authService'
import { useAuthStore } from '@/stores/auth'
import GoogleLoginButton from '@/components/auth/GoogleLoginButton.vue'

const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const name = ref('')

const router = useRouter()
const authStore = useAuthStore()

const isLoading = ref(false)
const errorMessage = ref('')

async function handleRegister() {
  if (isLoading.value) return

  errorMessage.value = ''

  // Verificar se as passwords coincidem
  if (password.value !== confirmPassword.value) {
    errorMessage.value = 'As palavras-passe não coincidem.'
    return
  }

  // O backend exige pelo menos 8 caracteres
  if (password.value.length < 8) {
    errorMessage.value = 'A palavra-passe deve ter pelo menos 8 caracteres.'
    return
  }

  isLoading.value = true

  try {
    const response = await register({
      name: name.value.trim(),
      email: email.value.trim(),
      password: password.value,
    })

    authStore.setSession(response)
    await router.push('/')
  } catch (error) {
    if (axios.isAxiosError<{ message?: string }>(error)) {
      errorMessage.value =
        error.response?.data?.message ??
        'Não foi possível criar a conta. Tenta novamente.'
    } else {
      errorMessage.value = 'Ocorreu um erro inesperado.'
    }
  } finally {
    isLoading.value = false
  }
}

async function handleGoogleCredential(idToken: string) {
  if (isLoading.value) return

  errorMessage.value = ''
  isLoading.value = true

  try {
    const response = await loginWithGoogle(idToken)

    authStore.setSession(response)
    await router.push('/')
  } catch (error) {
    if (axios.isAxiosError<{ message?: string }>(error)) {
      errorMessage.value =
        error.response?.data?.message ??
        'Não foi possível continuar com Google.'
    } else {
      errorMessage.value = 'Ocorreu um erro inesperado.'
    }
  } finally {
    isLoading.value = false
  }
}

</script>

<template>
  <main class="min-h-screen flex items-center justify-center px-4 py-8">
    <div class="w-full max-w-sm bg-white dark:bg-slate-900 rounded-2xl shadow-lg p-6 sm:p-8">

      <!-- Logótipo a adicionar posteriormente -->
      <p class="text-xl font-bold text-indigo-600 mb-6">
        hermes
      </p>

      <h1 class="text-2xl font-bold text-slate-900 dark:text-white mb-6">
        Criar conta
      </h1>

    <form class="flex flex-col gap-5" @submit.prevent="handleRegister">
        <BaseInput
            v-model="name"
            type="text"
            label="Nome completo"
            placeholder="Introduz o teu nome"
            autocomplete="name"
            required
        />
        <BaseInput
          v-model="email"
          type="email"
          label="Email"
          placeholder="exemplo@email.com"
          required
        />

        <BaseInput
          v-model="password"
          type="password"
          label="Palavra-passe"
          required
        />

        <BaseInput
          v-model="confirmPassword"
          type="password"
          label="Confirmar palavra-passe"
          required
        />

        <p
        v-if="errorMessage"
        role="alert"
        class="text-sm text-red-600 dark:text-red-400"
        >
        {{ errorMessage }}
        </p>

        <BaseButton
        type="submit"
        variant="primary"
        block
        :disabled="isLoading"
        >
        {{ isLoading ? 'A criar conta...' : 'Criar conta' }}
        </BaseButton>
      </form>


      <div class="my-6 flex items-center gap-4">
        <div class="h-px flex-1 bg-slate-200 dark:bg-slate-700"></div>

        <span class="text-xs font-medium uppercase text-slate-400">
            ou
        </span>

        <div class="h-px flex-1 bg-slate-200 dark:bg-slate-700"></div>
        </div>

        <GoogleLoginButton @credential="handleGoogleCredential" />

      <p class="text-sm text-center text-slate-500 dark:text-slate-400 mt-6">
        Já tens conta?
        <RouterLink to="/login" class="text-indigo-600 dark:text-indigo-400 font-semibold hover:underline">
          Iniciar sessão
        </RouterLink>
      </p>

    </div>
  </main>
</template>
