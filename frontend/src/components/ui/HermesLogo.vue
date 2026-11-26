<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
</script>

<template>
  <RouterLink
    :to="{ name: authStore.isAuthenticated ? 'MyRooms' : 'home' }"
    :aria-label="authStore.isAuthenticated ? 'My Rooms' : 'Hermes homepage'"
    class="text-3xl font-extrabold tracking-tight text-[#4f46e5] dark:text-indigo-400 select-none"
  >
    hermes
  </RouterLink>
</template>