<template>
  <div class="project-generator">
    <div class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900 mb-2">APIM Project Generator</h1>
      <p class="text-gray-600">Create a complete, deployment-ready Azure APIM project structure</p>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Sidebar - Project Configuration -->
      <div class="lg:col-span-1">
        <div class="card sticky top-4">
          <h3 class="font-semibold mb-4">Project Configuration</h3>
          
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Project Name</label>
              <input
                v-model="project.name"
                type="text"
                class="input w-full"
                placeholder="my-apim-project"
              />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
              <textarea
                v-model="project.description"
                class="input w-full"
                rows="3"
                placeholder="Project description..."
              ></textarea>
            </div>

            <div class="border-t pt-4">
              <h4 class="font-medium mb-3">Components</h4>
              <div class="space-y-2 text-sm">
                <div class="flex justify-between">
                  <span>APIs</span>
                  <span class="font-semibold">{{ project.apis.length }}</span>
                </div>
                <div class="flex justify-between">
                  <span>Backends</span>
                  <span class="font-semibold">{{ project.backends.length }}</span>
                </div>
                <div class="flex justify-between">
                  <span>Named Values</span>
                  <span class="font-semibold">{{ project.namedValues.length }}</span>
                </div>
                <div class="flex justify-between">
                  <span>Policy Fragments</span>
                  <span class="font-semibold">{{ project.policyFragments.length }}</span>
                </div>
                <div class="flex justify-between">
                  <span>Products</span>
                  <span class="font-semibold">{{ project.products.length }}</span>
                </div>
                <div class="flex justify-between">
                  <span>Schemas</span>
                  <span class="font-semibold">{{ project.schemas.length }}</span>
                </div>
              </div>
            </div>

            <div class="border-t pt-4">
              <button
                @click="generateProject"
                :disabled="!canGenerate || generating"
                class="btn btn-primary w-full mb-2"
              >
                {{ generating ? 'Generating...' : 'Generate Project' }}
              </button>
              
              <button
                @click="downloadProject"
                :disabled="!canGenerate || downloading"
                class="btn btn-secondary w-full"
              >
                {{ downloading ? 'Downloading...' : 'Download as ZIP' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Content - Component Builders -->
      <div class="lg:col-span-2 space-y-6">
        <!-- Tabs -->
        <div class="card">
          <div class="border-b border-gray-200 mb-4">
            <nav class="flex gap-4">
              <button
                v-for="tab in tabs"
                :key="tab.id"
                @click="activeTab = tab.id"
                :class="[
                  'pb-2 border-b-2 font-medium text-sm',
                  activeTab === tab.id
                    ? 'border-primary-500 text-primary-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700'
                ]"
              >
                {{ tab.label }}
              </button>
            </nav>
          </div>

          <!-- APIs Tab -->
          <div v-show="activeTab === 'apis'">
            <div class="flex justify-between items-center mb-4">
              <h3 class="font-semibold">APIs</h3>
              <button @click="addApi" class="btn btn-primary btn-sm">+ Add API</button>
            </div>
            
            <div v-if="project.apis.length === 0" class="text-center py-8 text-gray-500">
              No APIs added yet. Click "Add API" to get started.
            </div>

            <div v-else class="space-y-3">
              <div
                v-for="(api, idx) in project.apis"
                :key="idx"
                class="border rounded p-4 hover:bg-gray-50"
              >
                <div class="flex justify-between items-start mb-3">
                  <div class="flex-1">
                    <input
                      v-model="api.displayName"
                      class="input w-full mb-2"
                      placeholder="API Display Name"
                    />
                    <input
                      v-model="api.path"
                      class="input w-full"
                      placeholder="API Path (e.g., /api/v1)"
                    />
                  </div>
                  <button @click="project.apis.splice(idx, 1)" class="text-red-600 hover:text-red-700 ml-2">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </button>
                </div>
                <textarea
                  v-model="api.description"
                  class="input w-full text-sm"
                  rows="2"
                  placeholder="API description..."
                ></textarea>
              </div>
            </div>
          </div>

          <!-- Backends Tab -->
          <div v-show="activeTab === 'backends'">
            <div class="flex justify-between items-center mb-4">
              <h3 class="font-semibold">Backends</h3>
              <button @click="addBackend" class="btn btn-primary btn-sm">+ Add Backend</button>
            </div>

            <div v-if="project.backends.length === 0" class="text-center py-8 text-gray-500">
              No backends configured.
            </div>

            <div v-else class="space-y-3">
              <div
                v-for="(backend, idx) in project.backends"
                :key="idx"
                class="border rounded p-4"
              >
                <div class="flex justify-between items-start mb-3">
                  <div class="flex-1 space-y-2">
                    <input
                      v-model="backend.title"
                      class="input w-full"
                      placeholder="Backend Title"
                    />
                    <input
                      v-model="backend.url"
                      class="input w-full"
                      placeholder="Backend URL (https://...)"
                    />
                  </div>
                  <button @click="project.backends.splice(idx, 1)" class="text-red-600 hover:text-red-700 ml-2">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Named Values Tab -->
          <div v-show="activeTab === 'named-values'">
            <div class="flex justify-between items-center mb-4">
              <h3 class="font-semibold">Named Values</h3>
              <button @click="addNamedValue" class="btn btn-primary btn-sm">+ Add Named Value</button>
            </div>

            <div v-if="project.namedValues.length === 0" class="text-center py-8 text-gray-500">
              No named values configured.
            </div>

            <div v-else class="space-y-3">
              <div
                v-for="(nv, idx) in project.namedValues"
                :key="idx"
                class="border rounded p-4"
              >
                <div class="flex justify-between items-start mb-3">
                  <div class="flex-1 space-y-2">
                    <input
                      v-model="nv.displayName"
                      class="input w-full"
                      placeholder="Display Name"
                    />
                    <input
                      v-model="nv.value"
                      :type="nv.secret ? 'password' : 'text'"
                      class="input w-full"
                      placeholder="Value"
                    />
                    <label class="flex items-center text-sm">
                      <input
                        v-model="nv.secret"
                        type="checkbox"
                        class="mr-2"
                      />
                      Secret value
                    </label>
                  </div>
                  <button @click="project.namedValues.splice(idx, 1)" class="text-red-600 hover:text-red-700 ml-2">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Policy Fragments Tab -->
          <div v-show="activeTab === 'fragments'">
            <div class="flex justify-between items-center mb-4">
              <h3 class="font-semibold">Policy Fragments</h3>
              <button @click="loadFragmentsFromStore" class="btn btn-primary btn-sm">
                Load from Fragment Manager
              </button>
            </div>

            <div v-if="project.policyFragments.length === 0" class="text-center py-8 text-gray-500">
              No policy fragments added.
            </div>

            <div v-else class="space-y-3">
              <div
                v-for="(fragment, idx) in project.policyFragments"
                :key="idx"
                class="border rounded p-4"
              >
                <div class="flex justify-between items-center mb-2">
                  <div class="font-medium">{{ fragment.name }}</div>
                  <button @click="project.policyFragments.splice(idx, 1)" class="text-red-600 hover:text-red-700">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
                <div class="text-sm text-gray-600">{{ fragment.description 
                }}</div>
              </div>
            </div>
          </div>

          <!-- Products Tab -->
          <div v-show="activeTab === 'products'">
            <div class="flex justify-between items-center mb-4">
              <h3 class="font-semibold">Products</h3>
              <button @click="addProduct" class="btn btn-primary btn-sm">+ Add Product</button>
            </div>

            <div v-if="project.products.length === 0" class="text-center py-8 text-gray-500">
              No products configured.
            </div>

            <div v-else class="space-y-3">
              <div
                v-for="(product, idx) in project.products"
                :key="idx"
                class="border rounded p-4"
              >
                <div class="flex justify-between items-start mb-3">
                  <div class="flex-1 space-y-2">
                    <input
                      v-model="product.displayName"
                      class="input w-full"
                      placeholder="Product Name"
                    />
                    <textarea
                      v-model="product.description"
                      class="input w-full"
                      rows="2"
                      placeholder="Description"
                    ></textarea>
                    <div class="flex gap-4 text-sm">
                      <label class="flex items-center">
                        <input
                          v-model="product.subscriptionRequired"
                          type="checkbox"
                          class="mr-2"
                        />
                        Subscription Required
                      </label>
                      <label class="flex items-center">
                        <input
                          v-model="product.published"
                          type="checkbox"
                          class="mr-2"
                        />
                        Published
                      </label>
                    </div>
                  </div>
                  <button @click="project.products.splice(idx, 1)" class="text-red-600 hover:text-red-700 ml-2">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Generated Preview -->
        <div v-if="generatedFiles" class="card">
          <h3 class="font-semibold mb-4">Generated Project Structure</h3>
          <div class="bg-gray-900 text-gray-100 p-4 rounded font-mono text-sm max-h-96 overflow-y-auto">
            <div v-for="(_, path) in generatedFiles" :key="path" class="mb-1">
              📄 {{ path }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { usePolicyStore } from '@/stores/policy'
import axios from 'axios'

const policyStore = usePolicyStore()

const project = reactive({
  name: '',
  description: '',
  apis: [] as any[],
  backends: [] as any[],
  namedValues: [] as any[],
  policyFragments: [] as any[],
  products: [] as any[],
  tags: [] as string[],
  schemas: [] as any[]
})

const activeTab = ref('apis')
const generating = ref(false)
const downloading = ref(false)
const generatedFiles = ref<Record<string, string> | null>(null)

const tabs = [
  { id: 'apis', label: 'APIs' },
  { id: 'backends', label: 'Backends' },
  { id: 'named-values', label: 'Named Values' },
  { id: 'fragments', label: 'Policy Fragments' },
  { id: 'products', label: 'Products' }
]

const canGenerate = computed(() => {
  return project.name.trim().length > 0
})

function addApi() {
  project.apis.push({
    name: `api-${project.apis.length + 1}`,
    displayName: '',
    path: '',
    description: '',
    protocols: ['https'],
    serviceUrl: '',
    operations: []
  })
}

function addBackend() {
  project.backends.push({
    name: `backend-${project.backends.length + 1}`,
    title: '',
    url: '',
    protocol: 'http'
  })
}

function addNamedValue() {
  project.namedValues.push({
    name: `nv-${project.namedValues.length + 1}`,
    displayName: '',
    value: '',
    secret: false,
    tags: []
  })
}

function addProduct() {
  project.products.push({
    name: `product-${project.products.length + 1}`,
    displayName: '',
    description: '',
    subscriptionRequired: true,
    approvalRequired: false,
    published: true,
    apis: []
  })
}

function loadFragmentsFromStore() {
  const fragments = policyStore.fragments.map((f: any) => ({
    name: f.name,
    description: f.description,
    policyContent: f.content
  }))
  project.policyFragments = [...fragments]
}

async function generateProject() {
  generating.value = true
  try {
    const response = await axios.post('http://localhost:5000/api/project/generate', project)
    generatedFiles.value = response.data
  } catch (error) {
    console.error('Failed to generate project:', error)
    alert('Failed to generate project structure')
  } finally {
    generating.value = false
  }
}

async function downloadProject() {
  downloading.value = true
  try {
    const response = await axios.post('http://localhost:5000/api/project/download', project, {
      responseType: 'blob'
    })
    
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `${project.name || 'apim-project'}.zip`)
    document.body.appendChild(link)
    link.click()
    link.remove()
  } catch (error) {
    console.error('Failed to download project:', error)
    alert('Failed to download project')
  } finally {
    downloading.value = false
  }
}
</script>

<style scoped>
.btn-sm {
  @apply px-3 py-1 text-sm;
}
</style>
