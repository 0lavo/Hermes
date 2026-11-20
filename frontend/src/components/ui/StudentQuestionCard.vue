
<script setup lang="ts">
import { computed, ref, watch } from 'vue'
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

const emit = defineEmits<{
  (e: 'vote', newVotes: number): void
}>()

const hasVoted = ref(false)
const currentVotes = ref(props.votes)

const displayAuthor = computed(() =>
  props.anonymous ? 'Anonymous' : props.author
)

const voteLabel = computed(() =>
  hasVoted.value ? 'Remove your vote' : 'Vote for this question'
)

// Sincronizar votos com o componente pai
watch(
  () => props.votes,
  (newVotes) => {
    currentVotes.value = newVotes
  }
)

// Gerir votação
function handleVote() {
  if (hasVoted.value) {
    if (currentVotes.value > 0) {
      currentVotes.value -= 1
    }

    hasVoted.value = false
  } else {
    currentVotes.value += 1
    hasVoted.value = true
  }

  emit('vote', currentVotes.value)
}
</script>

<template>
  <div
    class="rounded-2xl border border-slate-200 dark:border-slate-700
           bg-white dark:bg-slate-800/90 p-5 md:p-6 shadow-xs
           flex gap-5 items-start transition-all
           hover:border-slate-300 dark:hover:border-slate-600"
  >
    <!-- Vote Button -->
    <button
      type="button"
      @click="handleVote"
      :aria-pressed="hasVoted"
      :aria-label="voteLabel"
      :class="[
        'flex flex-col items-center justify-center w-14 h-[68px]',
        'shrink-0 rounded-xl border transition-all duration-200',
        'cursor-pointer select-none',
        hasVoted
          ? 'bg-[#4f46e5] border-[#4f46e5] text-white shadow-sm'
          : 'bg-slate-50 dark:bg-slate-900/80 border-slate-200 dark:border-slate-700 text-indigo-600 dark:text-indigo-400 hover:border-indigo-300 dark:hover:border-indigo-500/50',
      ]"
    >
      <!-- Voted Icon -->
      <svg
        v-if="hasVoted"
        class="w-4 h-4 shrink-0"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        stroke-width="2.75"
        aria-hidden="true"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M5 13l4 4L19 7"
        />
      </svg>

      <!-- Upvote Icon -->
      <svg
        v-else
        class="w-4 h-4 shrink-0"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        stroke-width="2.5"
        aria-hidden="true"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M5 15l7-7 7 7"
        />
      </svg>

      <span class="text-lg font-bold leading-5 mt-1 tabular-nums">
        {{ currentVotes }}
      </span>
    </button>

    <!-- Student Content -->
    <div class="flex-1 min-w-0 flex flex-col gap-2">

      <!-- Author -->
      <div class="flex flex-wrap items-center gap-2">
        <span class="text-sm font-semibold text-slate-900 dark:text-white">
          {{ displayAuthor }}
        </span>

        <span
          v-if="anonymous"
          class="text-xs px-2 py-0.5 rounded-md
                 bg-slate-100 dark:bg-slate-700
                 text-slate-500 dark:text-slate-300 font-medium"
        >
          Anonymous to peers
        </span>
      </div>

      <!-- Question -->
      <p
        class="text-base text-slate-900 dark:text-slate-100
               font-medium leading-relaxed break-words"
      >
        {{ question }}
      </p>

      <!-- Badges -->
      <div class="flex flex-wrap items-center gap-2 mt-1">
        <BaseBadge v-if="tag" variant="tag">
          {{ tag }}
        </BaseBadge>

        <BaseBadge :variant="status">
          {{ statusText }}
        </BaseBadge>
      </div>

    </div>
  </div>
</template>
