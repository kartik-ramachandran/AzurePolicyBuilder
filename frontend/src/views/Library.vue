<template>
  <div class="library library-page">
    <div class="library-hero">
      <div>
        <p class="library-eyebrow">Shared workspace</p>
        <h1 class="library-title">Reusable Library</h1>
        <p class="library-subtitle">Keep APIM building blocks organised and ready for every project.</p>
      </div>
      <button @click="createNew" class="library-create">
        <svg class="w-5 h-5 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
        </svg>
        New {{ activeTabLabel }}
      </button>
    </div>

    <!-- Tabs -->
    <div class="library-tabs" role="tablist" aria-label="Library artifact types">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        @click="activeTab = tab.id"
        role="tab"
        :aria-selected="activeTab === tab.id"
        :class="[
          'library-tab',
          activeTab === tab.id ? 'library-tab--active' : ''
        ]"
      >
        {{ tab.label }}
        <span class="library-tab-count">{{ counts[tab.id] }}</span>
      </button>
    </div>

    <div v-if="loading" class="text-center py-12">
      <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
    </div>

    <!-- Fragments -->
    <div v-else-if="activeTab === 'fragments'">
      <div v-if="fragments.length === 0" class="card text-center py-12 text-gray-500">No fragments yet.</div>
      <div v-else class="grid grid-cols-1 gap-4">
        <div v-for="fragment in fragments" :key="fragment.id" class="card hover:shadow-md transition-shadow">
          <div class="flex justify-between items-start">
            <div class="flex-1">
              <h3 class="text-lg font-semibold text-gray-900 mb-1">{{ fragment.name }}</h3>
              <p class="text-gray-600 text-sm mb-3">{{ fragment.description }}</p>
              <div class="flex items-center space-x-4 text-sm text-gray-500">
                <span>Version {{ fragment.version }}</span>
                <span>Used {{ fragment.usageCount }} times</span>
              </div>
            </div>
            <div class="flex space-x-2 ml-4">
              <button @click="editFragment(fragment)" class="p-2 text-gray-600 hover:text-primary-600 hover:bg-primary-50 rounded" title="Edit">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
              </button>
              <button @click="removeFragment(fragment)" class="p-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded" title="Delete">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Products -->
    <div v-else-if="activeTab === 'products'">
      <div v-if="products.length === 0" class="card text-center py-12 text-gray-500">No reusable products yet.</div>
      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div v-for="product in products" :key="product.name" class="card hover:shadow-md transition-shadow">
          <div class="flex justify-between items-start">
            <div class="flex-1">
              <h3 class="text-lg font-semibold text-gray-900 mb-1">{{ product.displayName || product.name }}</h3>
              <p class="text-xs text-gray-400 mb-2">{{ product.name }}</p>
              <p class="text-gray-600 text-sm mb-3">{{ product.description }}</p>
              <div class="flex flex-wrap gap-2 text-xs">
                <span :class="product.published ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'" class="px-2 py-1 rounded-full">
                  {{ product.published ? 'Published' : 'Draft' }}
                </span>
                <span v-if="product.subscriptionRequired" class="px-2 py-1 rounded-full bg-blue-100 text-blue-700">Subscription</span>
                <span v-if="product.approvalRequired" class="px-2 py-1 rounded-full bg-amber-100 text-amber-700">Approval</span>
              </div>
            </div>
            <div class="flex space-x-2 ml-4">
              <button @click="editProduct(product)" class="p-2 text-gray-600 hover:text-primary-600 hover:bg-primary-50 rounded" title="Edit">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
              </button>
              <button @click="removeProduct(product)" class="p-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded" title="Delete">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Named Values -->
    <div v-else>
      <div v-if="namedValues.length === 0" class="card text-center py-12 text-gray-500">No reusable named values yet.</div>
      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div v-for="namedValue in namedValues" :key="namedValue.name" class="card hover:shadow-md transition-shadow">
          <div class="flex justify-between items-start">
            <div class="flex-1">
              <h3 class="text-lg font-semibold text-gray-900 mb-1">{{ namedValue.displayName || namedValue.name }}</h3>
              <p class="text-xs text-gray-400 mb-2">{{ namedValue.name }}</p>
              <p class="text-gray-700 text-sm font-mono break-all">
                {{ namedValue.secret ? '••••••••' : namedValue.value }}
              </p>
              <span v-if="namedValue.secret" class="mt-2 inline-block px-2 py-1 rounded-full bg-rose-100 text-rose-700 text-xs">Secret</span>
            </div>
            <div class="flex space-x-2 ml-4">
              <button @click="editNamedValue(namedValue)" class="p-2 text-gray-600 hover:text-primary-600 hover:bg-primary-50 rounded" title="Edit">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
              </button>
              <button @click="removeNamedValue(namedValue)" class="p-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded" title="Delete">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Fragment Editor Modal -->
    <div v-if="showFragmentModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div class="bg-white rounded-lg p-6 max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <h3 class="text-xl font-bold mb-4">{{ editingFragment ? 'Edit Fragment' : 'New Fragment' }}</h3>
        <div class="space-y-4">
          <div v-if="!editingFragment">
            <label class="block text-sm font-medium text-gray-700 mb-1">Start from policy template (optional)</label>
            <select v-model="templateStartId" class="input" @change="applyTemplateToFragment(templateStartId)">
              <option value="">— Write my own —</option>
              <option v-for="template in policyStore.templates" :key="template.id" :value="template.id">{{ template.name }}</option>
            </select>
          </div>
          <input v-model="fragmentForm.name" type="text" class="input" placeholder="Fragment name" />
          <input v-model="fragmentForm.description" type="text" class="input" placeholder="Description" />
          <div class="border border-gray-300 rounded-lg overflow-hidden" style="height: 300px;">
            <MonacoEditor v-model="fragmentForm.xml" language="xml" />
          </div>
          <div class="flex space-x-2">
            <button @click="saveFragment" class="btn btn-primary flex-1">{{ editingFragment ? 'Save Changes' : 'Create' }}</button>
            <button @click="showFragmentModal = false" class="btn btn-secondary flex-1">Cancel</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Product Editor Modal -->
    <div v-if="showProductModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div class="bg-white rounded-lg p-6 max-w-lg w-full">
        <h3 class="text-xl font-bold mb-4">{{ editingKey ? 'Edit Product' : 'New Product' }}</h3>
        <div class="space-y-4">
          <input v-model="productForm.name" :disabled="!!editingKey" type="text" class="input disabled:bg-gray-100" placeholder="product-name (id)" />
          <input v-model="productForm.displayName" type="text" class="input" placeholder="Display name" />
          <textarea v-model="productForm.description" rows="3" class="input" placeholder="Description"></textarea>
          <div class="flex flex-wrap gap-4">
            <label class="flex items-center gap-2 text-sm"><input v-model="productForm.published" type="checkbox" /> Published</label>
            <label class="flex items-center gap-2 text-sm"><input v-model="productForm.subscriptionRequired" type="checkbox" /> Subscription required</label>
            <label class="flex items-center gap-2 text-sm"><input v-model="productForm.approvalRequired" type="checkbox" /> Approval required</label>
          </div>
          <div class="flex space-x-2">
            <button @click="saveProduct" class="btn btn-primary flex-1">Save</button>
            <button @click="showProductModal = false" class="btn btn-secondary flex-1">Cancel</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Named Value Editor Modal -->
    <div v-if="showNamedValueModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div class="bg-white rounded-lg p-6 max-w-lg w-full">
        <h3 class="text-xl font-bold mb-4">{{ editingKey ? 'Edit Named Value' : 'New Named Value' }}</h3>
        <div class="space-y-4">
          <input v-model="namedValueForm.name" :disabled="!!editingKey" type="text" class="input disabled:bg-gray-100" placeholder="name (id)" />
          <input v-model="namedValueForm.displayName" type="text" class="input" placeholder="Display name" />
          <input v-model="namedValueForm.value" :type="namedValueForm.secret ? 'password' : 'text'" class="input" placeholder="Value or Key Vault reference" />
          <label class="flex items-center gap-2 text-sm"><input v-model="namedValueForm.secret" type="checkbox" /> Secret value</label>
          <div class="flex space-x-2">
            <button @click="saveNamedValue" class="btn btn-primary flex-1">Save</button>
            <button @click="showNamedValueModal = false" class="btn btn-secondary flex-1">Cancel</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { usePolicyStore } from '@/stores/policy'
import { libraryService, type LibraryProduct, type LibraryNamedValue } from '@/services/api'
import MonacoEditor from '@/components/MonacoEditor.vue'
import type { PolicyFragment } from '@/types/policy'

type TabId = 'fragments' | 'products' | 'namedValues'

const policyStore = usePolicyStore()

const tabs: Array<{ id: TabId; label: string }> = [
  { id: 'fragments', label: 'Fragments' },
  { id: 'products', label: 'Products' },
  { id: 'namedValues', label: 'Named Values' }
]

const activeTab = ref<TabId>('fragments')
const loading = ref(false)

const fragments = computed(() => policyStore.fragments)
const products = ref<LibraryProduct[]>([])
const namedValues = ref<LibraryNamedValue[]>([])

const counts = computed(() => ({
  fragments: fragments.value.length,
  products: products.value.length,
  namedValues: namedValues.value.length
}))

const activeTabLabel = computed(() => {
  const map: Record<TabId, string> = { fragments: 'Fragment', products: 'Product', namedValues: 'Named Value' }
  return map[activeTab.value]
})

// Fragment modal
const showFragmentModal = ref(false)
const editingFragment = ref<PolicyFragment | null>(null)
const templateStartId = ref('')
const fragmentForm = ref({ name: '', description: '', xml: '' })
const defaultFragmentXml = `<fragment>\n    <!-- Add policy statements here -->\n</fragment>`

// Product / named value modals
const showProductModal = ref(false)
const showNamedValueModal = ref(false)
const editingKey = ref('')
const productForm = ref<LibraryProduct>(blankProduct())
const namedValueForm = ref<LibraryNamedValue>(blankNamedValue())

function blankProduct(): LibraryProduct {
  return { name: '', displayName: '', description: '', subscriptionRequired: true, approvalRequired: false, published: true, apis: [] }
}
function blankNamedValue(): LibraryNamedValue {
  return { name: '', displayName: '', value: '', secret: false, tags: [] }
}

onMounted(async () => {
  loading.value = true
  await Promise.all([policyStore.loadFragments(), policyStore.loadTemplates(), reloadProducts(), reloadNamedValues()])
  loading.value = false
})

async function reloadProducts() {
  try { products.value = await libraryService.getProducts() } catch { products.value = [] }
}
async function reloadNamedValues() {
  try { namedValues.value = await libraryService.getNamedValues() } catch { namedValues.value = [] }
}

function createNew() {
  if (activeTab.value === 'fragments') {
    editingFragment.value = null
    templateStartId.value = ''
    fragmentForm.value = { name: '', description: '', xml: defaultFragmentXml }
    showFragmentModal.value = true
  } else if (activeTab.value === 'products') {
    editingKey.value = ''
    productForm.value = blankProduct()
    showProductModal.value = true
  } else {
    editingKey.value = ''
    namedValueForm.value = blankNamedValue()
    showNamedValueModal.value = true
  }
}

// Fragments
function editFragment(fragment: PolicyFragment) {
  editingFragment.value = fragment
  templateStartId.value = ''
  fragmentForm.value = { name: fragment.name, description: fragment.description, xml: fragment.xml }
  showFragmentModal.value = true
}

function applyTemplateToFragment(id: string) {
  const template = policyStore.templates.find(t => t.id === id)
  if (!template) return
  const indented = template.xml.split('\n').map(line => (line.trim().length ? `    ${line}` : line)).join('\n')
  fragmentForm.value.xml = `<fragment>\n${indented}\n</fragment>`
  if (!fragmentForm.value.name.trim()) fragmentForm.value.name = `${template.id}-fragment`
  if (!fragmentForm.value.description.trim()) fragmentForm.value.description = `Fragment wrapping the "${template.name}" template`
}

async function saveFragment() {
  if (!fragmentForm.value.name.trim()) { alert('Please enter a fragment name'); return }
  try {
    if (editingFragment.value) {
      await policyStore.updateFragment(editingFragment.value.id, {
        name: fragmentForm.value.name,
        description: fragmentForm.value.description,
        xml: fragmentForm.value.xml
      })
    } else {
      await policyStore.createFragment({
        name: fragmentForm.value.name,
        description: fragmentForm.value.description,
        xml: fragmentForm.value.xml,
        version: 1
      })
    }
    showFragmentModal.value = false
  } catch { alert('Failed to save fragment') }
}

async function removeFragment(fragment: PolicyFragment) {
  if (!confirm(`Delete fragment "${fragment.name}"?`)) return
  try { await policyStore.deleteFragment(fragment.id) } catch { alert('Failed to delete fragment') }
}

// Products
function editProduct(product: LibraryProduct) {
  editingKey.value = product.name
  productForm.value = { ...product, apis: [...product.apis] }
  showProductModal.value = true
}

async function saveProduct() {
  if (!productForm.value.name.trim()) { alert('Please enter a product name'); return }
  try {
    await libraryService.saveProduct({ ...productForm.value })
    await reloadProducts()
    showProductModal.value = false
  } catch { alert('Failed to save product') }
}

async function removeProduct(product: LibraryProduct) {
  if (!confirm(`Delete product "${product.displayName || product.name}"?`)) return
  try { await libraryService.deleteProduct(product.name); await reloadProducts() } catch { alert('Failed to delete product') }
}

// Named values
function editNamedValue(namedValue: LibraryNamedValue) {
  editingKey.value = namedValue.name
  namedValueForm.value = { ...namedValue, tags: [...namedValue.tags] }
  showNamedValueModal.value = true
}

async function saveNamedValue() {
  if (!namedValueForm.value.name.trim()) { alert('Please enter a name'); return }
  try {
    await libraryService.saveNamedValue({ ...namedValueForm.value })
    await reloadNamedValues()
    showNamedValueModal.value = false
  } catch { alert('Failed to save named value') }
}

async function removeNamedValue(namedValue: LibraryNamedValue) {
  if (!confirm(`Delete named value "${namedValue.displayName || namedValue.name}"?`)) return
  try { await libraryService.deleteNamedValue(namedValue.name); await reloadNamedValues() } catch { alert('Failed to delete named value') }
}
</script>

<style scoped>
.library-page {
  width: min(100%, 1280px);
  margin: 0 auto;
  padding: 42px 36px 64px;
  color: #0f172a;
}

.library-hero {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 28px;
  margin-bottom: 28px;
}

.library-eyebrow {
  margin: 0 0 8px;
  color: #2563eb;
  font-size: 11px;
  font-weight: 800;
  letter-spacing: .18em;
  text-transform: uppercase;
}

.library-title {
  margin: 0;
  color: #07152f;
  font-size: clamp(30px, 3vw, 42px);
  font-weight: 750;
  letter-spacing: -.045em;
  line-height: 1.05;
}

.library-subtitle {
  max-width: 650px;
  margin: 11px 0 0;
  color: #64748b;
  font-size: 15px;
  line-height: 1.65;
}

.library-create {
  display: inline-flex;
  min-height: 46px;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 0;
  border-radius: 14px;
  padding: 0 19px;
  color: white;
  background: linear-gradient(135deg, #ff9a3d 0%, #ff681e 100%);
  box-shadow: 0 12px 28px rgba(255, 105, 30, .22);
  font-size: 14px;
  font-weight: 750;
  transition: transform .18s ease, box-shadow .18s ease;
}

.library-create:hover {
  transform: translateY(-1px);
  box-shadow: 0 15px 32px rgba(255, 105, 30, .29);
}

.library-tabs {
  display: inline-flex;
  gap: 5px;
  margin-bottom: 26px;
  padding: 5px;
  border: 1px solid #dce8f7;
  border-radius: 16px;
  background: rgba(255, 255, 255, .82);
  box-shadow: 0 10px 28px rgba(30, 64, 175, .07);
  backdrop-filter: blur(14px);
}

.library-tab {
  display: inline-flex;
  min-height: 40px;
  align-items: center;
  gap: 10px;
  border: 0;
  border-radius: 11px;
  padding: 0 15px;
  color: #64748b;
  background: transparent;
  font-size: 13px;
  font-weight: 700;
  transition: color .18s ease, background .18s ease, box-shadow .18s ease;
}

.library-tab:hover {
  color: #1559d6;
  background: #f1f6ff;
}

.library-tab--active {
  color: white;
  background: linear-gradient(135deg, #2f7df4, #1559d6);
  box-shadow: 0 8px 18px rgba(37, 99, 235, .22);
}

.library-tab--active:hover {
  color: white;
  background: linear-gradient(135deg, #2f7df4, #1559d6);
}

.library-tab-count {
  display: grid;
  min-width: 22px;
  height: 22px;
  place-items: center;
  border-radius: 999px;
  padding: 0 6px;
  color: currentColor;
  background: rgba(148, 163, 184, .14);
  font-size: 11px;
  font-weight: 800;
}

.library-tab--active .library-tab-count {
  background: rgba(255, 255, 255, .18);
}

.library-page .card {
  border: 1px solid #dbe8f7;
  border-radius: 18px;
  background: rgba(255, 255, 255, .88);
  box-shadow: 0 10px 30px rgba(15, 50, 100, .06);
  backdrop-filter: blur(12px);
}

@media (max-width: 760px) {
  .library-page {
    padding: 26px 18px 48px;
  }

  .library-hero {
    align-items: stretch;
    flex-direction: column;
  }

  .library-create {
    align-self: flex-start;
  }

  .library-tabs {
    display: flex;
    width: 100%;
    overflow-x: auto;
  }

  .library-tab {
    flex: 1 0 auto;
  }
}
</style>
