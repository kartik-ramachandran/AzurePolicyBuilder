<template>
  <div class="fixed bottom-4 right-4 z-50">
    <!-- AI Assistant Trigger Button -->
    <button
      v-if="!showPanel"
      @click="showPanel = true"
      class="bg-gradient-to-r from-purple-600 to-blue-600 text-white rounded-full p-4 shadow-lg hover:shadow-xl transition-all duration-200 hover:scale-110"
      title="AI Assistant"
    >
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
      </svg>
    </button>

    <!-- AI Assistant Panel -->
    <div
      v-if="showPanel"
      class="bg-white rounded-lg shadow-2xl w-96 max-h-[600px] flex flex-col"
    >
      <!-- Header -->
      <div class="bg-gradient-to-r from-purple-600 to-blue-600 text-white p-4 rounded-t-lg flex justify-between items-center">
        <div class="flex items-center gap-2">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
          </svg>
          <span class="font-semibold">AI Assistant</span>
        </div>
        <div class="flex items-center gap-2">
          <button @click="showSettings = true" class="hover:bg-white/20 p-1 rounded">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          </button>
          <button @click="showPanel = false" class="hover:bg-white/20 p-1 rounded">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>

      <!-- Content -->
      <div class="flex-1 overflow-y-auto p-4 space-y-3">
        <!-- Not Configured Warning -->
        <div v-if="!isConfigured" class="bg-yellow-50 border border-yellow-200 rounded p-3 text-sm">
          <div class="flex items-start gap-2">
            <svg class="w-5 h-5 text-yellow-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
            <div>
              <p class="font-medium text-yellow-900">AI Assistant not configured</p>
              <p class="text-yellow-700 mt-1">Click settings to add your OpenAI API key</p>
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div v-else class="space-y-2">
          <h3 class="text-sm font-semibold text-gray-700">Quick Actions</h3>
          
          <button
            @click="activeMode = 'generate'"
            :class="[
              'w-full text-left p-3 rounded border-2 transition-colors',
              activeMode === 'generate'
                ? 'border-purple-500 bg-purple-50'
                : 'border-gray-200 hover:border-gray-300'
            ]"
          >
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
              <div>
                <div class="font-medium text-sm">Generate Policy</div>
                <div class="text-xs text-gray-600">Create APIM policy from description</div>
              </div>
            </div>
          </button>

          <button
            @click="activeMode = 'explain'"
            :class="[
              'w-full text-left p-3 rounded border-2 transition-colors',
              activeMode === 'explain'
                ? 'border-blue-500 bg-blue-50'
                : 'border-gray-200 hover:border-gray-300'
            ]"
          >
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <div>
                <div class="font-medium text-sm">Explain Policy</div>
                <div class="text-xs text-gray-600">Understand what a policy does</div>
              </div>
            </div>
          </button>

          <button
            @click="activeMode = 'improve'"
            :class="[
              'w-full text-left p-3 rounded border-2 transition-colors',
              activeMode === 'improve'
                ? 'border-green-500 bg-green-50'
                : 'border-gray-200 hover:border-gray-300'
            ]"
          >
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <div>
                <div class="font-medium text-sm">Improve Policy</div>
                <div class="text-xs text-gray-600">Get suggestions for enhancement</div>
              </div>
            </div>
          </button>
        </div>

        <!-- Generate Form -->
        <div v-if="activeMode === 'generate'" class="space-y-3 border-t pt-3">
          <div>
            <label class="block text-xs font-medium text-gray-700 mb-1">What does your policy need to do?</label>
            <textarea
              v-model="generateRequest.description"
              class="w-full border border-gray-300 rounded px-3 py-2 text-sm"
              rows="3"
              placeholder="E.g., Rate limit by subscription key with 100 calls per minute"
            ></textarea>
          </div>

          <div class="grid grid-cols-2 gap-2">
            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">Policy Type</label>
              <select v-model="generateRequest.policyType" class="w-full border border-gray-300 rounded px-2 py-1 text-sm">
                <option value="inbound">Inbound</option>
                <option value="backend">Backend</option>
                <option value="outbound">Outbound</option>
                <option value="on-error">On Error</option>
              </select>
            </div>

            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">Security Level</label>
              <select v-model="generateRequest.securityLevel" class="w-full border border-gray-300 rounded px-2 py-1 text-sm">
                <option value="low">Low</option>
                <option value="medium">Medium</option>
                <option value="high">High</option>
              </select>
            </div>
          </div>

          <button
            @click="handleGenerate"
            :disabled="loading || !generateRequest.description"
            class="w-full bg-purple-600 text-white py-2 rounded font-medium text-sm hover:bg-purple-700 disabled:bg-gray-400"
          >
            {{ loading ? 'Generating...' : 'Generate Policy' }}
          </button>
        </div>

        <!-- Result -->
        <div v-if="result" class="border-t pt-3 space-y-2">
          <div class="flex justify-between items-center">
            <h3 class="text-sm font-semibold text-gray-700">Result</h3>
            <button @click="copyResult" class="text-xs text-purple-600 hover:text-purple-700">
              Copy XML
            </button>
          </div>
          <div class="bg-gray-50 p-3 rounded text-xs overflow-x-auto">
            <pre>{{ result }}</pre>
          </div>
        </div>

        <!-- Error -->
        <div v-if="error" class="bg-red-50 border border-red-200 rounded p-3 text-sm">
          <p class="text-red-800">{{ error }}</p>
        </div>
      </div>
    </div>

    <!-- Settings Modal -->
    <div v-if="showSettings" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
        <h3 class="text-lg font-semibold mb-4">AI Assistant Settings</h3>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">OpenAI API Key</label>
            <input
              v-model="settingsForm.apiKey"
              type="password"
              placeholder="sk-..."
              class="w-full border border-gray-300 rounded px-3 py-2"
            />
            <p class="text-xs text-gray-600 mt-1">Your API key is stored locally and never sent to our servers</p>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Model</label>
            <select v-model="settingsForm.model" class="w-full border border-gray-300 rounded px-3 py-2">
              <option value="gpt-4">GPT-4</option>
              <option value="gpt-4-turbo">GPT-4 Turbo</option>
              <option value="gpt-3.5-turbo">GPT-3.5 Turbo</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Base URL (Optional)</label>
            <input
              v-model="settingsForm.baseURL"
              type="text"
              placeholder="https://api.openai.com/v1"
              class="w-full border border-gray-300 rounded px-3 py-2"
            />
            <p class="text-xs text-gray-600 mt-1">For Azure OpenAI or custom endpoints</p>
          </div>
        </div>

        <div class="flex gap-2 justify-end mt-6">
          <button @click="showSettings = false" class="px-4 py-2 text-gray-700 hover:bg-gray-100 rounded">
            Cancel
          </button>
          <button @click="saveSettings" class="px-4 py-2 bg-purple-600 text-white rounded hover:bg-purple-700">
            Save
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { aiAssistant } from '@/services/ai-assistant'

const showPanel = ref(false)
const showSettings = ref(false)
const activeMode = ref<'generate' | 'explain' | 'improve' | null>(null)
const loading = ref(false)
const result = ref('')
const error = ref('')
const isConfigured = ref(false)

const generateRequest = reactive({
  description: '',
  policyType: 'inbound' as 'inbound' | 'backend' | 'outbound' | 'on-error',
  securityLevel: 'medium' as 'low' | 'medium' | 'high'
})

const settingsForm = reactive({
  apiKey: '',
  model: 'gpt-4',
  baseURL: 'https://api.openai.com/v1'
})

onMounted(() => {
  isConfigured.value = aiAssistant.isConfigured()
  const config = localStorage.getItem('apim-ai-config')
  if (config) {
    const saved = JSON.parse(config)
    settingsForm.apiKey = saved.apiKey || ''
    settingsForm.model = saved.model || 'gpt-4'
    settingsForm.baseURL = saved.baseURL || 'https://api.openai.com/v1'
  }
})

async function handleGenerate() {
  loading.value = true
  error.value = ''
  result.value = ''

  try {
    const response = await aiAssistant.generatePolicy(generateRequest)
    result.value = response.policyXml
  } catch (err: any) {
    error.value = err.message || 'Failed to generate policy'
  } finally {
    loading.value = false
  }
}

function saveSettings() {
  aiAssistant.saveConfiguration(
    settingsForm.apiKey,
    settingsForm.model,
    settingsForm.baseURL
  )
  isConfigured.value = true
  showSettings.value = false
}

function copyResult() {
  navigator.clipboard.writeText(result.value)
}
</script>
