import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { PolicyTemplate, PolicyFragment, ValidationResult } from '@/types/policy'
import { templateService, fragmentService, validationService } from '@/services/api'

export const usePolicyStore = defineStore('policy', () => {
  // State
  const templates = ref<PolicyTemplate[]>([])
  const fragments = ref<PolicyFragment[]>([])
  const currentPolicy = ref<string>('')
  const validationResult = ref<ValidationResult | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Computed
  const hasErrors = computed(() => 
    validationResult.value?.errors && validationResult.value.errors.length > 0
  )

  const hasWarnings = computed(() => 
    validationResult.value?.warnings && validationResult.value.warnings.length > 0
  )

  // Actions
  async function loadTemplates() {
    isLoading.value = true
    error.value = null
    try {
      templates.value = await templateService.getTemplates()
    } catch (e) {
      error.value = 'Failed to load templates'
      console.error(e)
    } finally {
      isLoading.value = false
    }
  }

  async function loadFragments() {
    isLoading.value = true
    error.value = null
    try {
      fragments.value = await fragmentService.getFragments()
    } catch (e) {
      error.value = 'Failed to load fragments'
      console.error(e)
    } finally {
      isLoading.value = false
    }
  }

  async function validatePolicy(xml: string) {
    try {
      validationResult.value = await validationService.validatePolicy(xml)
    } catch (e) {
      console.error('Validation failed:', e)
      validationResult.value = {
        isValid: false,
        errors: [{ line: 0, column: 0, message: 'Validation service unavailable', severity: 'error' }],
        warnings: []
      }
    }
  }

  async function createFragment(fragment: Omit<PolicyFragment, 'id' | 'createdAt' | 'updatedAt' | 'usageCount'>) {
    try {
      const newFragment = await fragmentService.createFragment(fragment)
      fragments.value.push(newFragment)
      return newFragment
    } catch (e) {
      error.value = 'Failed to create fragment'
      console.error(e)
      throw e
    }
  }

  async function deleteFragment(id: string) {
    try {
      await fragmentService.deleteFragment(id)
      fragments.value = fragments.value.filter(f => f.id !== id)
    } catch (e) {
      error.value = 'Failed to delete fragment'
      console.error(e)
      throw e
    }
  }

  function setCurrentPolicy(xml: string) {
    currentPolicy.value = xml
  }

  return {
    // State
    templates,
    fragments,
    currentPolicy,
    validationResult,
    isLoading,
    error,
    // Computed
    hasErrors,
    hasWarnings,
    // Actions
    loadTemplates,
    loadFragments,
    validatePolicy,
    createFragment,
    deleteFragment,
    setCurrentPolicy
  }
})
