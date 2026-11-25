<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import hermesBird from '@/assets/hermesBird-nobg.png'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import ThemeToggle from '@/components/ui/ThemeToggle.vue'

interface PreviewQuestion {
  id: number
  author: string
  anonymous: boolean
  timeAgo: string
  question: string
  votes: number
  voted: boolean
}

const router = useRouter()
const joinModalOpen = ref(false)
const roomCode = ref('')
const quickQuestion = ref('')

const previewQuestions = ref<PreviewQuestion[]>([
  {
    id: 1,
    author: 'Mariana Silva',
    anonymous: false,
    timeAgo: '2m ago',
    question: 'What is the practical difference between session auth and JWT here?',
    votes: 13,
    voted: true,
  },
  {
    id: 2,
    author: 'Anonymous',
    anonymous: true,
    timeAgo: '4m ago',
    question: 'Can we walk through the last example one more time?',
    votes: 8,
    voted: false,
  },
  {
    id: 3,
    author: 'Anonymous',
    anonymous: true,
    timeAgo: '6m ago',
    question: 'How should we apply this pattern in our current project?',
    votes: 5,
    voted: false,
  },
])

function scrollToSection(sectionId: string) {
  const target = document.getElementById(sectionId)
  target?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

function goToRegister() {
  router.push('/register')
}

function goToLogin() {
  router.push('/login')
}

function togglePreviewVote(item: PreviewQuestion) {
  if (item.voted) {
    item.votes = Math.max(0, item.votes - 1)
    item.voted = false
  } else {
    item.votes += 1
    item.voted = true
  }
}

function handleQuickSend() {
  const trimmed = quickQuestion.value.trim()
  if (!trimmed) return

  previewQuestions.value.unshift({
    id: Date.now(),
    author: 'Anonymous',
    anonymous: true,
    timeAgo: 'Just now',
    question: trimmed,
    votes: 1,
    voted: true,
  })
  quickQuestion.value = ''
}

function handleJoinRoom() {
  if (!roomCode.value.trim()) return
  joinModalOpen.value = false
}

const steps = [
  {
    title: 'Join a Room in Seconds',
    description:
      'Enter a live session via a short room code or direct link without lengthy sign-ups or setup friction.',
    icon: 'door',
  },
  {
    title: 'Ask Questions Without Hesitation',
    description:
      'Post questions openly or anonymously the moment a concept feels unclear, before confusion spreads.',
    icon: 'chat',
  },
  {
    title: 'Prioritize and Learn Together',
    description:
      'Upvote shared questions so hosts answer what matters most to the audience and build collective clarity.',
    icon: 'spark',
  },
]

const features = [
  {
    title: 'Real-Time Question Flow',
    description:
      'Keep every participant synced as questions arrive and get answered live, avoiding repeated explanations.',
    icon: 'bolt',
    iconClass:
      'bg-indigo-100 dark:bg-indigo-950/80 text-[#4f46e5] dark:text-indigo-400',
  },
  {
    title: 'Anonymous Participation',
    description:
      'Give quieter attendees and beginners a safe space to speak up so real blockers surface early.',
    icon: 'shield',
    iconClass:
      'bg-emerald-50 dark:bg-emerald-950/70 text-emerald-600 dark:text-emerald-400',
  },
  {
    title: 'Community Upvoting',
    description:
      'Let audience demand highlight high-impact questions automatically so sessions focus on top priorities.',
    icon: 'upvote',
    iconClass:
      'bg-amber-50 dark:bg-amber-950/70 text-amber-600 dark:text-amber-400',
  },
  {
    title: 'Session Recap & History',
    description:
      'Keep questions and answers accessible after the session wraps so everyone can revisit key takeaways.',
    icon: 'layers',
    iconClass:
      'bg-blue-50 dark:bg-blue-950/70 text-blue-600 dark:text-blue-400',
  },
]
</script>

<template>
  <div class="min-h-screen flex flex-col text-slate-900 dark:text-slate-50">
    <!-- NAVBAR -->
    <header class="w-full">
      <div
        class="max-w-6xl mx-auto px-6 py-6 flex items-center justify-between gap-4"
      >
        <a
          href="/"
          aria-label="hermes"
          class="text-3xl font-extrabold tracking-tight text-[#4f46e5] dark:text-indigo-400 select-none"
        >
          hermes
        </a>

        <div class="flex items-center gap-2 sm:gap-3">
          <BaseButton
            variant="ghost"
            size="sm"
            @click="scrollToSection('how-it-works')"
          >
            How It Works
          </BaseButton>

          <BaseButton
            variant="ghost"
            size="sm"
            @click="scrollToSection('features')"
          >
            Features
          </BaseButton>

          <ThemeToggle />
        </div>
      </div>
    </header>

    <!-- HERO SECTION -->
    <section class="flex-1 flex flex-col justify-between">
      <div
        class="max-w-6xl w-full mx-auto px-6 py-10 md:py-16 my-auto grid grid-cols-1 lg:grid-cols-2 items-center gap-12"
      >
        <!-- Round Hermes Bird Mascot -->
        <div class="relative flex items-center justify-center">
          <div
            class="relative w-64 h-64 sm:w-72 sm:h-72 rounded-full overflow-hidden bg-slate-50 dark:bg-slate-100 border-4 border-[#e0e7ff] dark:border-indigo-400/35 shadow-lg dark:shadow-indigo-950/60 flex items-center justify-center p-4 transform-gpu transition-transform duration-300 ease-out hover:scale-[1.03]"
          >
            <img
              :src="hermesBird"
              alt="Hermes messenger bird mascot"
              class="w-full h-full object-contain select-none"
            />
          </div>
        </div>

        <!-- Primary Headline, Actions & Section Links -->
        <div class="flex flex-col items-center text-center max-w-md mx-auto">
          <h1
            class="text-4xl sm:text-5xl font-black tracking-tight leading-[1.12] text-slate-900 dark:text-white mb-5"
          >
            No question<br />
            should go<br />
            unanswered.
          </h1>

          <p
            class="text-base sm:text-lg text-slate-600 dark:text-slate-300 leading-relaxed mb-8 max-w-sm"
          >
            Participate, clear up doubts, and learn together in real time.
          </p>

          <div class="w-full max-w-sm flex flex-col gap-3.5">
            <BaseButton
              variant="primary"
              size="lg"
              block
              class="uppercase tracking-wider"
              @click="goToLogin"
            >
              GET STARTED
            </BaseButton>

            <BaseButton
              variant="secondary"
              size="lg"
              block
              class="uppercase tracking-wider text-[#4f46e5] dark:text-indigo-400"
              @click="goToRegister"
            >
              I ALREADY HAVE AN ACCOUNT
            </BaseButton>
          </div>

          <div class="mt-5 flex flex-wrap items-center justify-center gap-2">
            <BaseButton
              variant="ghost"
              size="sm"
              @click="scrollToSection('how-it-works')"
            >
              How It Works ↓
            </BaseButton>
            <BaseButton
              variant="ghost"
              size="sm"
              @click="scrollToSection('features')"
            >
              Explore Features ↓
            </BaseButton>
          </div>
        </div>
      </div>

      <!-- Bottom Pillars Bar -->
      <div
        class="border-t border-slate-200/70 dark:border-slate-800/80 bg-white/60 dark:bg-slate-900/40 backdrop-blur-xs py-5"
      >
        <div
          class="max-w-6xl mx-auto px-6 flex flex-wrap items-center justify-center gap-6 sm:gap-10 text-sm font-medium text-slate-600 dark:text-slate-400"
        >
          <div class="inline-flex items-center gap-2">
            <svg
              class="w-4 h-4 text-[#4f46e5] dark:text-indigo-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              stroke-width="2.2"
            >
              <circle cx="12" cy="12" r="9" />
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M12 7v5l3 2"
              />
            </svg>
            <span>Real time</span>
          </div>

          <div class="inline-flex items-center gap-2">
            <svg
              class="w-4 h-4 text-[#4f46e5] dark:text-indigo-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              stroke-width="2.2"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M12 3l7 4v5c0 4.5-3 8.4-7 9.5-4-1.1-7-5-7-9.5V7l7-4z"
              />
            </svg>
            <span>Anonymous participation</span>
          </div>

          <div class="inline-flex items-center gap-2">
            <svg
              class="w-4 h-4 text-[#4f46e5] dark:text-indigo-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              stroke-width="2.2"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M12 3l9 9-9 9-9-9 9-9z"
              />
              <circle cx="12" cy="12" r="2.5" />
            </svg>
            <span>Collaborative learning</span>
          </div>
        </div>
      </div>
    </section>

    <!-- HOW IT WORKS SECTION -->
    <section id="how-it-works" class="scroll-mt-6 py-20 px-6">
      <div class="max-w-6xl mx-auto">
        <div class="text-center max-w-xl mx-auto mb-12">
          <h2
            class="text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white mb-3"
          >
            How It Works
          </h2>
          <p class="text-slate-600 dark:text-slate-400">
            Move from initial curiosity to shared clarity in a few straightforward steps.
          </p>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
          <BaseCard
            v-for="step in steps"
            :key="step.title"
            interactive
            class="relative overflow-hidden transform-gpu transition-all duration-200 ease-out hover:scale-[1.03] hover:shadow-xl hover:border-indigo-300 dark:hover:border-indigo-500/50"
          >
            <div
              class="absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-[#4f46e5] via-indigo-500 to-violet-500"
              aria-hidden="true"
            />

            <div
              class="w-11 h-11 rounded-xl bg-[#e0e7ff] dark:bg-indigo-950/80 text-[#4f46e5] dark:text-indigo-400 flex items-center justify-center mb-4"
            >
              <svg
                v-if="step.icon === 'door'"
                class="w-5 h-5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2.2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M15 3h4a2 2 0 012 2v14a2 2 0 01-2 2h-4M10 17l5-5-5-5M15 12H3"
                />
              </svg>

              <svg
                v-else-if="step.icon === 'chat'"
                class="w-5 h-5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2.2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M8 10h8M8 14h5m-7 6l-2-4V6a2 2 0 012-2h12a2 2 0 012 2v8a2 2 0 01-2 2H8l-2 4z"
                />
              </svg>

              <svg
                v-else
                class="w-5 h-5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2.2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"
                />
              </svg>
            </div>

            <h3 class="text-lg font-bold text-slate-900 dark:text-white mb-2">
              {{ step.title }}
            </h3>
            <p class="text-sm text-slate-600 dark:text-slate-300 leading-relaxed">
              {{ step.description }}
            </p>
          </BaseCard>
        </div>
      </div>
    </section>

    <!-- FEATURES SECTION + STUDENT QUESTION CARD PREVIEW -->
    <section id="features" class="scroll-mt-6 py-12 pb-24 px-6">
      <div class="max-w-6xl mx-auto">
        <div class="text-center max-w-xl mx-auto mb-12">
          <h2
            class="text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white mb-3"
          >
            Features Built for Impact
          </h2>
          <p class="text-slate-600 dark:text-slate-400">
            See how live question cards, voting, and anonymity keep every session engaging.
          </p>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
          <!-- Left Column: Feature Cards with Grow Animation -->
          <div class="lg:col-span-6 grid grid-cols-1 sm:grid-cols-2 gap-5">
            <BaseCard
              v-for="item in features"
              :key="item.title"
              interactive
              class="transform-gpu transition-all duration-200 ease-out hover:scale-[1.03] hover:shadow-xl hover:border-indigo-300 dark:hover:border-indigo-500/50"
            >
              <div
                :class="[
                  'w-11 h-11 rounded-xl flex items-center justify-center mb-4',
                  item.iconClass,
                ]"
              >
                <svg
                  v-if="item.icon === 'bolt'"
                  class="w-5 h-5"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="2.2"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M13 2L3 14h9l-1 8 10-12h-9l1-8z"
                  />
                </svg>

                <svg
                  v-else-if="item.icon === 'shield'"
                  class="w-5 h-5"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="2.2"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"
                  />
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M9 12l2 2 4-4"
                  />
                </svg>

                <svg
                  v-else-if="item.icon === 'upvote'"
                  class="w-5 h-5"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="2.2"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M5 15l7-7 7 7"
                  />
                </svg>

                <svg
                  v-else
                  class="w-5 h-5"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="2.2"
                >
                  <polygon points="12 2 2 7 12 12 22 7 12 2" />
                  <polyline points="2 17 12 22 22 17" />
                  <polyline points="2 12 12 17 22 12" />
                </svg>
              </div>

              <h3 class="text-base font-bold text-slate-900 dark:text-white mb-1.5">
                {{ item.title }}
              </h3>
              <p class="text-sm text-slate-600 dark:text-slate-300 leading-relaxed">
                {{ item.description }}
              </p>
            </BaseCard>
          </div>

          <!-- Right Column: Interactive StudentQuestionCard-Style Live Room Preview -->
          <div class="lg:col-span-6">
            <BaseCard
              class="transform-gpu transition-all duration-200 ease-out hover:scale-[1.015] hover:shadow-xl"
            >
              <div class="flex items-start justify-between gap-3 mb-1">
                <h3 class="text-lg font-bold text-slate-900 dark:text-white">
                  Software Engineering
                </h3>
                <span
                  class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium bg-emerald-50 dark:bg-emerald-950/60 text-emerald-600 dark:text-emerald-400 border border-emerald-200/60 dark:border-emerald-800/60 shrink-0"
                >
                  <span class="h-1.5 w-1.5 rounded-full bg-emerald-500 animate-pulse" />
                  Live session
                </span>
              </div>

              <p class="text-xs font-mono text-slate-500 dark:text-slate-400 mb-5">
                Room #482916 · 24 participants
              </p>

              <p class="text-sm font-bold text-slate-800 dark:text-slate-200 mb-3">
                Class Questions
              </p>

              <div class="flex flex-col gap-3">
                <div
                  v-for="q in previewQuestions"
                  :key="q.id"
                  class="rounded-xl border border-slate-200/90 dark:border-slate-700/80 bg-white dark:bg-slate-900/60 p-3.5 flex items-center gap-3.5 transform-gpu transition-all duration-200 ease-out hover:scale-[1.02] hover:border-indigo-300 dark:hover:border-indigo-500/50"
                >
                  <button
                    type="button"
                    @click="togglePreviewVote(q)"
                    :aria-pressed="q.voted"
                    :class="[
                      'flex flex-col items-center justify-center w-12 h-14 rounded-xl border shrink-0 transition-all duration-200 cursor-pointer select-none active:scale-95',
                      q.voted
                        ? 'bg-[#4f46e5] border-[#4f46e5] text-white shadow-sm'
                        : 'bg-indigo-50/80 dark:bg-indigo-950/50 border-indigo-100 dark:border-indigo-800/60 text-[#4f46e5] dark:text-indigo-400 hover:bg-indigo-100 dark:hover:bg-indigo-900/60',
                    ]"
                  >
                    <svg
                      v-if="q.voted"
                      class="w-3.5 h-3.5"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      stroke-width="2.75"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        d="M5 13l4 4L19 7"
                      />
                    </svg>
                    <svg
                      v-else
                      class="w-3.5 h-3.5"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      stroke-width="2.5"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        d="M5 15l7-7 7 7"
                      />
                    </svg>
                    <span class="text-sm font-bold leading-tight mt-0.5 tabular-nums">
                      {{ q.votes }}
                    </span>
                  </button>

                  <div class="min-w-0 flex-1">
                    <p class="text-sm font-bold text-slate-900 dark:text-white leading-snug">
                      {{ q.question }}
                    </p>
                    <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">
                      {{ q.author }} · {{ q.timeAgo }}
                    </p>
                  </div>
                </div>
              </div>

              <!-- Quick Question Input Bar (com id, name e label acessíveis) -->
              <form
                @submit.prevent="handleQuickSend"
                class="mt-4 flex items-center gap-2 rounded-xl border border-slate-200 dark:border-slate-700 bg-slate-50/90 dark:bg-slate-900/80 px-3.5 py-2 focus-within:ring-2 focus-within:ring-[#4f46e5]"
              >
                <label for="quick-question-input" class="sr-only">
                  Type a sample question
                </label>
                <input
                  id="quick-question-input"
                  name="quickQuestion"
                  v-model="quickQuestion"
                  type="text"
                  placeholder="Try typing a question here..."
                  class="flex-1 bg-transparent text-sm text-slate-900 dark:text-white placeholder:text-slate-400 focus:outline-none"
                />
                <button
                  type="submit"
                  aria-label="Send sample question"
                  class="inline-flex items-center justify-center rounded-lg p-1.5 text-[#4f46e5] dark:text-indigo-400 hover:bg-indigo-100 dark:hover:bg-indigo-950/70 transition-colors cursor-pointer"
                >
                  <svg
                    class="w-4 h-4"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                    stroke-width="2.2"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M6 12L3.269 3.126A59.768 59.768 0 0121.485 12 59.77 59.77 0 013.27 20.876L5.999 12zm0 0h7.5"
                    />
                  </svg>
                </button>
              </form>
            </BaseCard>
          </div>
        </div>
      </div>
    </section>

    <!-- FOOTER -->
    <footer
      class="border-t border-slate-200/70 dark:border-slate-800/80 bg-white/50 dark:bg-slate-900/40 py-8 px-6"
    >
      <div
        class="max-w-6xl mx-auto flex flex-col sm:flex-row items-center justify-between gap-4 text-sm text-slate-500 dark:text-slate-400"
      >
        <span
          class="text-xl font-extrabold tracking-tight text-[#4f46e5] dark:text-indigo-400"
        >
          hermes
        </span>
        <p>© {{ new Date().getFullYear() }} hermes. All rights reserved.</p>
      </div>
    </footer>

    <!-- JOIN ROOM MODAL -->
    <BaseModal
      :open="joinModalOpen"
      title="Join a Room"
      @close="joinModalOpen = false"
    >
      <form @submit.prevent="handleJoinRoom" class="flex flex-col gap-4">
        <BaseInput
          id="room-code-input"
          v-model="roomCode"
          label="Room Code"
          placeholder="e.g. HRM-2026"
          hint="Enter the room code shared for your session."
          required
        />

        <div class="flex justify-end gap-3 mt-2">
          <BaseButton
            variant="ghost"
            type="button"
            @click="joinModalOpen = false"
          >
            Cancel
          </BaseButton>
          <BaseButton variant="primary" type="submit">
            Join Room
          </BaseButton>
        </div>
      </form>
    </BaseModal>
  </div>
</template>