<template>
  <div class="api-builder">
    <div class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900 mb-2">API Builder</h1>
      <p class="text-gray-600">Select and compile endpoints from multiple OpenAPI specifications</p>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-4 gap-6">
      <!-- Sidebar -->
      <div class="lg:col-span-1 space-y-4">
        <!-- Load Specs -->
        <div class="card">
          <h3 class="text-lg font-semibold mb-3">Load OpenAPI Specs</h3>
          
          <input
            type="file"
            ref="fileInput"
            @change="handleFileUpload"
            accept=".json,.yaml,.yml"
            multiple
            class="hidden"
          />
          
          <button @click="fileInput?.click()" class="btn btn-secondary w-full mb-3">
            <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
            </svg>
            Load JSON Files
          </button>

          <div class="flex gap-2">
            <input
              v-model="urlInput"
              type="text"
              placeholder="OpenAPI URL..."
              class="input flex-1"
              @keyup.enter="loadFromUrl"
            />
            <button @click="loadFromUrl" class="btn btn-secondary">
              Load
            </button>
          </div>
        </div>

        <!-- Loaded Specs -->
        <div class="card">
          <h3 class="text-lg font-semibold mb-3">Loaded Specs ({{ loadedSpecs.size }})</h3>
          
          <div v-if="loadedSpecs.size === 0" class="text-center text-gray-500 py-6">
            <svg class="w-12 h-12 mx-auto mb-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <p class="text-sm">No specs loaded</p>
          </div>

          <div v-else class="space-y-2 max-h-96 overflow-y-auto">
            <div
              v-for="[name, spec] in loadedSpecs"
              :key="name"
              @click="selectSpec(name)"
              :class="[
                'p-3 rounded border cursor-pointer transition-colors',
                selectedSpec === name
                  ? 'border-primary-500 bg-primary-50'
                  : 'border-gray-200 hover:border-primary-300'
              ]"
            >
              <div class="font-medium text-sm">{{ spec.info?.title || name }}</div>
              <div class="text-xs text-gray-500 mt-1">
                {{ Object.keys(spec.paths || {}).length }} endpoints
              </div>
              <button
                @click.stop="removeSpec(name)"
                class="text-red-500 hover:text-red-700 text-xs mt-2"
              >
                Remove
              </button>
            </div>
          </div>
        </div>

        <!-- Summary -->
        <div class="card bg-primary-50 border-primary-200">
          <h3 class="text-lg font-semibold mb-3">Selection Summary</h3>
          <div class="space-y-2 text-sm">
            <div class="flex justify-between">
              <span>Endpoints:</span>
              <span class="font-semibold">{{ totalSelectedEndpoints }}</span>
            </div>
            <div class="flex justify-between">
              <span>Schemas:</span>
              <span class="font-semibold">{{ totalSelectedSchemas }}</span>
            </div>
          </div>
          
          <button
            @click="downloadCompiledSpec"
            :disabled="totalSelectedEndpoints === 0"
            class="btn btn-primary w-full mt-4"
          >
            <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
            </svg>
            Download OpenAPI
          </button>
        </div>
      </div>

      <!-- Main Content -->
      <div class="lg:col-span-3">
        <div v-if="!selectedSpec" class="card text-center py-12">
          <svg class="w-20 h-20 mx-auto mb-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
          <h2 class="text-2xl font-bold mb-2">Welcome to API Builder</h2>
          <p class="text-gray-600 mb-6">Build custom API specifications by selecting endpoints from multiple sources</p>
          
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4 max-w-3xl mx-auto">
            <div class="p-4 bg-primary-50 rounded-lg">
              <div class="text-3xl font-bold text-primary-600 mb-2">1</div>
              <h4 class="font-semibold mb-1">Load Specs</h4>
              <p class="text-sm text-gray-600">Upload or load OpenAPI specifications</p>
            </div>
            <div class="p-4 bg-primary-50 rounded-lg">
              <div class="text-3xl font-bold text-primary-600 mb-2">2</div>
              <h4 class="font-semibold mb-1">Select Endpoints</h4>
              <p class="text-sm text-gray-600">Choose specific APIs and schemas</p>
            </div>
            <div class="p-4 bg-primary-50 rounded-lg">
              <div class="text-3xl font-bold text-primary-600 mb-2">3</div>
              <h4 class="font-semibold mb-1">Download</h4>
              <p class="text-sm text-gray-600">Get your compiled OpenAPI spec</p>
            </div>
          </div>
        </div>

        <div v-else class="space-y-4">
          <!-- Spec Info -->
          <div class="card">
            <h2 class="text-2xl font-bold mb-2">{{ currentSpec?.info?.title }}</h2>
            <p class="text-gray-600 mb-4">{{ currentSpec?.info?.description }}</p>
            <div class="flex gap-4 text-sm text-gray-500">
              <span>Version: {{ currentSpec?.info?.version }}</span>
              <span v-if="currentSpec?.servers?.[0]">Base URL: {{ currentSpec.servers[0].url }}</span>
            </div>
          </div>

          <!-- Endpoints -->
          <div class="card">
            <h3 class="text-xl font-bold mb-4">API Endpoints</h3>
            
            <div class="space-y-2">
              <div
                v-for="(methods, path) in currentSpec?.paths"
                :key="path"
                class="border border-gray-200 rounded-lg overflow-hidden"
              >
                <div
                  @click="togglePath(path)"
                  class="bg-gray-50 px-4 py-3 cursor-pointer hover:bg-gray-100 flex justify-between items-center"
                >
                  <div class="flex items-center gap-3">
                    <input
                      type="checkbox"
                      :checked="isEndpointSelected(selectedSpec, path)"
                      @click.stop="toggleEndpointSelection(selectedSpec, path)"
                      class="w-4 h-4"
                    />
                    <span class="font-mono text-sm">{{ path }}</span>
                  </div>
                  <svg
                    :class="['w-5 h-5 transition-transform', expandedPaths.has(path) ? 'rotate-180' : '']"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                  </svg>
                </div>

                <div v-if="expandedPaths.has(path)" class="p-4 space-y-2">
                  <template
                    v-for="(operation, method) in methods"
                    :key="method"
                  >
                    <div
                      v-if="String(method) !== 'parameters'"
                      class="flex items-start gap-3 p-2"
                    >
                      <span
                        :class="[
                          'px-2 py-1 text-xs font-semibold rounded uppercase',
                          getMethodClass(String(method))
                        ]"
                      >
                        {{ method }}
                      </span>
                      <div class="flex-1">
                        <div class="font-medium text-sm">{{ operation.summary || operation.operationId }}</div>
                        <div class="text-xs text-gray-500 mt-1">{{ operation.description }}</div>
                      </div>
                    </div>
                  </template>
                </div>
              </div>
            </div>
          </div>

          <!-- Schemas -->
          <div v-if="currentSpec?.components?.schemas" class="card">
            <h3 class="text-xl font-bold mb-4">Data Models</h3>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
              <div
                v-for="(schema, name) in currentSpec.components.schemas"
                :key="name"
                class="border border-gray-200 rounded p-3 hover:border-primary-300 cursor-pointer"
                @click="toggleSchemaSelection(selectedSpec, name)"
              >
                <div class="flex items-center gap-2">
                  <input
                    type="checkbox"
                    :checked="isSchemaSelected(selectedSpec, name)"
                    @click.stop="toggleSchemaSelection(selectedSpec, name)"
                    class="w-4 h-4"
                  />
                  <span class="font-medium text-sm">{{ name }}</span>
                </div>
                <div class="text-xs text-gray-500 mt-1">{{ schema.description }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

interface OpenAPISpec {
  info?: {
    title?: string
    description?: string
    version?: string
  }
  servers?: Array<{ url: string }>
  paths?: Record<string, any>
  components?: {
    schemas?: Record<string, any>
  }
}

const loadedSpecs = ref<Map<string, OpenAPISpec>>(new Map())
const selectedEndpoints = ref<Map<string, Set<string>>>(new Map())
const selectedSchemas = ref<Map<string, Set<string>>>(new Map())
const selectedSpec = ref<string | null>(null)
const expandedPaths = ref<Set<string>>(new Set())
const urlInput = ref('')
const fileInput = ref<HTMLInputElement | null>(null)

const currentSpec = computed(() => {
  if (!selectedSpec.value) return null
  return loadedSpecs.value.get(selectedSpec.value)
})

const totalSelectedEndpoints = computed(() => {
  let total = 0
  for (const endpoints of selectedEndpoints.value.values()) {
    total += endpoints.size
  }
  return total
})

const totalSelectedSchemas = computed(() => {
  let total = 0
  for (const schemas of selectedSchemas.value.values()) {
    total += schemas.size
  }
  return total
})

async function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement
  const files = target.files
  if (!files) return

  for (const file of Array.from(files)) {
    try {
      const text = await file.text()
      const spec = JSON.parse(text)
      const name = file.name.replace(/\.(json|yaml|yml)$/, '')
      loadedSpecs.value.set(name, spec)
      
      if (!selectedSpec.value) {
        selectedSpec.value = name
      }
    } catch (error) {
      console.error(`Failed to load ${file.name}:`, error)
      alert(`Failed to load ${file.name}`)
    }
  }
}

async function loadFromUrl() {
  if (!urlInput.value) return

  try {
    const response = await fetch(urlInput.value)
    if (!response.ok) throw new Error(`HTTP ${response.status}`)
    
    const spec = await response.json()
    const name = spec.info?.title || 'API Spec'
    loadedSpecs.value.set(name, spec)
    
    if (!selectedSpec.value) {
      selectedSpec.value = name
    }
    
    urlInput.value = ''
  } catch (error) {
    console.error('Failed to load from URL:', error)
    alert('Failed to load OpenAPI spec from URL')
  }
}

function selectSpec(name: string) {
  selectedSpec.value = name
  expandedPaths.value.clear()
}

function removeSpec(name: string) {
  loadedSpecs.value.delete(name)
  selectedEndpoints.value.delete(name)
  selectedSchemas.value.delete(name)
  
  if (selectedSpec.value === name) {
    selectedSpec.value = loadedSpecs.value.size > 0 
      ? Array.from(loadedSpecs.value.keys())[0]
      : null
  }
}

function togglePath(path: string) {
  if (expandedPaths.value.has(path)) {
    expandedPaths.value.delete(path)
  } else {
    expandedPaths.value.add(path)
  }
}

function toggleEndpointSelection(specName: string | null, path: string | number) {
  if (!specName) return
  const normalizedPath = String(path)
  if (!selectedEndpoints.value.has(specName)) {
    selectedEndpoints.value.set(specName, new Set())
  }
  
  const endpoints = selectedEndpoints.value.get(specName)!
  if (endpoints.has(normalizedPath)) {
    endpoints.delete(normalizedPath)
  } else {
    endpoints.add(normalizedPath)
  }
}

function isEndpointSelected(specName: string | null, path: string | number): boolean {
  if (!specName) return false
  return selectedEndpoints.value.get(specName)?.has(String(path)) || false
}

function toggleSchemaSelection(specName: string | null, schemaName: string | number) {
  if (!specName) return
  const normalizedSchemaName = String(schemaName)
  if (!selectedSchemas.value.has(specName)) {
    selectedSchemas.value.set(specName, new Set())
  }
  
  const schemas = selectedSchemas.value.get(specName)!
  if (schemas.has(normalizedSchemaName)) {
    schemas.delete(normalizedSchemaName)
  } else {
    schemas.add(normalizedSchemaName)
  }
}

function isSchemaSelected(specName: string | null, schemaName: string | number): boolean {
  if (!specName) return false
  return selectedSchemas.value.get(specName)?.has(String(schemaName)) || false
}

function getMethodClass(method: string): string {
  const classes = {
    get: 'bg-blue-100 text-blue-700',
    post: 'bg-green-100 text-green-700',
    put: 'bg-orange-100 text-orange-700',
    delete: 'bg-red-100 text-red-700',
    patch: 'bg-purple-100 text-purple-700',
    options: 'bg-gray-100 text-gray-700'
  }
  return classes[method.toLowerCase() as keyof typeof classes] || 'bg-gray-100 text-gray-700'
}

function downloadCompiledSpec() {
  const compiledSpec: any = {
    openapi: '3.0.0',
    info: {
      title: 'Compiled API Specification',
      version: '1.0.0',
      description: 'Compiled from multiple OpenAPI specifications'
    },
    paths: {},
    components: {
      schemas: {}
    }
  }

  // Compile selected endpoints
  for (const [specName, endpoints] of selectedEndpoints.value.entries()) {
    const spec = loadedSpecs.value.get(specName)
    if (!spec?.paths) continue

    for (const path of endpoints) {
      compiledSpec.paths[path] = spec.paths[path]
    }
  }

  // Compile selected schemas
  for (const [specName, schemas] of selectedSchemas.value.entries()) {
    const spec = loadedSpecs.value.get(specName)
    if (!spec?.components?.schemas) continue

    for (const schemaName of schemas) {
      compiledSpec.components.schemas[schemaName] = spec.components.schemas[schemaName]
    }
  }

  // Download
  const blob = new Blob([JSON.stringify(compiledSpec, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = 'compiled-openapi.json'
  a.click()
  URL.revokeObjectURL(url)
}
</script>
