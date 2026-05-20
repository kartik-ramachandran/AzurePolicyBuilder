<template>
  <div class="namedvalue-editor max-w-4xl">
    <div class="mb-6">
      <h2 class="text-2xl font-bold text-gray-900 mb-2">Named Value Configuration</h2>
      <p class="text-gray-600">Store configuration values and secrets for your APIs.</p>
    </div>
    
    <div class="bg-white rounded-xl border border-gray-200 p-6">
      <h3 class="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
        <div class="w-8 h-8 bg-green-100 rounded-lg flex items-center justify-center">
          <svg class="w-4 h-4 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
          </svg>
        </div>
        Value Details
      </h3>
      <div class="space-y-4">
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-semibold text-gray-900 mb-2">Name *</label>
            <input v-model="localNV.name" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="MY_CONFIG_VALUE" @input="emitUpdate" />
          </div>
          <div>
            <label class="block text-sm font-semibold text-gray-900 mb-2">Display Name</label>
            <input v-model="localNV.displayName" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="My Config Value" @input="emitUpdate" />
          </div>
        </div>
        <div>
          <label class="block text-sm font-semibold text-gray-900 mb-2">Value *</label>
          <input
            v-model="localNV.value"
            :type="localNV.secret ? 'password' : 'text'"
            class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            placeholder="Enter value..."
            @input="emitUpdate"
          />
        </div>
        <div class="flex items-center gap-2">
          <input
            v-model="localNV.secret"
            type="checkbox"
            class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
            @change="emitUpdate"
          />
          <label class="text-sm font-medium text-gray-700">Secret (hide value)</label>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  namedValue: any
}>()

const emit = defineEmits<{
  update: [data: any]
}>()

const localNV = ref({ ...props.namedValue })

watch(() => props.namedValue, (newNV) => {
  localNV.value = { ...newNV }
}, { deep: true })

function emitUpdate() {
  emit('update', { ...localNV.value })
}
</script>