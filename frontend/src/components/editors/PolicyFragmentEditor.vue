<template>
  <div class="fragment-editor max-w-5xl">
    <div class="mb-6">
      <h2 class="text-2xl font-bold text-gray-900 mb-2">Policy Fragment</h2>
      <p class="text-gray-600">Create reusable policy fragments that can be referenced across multiple APIs.</p>
    </div>
    
    <div class="space-y-6">
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <div class="w-8 h-8 bg-orange-100 rounded-lg flex items-center justify-center">
            <svg class="w-4 h-4 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 5a1 1 0 011-1h14a1 1 0 011 1v2a1 1 0 01-1 1H5a1 1 0 01-1-1V5zM4 13a1 1 0 011-1h6a1 1 0 011 1v6a1 1 0 01-1 1H5a1 1 0 01-1-1v-6zM16 13a1 1 0 011-1h2a1 1 0 011 1v6a1 1 0 01-1 1h-2a1 1 0 01-1-1v-6z" />
            </svg>
          </div>
          Fragment Details
        </h3>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-semibold text-gray-900 mb-2">Name *</label>
            <input v-model="localFragment.name" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="my-fragment" @input="emitUpdate" />
          </div>
          <div>
            <label class="block text-sm font-semibold text-gray-900 mb-2">Description</label>
            <textarea v-model="localFragment.description" rows="2" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="Fragment description..." @input="emitUpdate"></textarea>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <div class="w-8 h-8 bg-purple-100 rounded-lg flex items-center justify-center">
            <svg class="w-4 h-4 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          Policy Content
        </h3>
        <MonacoEditor
          v-model="localFragment.policyContent"
          language="xml"
          :height="400"
          @update:modelValue="emitUpdate"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import MonacoEditor from '@/components/MonacoEditor.vue'

const props = defineProps<{
  fragment: any
}>()

const emit = defineEmits<{
  update: [data: any]
}>()

const localFragment = ref({ ...props.fragment })

watch(() => props.fragment, (newFragment) => {
  localFragment.value = { ...newFragment }
}, { deep: true })

function emitUpdate() {
  emit('update', { ...localFragment.value })
}
</script>