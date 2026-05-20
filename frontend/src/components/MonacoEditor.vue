<template>
  <div ref="editorContainer" class="monaco-editor-container w-full h-full" />
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import * as monaco from 'monaco-editor'

interface Props {
  modelValue: string
  language?: string
  theme?: string
  readonly?: boolean
  options?: monaco.editor.IStandaloneEditorConstructionOptions
}

const props = withDefaults(defineProps<Props>(), {
  language: 'xml',
  theme: 'vs-light',
  readonly: false,
  options: () => ({})
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
  (e: 'change', value: string): void
}>()

const editorContainer = ref<HTMLElement | null>(null)
let editor: monaco.editor.IStandaloneCodeEditor | null = null

onMounted(() => {
  if (!editorContainer.value) return

  // Configure XML language
  monaco.languages.register({ id: 'xml' })
  
  // Create editor
  editor = monaco.editor.create(editorContainer.value, {
    value: props.modelValue,
    language: props.language,
    theme: props.theme,
    readOnly: props.readonly,
    automaticLayout: true,
    minimap: { enabled: true },
    scrollBeyondLastLine: false,
    fontSize: 14,
    lineNumbers: 'on',
    folding: true,
    formatOnPaste: true,
    formatOnType: true,
    ...props.options
  })

  // Listen to content changes
  editor.onDidChangeModelContent(() => {
    if (editor) {
      const value = editor.getValue()
      emit('update:modelValue', value)
      emit('change', value)
    }
  })
})

// Watch for external changes
watch(() => props.modelValue, (newValue) => {
  if (editor && editor.getValue() !== newValue) {
    editor.setValue(newValue)
  }
})

watch(() => props.language, (newLanguage) => {
  if (editor) {
    const model = editor.getModel()
    if (model) {
      monaco.editor.setModelLanguage(model, newLanguage)
    }
  }
})

onUnmounted(() => {
  editor?.dispose()
})

// Expose methods
defineExpose({
  focus: () => editor?.focus(),
  getEditor: () => editor
})
</script>

<style scoped>
.monaco-editor-container {
  min-height: 400px;
}
</style>
