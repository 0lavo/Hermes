<script setup lang="ts">
interface Props {
  variant?: 'live' | 'ready' | 'ended' | 'tag'
  dot?: boolean
}
withDefaults(defineProps<Props>(), { variant: 'tag', dot: false })
</script>

<template>
  <span
    :class="[
      'inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium leading-none',
      variant === 'live' ? 'bg-emerald-50 dark:bg-emerald-950/60 text-emerald-600 dark:text-emerald-400 border border-emerald-200/60 dark:border-emerald-800/60' : '',
      variant === 'ready' ? 'bg-blue-50 dark:bg-blue-950/60 text-blue-600 dark:text-blue-400 border border-blue-200/60 dark:border-blue-800/60' : '',
      variant === 'ended' ? 'bg-slate-100 dark:bg-slate-800 text-slate-500 dark:text-slate-400' : '',
      variant === 'tag' ? 'bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300' : '',
    ]"
  >
    <span v-if="dot" :class="['h-1.5 w-1.5 rounded-full', variant === 'live' ? 'bg-emerald-500 animate-pulse' : 'bg-current']" />
    <slot />
  </span>
</template>