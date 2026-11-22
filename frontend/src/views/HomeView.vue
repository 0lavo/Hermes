
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import BaseButton from '@/components/ui/BaseButton.vue'
import JoinRoomModal from '@/components/rooms/JoinRoomModal.vue'

const router = useRouter()
const authStore = useAuthStore()

const isJoinModalOpen = ref(false)

const joinedRoom = ref<{
  code: string
  anonymous: boolean
} | null>(null)

function handleJoined(data: { code: string; anonymous: boolean }) {
  joinedRoom.value = data
  isJoinModalOpen.value = false
}

async function handleLogout() {
  authStore.logout()
  await router.replace('/login')
}
</script>

<template>
  <div class="min-h-screen bg-slate-50 text-slate-900 dark:bg-slate-950 dark:text-white">
    <!-- Cabeçalho -->
    <header class="border-b border-slate-200 bg-white dark:border-slate-800 dark:bg-slate-900">
      <div class="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6">
        <RouterLink to="/" class="text-2xl font-extrabold tracking-tight">
          hermes
        </RouterLink>

        <BaseButton variant="ghost" size="sm" @click="handleLogout">
          Terminar sessão
        </BaseButton>
      </div>
    </header>

    <main class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <!-- Boas-vindas e ações -->
      <section
        class="flex flex-col gap-6 border-b border-slate-200 pb-8
               dark:border-slate-800 sm:flex-row sm:items-center sm:justify-between"
      >
        <h1 class="text-3xl font-extrabold tracking-tight sm:text-4xl">
          Olá, {{ authStore.user?.name }}.
        </h1>

        <BaseButton
          variant="secondary"
          size="lg"
          class="w-full sm:w-auto"
          @click="isJoinModalOpen = true"
        >
          Entrar numa sala
        </BaseButton>
      </section>

      <!-- Confirmação da entrada -->
      <section
        v-if="joinedRoom"
        role="status"
        class="mt-8 rounded-xl border border-emerald-200 bg-emerald-50
               p-5 text-emerald-900 dark:border-emerald-900
               dark:bg-emerald-950 dark:text-emerald-200"
      >
        <h2 class="font-bold">Entrada na sala confirmada!</h2>

        <p class="mt-2 text-sm">
          Sala: {{ joinedRoom.code }}
        </p>

        <p class="mt-1 text-sm">
          Identidade:
          {{ joinedRoom.anonymous ? 'Anónima' : 'Visível' }}
        </p>

        <p class="mt-3 text-xs">
          O espaço de participação será disponibilizado
          numa próxima etapa do projeto.
        </p>
      </section>
    </main>

    <!-- Modal da US09 -->
    <JoinRoomModal
      :open="isJoinModalOpen"
      @close="isJoinModalOpen = false"
      @joined="handleJoined"
    />
  </div>
</template>
