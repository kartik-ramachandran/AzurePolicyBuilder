<template>
  <div class="api-docs">
    <div class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900 mb-2">API Documentation</h1>
      <p class="text-gray-600">Interactive API documentation viewer</p>
    </div>

    <div v-if="!spec" class="card">
      <div class="text-center py-12">
        <svg class="w-20 h-20 mx-auto mb-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        <h3 class="text-lg font-semibold mb-2">Load OpenAPI Specification</h3>
        <p class="text-gray-600 mb-4">Upload or load an OpenAPI spec to view documentation</p>
        
        <div class="max-w-md mx-auto">
          <input
            type="file"
            ref="fileInput"
            @change="handleFileUpload"
            accept=".json,.yaml,.yml"
            class="hidden"
          />
          
          <button @click="fileInput?.click()" class="btn btn-primary mr-2">
            <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
            </svg>
            Upload File
          </button>
          
          <div class="flex gap-2 mt-4">
            <input
              v-model="urlInput"
              type="text"
              placeholder="Or enter OpenAPI URL..."
              class="input flex-1"
              @keyup.enter="loadFromUrl"
            />
            <button @click="loadFromUrl" class="btn btn-secondary">Load</button>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-4 gap-6">
      <!-- Sidebar Navigation -->
      <div class="lg:col-span-1">
        <div class="card sticky top-4">
          <div class="mb-4">
            <input
              v-model="searchQuery"
              type="search"
              placeholder="Search API..."
              class="input w-full"
            />
          </div>

          <nav class="space-y-2 max-h-[70vh] overflow-y-auto">
            <a
              href="#overview"
              class="block px-3 py-2 rounded hover:bg-primary-50 text-sm font-medium"
            >
              Overview
            </a>
            
            <div v-for="tag in tags" :key="tag" class="space-y-1">
              <div class="px-3 py-2 font-semibold text-sm text-gray-900">
                {{ tag }}
              </div>
              <a
                v-for="endpoint in getEndpointsByTag(tag)"
                :key="endpoint.path + endpoint.method"
                :href="`#${endpoint.method}-${endpoint.path.replace(/\//g, '-')}`"
                class="block px-3 py-2 pl-6 rounded hover:bg-primary-50 text-sm"
              >
                <span :class="['px-2 py-0.5 text-xs rounded uppercase font-semibold mr-2', getMethodClass(endpoint.method)]">
                  {{ endpoint.method }}
                </span>
                {{ endpoint.path }}
              </a>
            </div>

            <a
              v-if="spec.components?.schemas"
              href="#schemas"
              class="block px-3 py-2 rounded hover:bg-primary-50 text-sm font-medium"
            >
              Schemas
            </a>
          </nav>

          <button @click="spec = null" class="btn btn-secondary w-full mt-4">
            <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
            </svg>
            Load Different Spec
          </button>
        </div>
      </div>

      <!-- Main Content -->
      <div class="lg:col-span-3 space-y-6">
        <!-- API Info -->
        <div id="overview" class="card">
          <h2 class="text-3xl font-bold mb-2">{{ spec.info.title }}</h2>
          <div class="flex items-center gap-3 text-sm text-gray-600 mb-4">
            <span class="px-2 py-1 bg-primary-100 text-primary-700 rounded">v{{ spec.info.version }}</span>
            <span v-if="spec.servers?.[0]">{{ spec.servers[0].url }}</span>
          </div>
          <p class="text-gray-700 whitespace-pre-line">{{ spec.info.description }}</p>
          
          <div v-if="spec.info.contact || spec.info.license" class="mt-4 pt-4 border-t border-gray-200">
            <div v-if="spec.info.contact" class="text-sm">
              <span class="font-semibold">Contact:</span>
              <a v-if="spec.info.contact.email" :href="`mailto:${spec.info.contact.email}`" class="text-primary-600 hover:underline ml-2">
                {{ spec.info.contact.email }}
              </a>
            </div>
            <div v-if="spec.info.license" class="text-sm mt-1">
              <span class="font-semibold">License:</span>
              <span class="ml-2">{{ spec.info.license.name }}</span>
            </div>
          </div>
        </div>

        <!-- Endpoints -->
        <div v-for="tag in tags" :key="tag">
          <h3 class="text-2xl font-bold mb-4 text-gray-900">{{ tag }}</h3>
          
          <div class="space-y-4">
            <div
              v-for="endpoint in getEndpointsByTag(tag)"
              :key="endpoint.path + endpoint.method"
              :id="`${endpoint.method}-${endpoint.path.replace(/\//g, '-')}`"
              class="card"
            >
              <div class="flex items-start gap-3 mb-3">
                <span :class="['px-3 py-1 text-sm font-bold rounded uppercase', getMethodClass(endpoint.method)]">
                  {{ endpoint.method }}
                </span>
                <div class="flex-1">
                  <div class="font-mono text-lg font-semibold">{{ endpoint.path }}</div>
                  <div class="text-gray-700 mt-1">{{ endpoint.operation.summary }}</div>
                  <div v-if="endpoint.operation.description" class="text-gray-600 text-sm mt-2">
                    {{ endpoint.operation.description }}
                  </div>
                </div>
              </div>

              <!-- Parameters -->
              <div v-if="endpoint.operation.parameters?.length" class="mt-4">
                <h4 class="font-semibold mb-2">Parameters</h4>
                <div class="overflow-x-auto">
                  <table class="w-full text-sm">
                    <thead class="bg-gray-50">
                      <tr>
                        <th class="px-3 py-2 text-left">Name</th>
                        <th class="px-3 py-2 text-left">Type</th>
                        <th class="px-3 py-2 text-left">In</th>
                        <th class="px-3 py-2 text-left">Description</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="param in endpoint.operation.parameters" :key="param.name" class="border-t">
                        <td class="px-3 py-2 font-mono">
                          {{ param.name }}
                          <span v-if="param.required" class="text-red-500">*</span>
                        </td>
                        <td class="px-3 py-2">{{ param.schema?.type || 'string' }}</td>
                        <td class="px-3 py-2">
                          <span class="px-2 py-0.5 bg-gray-100 rounded text-xs">{{ param.in }}</span>
                        </td>
                        <td class="px-3 py-2 text-gray-600">{{ param.description }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>

              <!-- Request Body -->
              <div v-if="endpoint.operation.requestBody" class="mt-4">
                <h4 class="font-semibold mb-2">Request Body</h4>
                <div class="bg-gray-50 p-4 rounded">
                  <pre class="text-xs overflow-x-auto">{{ JSON.stringify(getSchemaExample(endpoint.operation.requestBody.content?.['application/json']?.schema), null, 2) }}</pre>
                </div>
              </div>

              <!-- Responses -->
              <div v-if="endpoint.operation.responses" class="mt-4">
                <h4 class="font-semibold mb-2">Responses</h4>
                <div class="space-y-2">
                  <div v-for="(response, code) in endpoint.operation.responses" :key="code" class="border rounded p-3">
                    <div class="flex items-center gap-2 mb-2">
                      <span :class="['px-2 py-1 rounded text-sm font-semibold', getStatusClass(code)]">
                        {{ code }}
                      </span>
                      <span class="text-gray-700">{{ response.description }}</span>
                    </div>
                    <div v-if="response.content?.['application/json']" class="bg-gray-50 p-3 rounded mt-2">
                      <pre class="text-xs overflow-x-auto">{{ JSON.stringify(getSchemaExample(response.content['application/json'].schema), null, 2) }}</pre>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Schemas -->
        <div v-if="spec.components?.schemas" id="schemas">
          <h3 class="text-2xl font-bold mb-4">Schemas</h3>
          
          <div class="space-y-4">
            <div
              v-for="(schema, name) in spec.components.schemas"
              :key="name"
              class="card"
            >
              <h4 class="text-lg font-semibold mb-2">{{ name }}</h4>
              <p v-if="schema.description" class="text-gray-600 text-sm mb-3">{{ schema.description }}</p>
              
              <div class="bg-gray-50 p-4 rounded">
                <pre class="text-xs overflow-x-auto">{{ JSON.stringify(schema, null, 2) }}</pre>
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
  info: {
    title: string
    version: string
    description?: string
    contact?: { email?: string }
    license?: { name: string }
  }
  servers?: Array<{ url: string }>
  paths: Record<string, any>
  components?: {
    schemas?: Record<string, any>
  }
}

const spec = ref<OpenAPISpec | null>(null)
const searchQuery = ref('')
const urlInput = ref( '')
const fileInput = ref<HTMLInputElement | null>(null)

const tags = computed(() => {
  if (!spec.value?.paths) return []
  
  const tagSet = new Set<string>()
  
  for (const path in spec.value.paths) {
    for (const method in spec.value.paths[path]) {
      if (method === 'parameters') continue
      const operation = spec.value.paths[path][method]
      const operationTags = operation.tags || ['Default']
      operationTags.forEach((tag: string) => tagSet.add(tag))
    }
  }
  
  return Array.from(tagSet)
})

function getEndpointsByTag(tag: string) {
  if (!spec.value?.paths) return []
  
  const endpoints: any[] = []
  
  for (const path in spec.value.paths) {
    for (const method in spec.value.paths[path]) {
      if (method === 'parameters') continue
      const operation = spec.value.paths[path][method]
      const operationTags = operation.tags || ['Default']
      
      if (operationTags.includes(tag)) {
        endpoints.push({ path, method, operation })
      }
    }
  }
  
  return endpoints
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

function getStatusClass(code: string | number): string {
  const codeNum = parseInt(String(code))
  if (codeNum >= 200 && codeNum < 300) return 'bg-green-100 text-green-700'
  if (codeNum >= 300 && codeNum < 400) return 'bg-blue-100 text-blue-700'
  if (codeNum >= 400 && codeNum < 500) return 'bg-orange-100 text-orange-700'
  if (codeNum >= 500) return 'bg-red-100 text-red-700'
  return 'bg-gray-100 text-gray-700'
}

function getSchemaExample(schema: any): any {
  if (!schema) return {}
  
  if (schema.$ref) {
    const refName = schema.$ref.split('/').pop()
    return { $ref: refName }
  }
  
  if (schema.type === 'object' && schema.properties) {
    const example: any = {}
    for (const [key, prop] of Object.entries(schema.properties as Record<string, any>)) {
      example[key] = getSchemaExample(prop)
    }
    return example
  }
  
  if (schema.type === 'array' && schema.items) {
    return [getSchemaExample(schema.items)]
  }
  
  if (schema.example !== undefined) return schema.example
  
  const defaults: Record<string, any> = {
    string: 'string',
    number: 0,
    integer: 0,
    boolean: false,
    array: [],
    object: {}
  }
  
  return defaults[schema.type] || null
}

async function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  try {
    const text = await file.text()
    spec.value = JSON.parse(text)
  } catch (error) {
    console.error('Failed to load file:', error)
    alert('Failed to load OpenAPI specification')
  }
}

async function loadFromUrl() {
  if (!urlInput.value) return

  try {
    const response = await fetch(urlInput.value)
    if (!response.ok) throw new Error(`HTTP ${response.status}`)
    
    spec.value = await response.json()
    urlInput.value = ''
  } catch (error) {
    console.error('Failed to load from URL:', error)
    alert('Failed to load OpenAPI spec from URL')
  }
}
</script>
