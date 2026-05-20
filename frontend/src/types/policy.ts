export interface PolicyTemplate {
  id: string
  name: string
  description: string
  category: 'inbound' | 'backend' | 'outbound' | 'on-error'
  xml: string
  parameters?: PolicyParameter[]
}

export interface PolicyParameter {
  name: string
  type: 'string' | 'number' | 'boolean' | 'expression'
  description: string
  defaultValue?: string
  required: boolean
}

export interface PolicyFragment {
  id: string
  name: string
  description: string
  xml: string
  version: number
  createdAt: string
  updatedAt: string
  usageCount: number
}

export interface ValidationResult {
  isValid: boolean
  errors: ValidationError[]
  warnings: ValidationWarning[]
}

export interface ValidationError {
  line: number
  column: number
  message: string
  severity: 'error'
}

export interface ValidationWarning {
  line: number
  column: number
  message: string
  severity: 'warning'
}

export interface PolicyDocument {
  id: string
  name: string
  xml: string
  scope: 'api' | 'operation' | 'product' | 'global'
  lastModified: string
}

export interface ArmTemplate {
  schema: string
  contentVersion: string
  parameters: Record<string, unknown>
  variables: Record<string, unknown>
  resources: unknown[]
}
