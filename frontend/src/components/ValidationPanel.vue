<template>
  <div class="validation-panel bg-white border border-gray-200 rounded-lg p-4">
    <div class="flex items-center justify-between mb-3">
      <h3 class="text-lg font-semibold text-gray-900">Validation Results</h3>
      <span 
        v-if="result" 
        :class="statusClass"
        class="px-3 py-1 rounded-full text-sm font-medium"
      >
        {{ statusText }}
      </span>
    </div>

    <div v-if="!result" class="text-gray-500 text-sm">
      No validation results yet. Edit the policy to see validation.
    </div>

    <div v-else class="space-y-2">
      <!-- Errors -->
      <div v-if="result.errors.length > 0" class="space-y-1">
        <div 
          v-for="(error, index) in result.errors" 
          :key="`error-${index}`"
          class="flex items-start space-x-2 p-2 bg-red-50 border border-red-200 rounded"
        >
          <svg class="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
          </svg>
          <div class="flex-1 text-sm">
            <div class="font-medium text-red-800">Line {{ error.line }}, Column {{ error.column }}</div>
            <div class="text-red-700">{{ error.message }}</div>
          </div>
        </div>
      </div>

      <!-- Warnings -->
      <div v-if="result.warnings.length > 0" class="space-y-1">
        <div 
          v-for="(warning, index) in result.warnings" 
          :key="`warning-${index}`"
          class="flex items-start space-x-2 p-2 bg-yellow-50 border border-yellow-200 rounded"
        >
          <svg class="w-5 h-5 text-yellow-500 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
          </svg>
          <div class="flex-1 text-sm">
            <div class="font-medium text-yellow-800">Line {{ warning.line }}, Column {{ warning.column }}</div>
            <div class="text-yellow-700">{{ warning.message }}</div>
          </div>
        </div>
      </div>

      <!-- Success message -->
      <div v-if="result.isValid && result.errors.length === 0" class="p-3 bg-green-50 border border-green-200 rounded">
        <div class="flex items-center space-x-2">
          <svg class="w-5 h-5 text-green-500" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
          </svg>
          <span class="text-sm font-medium text-green-800">Policy is valid</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { ValidationResult } from '@/types/policy'

interface Props {
  result: ValidationResult | null
}

const props = defineProps<Props>()

const statusText = computed(() => {
  if (!props.result) return 'Not validated'
  if (props.result.errors.length > 0) return 'Invalid'
  if (props.result.warnings.length > 0) return 'Valid with warnings'
  return 'Valid'
})

const statusClass = computed(() => {
  if (!props.result) return 'bg-gray-100 text-gray-700'
  if (props.result.errors.length > 0) return 'bg-red-100 text-red-700'
  if (props.result.warnings.length > 0) return 'bg-yellow-100 text-yellow-700'
  return 'bg-green-100 text-green-700'
})
</script>
