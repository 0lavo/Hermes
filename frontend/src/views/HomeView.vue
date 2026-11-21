
<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const router = useRouter()
const authStore = useAuthStore()

async function handleLogout() {
  authStore.logout()
  await router.replace('/login')
}
</script>

<template>
  <main>
    <h1>Hermes — Radar de Dúvidas</h1>

    <p>Bem-vindo, {{ authStore.user?.name }}</p>

    <p>Autenticado: {{ authStore.isAuthenticated }}</p>

    <button
      type="button"
      class="rounded-lg bg-red-500 px-4 py-2 text-white hover:bg-red-600"
      @click="handleLogout"
    >
      Terminar sessão
    </button>
  </main>
</template>
