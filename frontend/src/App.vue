<template>
  <div id="app" class="app-shell min-h-screen">
    <div class="ambient-orb ambient-orb--blue" aria-hidden="true"></div>
    <div class="ambient-orb ambient-orb--orange" aria-hidden="true"></div>

    <aside class="sidebar hidden xl:flex">
      <router-link to="/" class="brand-lockup">
        <span class="brand-mark">
          <span class="brand-mark__glow"></span>
          <CodeBracketIcon class="relative h-7 w-7" />
        </span>
        <span class="min-w-0">
          <span class="brand-eyebrow">Azure APIM</span>
          <span class="brand-name">Policy Studio</span>
        </span>
      </router-link>

      <div class="workspace-pill">
        <span class="workspace-avatar">PS</span>
        <span class="min-w-0 flex-1">
          <span class="block truncate text-sm font-bold text-white">Development</span>
          <span class="block text-xs text-slate-400">Local workspace</span>
        </span>
        <ChevronUpDownIcon class="h-4 w-4 text-slate-500" />
      </div>

      <nav class="sidebar-nav" aria-label="Main navigation">
        <div v-for="group in navGroups" :key="group.label" class="nav-group">
          <p class="nav-label">{{ group.label }}</p>
          <router-link
            v-for="item in group.items"
            :key="item.to"
            :to="item.to"
            class="nav-item"
            active-class="nav-item--active"
          >
            <component :is="item.icon" class="h-[18px] w-[18px] shrink-0" />
            <span class="truncate">{{ item.label }}</span>
            <span v-if="item.badge" class="nav-badge">{{ item.badge }}</span>
            <ChevronRightIcon v-else class="nav-chevron h-4 w-4" />
          </router-link>
        </div>
      </nav>

      <div class="sidebar-footer">
        <div class="status-card">
          <div class="flex items-center gap-2">
            <span class="status-pulse"><span></span></span>
            <span class="text-xs font-bold text-emerald-300">All systems ready</span>
          </div>
          <p class="mt-2 text-xs leading-5 text-slate-400">OpenAPI 3.x and Azure APIM compatible.</p>
        </div>
        <div class="flex items-center justify-between px-1 text-[11px] font-semibold text-slate-500">
          <span>Policy Studio</span>
          <span>v0.1</span>
        </div>
      </div>
    </aside>

    <div class="min-w-0 xl:pl-[17.5rem]">
      <header class="mobile-header xl:hidden">
        <router-link to="/" class="flex items-center gap-3" @click="mobileMenuOpen = false">
          <span class="brand-mark h-10 w-10 rounded-xl">
            <CodeBracketIcon class="relative h-5 w-5" />
          </span>
          <span>
            <span class="brand-eyebrow">Azure APIM</span>
            <span class="block text-base font-extrabold tracking-tight text-slate-950">Policy Studio</span>
          </span>
        </router-link>
        <button class="icon-button" type="button" aria-label="Toggle navigation" @click="mobileMenuOpen = !mobileMenuOpen">
          <XMarkIcon v-if="mobileMenuOpen" class="h-5 w-5" />
          <Bars3Icon v-else class="h-5 w-5" />
        </button>
      </header>

      <div v-if="mobileMenuOpen" class="mobile-menu xl:hidden">
        <nav class="grid gap-1.5 sm:grid-cols-2">
          <router-link
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            class="nav-item text-slate-600"
            active-class="nav-item--mobile-active"
            @click="mobileMenuOpen = false"
          >
            <component :is="item.icon" class="h-[18px] w-[18px]" />
            <span>{{ item.label }}</span>
          </router-link>
        </nav>
      </div>

      <header class="context-bar hidden xl:flex">
        <div class="flex min-w-0 items-center gap-3">
          <span class="context-icon"><component :is="currentItem.icon" class="h-[18px] w-[18px]" /></span>
          <div class="min-w-0">
            <div class="flex items-center gap-2 text-xs font-semibold text-slate-400">
              <span>Workspace</span><ChevronRightIcon class="h-3 w-3" /><span class="text-blue-600">{{ currentGroup }}</span>
            </div>
            <h1 class="truncate text-sm font-extrabold text-slate-900">{{ currentItem.label }}</h1>
          </div>
        </div>
        <div class="flex items-center gap-3">
          <div class="command-pill">
            <MagnifyingGlassIcon class="h-4 w-4 text-slate-400" />
            <span>Quick find</span>
            <kbd>⌘ K</kbd>
          </div>
        </div>
      </header>

      <main class="app-content">
        <router-view v-slot="{ Component }">
          <transition name="page" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </main>
    </div>

    <AIAssistant />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch, type Component } from 'vue'
import { useRoute } from 'vue-router'
import {
  ArchiveBoxIcon,
  Bars3Icon,
  BookOpenIcon,
  ChevronRightIcon,
  ChevronUpDownIcon,
  CodeBracketIcon,
  DocumentTextIcon,
  MagnifyingGlassIcon,
  PaperAirplaneIcon,
  PuzzlePieceIcon,
  Squares2X2Icon,
  WrenchScrewdriverIcon,
  XMarkIcon
} from '@heroicons/vue/24/outline'
import AIAssistant from '@/components/AIAssistant.vue'

const route = useRoute()
const mobileMenuOpen = ref(false)

interface NavItem {
  to: string
  label: string
  icon: Component
  badge?: string
}

const navGroups: Array<{ label: string; items: NavItem[] }> = [
  {
    label: 'Build',
    items: [
      { to: '/', label: 'Project Builder', icon: Squares2X2Icon },
      { to: '/templates', label: 'Templates', icon: ArchiveBoxIcon },
      { to: '/editor', label: 'Policy Editor', icon: CodeBracketIcon },
      { to: '/library', label: 'Library', icon: PuzzlePieceIcon }
    ]
  },
  {
    label: 'Explore & test',
    items: [
      { to: '/api-builder', label: 'API Builder', icon: WrenchScrewdriverIcon },
      { to: '/api-docs', label: 'API Docs', icon: BookOpenIcon },
      { to: '/api-tester', label: 'API Tester', icon: PaperAirplaneIcon }
    ]
  }
]

const navItems = navGroups.flatMap(group => group.items)
const currentItem = computed(() => navItems.find(item => item.to === route.path) ?? { label: 'Policy Studio', icon: DocumentTextIcon })
const currentGroup = computed(() => navGroups.find(group => group.items.some(item => item.to === route.path))?.label ?? 'Workspace')

watch(() => route.path, () => {
  mobileMenuOpen.value = false
})
</script>
