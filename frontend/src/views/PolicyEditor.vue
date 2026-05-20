<template>
  <div class="policy-editor">
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Left sidebar - Templates -->
      <div class="lg:col-span-1">
        <div class="card">
          <h2 class="text-xl font-bold mb-4">Policy Templates</h2>
          
          <div class="mb-4">
            <select v-model="selectedCategory" class="input">
              <option value="">All Categories</option>
              <option value="inbound">Inbound</option>
              <option value="backend">Backend</option>
              <option value="outbound">Outbound</option>
              <option value="on-error">On Error</option>
            </select>
          </div>

          <div class="space-y-2 max-h-96 overflow-y-auto">
            <div
              v-for="template in filteredTemplates"
              :key="template.id"
              @click="selectTemplate(template)"
              class="p-3 border border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 cursor-pointer transition-colors"
            >
              <div class="font-medium text-gray-900">{{ template.name }}</div>
              <div class="text-sm text-gray-600 mt-1">{{ template.description }}</div>
              <div class="text-xs text-gray-500 mt-1 capitalize">{{ template.category }}</div>
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="card mt-4">
          <h3 class="text-lg font-semibold mb-3">Quick Actions</h3>
          <div class="space-y-2">
            <button @click="validateCurrent" class="btn btn-primary w-full">
              <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              Validate Policy
            </button>
            <button @click="exportArm" class="btn btn-secondary w-full">
              <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
              </svg>
              Export ARM
            </button>
            <button @click="saveAsFragment" class="btn btn-secondary w-full">
              <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7v8a2 2 0 002 2h6M8 7V5a2 2 0 012-2h4.586a1 1 0 01.707.293l4.414 4.414a1 1 0 01.293.707V15a2 2 0 01-2 2h-2M8 7H6a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2v-2" />
              </svg>
              Save as Fragment
            </button>
          </div>
        </div>
      </div>

      <!-- Main editor area -->
      <div class="lg:col-span-2 space-y-4">
        <div class="card">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-xl font-bold">Policy Editor</h2>
            <div class="flex space-x-2">
              <button @click="formatXml" class="btn btn-secondary text-sm">
                Format XML
              </button>
              <button @click="clearEditor" class="btn btn-secondary text-sm">
                Clear
              </button>
            </div>
          </div>
          
          <div class="border border-gray-300 rounded-lg overflow-hidden" style="height: 500px;">
            <MonacoEditor
              v-model="policyXml"
              language="xml"
              :options="editorOptions"
              @change="onPolicyChange"
            />
          </div>
        </div>

        <!-- Validation Results -->
        <ValidationPanel :result="policyStore.validationResult" />
      </div>
    </div>

    <!-- Save Fragment Modal -->
    <div v-if="showFragmentModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div class="bg-white rounded-lg p-6 max-w-md w-full">
        <h3 class="text-lg font-bold mb-4">Save as Fragment</h3>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Fragment Name</label>
            <input v-model="fragmentName" type="text" class="input" placeholder="My Policy Fragment" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <textarea v-model="fragmentDescription" class="input" rows="3" placeholder="Describe this fragment..." />
          </div>
          <div class="flex space-x-2">
            <button @click="confirmSaveFragment" class="btn btn-primary flex-1">Save</button>
            <button @click="showFragmentModal = false" class="btn btn-secondary flex-1">Cancel</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { usePolicyStore } from '@/stores/policy'
import MonacoEditor from '@/components/MonacoEditor.vue'
import ValidationPanel from '@/components/ValidationPanel.vue'
import type { PolicyTemplate } from '@/types/policy'
import { exportService } from '@/services/api'

const policyStore = usePolicyStore()

const policyXml = ref(`<policies>
    <inbound>
        <base />
        <!-- Add inbound policies here -->
    </inbound>
    <backend>
        <base />
        <!-- Add backend policies here -->
    </backend>
    <outbound>
        <base />
        <!-- Add outbound policies here -->
    </outbound>
    <on-error>
        <base />
        <!-- Add error handling policies here -->
    </on-error>
</policies>`)

const selectedCategory = ref('')
const showFragmentModal = ref(false)
const fragmentName = ref('')
const fragmentDescription = ref('')

const editorOptions = {
  minimap: { enabled: true },
  lineNumbers: 'on' as const,
  roundedSelection: false,
  scrollBeyondLastLine: false,
  readOnly: false
}

const filteredTemplates = computed(() => {
  if (!selectedCategory.value) return policyStore.templates
  return policyStore.templates.filter(t => t.category === selectedCategory.value)
})

onMounted(async () => {
  await policyStore.loadTemplates()
})

function selectTemplate(template: PolicyTemplate) {
  policyXml.value = template.xml
  policyStore.setCurrentPolicy(template.xml)
}

function onPolicyChange(value: string) {
  policyStore.setCurrentPolicy(value)
}

async function validateCurrent() {
  await policyStore.validatePolicy(policyXml.value)
}

function formatXml() {
  try {
    const parser = new DOMParser()
    const xmlDoc = parser.parseFromString(policyXml.value, 'text/xml')
    const serializer = new XMLSerializer()
    const formatted = serializer.serializeToString(xmlDoc)
    policyXml.value = formatted
  } catch (e) {
    console.error('Failed to format XML:', e)
  }
}

function clearEditor() {
  policyXml.value = `<policies>
    <inbound>
        <base />
    </inbound>
    <backend>
        <base />
    </backend>
    <outbound>
        <base />
    </outbound>
    <on-error>
        <base />
    </on-error>
</policies>`
}

function saveAsFragment() {
  showFragmentModal.value = true
  fragmentName.value = ''
  fragmentDescription.value = ''
}

async function confirmSaveFragment() {
  if (!fragmentName.value.trim()) {
    alert('Please enter a fragment name')
    return
  }
  
  try {
    await policyStore.createFragment({
      name: fragmentName.value,
      description: fragmentDescription.value,
      xml: policyXml.value,
      version: 1
    })
    showFragmentModal.value = false
    alert('Fragment saved successfully!')
  } catch (e) {
    alert('Failed to save fragment')
  }
}

async function exportArm() {
  try {
    const armTemplate = await exportService.exportToArm(policyXml.value, 'api')
    const blob = new Blob([JSON.stringify(armTemplate, null, 2)], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'apim-policy.json'
    a.click()
    URL.revokeObjectURL(url)
  } catch (e) {
    alert('Failed to export ARM template')
  }
}
</script>
