<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  createRoom,
  endRoom,
  getMyRooms,
  startRoom,
  type Room,
} from '@/services/roomService'
import BaseBadge from '@/components/ui/BaseBadge.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import ThemeToggle from '@/components/ui/ThemeToggle.vue'
import JoinRoomModal from '@/components/rooms/JoinRoomModal.vue'

type DisplayRoomStatus = 'Live' | 'Ready' | 'Inactive' | 'Ended'

type RoomExtended = Omit<Room, 'status'> & {
  status: DisplayRoomStatus | string
  description?: string | null
  topic?: string | null
  created_at?: string | null
  startedAt?: string | null
  started_at?: string | null
  updatedAt?: string | null
  updated_at?: string | null
}

const router = useRouter()
const authStore = useAuthStore()

const rooms = ref<RoomExtended[]>([])
const isLoading = ref(false)
const errorMessage = ref('')
const copiedKey = ref<string | null>(null)

// Room filter
const roomFilter = ref<'all' | 'Live' | 'Ready'>('all')

// Join room modal
const isJoinOpen = ref(false)

// Create room modal
const isCreateOpen = ref(false)
const roomName = ref('')
const roomSubject = ref('')
const isCreating = ref(false)

// Start room confirmation modal
const roomToStart = ref<RoomExtended | null>(null)
const isStarting = ref(false)

// End room confirmation modal
const roomToEnd = ref<RoomExtended | null>(null)
const isEnding = ref(false)

// Archive & delete confirmation modals (kept disabled until backend endpoints exist)
const roomToArchive = ref<RoomExtended | null>(null)
const roomToDelete = ref<RoomExtended | null>(null)

// Local date cache fallback (per user) — never overrides API room status
const datesStorageScope = computed(() => {
  const userId =
    (authStore.user as { id?: string | number; email?: string } | null)?.id ??
    (authStore.user as { email?: string } | null)?.email ??
    'default'
  return `hermes:my-rooms:dates:${String(userId)}`
})

const datesByKey = ref<Record<string, string>>({})

const dateFilter = ref<'all' | 'today' | 'week' | 'month'>('all')
const pageSize = 4
const currentPage = ref(1)

function getRoomKey(room: RoomExtended): string {
  if (room.code && room.code.trim()) return `code:${room.code.trim()}`
  return `id:${String(room.id)}`
}

function normalizeStatus(rawStatus: unknown): DisplayRoomStatus {
  const value = String(rawStatus ?? '').trim().toLowerCase()
  if (value === 'live' || value === 'active') return 'Live'
  if (value === 'inactive' || value === 'archived') return 'Inactive'
  if (value === 'ended' || value === 'closed' || value === 'finished') return 'Ended'
  return 'Ready'
}

function getRoomDescription(room: RoomExtended): string {
  return (room.subject || room.description || room.topic || '').trim()
}

function getRoomDateValue(room: RoomExtended): string {
  const fromApi =
    room.createdAt ||
    room.created_at ||
    room.startedAt ||
    room.started_at ||
    room.updatedAt ||
    room.updated_at

  if (fromApi && !Number.isNaN(new Date(fromApi).getTime())) {
    return fromApi
  }

  const key = getRoomKey(room)
  if (datesByKey.value[key]) {
    return datesByKey.value[key]
  }

  return new Date().toISOString()
}

function openInNewTab(path: string) {
  const resolved = router.resolve(path)
  const targetUrl = new URL(resolved.href, window.location.origin).toString()

  const anchor = document.createElement('a')
  anchor.href = targetUrl
  anchor.target = '_blank'
  anchor.rel = 'noopener noreferrer'
  document.body.appendChild(anchor)
  anchor.click()
  document.body.removeChild(anchor)
}

function applyApiRooms(list: RoomExtended[]): RoomExtended[] {
  const nextDates: Record<string, string> = { ...datesByKey.value }

  const mapped = list.map((room) => {
    const key = getRoomKey(room)
    const normalizedApiStatus = normalizeStatus(room.status)
    const dateVal = getRoomDateValue(room)
    if (!nextDates[key]) {
      nextDates[key] = dateVal
    }

    return {
      ...room,
      status: normalizedApiStatus,
      createdAt: dateVal,
    }
  })

  datesByKey.value = nextDates
  return mapped
}

function loadSavedDates() {
  try {
    const raw = localStorage.getItem(datesStorageScope.value)
    if (!raw) {
      datesByKey.value = {}
      return
    }
    const parsed = JSON.parse(raw) as Record<string, string>
    datesByKey.value = parsed && typeof parsed === 'object' ? parsed : {}
  } catch {
    datesByKey.value = {}
  }
}

function saveDates() {
  try {
    localStorage.setItem(datesStorageScope.value, JSON.stringify(datesByKey.value))
  } catch {
    // Ignore storage errors
  }
}

watch(
  [datesByKey, datesStorageScope],
  () => {
    saveDates()
  },
  { deep: true },
)

watch(datesStorageScope, () => {
  loadSavedDates()
  rooms.value = applyApiRooms(rooms.value)
})

const firstName = computed(() => {
  const fullName = authStore.user?.name?.trim()
  if (!fullName) return 'User'
  return fullName.split(/\s+/)[0]
})

const allActivePool = computed(() =>
  rooms.value.filter((room) => {
    const status = normalizeStatus(room.status)
    return status !== 'Ended' && status !== 'Inactive'
  }),
)

const activeRooms = computed(() => {
  if (roomFilter.value === 'all') return allActivePool.value
  return allActivePool.value.filter(
    (room) => normalizeStatus(room.status) === roomFilter.value,
  )
})

const archivedRooms = computed(() => {
  const base = rooms.value.filter((room) => {
    const status = normalizeStatus(room.status)
    return status === 'Ended' || status === 'Inactive'
  })

  if (dateFilter.value === 'all') return base

  const now = Date.now()
  const dayMs = 24 * 60 * 60 * 1000
  const maxAgeMs =
    dateFilter.value === 'today'
      ? dayMs
      : dateFilter.value === 'week'
        ? 7 * dayMs
        : 30 * dayMs

  return base.filter((room) => {
    const timestamp = new Date(getRoomDateValue(room)).getTime()
    if (Number.isNaN(timestamp)) return true
    return now - timestamp <= maxAgeMs
  })
})

const totalPages = computed(() =>
  Math.max(1, Math.ceil(archivedRooms.value.length / pageSize)),
)

const paginatedArchivedRooms = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return archivedRooms.value.slice(start, start + pageSize)
})

const archiveRangeStart = computed(() => {
  if (archivedRooms.value.length === 0) return 0
  return (currentPage.value - 1) * pageSize + 1
})

const archiveRangeEnd = computed(() =>
  Math.min(currentPage.value * pageSize, archivedRooms.value.length),
)

watch([dateFilter, archivedRooms], () => {
  if (currentPage.value > totalPages.value) {
    currentPage.value = totalPages.value
  }
})

function formatDateTime(room: RoomExtended): string {
  const raw = getRoomDateValue(room)
  const date = new Date(raw)
  if (Number.isNaN(date.getTime())) {
    return new Date().toLocaleString('en-US', {
      month: 'short',
      day: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  }
  return date.toLocaleString('en-US', {
    month: 'short',
    day: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

async function loadRooms() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const fetched = (await getMyRooms()) as RoomExtended[]
    rooms.value = applyApiRooms(fetched)
  } catch {
    errorMessage.value = 'Could not load rooms.'
  } finally {
    isLoading.value = false
  }
}

function openJoinModal() {
  isJoinOpen.value = true
}

function closeJoinModal() {
  isJoinOpen.value = false
}

function openCreateModal() {
  roomName.value = ''
  roomSubject.value = ''
  isCreateOpen.value = true
}

function closeCreateModal() {
  isCreateOpen.value = false
}

async function handleCreateRoom() {
  if (!roomName.value.trim() || isCreating.value) return

  isCreating.value = true
  errorMessage.value = ''

  try {
    const subjectTrimmed = roomSubject.value.trim() || undefined
    const created = (await createRoom({
      name: roomName.value.trim(),
      subject: subjectTrimmed,
    })) as RoomExtended

    if (subjectTrimmed && !getRoomDescription(created)) {
      created.subject = subjectTrimmed
    }

    const key = getRoomKey(created)
    const nowIso = getRoomDateValue(created)
    datesByKey.value = {
      ...datesByKey.value,
      [key]: nowIso,
    }
    created.createdAt = nowIso
    created.status = normalizeStatus(created.status)

    rooms.value = [created, ...rooms.value]
    closeCreateModal()
  } catch {
    errorMessage.value = 'Could not create room.'
  } finally {
    isCreating.value = false
  }
}

function requestStartRoom(room: RoomExtended) {
  roomToStart.value = room
}

function cancelStartRoom() {
  roomToStart.value = null
}

async function confirmStartRoom() {
  if (!roomToStart.value || isStarting.value) return

  const target = roomToStart.value
  const roomCode = target.code
  isStarting.value = true
  errorMessage.value = ''

  try {
    await startRoom(roomCode)
    target.status = 'Live'
    roomToStart.value = null
    openInNewTab(`/dashboard/${roomCode}`)
    await loadRooms()
  } catch {
    await loadRooms()
    errorMessage.value = 'Could not start room. Please try again.'
  } finally {
    isStarting.value = false
  }
}

function requestEndRoom(room: RoomExtended) {
  roomToEnd.value = room
}

function cancelEndRoom() {
  roomToEnd.value = null
}

async function confirmEndRoom() {
  if (!roomToEnd.value || isEnding.value) return

  const target = roomToEnd.value
  isEnding.value = true
  errorMessage.value = ''

  try {
    await endRoom(target.code)
    target.status = 'Ended'
    roomToEnd.value = null
    await loadRooms()
  } catch {
    await loadRooms()
    errorMessage.value = 'Could not end room. Please try again.'
  } finally {
    isEnding.value = false
  }
}

function requestArchiveRoom(_room: RoomExtended) {
  // Temporarily disabled until backend endpoints are implemented
}

function cancelArchiveRoom() {
  roomToArchive.value = null
}

async function confirmArchiveRoom() {
  // Temporarily disabled until backend endpoints are implemented
  roomToArchive.value = null
}

function handleRestoreRoom(_room: RoomExtended) {
  // Temporarily disabled until backend endpoints are implemented
}

function requestDeletePermanent(_room: RoomExtended) {
  // Temporarily disabled until backend endpoints are implemented
}

function cancelDeletePermanent() {
  roomToDelete.value = null
}

function confirmDeletePermanent() {
  // Temporarily disabled until backend endpoints are implemented
  roomToDelete.value = null
}

function handleExportCsv() {
  if (archivedRooms.value.length === 0) {
    errorMessage.value = 'There are no archived sessions to export.'
    return
  }

  errorMessage.value = ''
  const headers = [
    'Session Name',
    'Subject',
    'Room Code',
    'Date & Time',
    'Status',
  ]
  const rows = archivedRooms.value.map((room) => [
    room.name,
    getRoomDescription(room),
    room.code,
    formatDateTime(room),
    normalizeStatus(room.status),
  ])

  const csvBody = [headers, ...rows]
    .map((row) =>
      row
        .map((cell) => `"${String(cell ?? '').replaceAll('"', '""')}"`)
        .join(';'),
    )
    .join('\r\n')

  const blob = new Blob([`\uFEFF${csvBody}`], {
    type: 'text/csv;charset=utf-8;',
  })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.setAttribute('download', 'hermes-archives.csv')
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

async function handleCopyCode(room: RoomExtended) {
  const key = getRoomKey(room)
  try {
    await navigator.clipboard.writeText(room.code)
    copiedKey.value = key
    window.setTimeout(() => {
      if (copiedKey.value === key) {
        copiedKey.value = null
      }
    }, 2000)
  } catch {
    errorMessage.value = 'Could not copy room code.'
  }
}

async function handleLogout() {
  const storeWithLogout = authStore as unknown as {
    logout?: () => void
    clearSession?: () => void
    setSession?: (session: null) => void
    $reset?: () => void
  }

  if (typeof storeWithLogout.logout === 'function') {
    storeWithLogout.logout()
  } else if (typeof storeWithLogout.clearSession === 'function') {
    storeWithLogout.clearSession()
  } else if (typeof storeWithLogout.setSession === 'function') {
    storeWithLogout.setSession(null)
  } else if (typeof storeWithLogout.$reset === 'function') {
    storeWithLogout.$reset()
  }

  await router.push('/login')
}

onMounted(() => {
  loadSavedDates()
  void loadRooms()
})
</script>

<template>
  <main class="min-h-screen pb-14 sm:pb-16 text-slate-900 dark:text-slate-100">
    <!-- Top Bar -->
    <header
      class="sticky top-0 z-30 bg-white/95 dark:bg-slate-900/95 backdrop-blur-xs border-b border-slate-200 dark:border-slate-800 px-4 sm:px-8 py-3.5 flex items-center justify-between gap-3"
    >
      <span class="text-xl font-bold tracking-tight text-indigo-600 dark:text-indigo-400">
        hermes
      </span>

      <div class="flex items-center gap-2 sm:gap-2.5">
        <ThemeToggle />
        <BaseButton
          type="button"
          variant="danger"
          size="sm"
          @click="handleLogout"
        >
          Logout
        </BaseButton>
      </div>
    </header>

    <div class="max-w-6xl mx-auto px-4 sm:px-8 pt-6 sm:pt-8">
      <!-- Greeting & Primary Actions -->
      <div
        class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-7 sm:mb-8"
      >
        <h1 class="text-2xl sm:text-3xl font-bold tracking-tight text-slate-900 dark:text-white">
          Hello, {{ firstName }}.
        </h1>

        <div class="flex flex-col sm:flex-row gap-2.5 sm:gap-3 w-full sm:w-auto">
          <BaseButton
            type="button"
            variant="secondary"
            class="w-full sm:w-auto"
            @click="openJoinModal"
          >
            Join a room
          </BaseButton>
          <BaseButton
            type="button"
            variant="primary"
            class="w-full sm:w-auto"
            @click="openCreateModal"
          >
            + Create a room
          </BaseButton>
        </div>
      </div>

      <p
        v-if="errorMessage"
        role="alert"
        class="mb-6 rounded-xl border border-red-200 dark:border-red-900/60 bg-red-50 dark:bg-red-950/50 px-4 py-3 text-sm font-medium text-red-700 dark:text-red-300"
      >
        {{ errorMessage }}
      </p>

      <!-- Active / Ready Rooms Grid -->
      <section aria-labelledby="my-rooms-heading" class="mb-10 sm:mb-12">
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-5">
          <h2
            id="my-rooms-heading"
            class="text-lg sm:text-xl font-bold text-slate-900 dark:text-white"
          >
            My Rooms
          </h2>

          <div class="flex items-center gap-2 w-full sm:w-auto">
            <select
              id="room-filter"
              name="roomFilter"
              v-model="roomFilter"
              aria-label="Filter rooms"
              class="w-full sm:w-auto min-h-[40px] rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 px-3.5 py-2 text-xs font-semibold text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            >
              <option value="all">All rooms ({{ allActivePool.length }})</option>
              <option value="Live">Live (Active)</option>
              <option value="Ready">Ready</option>
            </select>
          </div>
        </div>

        <p v-if="isLoading" class="text-sm font-medium text-slate-600 dark:text-slate-300">
          Loading rooms...
        </p>

        <BaseCard
          v-else-if="activeRooms.length === 0"
          class="text-center"
        >
          <p class="text-sm font-medium text-slate-600 dark:text-slate-300">
            No active or ready rooms available.
          </p>
        </BaseCard>

        <div
          v-else
          class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6"
        >
          <BaseCard
            v-for="room in activeRooms"
            :key="getRoomKey(room)"
            :active="normalizeStatus(room.status) === 'Live'"
            class="flex flex-col justify-between gap-5"
          >
            <div>
              <!-- Status Badge & Explicit Active Indicator -->
              <div class="flex items-center justify-between gap-2 mb-3">
                <BaseBadge
                  :variant="normalizeStatus(room.status) === 'Live' ? 'live' : 'ready'"
                  :dot="true"
                >
                  {{ normalizeStatus(room.status) === 'Live' ? 'Live · Active' : 'Ready' }}
                </BaseBadge>

                <span
                  v-if="normalizeStatus(room.status) === 'Live'"
                  class="text-[11px] font-bold uppercase tracking-wider text-emerald-600 dark:text-emerald-400"
                >
                  Session active
                </span>
              </div>

              <!-- Room Title & Optional Description -->
              <h3 class="text-lg font-bold text-slate-900 dark:text-white leading-snug break-words">
                {{ room.name }}
              </h3>
              <p
                v-if="getRoomDescription(room)"
                class="text-xs font-medium text-slate-600 dark:text-slate-300 mt-1 break-words"
              >
                {{ getRoomDescription(room) }}
              </p>

              <!-- Room Code Box -->
              <div
                class="mt-4 rounded-xl bg-slate-100 dark:bg-slate-900/90 border border-slate-200 dark:border-slate-700 px-3.5 py-2.5 flex items-center justify-between gap-3"
              >
                <div class="min-w-0">
                  <span
                    class="block text-[10px] font-bold uppercase tracking-wider text-slate-500 dark:text-slate-400"
                  >
                    ROOM CODE
                  </span>
                  <span
                    class="font-mono text-sm sm:text-base font-bold tracking-wider text-slate-900 dark:text-white truncate block"
                  >
                    {{ room.code }}
                  </span>
                </div>

                <BaseButton
                  type="button"
                  variant="secondary"
                  size="sm"
                  class="shrink-0"
                  @click="handleCopyCode(room)"
                >
                  {{ copiedKey === getRoomKey(room) ? 'Copied!' : 'Copy' }}
                </BaseButton>
              </div>
            </div>

            <!-- Card Actions -->
            <div class="flex flex-col gap-2">
              <BaseButton
                v-if="normalizeStatus(room.status) === 'Live'"
                type="button"
                variant="primary"
                block
                @click="openInNewTab(`/dashboard/${room.code}`)"
              >
                Open live dashboard
              </BaseButton>
              <BaseButton
                v-else
                type="button"
                variant="slate"
                block
                @click="requestStartRoom(room)"
              >
                Start
              </BaseButton>

              <BaseButton
                v-if="normalizeStatus(room.status) === 'Live'"
                type="button"
                variant="danger"
                block
                @click="requestEndRoom(room)"
              >
                End room
              </BaseButton>

              <BaseButton
                type="button"
                variant="secondary"
                block
                @click="router.push(`/rooms/${room.code}/history`)"
              >
                View history
              </BaseButton>

              <BaseButton
                type="button"
                variant="ghost"
                block
                :disabled="true"
                title="Coming soon (requires backend archive support)"
                @click="requestArchiveRoom(room)"
              >
                Archive
              </BaseButton>
            </div>
          </BaseCard>
        </div>
      </section>

      <!-- Archives Section -->
      <section aria-labelledby="archives-heading">
        <div
          class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 sm:gap-4 mb-5"
        >
          <div>
            <h2
              id="archives-heading"
              class="text-lg sm:text-xl font-bold text-slate-900 dark:text-white"
            >
              Archives
            </h2>
            <p class="text-xs font-medium text-slate-600 dark:text-slate-300 mt-0.5">
              Archived rooms and past session history
            </p>
          </div>

          <div class="flex flex-col sm:flex-row items-stretch sm:items-center gap-2.5 w-full sm:w-auto">
            <select
              id="archive-date-filter"
              name="dateFilter"
              v-model="dateFilter"
              aria-label="Filter by Date"
              class="w-full sm:w-auto min-h-[40px] rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 px-3.5 py-2 text-xs font-semibold text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            >
              <option value="all">Filter by Date</option>
              <option value="today">Last 24 hours</option>
              <option value="week">Last 7 days</option>
              <option value="month">Last 30 days</option>
            </select>

            <BaseButton
              type="button"
              variant="secondary"
              size="sm"
              class="w-full sm:w-auto"
              :disabled="archivedRooms.length === 0"
              @click="handleExportCsv"
            >
              Export All (.CSV)
            </BaseButton>
          </div>
        </div>

        <BaseCard class="p-0! overflow-hidden">
          <!-- Mobile List View (< md) -->
          <div class="divide-y divide-slate-200 dark:divide-slate-700/80 md:hidden">
            <div
              v-if="archivedRooms.length === 0"
              class="px-4 py-8 text-center text-xs font-medium text-slate-600 dark:text-slate-300"
            >
              No archived sessions found.
            </div>

            <div
              v-for="room in paginatedArchivedRooms"
              :key="`mobile-${getRoomKey(room)}`"
              class="p-4 flex flex-col gap-3"
            >
              <div class="flex items-start justify-between gap-2">
                <div class="min-w-0">
                  <p class="font-bold text-sm text-slate-900 dark:text-white break-words">
                    {{ room.name }}
                  </p>
                  <p
                    v-if="getRoomDescription(room)"
                    class="text-xs text-slate-600 dark:text-slate-300 mt-0.5 break-words"
                  >
                    {{ getRoomDescription(room) }}
                  </p>
                </div>
                <BaseBadge variant="ended" class="shrink-0">
                  {{ normalizeStatus(room.status) }}
                </BaseBadge>
              </div>

              <div class="flex flex-wrap items-center justify-between gap-2 text-xs text-slate-600 dark:text-slate-300">
                <code class="rounded-md bg-slate-100 dark:bg-slate-900 px-2 py-1 font-semibold text-slate-900 dark:text-white">
                  {{ room.code }}
                </code>
                <span>{{ formatDateTime(room) }}</span>
              </div>

              <div class="grid grid-cols-1 gap-2 pt-1">
                <BaseButton
                  type="button"
                  variant="secondary"
                  size="sm"
                  block
                  @click="router.push(`/rooms/${room.code}/history`)"
                >
                  View history
                </BaseButton>
                <div class="grid grid-cols-2 gap-2">
                  <BaseButton
                    type="button"
                    variant="secondary"
                    size="sm"
                    block
                    :disabled="true"
                    title="Coming soon (requires backend restore support)"
                    @click="handleRestoreRoom(room)"
                  >
                    Restore
                  </BaseButton>
                  <BaseButton
                    type="button"
                    variant="danger"
                    size="sm"
                    block
                    :disabled="true"
                    title="Coming soon (requires backend delete support)"
                    @click="requestDeletePermanent(room)"
                  >
                    Delete
                  </BaseButton>
                </div>
              </div>
            </div>
          </div>

          <!-- Desktop Table View (>= md) -->
          <div class="hidden md:block overflow-x-auto">
            <table class="w-full text-left text-sm">
              <thead
                class="border-b border-slate-200 dark:border-slate-700/80 bg-slate-100/80 dark:bg-slate-900/70 text-[11px] font-bold uppercase tracking-wider text-slate-600 dark:text-slate-300"
              >
                <tr>
                  <th class="px-5 py-3.5">SESSION NAME</th>
                  <th class="px-5 py-3.5">ROOM CODE</th>
                  <th class="px-5 py-3.5">DATE & TIME</th>
                  <th class="px-5 py-3.5">STATUS</th>
                  <th class="px-5 py-3.5 text-right">ACTIONS</th>
                </tr>
              </thead>

              <tbody class="divide-y divide-slate-200 dark:divide-slate-700/80">
                <tr v-if="archivedRooms.length === 0">
                  <td
                    colspan="5"
                    class="px-5 py-8 text-center text-xs font-medium text-slate-600 dark:text-slate-300"
                  >
                    No archived sessions found.
                  </td>
                </tr>

                <tr
                  v-for="room in paginatedArchivedRooms"
                  :key="`desktop-${getRoomKey(room)}`"
                  class="hover:bg-slate-50 dark:hover:bg-slate-900/40 transition-colors"
                >
                  <td class="px-5 py-3.5">
                    <p class="font-bold text-slate-900 dark:text-white">
                      {{ room.name }}
                    </p>
                    <p
                      v-if="getRoomDescription(room)"
                      class="text-xs text-slate-600 dark:text-slate-300"
                    >
                      {{ getRoomDescription(room) }}
                    </p>
                  </td>

                  <td class="px-5 py-3.5">
                    <code
                      class="inline-block rounded-md bg-slate-100 dark:bg-slate-900 px-2.5 py-1 text-xs font-bold text-slate-900 dark:text-white"
                    >
                      {{ room.code }}
                    </code>
                  </td>

                  <td class="px-5 py-3.5 text-xs font-medium text-slate-600 dark:text-slate-300 whitespace-nowrap">
                    {{ formatDateTime(room) }}
                  </td>

                  <td class="px-5 py-3.5">
                    <BaseBadge variant="ended">
                      {{ normalizeStatus(room.status) }}
                    </BaseBadge>
                  </td>

                  <td class="px-5 py-3.5">
                    <div class="flex items-center justify-end gap-2 flex-wrap">
                      <BaseButton
                        type="button"
                        variant="secondary"
                        size="sm"
                        @click="router.push(`/rooms/${room.code}/history`)"
                      >
                        View history
                      </BaseButton>

                      <BaseButton
                        type="button"
                        variant="secondary"
                        size="sm"
                        :disabled="true"
                        title="Coming soon (requires backend restore support)"
                        @click="handleRestoreRoom(room)"
                      >
                        Restore
                      </BaseButton>

                      <BaseButton
                        type="button"
                        variant="danger"
                        size="sm"
                        :disabled="true"
                        title="Coming soon (requires backend delete support)"
                        @click="requestDeletePermanent(room)"
                      >
                        Delete permanently
                      </BaseButton>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination Footer -->
          <div
            class="px-4 sm:px-5 py-3.5 border-t border-slate-200 dark:border-slate-700/80 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 text-xs font-medium text-slate-600 dark:text-slate-300"
          >
            <span>
              Showing {{ archiveRangeStart }}–{{ archiveRangeEnd }} of
              {{ archivedRooms.length }} past sessions
            </span>

            <div class="flex items-center gap-2">
              <BaseButton
                type="button"
                variant="secondary"
                size="sm"
                :disabled="currentPage <= 1"
                @click="currentPage = Math.max(1, currentPage - 1)"
              >
                Previous
              </BaseButton>
              <BaseButton
                type="button"
                variant="secondary"
                size="sm"
                :disabled="currentPage >= totalPages"
                @click="currentPage = Math.min(totalPages, currentPage + 1)"
              >
                Next
              </BaseButton>
            </div>
          </div>
        </BaseCard>
      </section>
    </div>

    <!-- Join Room Modal -->
    <JoinRoomModal
      :open="isJoinOpen"
      @close="closeJoinModal"
    />

    <!-- Create Room Modal -->
    <BaseModal
      :open="isCreateOpen"
      title="Create a new room"
      @close="closeCreateModal"
    >
      <form class="flex flex-col gap-4" @submit.prevent="handleCreateRoom">
        <BaseInput
          v-model="roomName"
          type="text"
          label="Room name"
          placeholder="e.g. Introduction to Biology"
          required
        />
        <BaseInput
          v-model="roomSubject"
          type="text"
          label="Subject / Description (optional)"
          placeholder="e.g. BIO 101 · Cell biology"
        />

        <div class="flex flex-col-reverse sm:flex-row justify-end gap-2.5 sm:gap-3 pt-2">
          <BaseButton
            type="button"
            variant="secondary"
            class="w-full sm:w-auto"
            @click="closeCreateModal"
          >
            Cancel
          </BaseButton>
          <BaseButton
            type="submit"
            variant="primary"
            class="w-full sm:w-auto"
            :disabled="isCreating"
          >
            {{ isCreating ? 'Creating...' : 'Create room' }}
          </BaseButton>
        </div>
      </form>
    </BaseModal>

    <!-- Start Room Confirmation Modal -->
    <BaseModal
      :open="roomToStart !== null"
      title="Start room"
      @close="cancelStartRoom"
    >
      <p class="text-sm text-slate-600 dark:text-slate-300 mb-5">
        Are you sure you want to start
        <strong class="text-slate-900 dark:text-white">{{ roomToStart?.name }}</strong> and open the live dashboard in a new tab?
      </p>

      <div class="flex flex-col-reverse sm:flex-row justify-end gap-2.5 sm:gap-3">
        <BaseButton
          type="button"
          variant="secondary"
          class="w-full sm:w-auto"
          @click="cancelStartRoom"
        >
          Cancel
        </BaseButton>
        <BaseButton
          type="button"
          variant="primary"
          class="w-full sm:w-auto"
          :disabled="isStarting"
          @click="confirmStartRoom"
        >
          {{ isStarting ? 'Starting...' : 'Start room' }}
        </BaseButton>
      </div>
    </BaseModal>

    <!-- Archive Confirmation Modal -->
    <BaseModal
      :open="roomToArchive !== null"
      title="Archive room"
      @close="cancelArchiveRoom"
    >
      <p class="text-sm text-slate-600 dark:text-slate-300 mb-5">
        Are you sure you want to archive
        <strong class="text-slate-900 dark:text-white">{{ roomToArchive?.name }}</strong>? Its status will be set to Inactive.
      </p>

      <div class="flex flex-col-reverse sm:flex-row justify-end gap-2.5 sm:gap-3">
        <BaseButton
          type="button"
          variant="secondary"
          class="w-full sm:w-auto"
          @click="cancelArchiveRoom"
        >
          Cancel
        </BaseButton>
        <BaseButton
          type="button"
          variant="primary"
          class="w-full sm:w-auto"
          :disabled="true"
          @click="confirmArchiveRoom"
        >
          Archive
        </BaseButton>
      </div>
    </BaseModal>

    <!-- Permanent Delete Confirmation Modal -->
    <BaseModal
      :open="roomToDelete !== null"
      title="Delete permanently"
      @close="cancelDeletePermanent"
    >
      <p class="text-sm text-slate-600 dark:text-slate-300 mb-5">
        Are you sure you want to permanently delete
        <strong class="text-slate-900 dark:text-white">{{ roomToDelete?.name }}</strong>? This action cannot be undone.
      </p>

      <div class="flex flex-col-reverse sm:flex-row justify-end gap-2.5 sm:gap-3">
        <BaseButton
          type="button"
          variant="secondary"
          class="w-full sm:w-auto"
          @click="cancelDeletePermanent"
        >
          Cancel
        </BaseButton>
        <BaseButton
          type="button"
          variant="danger"
          class="w-full sm:w-auto"
          :disabled="true"
          @click="confirmDeletePermanent"
        >
          Delete permanently
        </BaseButton>
      </div>
    </BaseModal>

    <!-- End Room Confirmation Modal -->
    <BaseModal
      :open="roomToEnd !== null"
      title="End room"
      @close="cancelEndRoom"
    >
      <p class="text-sm text-slate-600 dark:text-slate-300 mb-5">
        Are you sure you want to end
        <strong class="text-slate-900 dark:text-white">{{ roomToEnd?.name }}</strong>?
      </p>

      <div class="flex flex-col-reverse sm:flex-row justify-end gap-2.5 sm:gap-3">
        <BaseButton
          type="button"
          variant="secondary"
          class="w-full sm:w-auto"
          @click="cancelEndRoom"
        >
          Cancel
        </BaseButton>
        <BaseButton
          type="button"
          variant="primary"
          class="w-full sm:w-auto"
          :disabled="isEnding"
          @click="confirmEndRoom"
        >
          {{ isEnding ? 'Ending...' : 'End room' }}
        </BaseButton>
      </div>
    </BaseModal>
  </main>
</template>