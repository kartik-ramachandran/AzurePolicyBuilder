<template>
  <div class="fragment-manager">
    <div class="mb-6 flex justify-between items-center">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 mb-2">Policy Fragment Manager</h1>
        <p class="text-gray-600">Create and manage reusable policy fragments</p>
      </div>
      <button @click="createNew" class="btn btn-primary">
        <svg class="w-5 h-5 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
        </svg>
        New Fragment
      </button>
    </div>

    <div v-if="policyStore.isLoading" class="text-center py-12">
      <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      <p class="mt-4 text-gray-600">Loading fragments...</p>
    </div>

    <div v-else-if="policyStore.fragments.length === 0" class="card text-center py-12">
      <svg class="w-16 h-16 mx-auto text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
      </svg>
      <h3 class="text-lg font-semibold text-gray-900 mb-2">No fragments yet</h3>
      <p class="text-gray-600 mb-4">Create your first reusable policy fragment</p>
      <button @click="createNew" class="btn btn-primary">Create Fragment</button>
    </div>

    <div v-else class="grid grid-cols-1 gap-4">
      <div
        v-for="fragment in policyStore.fragments"
        :key="fragment.id"
        class="card hover:shadow-md transition-shadow"
      >
        <div class="flex justify-between items-start">
          <div class="flex-1">
            <h3 class="text-lg font-semibold text-gray-900 mb-1">{{ fragment.name }}</h3>
            <p class="text-gray-600 text-sm mb-3">{{ fragment.description }}</p>
            <div class="flex items-center space-x-4 text-sm text-gray-500">
              <span>Version {{ fragment.version }}</span>
              <span>Used {{ fragment.usageCount }} times</span>
              <span>Updated {{ formatDate(fragment.updatedAt) }}</span>
            </div>
          </div>
          <div class="flex space-x-2 ml-4">
            <button
              @click="viewFragment(fragment)"
              class="p-2 text-gray-600 hover:text-primary-600 hover:bg-primary-50 rounded"
              title="View"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
            </button>
            <button
              @click="copyFragment(fragment)"
              class="p-2 text-gray-600 hover:text-primary-600 hover:bg-primary-50 rounded"
              title="Copy"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
              </svg>
            </button>
            <button
              @click="deleteFragment(fragment.id)"
              class="p-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded"
              title="Delete"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Fragment Viewer Modal -->
    <div v-if="viewingFragment" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div class="bg-white rounded-lg p-6 max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <div class="flex justify-between items-start mb-4">
          <div>
            <h3 class="text-2xl font-bold">{{ viewingFragment.name }}</h3>
            <p class="text-gray-600 mt-1">{{ viewingFragment.description }}</p>
          </div>
          <button @click="viewingFragment = null" class="text-gray-400 hover:text-gray-600">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        
        <div class="border border-gray-300 rounded-lg overflow-hidden" style="height: 400px;">
          <MonacoEditor
            :model-value="viewingFragment.xml"
            language="xml"
            :readonly="true"
          />
        </div>

        <div class="flex space-x-2 mt-4">
          <button @click="useFragmentInEditor(viewingFragment)" class="btn btn-primary flex-1">Use in Editor</button>
          <button @click="copyFragment(viewingFragment)" class="btn btn-secondary flex-1">Copy XML</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { usePolicyStore } from '@/stores/policy'
import MonacoEditor from '@/components/MonacoEditor.vue'
import type { PolicyFragment } from '@/types/policy'

const router = useRouter()
const policyStore = usePolicyStore()
const viewingFragment = ref<PolicyFragment | null>(null)

onMounted(async () => {
  await policyStore.loadFragments()
})

function createNew() {
  router.push('/')
}

function viewFragment(fragment: PolicyFragment) {
  viewingFragment.value = fragment
}

function copyFragment(fragment: PolicyFragment) {
  navigator.clipboard.writeText(fragment.xml)
  alert('Fragment XML copied to clipboard!')
}

function useFragmentInEditor(fragment: PolicyFragment) {
  policyStore.setCurrentPolicy(fragment.xml)
  router.push('/')
}

async function deleteFragment(id: string) {
  if (confirm('Are you sure you want to delete this fragment?')) {
    try {
      await policyStore.deleteFragment(id)
      alert('Fragment deleted successfully')
    } catch (e) {
      alert('Failed to delete fragment')
    }
  }
}

function formatDate(dateString: string) {
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24))
  
  if (diffDays === 0) return 'Today'
  if (diffDays === 1) return 'Yesterday'
  if (diffDays < 7) return `${diffDays} days ago`
  if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`
  return date.toLocaleDateString()
}
</script>
