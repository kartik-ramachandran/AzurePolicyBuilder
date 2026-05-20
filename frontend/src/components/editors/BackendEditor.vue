<template>
  <div class="backend-editor max-w-4xl">
    <div class="mb-6">
      <h2 class="text-2xl font-bold text-gray-900 mb-2">Backend Configuration</h2>
      <p class="text-gray-600">Configure backend service endpoints for your APIs.</p>
    </div>
    
    <div class="bg-white rounded-xl border border-gray-200 p-6">
      <h3 class="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
        <div class="w-8 h-8 bg-purple-100 rounded-lg flex items-center justify-center">
          <svg class="w-4 h-4 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 12h14M5 12a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v4a2 2 0 01-2 2M5 12a2 2 0 00-2 2v4a2 2 0 002 2h14a2 2 0 002-2v-4a2 2 0 00-2-2m-2-4h.01M17 16h.01" />
          </svg>
        </div>
        Backend Details
      </h3>
      <div class="grid grid-cols-2 gap-4">
        <div>
          <label class="block text-sm font-semibold text-gray-900 mb-2">Name *</label>
          <input v-model="localBackend.name" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="my-backend" @input="emitUpdate" />
        </div>
        <div>
          <label class="block text-sm font-semibold text-gray-900 mb-2">Title *</label>
          <input v-model="localBackend.title" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="My Backend" @input="emitUpdate" />
        </div>
        <div>
          <label class="block text-sm font-semibold text-gray-900 mb-2">URL *</label>
          <input v-model="localBackend.url" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="https://backend.example.com" @input="emitUpdate" />
        </div>
        <div>
          <label class="block text-sm font-semibold text-gray-900 mb-2">Protocol</label>
          <select v-model="localBackend.protocol" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white" @change="emitUpdate">
            <option value="http">HTTP</option>
            <option value="soap">SOAP</option>
          </select>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  backend: any
}>()

const emit = defineEmits<{
  update: [data: any]
}>()

const localBackend = ref({ ...props.backend })

watch(() => props.backend, (newBackend) => {
  localBackend.value = { ...newBackend }
}, { deep: true })

function emitUpdate() {
  emit('update', { ...localBackend.value })
}
</script>