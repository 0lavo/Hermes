<script setup lang="ts">
interface Props {
  isOpen: boolean
  title?: string
}
defineProps<Props>()
const emit = defineEmits<{ (e: 'close'): void }>()
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs">
    <div class="w-full max-w-lg bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 shadow-xl overflow-hidden p-6 flex flex-col gap-4 animate-in fade-in zoom-in-95 duration-150">
      <div class="flex items-center justify-between">
        <h3 v-if="title" class="text-xl font-semibold text-slate-900 dark:text-white">{{ title }}</h3>
        <button @click="emit('close')" class="p-2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 rounded-lg cursor-pointer">
          ✕
        </button>
      </div>
      <div>
        <slot />
      </div>
    </div>
  </div>
</template>