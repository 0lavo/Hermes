<script setup lang="ts">
import { ref } from 'vue'
import BaseBadge from '@/components/ui/BaseBadge.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import StudentQuestionCard from '@/components/ui/StudentQuestionCard.vue'
import ThemeToggle from '@/components/ui/ThemeToggle.vue'

const isModalOpen = ref(false)
const sampleEmail = ref('')
const sampleRoomName = ref('')
const modalInput = ref('')
const formFeedback = ref('')

const handleDemoSubmit = () => {
  formFeedback.value = `Formulário submetido com sucesso! Sala: "${sampleRoomName.value || 'Sem nome'}"`
}
</script>

<template>
  <div class="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 transition-colors duration-200 p-6 md:p-10">
    <div class="max-w-5xl mx-auto flex flex-col gap-10">
      <!-- Cabeçalho -->
      <header class="flex flex-wrap items-center justify-between gap-4 pb-6 border-b border-slate-200 dark:border-slate-800">
        <div>
          <span class="font-mono text-xs uppercase tracking-wider text-indigo-600 dark:text-indigo-400 font-semibold">
            Hermes UI Foundation
          </span>
          <h1 class="text-2xl md:text-3xl font-bold mt-1">
            Design System & Component Playground
          </h1>
          <p class="text-sm text-slate-600 dark:text-slate-400 mt-1">
          </p>
        </div>

        <ThemeToggle />
      </header>

      <!-- Secção 1: Botões e Tipo por Defeito -->
      <section class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-6 shadow-xs flex flex-col gap-5">
        <div>
          <h2 class="text-lg font-bold">1. BaseButton (Variantes, Tamanhos e Safe Type)</h2>
          <p class="text-sm text-slate-500 dark:text-slate-400">
            Todos os botões usam <code class="font-mono text-xs bg-slate-100 dark:bg-slate-800 px-1.5 py-0.5 rounded">type="button"</code> por defeito para evitar submissões acidentais.
          </p>
        </div>

        <div class="flex flex-wrap items-center gap-3">
          <BaseButton variant="primary">Primary</BaseButton>
          <BaseButton variant="secondary">Secondary</BaseButton>
          <BaseButton variant="slate">Slate</BaseButton>
          <BaseButton variant="warning">Warning</BaseButton>
          <BaseButton variant="danger">Danger</BaseButton>
          <BaseButton variant="ghost">Ghost</BaseButton>
          <BaseButton disabled>Disabled</BaseButton>
        </div>

        <div class="flex flex-wrap items-center gap-3 pt-2 border-t border-slate-100 dark:border-slate-800">
          <BaseButton size="sm">Small (sm)</BaseButton>
          <BaseButton size="md">Medium (md)</BaseButton>
          <BaseButton size="lg">Large (lg)</BaseButton>
        </div>
      </section>

      <!-- Secção 2: Badges -->
      <section class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-6 shadow-xs flex flex-col gap-4">
        <div>
          <h2 class="text-lg font-bold">2. BaseBadge (Estados Semânticos)</h2>
          <p class="text-sm text-slate-500 dark:text-slate-400">
            Indicadores de estado de sala e etiquetas de tópico.
          </p>
        </div>

        <div class="flex flex-wrap items-center gap-3">
          <BaseBadge variant="live">Live</BaseBadge>
          <BaseBadge variant="ready">Ready</BaseBadge>
          <BaseBadge variant="ended">Ended</BaseBadge>
          <BaseBadge variant="tag">#EngenhariaDeSoftware</BaseBadge>
        </div>
      </section>

      <!-- Secção 3: Formulários (BaseInput) -->
      <section class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-6 md:p-8 shadow-xs flex flex-col gap-5">
        <h2 class="text-xl font-bold text-slate-900 dark:text-white">
          3. Formulários (BaseInput)
        </h2>

        <form @submit.prevent="handleDemoSubmit" class="flex flex-col gap-5 max-w-md">
          <BaseInput
            id="test-email"
            v-model="sampleEmail"
            type="email"
            label="EMAIL DO UTILIZADOR"
            placeholder="dr.lee@university.edu"
          />

          <BaseInput
            id="test-question"
            v-model="sampleRoomName"
            type="textarea"
            :rows="4"
            label="pergunta"
            placeholder="Type your question here..."
          />

          <div class="flex flex-wrap items-center gap-3 pt-1">
            <BaseButton type="submit" variant="primary">
              Enviar
            </BaseButton>
            <BaseButton
              variant="secondary"
              @click="formFeedback = 'Ação cancelada (o formulário não foi submetido).'"
            >
              Cancelar
            </BaseButton>
          </div>
        </form>

        <p
          v-if="formFeedback"
          class="text-sm font-medium text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-950/50 border border-indigo-200 dark:border-indigo-800/60 rounded-xl px-4 py-2.5 max-w-md"
        >
          {{ formFeedback }}
        </p>
      </section>

      <!-- Secção 4: Modal Interativo -->
      <section class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-6 shadow-xs flex flex-col gap-4">
        <div>
          <h2 class="text-lg font-bold">4. BaseModal (Fecho com Esc, Overlay ou Botão)</h2>
          <p class="text-sm text-slate-500 dark:text-slate-400">
            Abre o modal abaixo e testa fechar pressionando a tecla <kbd class="font-mono text-xs px-1.5 py-0.5 rounded border border-slate-300 dark:border-slate-700 bg-slate-100 dark:bg-slate-800">Esc</kbd>.
          </p>
        </div>

        <div>
          <BaseButton variant="primary" @click="isModalOpen = true">
            Abrir Modal de Teste
          </BaseButton>
        </div>
      </section>

      <!-- Secção 5: StudentQuestionCard (Público vs. Anónimo) -->
      <section class="flex flex-col gap-4">
        <div>
          <h2 class="text-lg font-bold">5. StudentQuestionCard (Autor Visível vs. Anónimo)</h2>
          <p class="text-sm text-slate-500 dark:text-slate-400">
            Testa o voto (dimensões fixas antes/depois) e verifica que o cartão anónimo oculta o nome real do autor.
          </p>
        </div>

        <div class="flex flex-col gap-4">
          <StudentQuestionCard
            author="Mariana Silva"
            :anonymous="false"
            question="Qual é a diferença prática entre autenticação baseada em sessão e JWT neste projeto?"
            :votes="12"
            tag="Segurança"
            status="live"
            status-text="Open"
          />

          <StudentQuestionCard
            author="João Pedro Costa"
            :anonymous="true"
            question="O exame final vai incluir matéria das aulas práticas de Docker e CI/CD?"
            :votes="27"
            tag="Avaliação"
            status="ready"
            status-text="Answered"
          />
        </div>
      </section>
    </div>

    <!-- Modal de Teste -->
    <BaseModal
      :open="isModalOpen"
      title="Criar Nova Sala (Teste)"
      @close="isModalOpen = false"
    >
      <div class="flex flex-col gap-4">
        <p class="text-sm text-slate-600 dark:text-slate-300">
          Este modal fecha ao clicar no <strong>✕</strong>, ao clicar fora da janela ou ao carregar na tecla <strong>Esc</strong>.
        </p>

        <BaseInput
          id="modal-code-input"
          v-model="modalInput"
          label="Código de Acesso da Sala"
          placeholder="Ex: HERMES-2026"
        />

        <div class="flex justify-end gap-3 pt-2">
          <BaseButton variant="secondary" @click="isModalOpen = false">
            Cancelar
          </BaseButton>
          <BaseButton variant="primary" @click="isModalOpen = false">
            Confirmar
          </BaseButton>
        </div>
      </div>
    </BaseModal>
  </div>
</template>