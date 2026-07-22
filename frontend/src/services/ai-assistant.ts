import axios from 'axios'

export interface AIMessage {
  role: 'user' | 'assistant' | 'system'
  content: string
}

export interface PolicyGenerationRequest {
  description: string
  policyType: 'inbound' | 'backend' | 'outbound' | 'on-error'
  apiContext?: string
  securityLevel?: 'low' | 'medium' | 'high'
  additionalRequirements?: string
}

export interface PolicyGenerationResponse {
  policyXml: string
  explanation: string
  prerequisites: string[]
  securityNotes: string[]
}

export type AIProvider = 'openai' | 'azure'

export interface AIConfiguration {
  provider: AIProvider
  apiKey: string
  /** OpenAI model name, or the Azure OpenAI / AI Foundry deployment name */
  model: string
  /** OpenAI-compatible base URL, or the Azure resource endpoint (https://<resource>.openai.azure.com or https://<resource>.services.ai.azure.com) */
  baseURL: string
  /** Azure only: api-version query parameter */
  apiVersion: string
}

const DEFAULT_CONFIG: AIConfiguration = {
  provider: 'openai',
  apiKey: '',
  model: 'gpt-4',
  baseURL: 'https://api.openai.com/v1',
  apiVersion: '2024-10-21'
}

class AIAssistantService {
  private config: AIConfiguration = { ...DEFAULT_CONFIG }

  constructor() {
    this.loadConfiguration()
  }

  private loadConfiguration() {
    const config = localStorage.getItem('apim-ai-config')
    if (config) {
      try {
        const saved = JSON.parse(config)
        this.config = {
          provider: saved.provider === 'azure' ? 'azure' : 'openai',
          apiKey: saved.apiKey || '',
          model: saved.model || DEFAULT_CONFIG.model,
          baseURL: saved.baseURL || DEFAULT_CONFIG.baseURL,
          apiVersion: saved.apiVersion || DEFAULT_CONFIG.apiVersion
        }
      } catch (error) {
        console.warn('Failed to load AI configuration:', error)
      }
    }
  }

  public saveConfiguration(config: Partial<AIConfiguration>) {
    this.config = { ...this.config, ...config }
    localStorage.setItem('apim-ai-config', JSON.stringify(this.config))
  }

  public getConfiguration(): AIConfiguration {
    return { ...this.config }
  }

  public isConfigured(): boolean {
    return !!this.config.apiKey
  }

  private async chatCompletion(messages: AIMessage[], temperature: number, maxTokens: number): Promise<string> {
    const { provider, apiKey, model, baseURL, apiVersion } = this.config
    const trimmedBase = baseURL.replace(/\/+$/, '')

    const url =
      provider === 'azure'
        ? `${trimmedBase}/openai/deployments/${model}/chat/completions?api-version=${apiVersion}`
        : `${trimmedBase}/chat/completions`

    const headers: Record<string, string> = { 'Content-Type': 'application/json' }
    if (provider === 'azure') {
      headers['api-key'] = apiKey
    } else {
      headers['Authorization'] = `Bearer ${apiKey}`
    }

    // Azure infers the model from the deployment in the URL; sending it anyway is harmless
    const response = await axios.post(
      url,
      { model, messages, temperature, max_tokens: maxTokens },
      { headers }
    )

    return response.data.choices[0].message.content
  }

  private buildPolicyPrompt(request: PolicyGenerationRequest): string {
    return `Generate an Azure API Management policy fragment based on the following requirements:

Description: ${request.description}
Policy Type: ${request.policyType}
API Context: ${request.apiContext || 'General purpose API'}
Security Level: ${request.securityLevel || 'medium'}
Additional Requirements: ${request.additionalRequirements || 'None'}

Please provide:
1. A complete XML policy fragment that follows Azure APIM policy syntax
2. A brief explanation of what the policy does
3. Any prerequisites or dependencies
4. Security considerations

Format your response as:
POLICY_FRAGMENT:
[XML policy here - make sure it's valid APIM policy XML]

EXPLANATION:
[Brief explanation of the policy]

PREREQUISITES:
- [Prerequisite 1]
- [Prerequisite 2]

SECURITY_NOTES:
- [Security note 1]
- [Security note 2]`
  }

  public async generatePolicy(request: PolicyGenerationRequest): Promise<PolicyGenerationResponse> {
    if (!this.isConfigured()) {
      throw new Error('AI Assistant is not configured. Please provide an API key in settings.')
    }

    const messages: AIMessage[] = [
      {
        role: 'system',
        content: 'You are an expert in Azure API Management policies. You help developers create secure, efficient, and best-practice APIM policy fragments. Always provide valid XML that follows APIM policy syntax.'
      },
      {
        role: 'user',
        content: this.buildPolicyPrompt(request)
      }
    ]

    try {
      // Lower temperature for more consistent code generation
      const content = await this.chatCompletion(messages, 0.3, 2000)
      return this.parseResponse(content)
    } catch (error: any) {
      if (error.response?.status === 401) {
        throw new Error('Invalid API key. Please check your settings.')
      } else if (error.response?.status === 429) {
        throw new Error('Rate limit exceeded. Please try again later.')
      } else {
        throw new Error(`AI service error: ${error.message}`)
      }
    }
  }

  private parseResponse(content: string): PolicyGenerationResponse {
    // Extract policy XML
    const policyMatch = content.match(/POLICY_FRAGMENT:([\s\S]*?)(?=EXPLANATION:|$)/i)
    const policyXml = policyMatch ? policyMatch[1].trim() : ''

    // Extract explanation
    const explanationMatch = content.match(/EXPLANATION:([\s\S]*?)(?=PREREQUISITES:|$)/i)
    const explanation = explanationMatch ? explanationMatch[1].trim() : ''

    // Extract prerequisites
    const prerequisitesMatch = content.match(/PREREQUISITES:([\s\S]*?)(?=SECURITY_NOTES:|$)/i)
    const prerequisitesText = prerequisitesMatch ? prerequisitesMatch[1].trim() : ''
    const prerequisites = prerequisitesText
      .split('\n')
      .map(line => line.replace(/^[-*]\s*/, '').trim())
      .filter(line => line.length > 0)

    // Extract security notes
    const securityMatch = content.match(/SECURITY_NOTES:([\s\S]*?)$/i)
    const securityText = securityMatch ? securityMatch[1].trim() : ''
    const securityNotes = securityText
      .split('\n')
      .map(line => line.replace(/^[-*]\s*/, '').trim())
      .filter(line => line.length > 0)

    return {
      policyXml,
      explanation,
      prerequisites,
      securityNotes
    }
  }

  public async explainPolicy(policyXml: string): Promise<string> {
    if (!this.isConfigured()) {
      throw new Error('AI Assistant is not configured. Please provide an API key in settings.')
    }

    const messages: AIMessage[] = [
      {
        role: 'system',
        content: 'You are an expert in Azure API Management policies. You help developers understand APIM policies by providing clear, concise explanations.'
      },
      {
        role: 'user',
        content: `Please explain the following Azure APIM policy in simple terms:

${policyXml}

Provide:
1. What this policy does
2. When to use it
3. Any potential issues or limitations`
      }
    ]

    try {
      return await this.chatCompletion(messages, 0.5, 1000)
    } catch (error: any) {
      throw new Error(`AI service error: ${error.message}`)
    }
  }

  public async improvePolicy(policyXml: string, requirements: string): Promise<string> {
    if (!this.isConfigured()) {
      throw new Error('AI Assistant is not configured. Please provide an API key in settings.')
    }

    const messages: AIMessage[] = [
      {
        role: 'system',
        content: 'You are an expert in Azure API Management policies. You help developers improve their APIM policies for better security, performance, and best practices.'
      },
      {
        role: 'user',
        content: `Please improve the following Azure APIM policy based on these requirements:

Current Policy:
${policyXml}

Requirements:
${requirements}

Provide the improved policy XML with explanations of the changes.`
      }
    ]

    try {
      return await this.chatCompletion(messages, 0.3, 2000)
    } catch (error: any) {
      throw new Error(`AI service error: ${error.message}`)
    }
  }
}

export const aiAssistant = new AIAssistantService()
