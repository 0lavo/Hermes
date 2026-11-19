<script setup lang="ts">
interface Props {
  modelValue?: string
  label?: string
  placeholder?: string
  multiline?: boolean
  rows?: number
  disabled?: boolean
}
withDefaults(defineProps<Props>(), { modelValue: '', multiline: false, rows: 3, disabled: false })
const emit = defineEmits<{ (e: 'update:modelValue', value: string): void }>()
</script>

<template>
  <div class="flex flex-col gap-1.5 w-full">
    <label v-if="label" class="text-xs font-bold uppercase tracking-wider text-slate-700 dark:text-slate-300">
      {{ label }}
    </label>
    <textarea
      v-if="multiline"
      :value="modelValue"
      :placeholder="placeholder"
      :rows="rows"
      :disabled="disabled"
      @input="emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
      class="w-full rounded-xl border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 px-4 py-3 text-base font-normal text-slate-900 dark:text-white placeholder:text-slate-400 dark:placeholder:text-slate-500 transition-all focus:border-[#4f46e5] focus:outline-none focus:ring-4 focus:ring-indigo-500/20 disabled:bg-slate-100 dark:disabled:bg-slate-900/50 disabled:cursor-not-allowed"
    />
    <input
      v-else
      :value="modelValue"
      :placeholder="placeholder"
      :disabled="disabled"
      @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)"
      class="w-full min-h-[44px] rounded-xl border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 px-4 py-2 text-base font-normal text-slate-900 dark:text-white placeholder:text-slate-400 dark:placeholder:text-slate-500 transition-all focus:border-[#4f46e5] focus:outline-none focus:ring-4 focus:ring-indigo-500/20 disabled:bg-slate-100 dark:disabled:bg-slate-900/50 disabled:cursor-not-allowed"
    />
  </div>
</template>