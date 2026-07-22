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
  },

  async incrementUsage(id: string): Promise<PolicyFragment> {
    const { data } = await api.post<PolicyFragment>(`/fragments/${id}/increment-usage`)
    return data
  }
}

export const projectService = {
  async generateProject(project: unknown): Promise<Record<string, string>> {
    const { data } = await api.post<Record<string, string>>('/project/generate', project)
    return data
  },

  async downloadProject(project: unknown): Promise<Blob> {
    const { data } = await api.post('/project/download', project, { responseType: 'blob' })
    return data
  }
}

export interface LibraryProduct {
  name: string
  displayName: string
  description: string
  subscriptionRequired: boolean
  approvalRequired: boolean
  published: boolean
  apis: string[]
}

export interface LibraryNamedValue {
  name: string
  displayName: string
  value: string
  secret: boolean
  tags: string[]
}

export const libraryService = {
  async getProducts(): Promise<LibraryProduct[]> {
    const { data } = await api.get<LibraryProduct[]>('/library/products')
    return data
  },

  async saveProduct(product: LibraryProduct): Promise<LibraryProduct> {
    const { data } = await api.post<LibraryProduct>('/library/products', product)
    return data
  },

  async deleteProduct(name: string): Promise<void> {
    await api.delete(`/library/products/${encodeURIComponent(name)}`)
  },

  async getNamedValues(): Promise<LibraryNamedValue[]> {
    const { data } = await api.get<LibraryNamedValue[]>('/library/named-values')
    return data
  },

  async saveNamedValue(namedValue: LibraryNamedValue): Promise<LibraryNamedValue> {
    const { data } = await api.post<LibraryNamedValue>('/library/named-values', namedValue)
    return data
  },

  async deleteNamedValue(name: string): Promise<void> {
    await api.delete(`/library/named-values/${encodeURIComponent(name)}`)
  }
}

export interface AzureApimRequest {
  subscriptionId: string
  resourceGroup: string
  serviceName: string
  apiNames?: string[]
}

export interface ApimInventory {
  serviceName: string
  apis: Array<{ name: string; displayName: string; path: string }>
  productCount: number
  namedValueCount: number
  policyFragmentCount: number
  backendCount: number
}

export const importService = {
  async importZip(file: File): Promise<any> {
    const form = new FormData()
    form.append('file', file)
    const { data } = await api.post('/project/import/zip', form, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    return data
  },

  async importFolder(path: string): Promise<any> {
    const { data } = await api.post('/project/import/folder', { path })
    return data
  },

  async importGitHub(request: { repoUrl: string; branch?: string; path?: string; pat?: string }): Promise<any> {
    const { data } = await api.post('/project/import/github', request)
    return data
  },

  async azureInventory(request: AzureApimRequest): Promise<ApimInventory> {
    const { data } = await api.post<ApimInventory>('/azure/apim/inventory', request)
    return data
  },

  async azureImport(request: AzureApimRequest): Promise<any> {
    const { data } = await api.post('/azure/apim/import', request)
    return data
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
