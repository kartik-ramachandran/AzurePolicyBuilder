import axios from 'axios'
import type { PolicyTemplate, PolicyFragment, ValidationResult, ArmTemplate } from '@/types/policy'

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

export const templateService = {
  async getTemplates(): Promise<PolicyTemplate[]> {
    const { data } = await api.get<PolicyTemplate[]>('/templates')
    return data
  },

  async getTemplateById(id: string): Promise<PolicyTemplate> {
    const { data } = await api.get<PolicyTemplate>(`/templates/${id}`)
    return data
  },

  async getTemplatesByCategory(category: string): Promise<PolicyTemplate[]> {
    const { data } = await api.get<PolicyTemplate[]>(`/templates/category/${category}`)
    return data
  }
}

export const validationService = {
  async validatePolicy(xml: string): Promise<ValidationResult> {
    const { data } = await api.post<ValidationResult>('/validate', { xml })
    return data
  },

  async validateExpression(expression: string): Promise<ValidationResult> {
    const { data } = await api.post<ValidationResult>('/validate/expression', { expression })
    return data
  }
}

export const fragmentService = {
  async getFragments(): Promise<PolicyFragment[]> {
    const { data } = await api.get<PolicyFragment[]>('/fragments')
    return data
  },

  async getFragmentById(id: string): Promise<PolicyFragment> {
    const { data } = await api.get<PolicyFragment>(`/fragments/${id}`)
    return data
  },

  async createFragment(fragment: Omit<PolicyFragment, 'id' | 'createdAt' | 'updatedAt' | 'usageCount'>): Promise<PolicyFragment> {
    const { data } = await api.post<PolicyFragment>('/fragments', fragment)
    return data
  },

  async updateFragment(id: string, fragment: Partial<PolicyFragment>): Promise<PolicyFragment> {
    const { data } = await api.put<PolicyFragment>(`/fragments/${id}`, fragment)
    return data
  },

  async deleteFragment(id: string): Promise<void> {
    await api.delete(`/fragments/${id}`)
  }
}

export const exportService = {
  async exportToArm(xml: string, scope: string): Promise<ArmTemplate> {
    const { data } = await api.post<ArmTemplate>('/export/arm', { xml, scope })
    return data
  },

  async exportToBicep(xml: string, scope: string): Promise<string> {
    const { data } = await api.post<{ bicep: string }>('/export/bicep', { xml, scope })
    return data.bicep
  }
}
