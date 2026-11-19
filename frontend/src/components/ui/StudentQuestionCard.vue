<script setup lang="ts">
import { ref } from 'vue'
import BaseBadge from './BaseBadge.vue'

interface Props {
  author: string
  anonymous?: boolean
  question: string
  votes: number
  tag?: string
  status?: 'live' | 'ready' | 'ended' | 'tag'
  statusText?: string
}

const props = withDefaults(defineProps<Props>(), {
  anonymous: false,
  votes: 0,
  status: 'live',
  statusText: 'Open',
})

const emit = defineEmits<{ (e: 'vote', newVotes: number): void }>()

const hasVoted = ref(false)
const currentVotes = ref(props.votes)

const handleVote = () => {
  if (!hasVoted.value) {
    hasVoted.value = true
    currentVotes.value += 1
  } else {
    hasVoted.value = false
    currentVotes.value -= 1
  }
  emit('vote', currentVotes.value)
}
</script>

<template>
  <div class="rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800/90 p-5 md:p-6 shadow-xs flex gap-5 items-start transition-all hover:border-slate-300 dark:hover:border-slate-600">
    
    <!-- Botão de Votação com Dimensões Fixas (w-14 h-[68px]) -->
    <button
      type="button"
      @click="handleVote"
      :aria-pressed="hasVoted"
      :class="[
        'flex flex-col items-center justify-center w-14 h-[68px] shrink-0 rounded-xl border transition-all duration-200 cursor-pointer select-none',
        hasVoted
          ? 'bg-[#4f46e5] border-[#4f46e5] text-white shadow-sm'
          : 'bg-slate-50 dark:bg-slate-900/80 border-slate-200 dark:border-slate-700 text-indigo-600 dark:text-indigo-400 hover:border-indigo-300 dark:hover:border-indigo-500/50',
      ]"
    >
      <!-- Visto (✓) quando votado -->
      <svg
        v-if="hasVoted"
        class="w-4 h-4 shrink-0"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        stroke-width="2.75"
      >
        <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
      </svg>

      <!-- Setinha (^) antes de votar -->
      <svg
        v-else
        class="w-4 h-4 shrink-0"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        stroke-width="2.5"
      >
        <path stroke-linecap="round" stroke-linejoin="round" d="M5 15l7-7 7 7" />
      </svg>

      <span class="text-lg font-bold leading-5 mt-1 tabular-nums">
        {{ currentVotes }}
      </span>
    </button>

    <!-- Conteúdo da Pergunta -->
    <div class="flex-1 flex flex-col gap-2">
      <div class="flex flex-wrap items-center gap-2">
        <span class="text-sm font-semibold text-slate-900 dark:text-white">{{ author }}</span>
        <span
          v-if="anonymous"
          class="text-xs px-2 py-0.5 rounded-md bg-slate-100 dark:bg-slate-700 text-slate-500 dark:text-slate-300 font-medium"
        >
          Anonymous to peers
        </span>
      </div>

      <p class="text-base text-slate-900 dark:text-slate-100 font-medium leading-relaxed">
        {{ question }}
      </p>

      <div class="flex items-center gap-2 mt-1">
        <BaseBadge v-if="tag" variant="tag">{{ tag }}</BaseBadge>
        <BaseBadge :variant="status">{{ statusText }}</BaseBadge>
      </div>
    </div>

  </div>
</template>