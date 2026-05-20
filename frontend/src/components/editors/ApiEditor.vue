<template>
  <div class="api-editor max-w-5xl">
    <div class="mb-6">
      <h2 class="text-2xl font-bold text-gray-900 mb-2">API Configuration</h2>
      <p class="text-gray-600">Define your API endpoint, policies, and OpenAPI specification.</p>
    </div>
    
    <div class="space-y-6">
      <!-- Basic Info -->
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <div class="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center">
            <svg class="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          Basic Information
        </h3>
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-semibold text-gray-900 mb-2">Name *</label>
            <input v-model="localApi.name" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="my-api" @input="emitUpdate" />
          </div>
          <div>
            <label class="block text-sm font-semibold text-gray-900 mb-2">Display Name *</label>
            <input v-model="localApi.displayName" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="My API" @input="emitUpdate" />
          </div>
          <div>
            <label class="block text-sm font-semibold text-gray-900 mb-2">Path *</label>
            <input v-model="localApi.path" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="/api/v1" @input="emitUpdate" />
          </div>
          <div>
            <label class="block text-sm font-semibold text-gray-900 mb-2">Service URL *</label>
            <input v-model="localApi.serviceUrl" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="https://backend.example.com" @input="emitUpdate" />
          </div>
          <div class="col-span-2">
            <label class="block text-sm font-semibold text-gray-900 mb-2">Description</label>
            <textarea v-model="localApi.description" rows="3" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="API description..." @input="emitUpdate"></textarea>
          </div>
        </div>
      </div>

      <!-- Policy -->
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <div class="w-8 h-8 bg-purple-100 rounded-lg flex items-center justify-center">
            <svg class="w-4 h-4 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          API Policy
        </h3>
        <MonacoEditor
          v-model="localApi.policy"
          language="xml"
          :height="300"
          @update:modelValue="emitUpdate"
        />
      </div>

      <!-- OpenAPI Spec -->
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <div class="w-8 h-8 bg-green-100 rounded-lg flex items-center justify-center">
            <svg class="w-4 h-4 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h7" />
            </svg>
          </div>
          OpenAPI Specification
        </h3>
        <div class="mb-4">
          <label class="block text-sm font-semibold text-gray-900 mb-2">Format</label>
          <select v-model="localApi.specificationFormat" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white" @change="emitUpdate">
            <option value="openapi">OpenAPI (JSON)</option>
            <option value="openapi-yaml">OpenAPI (YAML)</option>
            <option value="swagger">Swagger</option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-semibold text-gray-900 mb-2">Specification Content</label>
          <textarea
            v-model="localApi.specificationContent"
            rows="10"
            class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent font-mono text-sm"
            placeholder="Paste your OpenAPI specification here..."
            @input="emitUpdate"
          ></textarea>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import MonacoEditor from '@/components/MonacoEditor.vue'

const props = defineProps<{
  api: any
}>()

const emit = defineEmits<{
  update: [data: any]
}>()

const localApi = ref({ ...props.api })

watch(() => props.api, (newApi) => {
  localApi.value = { ...newApi }
}, { deep: true })

function emitUpdate() {
  emit('update', { ...localApi.value })
}
</script>