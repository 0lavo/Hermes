
<script setup lang="ts">
import { computed, useId } from 'vue'

type InputType =
  | 'text'
  | 'email'
  | 'password'
  | 'number'
  | 'tel'
  | 'url'
  | 'search'
  | 'textarea'

interface Props {
  modelValue?: string
  label?: string
  id?: string
  type?: InputType
  placeholder?: string
  disabled?: boolean
  rows?: number
  error?: string
  hint?: string
  required?: boolean
  autocomplete?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  type: 'text',
  disabled: false,
  rows: 4,
  required: false,
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const fallbackId = useId()

const resolvedId = computed(() => props.id ?? fallbackId)

const descriptionId = computed(
  () => `${resolvedId.value}-description`
)

const hasDescription = computed(
  () => !!props.error || !!props.hint
)

const inputClasses = computed(() => [
  'w-full rounded-xl border bg-white dark:bg-slate-900',
  'px-4 py-3 text-sm text-slate-900 dark:text-white',
  'placeholder:text-slate-400',
  'focus:outline-none focus:ring-2 transition-colors',
  'disabled:opacity-50 disabled:cursor-not-allowed',
  props.error
    ? 'border-red-500 focus:ring-red-500'
    : 'border-slate-300 dark:border-slate-700 focus:ring-[#4f46e5]',
])

function handleInput(event: Event) {
  const target = event.target as HTMLInputElement | HTMLTextAreaElement
  emit('update:modelValue', target.value)
}
</script>

<template>
  <div class="flex flex-col gap-2">

    <!-- Label -->
    <label
      v-if="label"
      :for="resolvedId"
      class="text-xs font-bold uppercase tracking-wider
             text-slate-700 dark:text-slate-300 cursor-pointer"
    >
      {{ label }}

      <span v-if="required" class="text-red-500">
        *
      </span>
    </label>

    <!-- Textarea -->
    <textarea
      v-if="type === 'textarea'"
      :id="resolvedId"
      :rows="rows"
      :value="modelValue"
      :placeholder="placeholder"
      :disabled="disabled"
      :required="required"
      :aria-invalid="!!error"
      :aria-describedby="hasDescription ? descriptionId : undefined"
      :class="inputClasses"
      @input="handleInput"
    />

    <!-- Input -->
    <input
      v-else
      :id="resolvedId"
      :type="type"
      :value="modelValue"
      :placeholder="placeholder"
      :disabled="disabled"
      :required="required"
      :autocomplete="autocomplete"
      :aria-invalid="!!error"
      :aria-describedby="hasDescription ? descriptionId : undefined"
      :class="[
        inputClasses,
        'min-h-[44px]'
      ]"
      @input="handleInput"
    />

    <!-- Hint / Error -->
    <p
      v-if="hasDescription"
      :id="descriptionId"
      :class="[
        'text-xs',
        error ? 'text-red-500' : 'text-slate-500 dark:text-slate-400'
      ]"
    >
      {{ error || hint }}
    </p>

  </div>
</template>
