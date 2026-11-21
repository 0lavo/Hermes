
<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref } from 'vue'

interface GoogleCredentialResponse {
  credential?: string
}

interface GoogleId {
  initialize: (config: {
    client_id: string
    callback: (response: GoogleCredentialResponse) => void
  }) => void

  renderButton: (
    element: HTMLElement,
    options: {
      theme: string
      size: string
      text: string
      shape: string
      width: number
    },
  ) => void
}

declare global {
  interface Window {
    google?: {
      accounts: {
        id: GoogleId
      }
    }
  }
}

const emit = defineEmits<{
  credential: [idToken: string]
}>()

const buttonContainer = ref<HTMLElement | null>(null)

let resizeObserver: ResizeObserver | null = null
let lastRenderedWidth = 0

function renderGoogleButton() {
  const container = buttonContainer.value
  const googleId = window.google?.accounts.id

  if (!container || !googleId) return

  // Largura disponível, respeitando o limite da Google
  const width = Math.min(Math.floor(container.clientWidth), 400)

  if (width === 0 || width === lastRenderedWidth) return

  lastRenderedWidth = width

  // Remove a versão anterior antes de desenhar novamente
  container.replaceChildren()

  googleId.renderButton(container, {
    theme: 'outline',
    size: 'large',
    text: 'signin_with',
    shape: 'rectangular',
    width,
  })
}

function initializeGoogle() {
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID
  const googleId = window.google?.accounts.id

  if (!clientId || !googleId || !buttonContainer.value) return

  googleId.initialize({
    client_id: clientId,
    callback: (response) => {
      if (response.credential) {
        emit('credential', response.credential)
      }
    },
  })

  // Observa alterações de largura do contentor
  resizeObserver = new ResizeObserver(() => {
    renderGoogleButton()
  })

  resizeObserver.observe(buttonContainer.value)
  renderGoogleButton()
}

let script: HTMLScriptElement | null = null

onMounted(() => {
  if (window.google?.accounts.id) {
    initializeGoogle()
    return
  }

  script = document.querySelector<HTMLScriptElement>(
    'script[src="https://accounts.google.com/gsi/client"]',
  )

  script?.addEventListener('load', initializeGoogle, { once: true })
})

onBeforeUnmount(() => {
  resizeObserver?.disconnect()
  script?.removeEventListener('load', initializeGoogle)
})
</script>

<template>
  <div class="w-full">
    <div ref="buttonContainer" class="w-full"></div>
  </div>
</template>
