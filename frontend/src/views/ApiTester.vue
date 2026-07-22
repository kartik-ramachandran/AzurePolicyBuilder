<template>
  <div class="api-tester">
    <div class="mb-7 flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
      <div>
        <div class="mb-3 flex items-center gap-2 text-xs font-extrabold uppercase tracking-[0.22em] text-blue-600">
          <span class="inline-block h-2 w-2 rounded-full bg-orange-500 shadow-[0_0_0_5px_rgba(249,115,22,.1)]"></span>
          Request workspace
        </div>
        <h1 class="mb-2 text-4xl font-bold text-gray-900 sm:text-5xl">API Tester</h1>
        <p class="max-w-2xl text-gray-600">Compose, authenticate and inspect API calls in one focused workspace.</p>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <span class="inline-flex items-center gap-2 rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1.5 text-xs font-bold text-emerald-700">
          <span class="h-1.5 w-1.5 rounded-full bg-emerald-500"></span> Agent online
        </span>
        <span class="rounded-full border border-blue-100 bg-blue-50 px-3 py-1.5 text-xs font-bold text-blue-700">Local environment</span>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-5 xl:grid-cols-[17rem_minmax(0,1fr)]">
      <!-- Sidebar - Collections & History -->
      <div>
        <div class="card top-24 xl:sticky">
          <div class="flex items-center justify-between mb-4">
            <div>
              <p class="text-[10px] font-extrabold uppercase tracking-[0.2em] text-slate-400">Library</p>
              <h3 class="mt-0.5 font-extrabold text-slate-900">Collections</h3>
            </div>
            <button @click="showNewCollection = true" class="grid h-9 w-9 place-items-center rounded-xl bg-blue-50 text-primary-600 transition hover:bg-blue-600 hover:text-white" title="New collection">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
            </button>
          </div>

          <!-- Collections List -->
          <div class="space-y-2 mb-6 max-h-60 overflow-y-auto">
            <div
              v-for="collection in collections"
              :key="collection.id"
              class="rounded-xl border border-transparent p-2.5 transition hover:border-blue-100 hover:bg-blue-50/70"
            >
              <div class="flex items-center justify-between cursor-pointer" @click="toggleCollection(collection)">
                <div class="flex items-center gap-2">
                  <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
                  </svg>
                  <span class="text-sm font-medium">{{ collection.name }}</span>
                </div>
                <span class="text-xs text-gray-500">{{ collection.requests.length }}</span>
              </div>

              <div v-if="selectedCollection?.id === collection.id" class="ml-6 mt-1 space-y-1">
                <div
                  v-for="(saved, sIdx) in collection.requests"
                  :key="sIdx"
                  class="flex items-center justify-between rounded px-1 py-0.5 hover:bg-gray-100"
                >
                  <div class="flex-1 cursor-pointer truncate" @click="loadSavedRequest(saved)">
                    <span :class="['text-xs font-semibold mr-1', getMethodClass(saved.method)]">{{ saved.method }}</span>
                    <span class="text-xs text-gray-700">{{ saved.name }}</span>
                  </div>
                  <button
                    @click="deleteSavedRequest(collection, sIdx)"
                    class="text-gray-400 hover:text-red-600 ml-1"
                    title="Remove"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
                <div v-if="collection.requests.length === 0" class="text-xs text-gray-400">
                  No saved requests
                </div>
              </div>
            </div>
          </div>

          <!-- History -->
          <div class="border-t border-slate-100 pt-5">
            <div class="mb-2 flex items-center justify-between">
              <h3 class="font-extrabold text-slate-900">Recent</h3>
              <span class="rounded-full bg-slate-100 px-2 py-0.5 text-[10px] font-bold text-slate-500">{{ history.length }}</span>
            </div>
            <div class="space-y-1 max-h-60 overflow-y-auto">
              <div
                v-for="(item, idx) in history.slice(0, 10)"
                :key="idx"
                class="p-2 rounded hover:bg-gray-50 cursor-pointer text-sm"
                @click="loadFromHistory(item)"
              >
                <div :class="['text-xs font-semibold mb-1', getMethodClass(item.method)]">
                  {{ item.method }}
                </div>
                <div class="text-xs text-gray-600 truncate">{{ item.url }}</div>
              </div>
            </div>
          </div>

          <button @click="clearHistory" class="btn btn-secondary w-full mt-4 text-sm">
            Clear History
          </button>
        </div>
      </div>

      <!-- Main Content - Request Builder -->
      <div class="min-w-0 space-y-5">
        <div class="card overflow-hidden !p-0">
          <div class="flex items-center justify-between border-b border-blue-100/70 bg-gradient-to-r from-blue-50/80 to-orange-50/40 px-5 py-3.5">
            <div class="flex items-center gap-2 text-sm font-extrabold text-slate-800">
              <span class="grid h-7 w-7 place-items-center rounded-lg bg-white text-blue-600 shadow-sm">
                <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 12h14m-6-6 6 6-6 6" /></svg>
              </span>
              New request
            </div>
            <span class="text-xs font-semibold text-slate-400">Unsaved request</span>
          </div>
          <div class="p-5 sm:p-6">
          <!-- Request URL -->
          <div class="request-composer mb-5 flex flex-col gap-2 rounded-2xl border border-blue-100 bg-slate-50/70 p-2 shadow-inner sm:flex-row">
            <select v-model="request.method" class="input font-extrabold sm:w-32">
              <option value="GET">GET</option>
              <option value="POST">POST</option>
              <option value="PUT">PUT</option>
              <option value="PATCH">PATCH</option>
              <option value="DELETE">DELETE</option>
              <option value="OPTIONS">OPTIONS</option>
            </select>
            <input
              v-model="request.url"
              type="text"
              placeholder="https://api.example.com/endpoint"
              class="input flex-1"
            />
            <button @click="sendRequest" class="btn btn-primary min-w-28" :disabled="loading">
              <svg v-if="!loading" class="mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m5 12 14-7-4 14-3-6-7-1Z" /></svg>
              <svg v-else class="mr-2 h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 0 1 8-8v4a4 4 0 0 0-4 4H4Z"/></svg>
              {{ loading ? 'Sending' : 'Send' }}
            </button>
            <button @click="saveToCollection" class="btn btn-secondary" title="Save request">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4" />
              </svg>
            </button>
          </div>

          <!-- Tabs -->
          <div class="mb-5 border-b border-gray-200">
            <nav class="flex gap-7 overflow-x-auto">
              <button
                v-for="tab in requestTabs"
                :key="tab"
                @click="activeRequestTab = tab"
                :class="[
                  'pb-3 border-b-2 font-bold text-sm transition-colors',
                  activeRequestTab === tab
                    ? 'border-primary-500 text-primary-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700'
                ]"
              >
                {{ tab }}
              </button>
            </nav>
          </div>

          <!-- Tab Content -->
          <div>
            <!-- Headers Tab -->
            <div v-show="activeRequestTab === 'Headers'" class="space-y-2">
              <div v-for="(header, idx) in request.headers" :key="idx" class="flex gap-2">
                <input
                  v-model="header.key"
                  type="text"
                  placeholder="Key"
                  class="input flex-1"
                />
                <input
                  v-model="header.value"
                  type="text"
                  placeholder="Value"
                  class="input flex-1"
                />
                <button @click="request.headers.splice(idx, 1)" class="btn btn-secondary">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                </button>
              </div>
              <button @click="request.headers.push({ key: '', value: '' })" class="btn btn-secondary">
                Add Header
              </button>
            </div>

            <!-- Body Tab -->
            <div v-show="activeRequestTab === 'Body'">
              <div class="mb-3">
                <select v-model="request.bodyType" class="input w-48">
                  <option value="none">None</option>
                  <option value="json">JSON</option>
                  <option value="xml">XML</option>
                  <option value="text">Text</option>
                  <option value="form">Form Data</option>
                </select>
              </div>
              <textarea
                v-if="request.bodyType !== 'none' && request.bodyType !== 'form'"
                v-model="request.body"
                class="input w-full font-mono text-sm"
                rows="10"
                :placeholder="getBodyPlaceholder()"
              ></textarea>
              <div v-else-if="request.bodyType === 'form'" class="space-y-2">
                <div v-for="(field, idx) in request.formData" :key="idx" class="flex gap-2">
                  <input v-model="field.key" placeholder="Key" class="input flex-1" />
                  <input v-model="field.value" placeholder="Value" class="input flex-1" />
                  <button @click="request.formData.splice(idx, 1)" class="btn btn-secondary">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
                <button @click="request.formData.push({ key: '', value: '' })" class="btn btn-secondary">
                  Add Field
                </button>
              </div>
            </div>

            <!-- Auth Tab -->
            <div v-show="activeRequestTab === 'Auth'">
              <select v-model="request.authType" class="input w-48 mb-3">
                <option value="none">No Auth</option>
                <option value="bearer">Bearer Token</option>
                <option value="apikey">API Key</option>
                <option value="basic">Basic Auth</option>
              </select>

              <div v-if="request.authType === 'bearer'" class="space-y-2">
                <label class="block text-sm font-medium">Token</label>
                <input v-model="request.auth.token" type="text" class="input w-full" />
              </div>

              <div v-else-if="request.authType === 'apikey'" class="space-y-2">
                <label class="block text-sm font-medium">Key</label>
                <input v-model="request.auth.key" type="text" class="input w-full mb-2" />
                <label class="block text-sm font-medium">Value</label>
                <input v-model="request.auth.value" type="text" class="input w-full" />
              </div>

              <div v-else-if="request.authType === 'basic'" class="space-y-2">
                <label class="block text-sm font-medium">Username</label>
                <input v-model="request.auth.username" type="text" class="input w-full mb-2" />
                <label class="block text-sm font-medium">Password</label>
                <input v-model="request.auth.password" type="password" class="input w-full" />
              </div>
            </div>
          </div>
          </div>
        </div>

        <!-- Response -->
        <div v-if="response" class="card overflow-hidden">
          <div class="flex items-center justify-between mb-4">
            <h3 class="font-semibold">Response</h3>
            <div class="flex items-center gap-4 text-sm">
              <span :class="['px-2 py-1 rounded font-semibold', getStatusClass(response.status)]">
                {{ response.status }} {{ response.statusText }}
              </span>
              <span class="text-gray-600">{{ response.time }}ms</span>
              <span class="text-gray-600">{{ formatSize(response.size) }}</span>
            </div>
          </div>

          <!-- Response Tabs -->
          <div class="border-b border-gray-200 mb-4">
            <nav class="flex gap-6">
              <button
                v-for="tab in responseTabs"
                :key="tab"
                @click="activeResponseTab = tab"
                :class="[
                  'pb-2 border-b-2 font-medium text-sm',
                  activeResponseTab === tab
                    ? 'border-primary-500 text-primary-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700'
                ]"
              >
                {{ tab }}
              </button>
            </nav>
          </div>

          <!-- Response Content -->
          <div>
            <div v-show="activeResponseTab === 'Body'">
              <div class="bg-gray-50 p-4 rounded overflow-x-auto">
                <pre class="text-sm">{{ formatResponse(response.data) }}</pre>
              </div>
            </div>

            <div v-show="activeResponseTab === 'Headers'">
              <div class="space-y-2">
                <div v-for="(value, key) in response.headers" :key="key" class="flex border-b pb-2">
                  <span class="font-semibold w-48">{{ key }}:</span>
                  <span class="text-gray-700">{{ value }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-else-if="!error" class="response-empty">
          <div class="response-empty__icon">
            <svg class="h-7 w-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8" d="M8 9h8m-8 4h5m-8 8h14a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-3l-2-2h-4L8 5H5a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2Z" />
            </svg>
          </div>
          <div>
            <h3 class="font-extrabold text-slate-900">Your response will appear here</h3>
            <p class="mt-1 text-sm text-slate-500">Enter a URL, configure the request and press Send.</p>
          </div>
          <div class="ml-auto hidden items-center gap-4 text-xs font-bold text-slate-400 sm:flex">
            <span>Status</span><span>Time</span><span>Size</span>
          </div>
        </div>

        <!-- Error -->
        <div v-if="error" class="card bg-red-50 border-red-200">
          <div class="flex items-start gap-3">
            <svg class="w-5 h-5 text-red-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <h4 class="font-semibold text-red-900 mb-1">Request Failed</h4>
              <p class="text-red-700 text-sm">{{ error }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Save to Collection Modal -->
    <div v-if="showSaveModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
        <h3 class="text-lg font-semibold mb-4">Save Request to Collection</h3>

        <label class="block text-sm font-medium mb-1">Request Name</label>
        <input
          v-model="saveRequestName"
          type="text"
          placeholder="My request"
          class="input w-full mb-4"
        />

        <label class="block text-sm font-medium mb-1">Collection</label>
        <select v-model="saveTargetCollectionId" class="input w-full mb-4">
          <option v-for="collection in collections" :key="collection.id" :value="collection.id">
            {{ collection.name }}
          </option>
          <option value="__new__">+ New collection…</option>
        </select>

        <input
          v-if="saveTargetCollectionId === '__new__'"
          v-model="saveNewCollectionName"
          type="text"
          placeholder="New collection name"
          class="input w-full mb-4"
          @keyup.enter="confirmSaveToCollection"
        />

        <div class="flex gap-2 justify-end">
          <button @click="showSaveModal = false" class="btn btn-secondary">Cancel</button>
          <button @click="confirmSaveToCollection" class="btn btn-primary">Save</button>
        </div>
      </div>
    </div>

    <!-- New Collection Modal -->
    <div v-if="showNewCollection" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
        <h3 class="text-lg font-semibold mb-4">New Collection</h3>
        <input
          v-model="newCollectionName"
          type="text"
          placeholder="Collection Name"
          class="input w-full mb-4"
          @keyup.enter="createCollection"
        />
        <div class="flex gap-2 justify-end">
          <button @click="showNewCollection = false" class="btn btn-secondary">Cancel</button>
          <button @click="createCollection" class="btn btn-primary">Create</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import axios from 'axios'

interface RequestHeader {
  key: string
  value: string
}

interface FormField {
  key: string
  value: string
}

interface Request {
  method: string
  url: string
  headers: RequestHeader[]
  bodyType: 'none' | 'json' | 'xml' | 'text' | 'form'
  body: string
  formData: FormField[]
  authType: 'none' | 'bearer' | 'apikey' | 'basic'
  auth: {
    token?: string
    key?: string
    value?: string
    username?: string
    password?: string
  }
}

interface Response {
  status: number
  statusText: string
  headers: Record<string, string>
  data: any
  time: number
  size: number
}

interface Collection {
  id: string
  name: string
  requests: any[]
}

const request = reactive<Request>({
  method: 'GET',
  url: '',
  headers: [{ key: 'Content-Type', value: 'application/json' }],
  bodyType: 'none',
  body: '',
  formData: [],
  authType: 'none',
  auth: {}
})

const response = ref<Response | null>(null)
const error = ref<string | null>(null)
const loading = ref(false)

const activeRequestTab = ref('Headers')
const activeResponseTab = ref('Body')
const requestTabs = ['Headers', 'Body', 'Auth']
const responseTabs = ['Body', 'Headers']

const collections = ref<Collection[]>([])
const selectedCollection = ref<Collection | null>(null)
const showNewCollection = ref(false)
const newCollectionName = ref('')
const showSaveModal = ref(false)
const saveRequestName = ref('')
const saveTargetCollectionId = ref('')
const saveNewCollectionName = ref('')
const history = ref<any[]>([])

// Load from localStorage
collections.value = JSON.parse(localStorage.getItem('api-tester-collections') || '[]')
history.value = JSON.parse(localStorage.getItem('api-tester-history') || '[]')

function getMethodClass(method: string): string {
  const classes: Record<string, string> = {
    GET: 'text-blue-600',
    POST: 'text-green-600',
    PUT: 'text-orange-600',
    DELETE: 'text-red-600',
    PATCH: 'text-purple-600'
  }
  return classes[method] || 'text-gray-600'
}

function getStatusClass(status: number): string {
  if (status >= 200 && status < 300) return 'bg-green-100 text-green-700'
  if (status >= 300 && status < 400) return 'bg-blue-100 text-blue-700'
  if (status >= 400 && status < 500) return 'bg-orange-100 text-orange-700'
  return 'bg-red-100 text-red-700'
}

function getBodyPlaceholder(): string {
  const placeholders: Record<string, string> = {
    json: '{\n  "key": "value"\n}',
    xml: '<root>\n  <element>value</element>\n</root>',
    text: 'Enter text here...'
  }
  return placeholders[request.bodyType] || ''
}

async function sendRequest() {
  loading.value = true
  error.value = null
  response.value = null

  try {
    const startTime = performance.now()

    // Build headers
    const headers: Record<string, string> = {}
    request.headers.forEach(h => {
      if (h.key && h.value) headers[h.key] = h.value
    })

    // Add auth headers
    if (request.authType === 'bearer' && request.auth.token) {
      headers['Authorization'] = `Bearer ${request.auth.token}`
    } else if (request.authType === 'apikey' && request.auth.key && request.auth.value) {
      headers[request.auth.key] = request.auth.value
    } else if (request.authType === 'basic' && request.auth.username && request.auth.password) {
      const encoded = btoa(`${request.auth.username}:${request.auth.password}`)
      headers['Authorization'] = `Basic ${encoded}`
    }

    // Build body
    let data: any = null
    if (request.bodyType === 'json' && request.body) {
      data = JSON.parse(request.body)
    } else if (request.bodyType === 'form') {
      const formData = new FormData()
      request.formData.forEach(f => {
        if (f.key && f.value) formData.append(f.key, f.value)
      })
      data = formData
    } else if (request.body) {
      data = request.body
    }

    const result = await axios({
      method: request.method,
      url: request.url,
      headers,
      data
    })

    const endTime = performance.now()

    response.value = {
      status: result.status,
      statusText: result.statusText,
      headers: result.headers as Record<string, string>,
      data: result.data,
      time: Math.round(endTime - startTime),
      size: JSON.stringify(result.data).length
    }

    // Add to history
    addToHistory()
  } catch (err: any) {
    if (err.response) {
      response.value = {
        status: err.response.status,
        statusText: err.response.statusText,
        headers: err.response.headers,
        data: err.response.data,
        time: 0,
        size: 0
      }
    } else {
      error.value = err.message || 'Request failed'
    }
  } finally {
    loading.value = false
  }
}

function formatResponse(data: any): string {
  if (typeof data === 'string') return data
  return JSON.stringify(data, null, 2)
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function addToHistory() {
  const item = {
    method: request.method,
    url: request.url,
    timestamp: new Date().toISOString()
  }
  history.value.unshift(item)
  if (history.value.length > 50) history.value.pop()
  localStorage.setItem('api-tester-history', JSON.stringify(history.value))
}

function loadFromHistory(item: any) {
  request.method = item.method
  request.url = item.url
}

function clearHistory() {
  history.value = []
  localStorage.removeItem('api-tester-history')
}

function createCollection() {
  if (!newCollectionName.value.trim()) return

  const collection: Collection = {
    id: Date.now().toString(),
    name: newCollectionName.value,
    requests: []
  }

  collections.value.push(collection)
  localStorage.setItem('api-tester-collections', JSON.stringify(collections.value))

  newCollectionName.value = ''
  showNewCollection.value = false
}

function persistCollections() {
  localStorage.setItem('api-tester-collections', JSON.stringify(collections.value))
}

function toggleCollection(collection: Collection) {
  selectedCollection.value = selectedCollection.value?.id === collection.id ? null : collection
}

function saveToCollection() {
  if (!request.url.trim()) {
    alert('Enter a request URL before saving')
    return
  }
  saveRequestName.value = request.url
  saveTargetCollectionId.value =
    selectedCollection.value?.id ?? collections.value[0]?.id ?? '__new__'
  saveNewCollectionName.value = ''
  showSaveModal.value = true
}

function confirmSaveToCollection() {
  let target: Collection | undefined
  if (saveTargetCollectionId.value === '__new__') {
    const name = saveNewCollectionName.value.trim()
    if (!name) {
      alert('Enter a name for the new collection')
      return
    }
    target = { id: Date.now().toString(), name, requests: [] }
    collections.value.push(target)
  } else {
    target = collections.value.find(c => c.id === saveTargetCollectionId.value)
  }
  if (!target) return

  target.requests.push({
    name: saveRequestName.value.trim() || request.url,
    method: request.method,
    url: request.url,
    headers: request.headers.map(h => ({ ...h })),
    bodyType: request.bodyType,
    body: request.body,
    formData: request.formData.map(f => ({ ...f })),
    authType: request.authType,
    auth: { ...request.auth }
  })

  persistCollections()
  selectedCollection.value = target
  showSaveModal.value = false
}

function loadSavedRequest(saved: any) {
  request.method = saved.method
  request.url = saved.url
  request.headers = (saved.headers ?? []).map((h: RequestHeader) => ({ ...h }))
  request.bodyType = saved.bodyType ?? 'none'
  request.body = saved.body ?? ''
  request.formData = (saved.formData ?? []).map((f: FormField) => ({ ...f }))
  request.authType = saved.authType ?? 'none'
  request.auth = { ...(saved.auth ?? {}) }
}

function deleteSavedRequest(collection: Collection, index: number) {
  collection.requests.splice(index, 1)
  persistCollections()
}
</script>

<style scoped>
.response-empty {
  display: flex;
  min-height: 8rem;
  align-items: center;
  gap: 1rem;
  border: 1px dashed rgba(147, 197, 253, 0.8);
  border-radius: 1.25rem;
  padding: 1.25rem;
  background: linear-gradient(120deg, rgba(239, 246, 255, 0.78), rgba(255, 247, 237, 0.58));
}

.response-empty__icon {
  display: grid;
  height: 3.25rem;
  width: 3.25rem;
  flex: none;
  place-items: center;
  border-radius: 1rem;
  color: #2563eb;
  background: white;
  box-shadow: 0 10px 24px rgba(37, 99, 235, 0.12);
}

@media (min-width: 640px) {
  .request-composer > .input:not(:first-child) {
    border-color: transparent;
    box-shadow: none;
  }
}
</style>
