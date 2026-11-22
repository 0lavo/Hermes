<script setup lang="ts">
import { nextTick, onUnmounted, ref, useId, watch } from 'vue'

interface Props {
  open: boolean
  title?: string
  closeOnBackdrop?: boolean
  closeOnEscape?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  closeOnBackdrop: true,
  closeOnEscape: true,
})

const emit = defineEmits<{
  (e: 'close'): void
}>()

const modalRef = ref<HTMLElement | null>(null)
const titleId = useId()

let previousActiveElement: HTMLElement | null = null
let previousOverflow = ''
let isBodyLocked = false

const focusableSelector = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(',')

function getFocusableElements(): HTMLElement[] {
  if (!modalRef.value) return []

  return Array.from(
    modalRef.value.querySelectorAll<HTMLElement>(focusableSelector)
  ).filter((element) => !element.hasAttribute('disabled'))
}

function handleKeydown(event: KeyboardEvent) {
  if (!props.open) return

  if (event.key === 'Escape' && props.closeOnEscape) {
    event.preventDefault()
    emit('close')
    return
  }

  if (event.key === 'Tab') {
    const elements = getFocusableElements()

    if (elements.length === 0) {
      event.preventDefault()
      modalRef.value?.focus()
      return
    }

    const first = elements[0]
    const last = elements[elements.length - 1]

    if (!first || !last) return

    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault()
      last.focus()
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault()
      first.focus()
    }
  }
}

async function openModal() {
  previousActiveElement =
    document.activeElement instanceof HTMLElement
      ? document.activeElement
      : null

  if (!isBodyLocked) {
    previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    isBodyLocked = true
  }

  window.addEventListener('keydown', handleKeydown)

  await nextTick()

  const elements = getFocusableElements()

  if (elements.length > 0) {
    elements[0]?.focus()
  } else {
    modalRef.value?.focus()
  }
}

function closeModal() {
  window.removeEventListener('keydown', handleKeydown)

  if (isBodyLocked) {
    document.body.style.overflow = previousOverflow
    isBodyLocked = false
  }

  previousActiveElement?.focus()
  previousActiveElement = null
}

function handleBackdropClick() {
  if (props.closeOnBackdrop) {
    emit('close')
  }
}

watch(
  () => props.open,
  (isOpen, wasOpen) => {
    if (isOpen) {
      void openModal()
    } else if (wasOpen) {
      closeModal()
    }
  },
  { immediate: true }
)

onUnmounted(() => {
  closeModal()
})
</script>

<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition-opacity duration-200"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-opacity duration-150"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="open"
        class="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto p-4 bg-slate-950/60 backdrop-blur-xs"
        @click.self="handleBackdropClick"
      >
        <div
          ref="modalRef"
          role="dialog"
          aria-modal="true"
          :aria-labelledby="title ? titleId : undefined"
          tabindex="-1"
          class="my-auto w-full max-w-lg rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-6 shadow-xl text-slate-900 dark:text-white outline-none"
        >
          <div class="flex items-center justify-between gap-4 mb-4">
            <h2
              v-if="title"
              :id="titleId"
              class="text-lg font-bold"
            >
              {{ title }}
            </h2>

            <button
              type="button"
              @click="emit('close')"
              aria-label="Fechar modal"
              class="ml-auto min-h-[44px] min-w-[44px] inline-flex items-center justify-center rounded-xl text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#4f46e5] cursor-pointer"
            >
              ✕
            </button>
          </div>

          <slot />
        </div>
      </div>
    </Transition>
  </Teleport>
</template>