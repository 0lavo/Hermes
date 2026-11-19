<script setup lang="ts">
import { ref } from 'vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseBadge from '@/components/ui/BaseBadge.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import ThemeToggle from '@/components/ui/ThemeToggle.vue'
import StudentQuestionCard from '@/components/ui/StudentQuestionCard.vue';

const inputValue = ref('')
const textareaValue = ref('')
const isModalOpen = ref(false)
</script>

<template>
  <div class="min-h-screen p-6 md:p-12 max-w-4xl mx-auto flex flex-col gap-8">
    
    <!-- Cabeçalho & Botão de Dark/Light Mode -->
    <header class="flex items-center justify-between">
      <div>
        <span class="text-xs font-semibold uppercase tracking-wider text-indigo-600 dark:text-indigo-400">Hermes Design System</span>
        <h1 class="text-3xl font-bold tracking-tight text-slate-900 dark:text-white mt-1">Painel de Teste dos Componentes</h1>
      </div>
      <ThemeToggle />
    </header>

    <!-- 1. Teste de Etiquetas (BaseBadge) -->
    <BaseCard>
      <h2 class="text-xl font-semibold text-slate-900 dark:text-white mb-4">1. Etiquetas & Estados (BaseBadge)</h2>
      <div class="flex flex-wrap gap-2 items-center">
        <BaseBadge variant="live" :dot="true">Live</BaseBadge>
        <BaseBadge variant="ready">Ready</BaseBadge>
        <BaseBadge variant="ended">Ended</BaseBadge>
        <BaseBadge variant="tag">Cell membranes</BaseBadge>
      </div>
    </BaseCard>

    <!-- 2. Teste de Botões (BaseButton) -->
    <BaseCard>
      <h2 class="text-xl font-semibold text-slate-900 dark:text-white mb-4">2. Botões (BaseButton)</h2>
      <div class="flex flex-wrap gap-3 items-center">
        <BaseButton variant="primary">Primary (Indigo)</BaseButton>
        <BaseButton variant="secondary">Secondary</BaseButton>
        <BaseButton variant="slate">Slate Navy</BaseButton>
        <BaseButton variant="warning">Warning / Unpause</BaseButton>
        <BaseButton variant="danger">Danger</BaseButton>
        <BaseButton variant="secondary" :disabled="true">Disabled</BaseButton>
      </div>
    </BaseCard>

    <!-- 3. Teste de Formulários (BaseInput) -->
    <BaseCard>
      <h2 class="text-xl font-semibold text-slate-900 dark:text-white mb-4">3. Formulários (BaseInput)</h2>
      <div class="flex flex-col gap-4 max-w-md">
        <BaseInput 
          label="Email do Utilizador" 
          placeholder="dr.lee@university.edu" 
          v-model="inputValue" 
        />
        <BaseInput 
          label="What's on your mind?" 
          placeholder="Type your question here..." 
          :multiline="true" 
          v-model="textareaValue" 
        />
      </div>
    </BaseCard>

    <!-- 4. Teste de Modais (BaseModal) -->
    <BaseCard>
      <h2 class="text-xl font-semibold text-slate-900 dark:text-white mb-4">4. Janela Modal (BaseModal)</h2>
      <div class="flex items-center gap-4">
        <BaseButton @click="isModalOpen = true" variant="primary">Abrir Modal de Teste</BaseButton>
      </div>
    </BaseCard>

    <!-- Modal de Demonstração -->
    <BaseModal :isOpen="isModalOpen" title="Criar Nova Sala" @close="isModalOpen = false">
      <div class="flex flex-col gap-4">
        <p class="text-sm text-slate-600 dark:text-slate-300">Insere o nome da nova sala de aula para gerar o código de acesso automático.</p>
        <BaseInput label="Nome da Sala" placeholder="Ex: Introduction to Biology" />
        <div class="flex justify-end gap-3 mt-2">
          <BaseButton variant="secondary" @click="isModalOpen = false">Cancelar</BaseButton>
          <BaseButton variant="primary" @click="isModalOpen = false">Criar Sala</BaseButton>
        </div>
      </div>
    </BaseModal>

    <!-- 5. Teste de Cartões de Alunos (StudentQuestionCard) -->
    <BaseCard>
      <h2 class="text-xl font-semibold text-slate-900 dark:text-white mb-4">5. Cartões de Perguntas (Q&A Alunos)</h2>
      <div class="flex flex-col gap-4">
        <StudentQuestionCard 
          author="Jamie Park" 
          :anonymous="true"
          question="Why can oxygen pass through the cell membrane, but glucose needs a transport protein?"
          :votes="18"
          tag="Cell membranes"
          status="live"
          statusText="Open"
        />
        <StudentQuestionCard 
          author="Maya Chen" 
          question="Does active transport always use ATP directly, or can it use energy from another source?"
          :votes="12"
          tag="Cellular energy"
          status="live"
          statusText="Open"
        />
      </div>
    </BaseCard>

  </div>
</template>
