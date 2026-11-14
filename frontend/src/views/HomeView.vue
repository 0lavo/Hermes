<script setup lang="ts">
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
</script>

<template>
  <main>
    <h1>Hermes — Radar de Dúvidas</h1>

    <p>Autenticado: {{ auth.isAuthenticated }}</p>

    <button @click="auth.setToken('token-teste')">
      Simular login
    </button>

    <button @click="auth.logout()">
      Logout
    </button>
  </main>
</template>