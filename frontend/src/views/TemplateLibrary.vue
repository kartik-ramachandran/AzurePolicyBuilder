<template>
  <div class="template-library">
    <div class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900 mb-2">Policy Template Library</h1>
      <p class="text-gray-600">Browse and use common APIM policy templates</p>
    </div>

    <div class="mb-6">
      <div class="flex space-x-4">
        <button
          v-for="category in categories"
          :key="category.value"
          @click="selectedCategory = category.value"
          :class="[
            'px-4 py-2 rounded-lg font-medium transition-colors',
            selectedCategory === category.value
              ? 'bg-primary-600 text-white'
              : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
          ]"
        >
          {{ category.label }}
        </button>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div
        v-for="template in filteredTemplates"
        :key="template.id"
        class="card hover:shadow-md transition-shadow cursor-pointer"
        @click="viewTemplate(template)"
      >
        <div class="flex items-start justify-between mb-3">
          <h3 class="text-lg font-semibold text-gray-900">{{ template.name }}</h3>
          <span
            class="px-2 py-1 text-xs font-medium rounded-full capitalize"
            :class="getCategoryClass(template.category)"
          >
            {{ template.category }}
          </span>
        </div>
        <p class="text-gray-600 text-sm mb-4">{{ template.description }}</p>
        <div class="flex items-center justify-between">
          <button
            @click.stop="copyTemplate(template)"
            class="text-primary-600 hover:text-primary-700 text-sm font-medium"
          >
            Copy XML
          </button>
          <button
            @click.stop="useTemplate(template)"
            class="btn btn-primary text-sm"
          >
            Use Template
          </button>
        </div>
      </div>
    </div>

    <!-- Template Preview Modal -->
    <div v-if="previewTemplate" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div class="bg-white rounded-lg p-6 max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <div class="flex justify-between items-start mb-4">
          <div>
            <h3 class="text-2xl font-bold">{{ previewTemplate.name }}</h3>
            <p class="text-gray-600 mt-1">{{ previewTemplate.description }}</p>
          </div>
          <button @click="previewTemplate = null" class="text-gray-400 hover:text-gray-600">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        
        <div class="border border-gray-300 rounded-lg overflow-hidden mb-4" style="height: 400px;">
          <MonacoEditor
            :model-value="previewTemplate.xml"
            language="xml"
            :readonly="true"
          />
        </div>

        <div v-if="previewTemplate.parameters && previewTemplate.parameters.length > 0" class="mb-4">
          <h4 class="font-semibold mb-2">Parameters:</h4>
          <div class="space-y-2">
            <div v-for="param in previewTemplate.parameters" :key="param.name" class="bg-gray-50 p-3 rounded">
              <div class="font-medium">{{ param.name }} <span class="text-red-500" v-if="param.required">*</span></div>
              <div class="text-sm text-gray-600">{{ param.description }}</div>
              <div class="text-xs text-gray-500 mt-1">Type: {{ param.type }}</div>
            </div>
          </div>
        </div>

        <div class="flex space-x-2">
          <button @click="useTemplate(previewTemplate)" class="btn btn-primary flex-1">Use This Template</button>
          <button @click="copyTemplate(previewTemplate)" class="btn btn-secondary flex-1">Copy XML</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { usePolicyStore } from '@/stores/policy'
import MonacoEditor from '@/components/MonacoEditor.vue'
import type { PolicyTemplate } from '@/types/policy'

const router = useRouter()
const policyStore = usePolicyStore()

const selectedCategory = ref('')
const previewTemplate = ref<PolicyTemplate | null>(null)

const categories = [
  { label: 'All', value: '' },
  { label: 'Inbound', value: 'inbound' },
  { label: 'Backend', value: 'backend' },
  { label: 'Outbound', value: 'outbound' },
  { label: 'On Error', value: 'on-error' }
]

const filteredTemplates = computed(() => {
  if (!selectedCategory.value) return policyStore.templates
  return policyStore.templates.filter(t => t.category === selectedCategory.value)
})

onMounted(async () => {
  await policyStore.loadTemplates()
})

function getCategoryClass(category: string) {
  const classes = {
    inbound: 'bg-blue-100 text-blue-700',
    backend: 'bg-purple-100 text-purple-700',
    outbound: 'bg-green-100 text-green-700',
    'on-error': 'bg-red-100 text-red-700'
  }
  return classes[category as keyof typeof classes] || 'bg-gray-100 text-gray-700'
}

function viewTemplate(template: PolicyTemplate) {
  previewTemplate.value = template
}

function useTemplate(template: PolicyTemplate) {
  policyStore.setCurrentPolicy(template.xml)
  previewTemplate.value = null
  router.push('/editor')
}

function copyTemplate(template: PolicyTemplate) {
  navigator.clipboard.writeText(template.xml)
  alert('Template XML copied to clipboard!')
}
</script>
