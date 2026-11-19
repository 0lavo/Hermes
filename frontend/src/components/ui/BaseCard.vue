<script setup lang="ts">
interface Props {
  interactive?: boolean
  active?: boolean
}

defineProps<Props>()
</script>

<template>
  <div
    :class="[
      'rounded-2xl border transition-all duration-200 p-5 md:p-6',
      'bg-white dark:bg-slate-800/90',
      active
        ? 'border-emerald-500 ring-1 ring-emerald-500/20 shadow-sm'
        : 'border-slate-200 dark:border-slate-700/80 shadow-2xs',
      interactive
        ? 'hover:border-slate-300 dark:hover:border-slate-600 hover:shadow-sm cursor-pointer'
        : '',
    ]"
  >
    <slot />
  </div>
</template>