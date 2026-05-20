<template>
  <div class="product-editor max-w-4xl">
    <div class="mb-6">
      <h2 class="text-2xl font-bold text-gray-900 mb-2">Product Configuration</h2>
      <p class="text-gray-600">Configure products to group APIs and manage subscriptions.</p>
    </div>
    
    <div class="space-y-6">
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <div class="w-8 h-8 bg-indigo-100 rounded-lg flex items-center justify-center">
            <svg class="w-4 h-4 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
            </svg>
          </div>
          Basic Information
        </h3>
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-semibold text-gray-900 mb-2">Name *</label>
            <input v-model="localProduct.name" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="my-product" @input="emitUpdate" />
          </div>
          <div>
            <label class="block text-sm font-semibold text-gray-900 mb-2">Display Name *</label>
            <input v-model="localProduct.displayName" type="text" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="My Product" @input="emitUpdate" />
          </div>
          <div class="col-span-2">
            <label class="block text-sm font-semibold text-gray-900 mb-2">Description</label>
            <textarea v-model="localProduct.description" rows="3" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" placeholder="Product description..." @input="emitUpdate"></textarea>
          </div>
        </div>
        <div class="mt-6 space-y-3">
          <div class="flex items-center gap-2">
            <input v-model="localProduct.subscriptionRequired" type="checkbox" class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500" @change="emitUpdate" />
            <label class="text-sm font-medium text-gray-700">Subscription Required</label>
          </div>
          <div class="flex items-center gap-2">
            <input v-model="localProduct.approvalRequired" type="checkbox" class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500" @change="emitUpdate" />
            <label class="text-sm font-medium text-gray-700">Approval Required</label>
          </div>
          <div class="flex items-center gap-2">
            <input v-model="localProduct.published" type="checkbox" class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500" @change="emitUpdate" />
            <label class="text-sm font-medium text-gray-700">Published</label>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <div class="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center">
            <svg class="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 9l3 3-3 3m5 0h3M5 20h14a2 2 0 002-2V6a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
          Associated APIs
        </h3>
        <div v-if="apis.length === 0" class="text-sm text-gray-500 bg-gray-50 p-4 rounded-lg">No APIs available. Add APIs first.</div>
        <div v-else class="space-y-2">
          <div v-for="api in apis" :key="api.name" class="flex items-center gap-2 p-3 hover:bg-gray-50 rounded-lg transition-colors">
            <input
              type="checkbox"
              :value="api.name"
              :checked="localProduct.apis.includes(api.name)"
              @change="toggleApi(api.name)"
              class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
            />
            <label class="text-sm font-medium text-gray-700">{{ api.displayName || api.name }}</label>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  product: any
  apis: any[]
}>()

const emit = defineEmits<{
  update: [data: any]
}>()

const localProduct = ref({ ...props.product, apis: props.product.apis || [] })

watch(() => props.product, (newProduct) => {
  localProduct.value = { ...newProduct, apis: newProduct.apis || [] }
}, { deep: true })

function toggleApi(apiName: string) {
  const index = localProduct.value.apis.indexOf(apiName)
  if (index > -1) {
    localProduct.value.apis.splice(index, 1)
  } else {
    localProduct.value.apis.push(apiName)
  }
  emitUpdate()
}

function emitUpdate() {
  emit('update', { ...localProduct.value })
}
</script>