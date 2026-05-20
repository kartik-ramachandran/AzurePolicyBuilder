<template>
  <div id="app" class="min-h-screen bg-gradient-to-br from-white via-sky-50 to-blue-50">
    <header class="sticky top-0 z-40 border-b border-sky-100/80 bg-white/85 shadow-sm shadow-sky-100/60 backdrop-blur-xl">
      <div class="mx-auto max-w-[90rem] px-4 sm:px-6 lg:px-8">
        <div class="flex flex-col gap-4 py-4 lg:flex-row lg:items-center lg:justify-between">
          <router-link to="/" class="group flex items-center gap-3">
            <span class="grid h-12 w-12 place-items-center rounded-2xl bg-gradient-to-br from-sky-300 via-blue-500 to-blue-700 text-white shadow-lg shadow-blue-200 transition-transform group-hover:-translate-y-0.5">
              <svg class="h-7 w-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 20 4 12l6-8m4 16 6-8-6-8M14 4l-4 16" />
              </svg>
            </span>
            <span>
              <span class="block text-[11px] font-black uppercase tracking-[0.32em] text-sky-500">Azure APIM</span>
              <span class="block text-2xl font-black tracking-tight text-slate-950">Policy Studio</span>
            </span>
          </router-link>

          <nav class="flex flex-wrap items-center gap-2 rounded-3xl border border-sky-100 bg-sky-50/80 p-2 shadow-inner shadow-white">
            <router-link
              v-for="item in navItems"
              :key="item.to"
              :to="item.to"
              class="group inline-flex items-center gap-2 rounded-2xl px-4 py-2.5 text-sm font-black text-slate-600 transition hover:bg-white hover:text-blue-700 hover:shadow-sm"
              active-class="bg-white text-blue-700 shadow-md shadow-sky-100"
            >
              <span class="grid h-7 w-7 place-items-center rounded-xl bg-white text-blue-600 shadow-sm transition group-[.router-link-active]:bg-blue-600 group-[.router-link-active]:text-white">
                <svg v-if="item.icon === 'builder'" class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 7h16M4 12h16M4 17h10" />
                </svg>
                <svg v-else-if="item.icon === 'templates'" class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 5h7v7H4V5Zm9 0h7v7h-7V5ZM4 14h7v5H4v-5Zm9 0h7v5h-7v-5Z" />
                </svg>
                <svg v-else-if="item.icon === 'policy'" class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7h8M8 12h8m-8 5h5M6 3h12a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2Z" />
                </svg>
                <svg v-else-if="item.icon === 'api'" class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 9h8M8 15h8M4 6a2 2 0 0 1 2-2h2l2 2h8a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6Z" />
                </svg>
                <svg v-else class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m9 12 2 2 4-5M12 3a9 9 0 1 1 0 18 9 9 0 0 1 0-18Z" />
                </svg>
              </span>
              <span>{{ item.label }}</span>
            </router-link>
          </nav>

          <div class="hidden items-center gap-3 xl:flex">
            <div class="rounded-2xl border border-cyan-100 bg-cyan-50 px-4 py-2 text-sm font-black text-cyan-700">
              OpenAPI Ready
            </div>
            <button class="rounded-2xl bg-blue-600 px-4 py-2.5 text-sm font-black text-white shadow-lg shadow-blue-200 hover:bg-blue-700">
              Generate
            </button>
          </div>
        </div>
      </div>
    </header>
    
    <main class="mx-auto max-w-[90rem] px-4 py-8 sm:px-6 lg:px-8">
      <router-view />
    </main>

    <!-- AI Assistant -->
    <AIAssistant />
  </div>
</template>

<script setup lang="ts">
import AIAssistant from '@/components/AIAssistant.vue'

const navItems = [
  { to: '/', label: 'Project Builder', icon: 'builder' },
  { to: '/templates', label: 'Templates', icon: 'templates' },
  { to: '/editor', label: 'Policy Editor', icon: 'policy' },
  { to: '/api-builder', label: 'API Builder', icon: 'api' },
  { to: '/api-tester', label: 'API Tester', icon: 'tester' }
]
</script>
