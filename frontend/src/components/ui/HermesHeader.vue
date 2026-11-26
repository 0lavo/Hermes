<script setup lang="ts">
import HermesLogo from './HermesLogo.vue'
</script>

<template>
  <header class="w-full">
    <div
      class="max-w-6xl mx-auto px-6 py-6 min-h-24 flex items-center justify-between gap-4"
    >
      <HermesLogo />

      <div class="flex items-center gap-2 sm:gap-3">
        <slot />
      </div>
    </div>
  </header>
</template>