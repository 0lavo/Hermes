<script setup lang="ts">
interface Props {
  variant?: 'primary' | 'secondary' | 'slate' | 'danger' | 'warning' | 'ghost'
  size?: 'sm' | 'md' | 'lg'
  disabled?: boolean
  block?: boolean
}

withDefaults(defineProps<Props>(), {
  variant: 'primary',
  size: 'md',
  disabled: false,
  block: false,
})
</script>

<template>
  <button
    :disabled="disabled"
    :class="[
      'inline-flex items-center justify-center gap-2 font-medium select-none cursor-pointer',
      'transition-all duration-200 ease-out transform hover:scale-[1.02] active:scale-[0.98]',
      
      // Tamanhos
      size === 'sm' ? 'text-xs px-3.5 py-2 min-h-[36px] rounded-lg' : '',
      size === 'md' ? 'text-sm px-4.5 py-2.5 min-h-[44px] rounded-xl font-semibold' : '',
      size === 'lg' ? 'text-sm px-6 py-3.5 min-h-[48px] rounded-xl font-bold' : '',
      
      // Variantes
      !disabled && variant === 'primary' 
        ? 'bg-[#4f46e5] hover:bg-[#4338ca] text-white shadow-md hover:shadow-indigo-500/25' : '',
      !disabled && variant === 'secondary' 
        ? 'bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 text-slate-900 dark:text-white border border-slate-300 dark:border-slate-600 shadow-xs' : '',
      
      // Slate Navy: #0f172a no Light Mode, e alto contraste (slate-100 com texto escuro) no Dark Mode
      !disabled && variant === 'slate' 
        ? 'bg-[#0f172a] hover:bg-slate-800 text-white dark:bg-slate-100 dark:hover:bg-white dark:text-slate-900 shadow-md' : '',
      
      !disabled && variant === 'danger' 
        ? 'bg-red-500 hover:bg-red-600 text-white shadow-md' : '',
      !disabled && variant === 'warning' 
        ? 'bg-amber-300 dark:bg-amber-500 hover:bg-amber-400 dark:hover:bg-amber-400 text-amber-950 dark:text-slate-950 font-bold shadow-md' : '',
      !disabled && variant === 'ghost' 
        ? 'bg-transparent hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-200' : '',
      
      // Estado desativado
      disabled 
        ? 'bg-slate-100 dark:bg-slate-900 text-slate-400 dark:text-slate-600 border border-slate-200 dark:border-slate-800 cursor-not-allowed shadow-none hover:scale-100' : '',
      
      block ? 'w-full' : '',
    ]"
  >
    <slot />
  </button>
</template>