<template>
  <div class="project-builder min-h-[calc(100vh-7rem)] overflow-hidden rounded-[1.5rem] border border-sky-100 bg-white text-slate-950 shadow-xl shadow-sky-100/80">
    <div class="relative isolate min-h-[calc(100vh-7rem)] bg-gradient-to-br from-white via-sky-50 to-blue-100">
      <div class="absolute inset-0 -z-10 opacity-60 [background-image:linear-gradient(rgba(14,165,233,.12)_1px,transparent_1px),linear-gradient(90deg,rgba(14,165,233,.12)_1px,transparent_1px)] [background-size:38px_38px]"></div>

      <header class="border-b border-sky-100 bg-white/90 px-5 py-5 shadow-sm backdrop-blur-xl sm:px-8">
        <div class="flex flex-col gap-5 xl:flex-row xl:items-center xl:justify-between">
          <div class="flex items-center gap-4">
            <div class="grid h-14 w-14 place-items-center rounded-2xl bg-gradient-to-br from-sky-300 to-blue-600 text-white shadow-lg shadow-sky-300/40">
              <svg class="h-8 w-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 7h16M4 12h16M4 17h10" />
              </svg>
            </div>
            <div>
              <p class="text-xs font-black uppercase tracking-[0.35em] text-sky-500">APIM Import Console</p>
              <h1 class="text-3xl font-black text-slate-950 sm:text-4xl">{{ projectName }}</h1>
            </div>
          </div>

          <div class="grid grid-cols-3 gap-3 text-center sm:flex">
            <div class="rounded-2xl border border-sky-100 bg-sky-50 px-5 py-3 shadow-sm">
              <div class="text-2xl font-black text-blue-700">{{ discoveredApis.length }}</div>
              <div class="text-xs font-semibold text-slate-500">Detected</div>
            </div>
            <div class="rounded-2xl border border-sky-100 bg-blue-50 px-5 py-3 shadow-sm">
              <div class="text-2xl font-black text-blue-700">{{ selectedDiscoveryCount }}</div>
              <div class="text-xs font-semibold text-slate-500">Selected</div>
            </div>
            <div class="rounded-2xl border border-sky-100 bg-cyan-50 px-5 py-3 shadow-sm">
              <div class="text-2xl font-black text-blue-700">{{ selectedOperationCount }}</div>
              <div class="text-xs font-semibold text-slate-500">Selected Ops</div>
            </div>
          </div>
        </div>
      </header>

      <main class="grid min-h-[calc(100vh-14rem)] grid-cols-1 lg:grid-cols-[23rem_minmax(0,1fr)]">
        <aside class="border-b border-sky-100 bg-white/65 p-5 backdrop-blur-xl lg:border-b-0 lg:border-r lg:p-6">
          <div class="space-y-5">
            <section class="rounded-3xl border border-sky-100 bg-white p-5 shadow-lg shadow-sky-100/80">
              <div class="mb-4 flex items-center justify-between">
                <div>
                  <p class="text-xs font-black uppercase tracking-[0.25em] text-blue-500">Source</p>
                  <h2 class="text-lg font-black text-slate-950">Swagger Intake</h2>
                </div>
                <span class="rounded-full bg-cyan-100 px-3 py-1 text-xs font-black text-cyan-700">JSON</span>
              </div>

              <label
                class="group flex cursor-pointer flex-col items-center justify-center gap-3 rounded-2xl border border-dashed border-sky-300 bg-sky-50 px-4 py-7 text-center hover:border-blue-500 hover:bg-blue-50"
                @dragover.prevent
                @drop.prevent="handleDrop"
              >
                <input class="hidden" type="file" accept=".json,application/json" multiple @change="handleFileUpload" />
                <div class="grid h-14 w-14 place-items-center rounded-2xl bg-gradient-to-br from-sky-300 to-blue-500 text-white shadow-lg shadow-sky-300/40">
                  <svg class="h-7 w-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 16V4m0 0 4 4m-4-4-4 4M4 20h16" />
                  </svg>
                </div>
                <span class="text-sm font-black text-slate-950">Drop or select swagger JSON</span>
                <span class="text-xs font-medium text-slate-500">{{ sourceName || 'OpenAPI 3.x or Swagger 2.0. Multiple files supported.' }}</span>
              </label>

              <textarea
                v-model="rawSpec"
                class="mt-4 h-36 w-full resize-none rounded-2xl border border-sky-100 bg-white px-4 py-3 font-mono text-xs text-slate-700 outline-none ring-sky-300/30 placeholder:text-slate-400 focus:ring-4"
                placeholder="{ &quot;openapi&quot;: &quot;3.0.0&quot;, &quot;paths&quot;: { ... } }"
              ></textarea>

              <div class="mt-4 grid grid-cols-2 gap-3">
                <button class="rounded-2xl bg-blue-600 px-4 py-3 text-sm font-black text-white shadow-lg shadow-blue-200 hover:bg-blue-700" @click="analyzeSpec">
                  Analyze
                </button>
                <button class="rounded-2xl border border-sky-200 bg-white px-4 py-3 text-sm font-black text-blue-700 hover:bg-sky-50" @click="loadSample">
                  Use Sample
                </button>
              </div>

              <p v-if="parseError" class="mt-4 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm font-semibold text-rose-700">{{ parseError }}</p>
            </section>

            <section class="rounded-3xl border border-sky-100 bg-white p-5 shadow-lg shadow-sky-100/70">
              <p class="text-xs font-black uppercase tracking-[0.25em] text-blue-500">Package</p>
              <input v-model="projectName" class="mt-3 w-full rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-sm font-black text-slate-950 outline-none ring-sky-300/30 focus:ring-4" />
              <textarea v-model="projectDescription" rows="3" class="mt-3 w-full resize-none rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-sm text-slate-700 outline-none ring-sky-300/30 focus:ring-4"></textarea>
              <p class="mt-3 text-xs font-semibold text-slate-500">{{ draftStatus }}</p>
            </section>
          </div>
        </aside>

        <section class="overflow-y-auto p-5 sm:p-8">
          <div v-if="!hasAnalyzed" class="mx-auto grid max-w-6xl gap-8 py-6 lg:grid-cols-[1.05fr_.95fr] lg:items-center">
            <div>
              <p class="mb-4 inline-flex rounded-full border border-sky-200 bg-white px-4 py-2 text-sm font-black text-blue-700 shadow-sm">Swagger in. APIM package out.</p>
              <h2 class="max-w-4xl text-5xl font-black leading-none text-slate-950 sm:text-7xl">
                Confirm exactly what lands in APIM.
              </h2>
              <p class="mt-6 max-w-2xl text-lg leading-8 text-slate-700">
                Read one or many swagger/OpenAPI JSON files, extract APIs, paths, operations, service URL, schema count, then select the exact APIs and operations that should move into APIM.
              </p>
              <div class="mt-8 flex flex-wrap gap-3">
                <button class="rounded-2xl bg-blue-600 px-5 py-3 font-black text-white shadow-lg shadow-blue-200 hover:bg-blue-700" @click="triggerFilePicker">
                  Import JSON
                </button>
                <button class="rounded-2xl border border-sky-200 bg-white px-5 py-3 font-black text-blue-700 shadow-sm hover:bg-sky-50" @click="loadSample">
                  Preview Sample
                </button>
              </div>
            </div>

            <div class="rounded-[2rem] border border-sky-100 bg-white p-5 shadow-2xl shadow-sky-100">
              <div class="rounded-[1.5rem] bg-gradient-to-br from-sky-50 to-blue-50 p-5">
                <div class="mb-5 flex items-center justify-between">
                  <span class="text-sm font-black text-slate-950">Import pipeline</span>
                  <span class="rounded-full bg-blue-600 px-3 py-1 text-xs font-black text-white">LIVE</span>
                </div>
                <div class="space-y-3">
                  <div v-for="step in pipelineSteps" :key="step" class="flex items-center gap-3 rounded-2xl border border-sky-100 bg-white px-4 py-4 shadow-sm">
                    <span class="grid h-9 w-9 place-items-center rounded-xl bg-sky-300 text-sm font-black text-blue-950">{{ step.charAt(0) }}</span>
                    <span class="font-black text-slate-800">{{ step }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-else-if="builderMode === 'review'" class="mx-auto max-w-7xl space-y-6">
            <div class="flex flex-col gap-4 2xl:flex-row 2xl:items-end 2xl:justify-between">
              <div class="min-w-0">
                <p class="text-xs font-black uppercase tracking-[0.35em] text-blue-500">Confirmation</p>
                <h2 class="mt-2 text-4xl font-black text-slate-950">Choose APIs to import</h2>
                <p class="mt-3 max-w-3xl text-slate-600">{{ discoverySummary }}</p>
              </div>
              <div class="flex shrink-0 flex-col gap-3 sm:flex-row sm:flex-nowrap sm:items-center">
                <button class="whitespace-nowrap rounded-2xl border border-sky-200 bg-white px-5 py-3 font-black text-blue-700 hover:bg-sky-50" @click="openSelectionModal()">
                  Review API Selection
                </button>
                <button class="whitespace-nowrap rounded-2xl border border-rose-200 bg-white px-5 py-3 font-black text-rose-600 hover:bg-rose-50" @click="resetImports">
                  Reset Imports
                </button>
                <button class="whitespace-nowrap rounded-2xl bg-blue-600 px-6 py-3 font-black text-white shadow-lg shadow-blue-200 disabled:cursor-not-allowed disabled:opacity-40" :disabled="selectedDiscoveryCount === 0" @click="confirmImport">
                  Confirm Import
                </button>
              </div>
            </div>

            <div class="grid gap-4 xl:grid-cols-2">
              <article
                v-for="api in discoveredApis"
                :key="api.id"
                :class="[
                  'rounded-[1.75rem] border p-5 shadow-lg',
                  api.selected ? 'border-blue-200 bg-white shadow-blue-100' : 'border-sky-100 bg-white/75 shadow-sky-100'
                ]"
              >
                <div class="flex items-start justify-between gap-4">
                  <div class="min-w-0">
                    <div class="flex flex-wrap items-center gap-2">
                      <span class="rounded-full bg-blue-600 px-3 py-1 text-xs font-black text-white">{{ api.format }}</span>
                      <span class="rounded-full bg-sky-100 px-3 py-1 text-xs font-black text-blue-700">{{ api.version }}</span>
                      <span class="max-w-[14rem] truncate rounded-full bg-slate-100 px-3 py-1 text-xs font-black text-slate-600">{{ api.sourceName }}</span>
                    </div>
                    <h3 class="mt-4 truncate text-2xl font-black text-slate-950">{{ api.displayName }}</h3>
                    <p class="mt-2 text-sm text-slate-600">{{ api.description || 'No description provided.' }}</p>
                  </div>
                  <label class="relative inline-flex cursor-pointer items-center">
                    <input v-model="api.selected" type="checkbox" class="peer sr-only" @change="toggleApiSelection(api)" />
                    <span class="h-8 w-14 rounded-full bg-slate-200 after:absolute after:left-1 after:top-1 after:h-6 after:w-6 after:rounded-full after:bg-white after:shadow after:transition peer-checked:bg-blue-600 peer-checked:after:translate-x-6"></span>
                  </label>
                </div>

                <div class="mt-5 grid grid-cols-3 gap-3">
                  <div class="rounded-2xl bg-sky-50 p-3">
                    <div class="text-xl font-black text-blue-700">{{ selectedApiOperations(api).length }}/{{ api.operations.length }}</div>
                    <div class="text-xs font-semibold text-slate-500">Ops selected</div>
                  </div>
                  <div class="rounded-2xl bg-sky-50 p-3">
                    <div class="truncate text-xl font-black text-blue-700">{{ api.path || '/' }}</div>
                    <div class="text-xs font-semibold text-slate-500">APIM path</div>
                  </div>
                  <div class="rounded-2xl bg-sky-50 p-3">
                    <div class="text-xl font-black text-blue-700">{{ api.schemaCount }}</div>
                    <div class="text-xs font-semibold text-slate-500">Schemas</div>
                  </div>
                </div>

                <div class="mt-5 rounded-2xl border border-sky-100 bg-sky-50 p-4">
                  <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                    <p class="text-sm font-semibold text-slate-600">
                      {{ selectedApiOperations(api).length }} of {{ api.operations.length }} operations selected for import.
                    </p>
                    <button class="whitespace-nowrap rounded-xl bg-blue-600 px-5 py-2 text-sm font-black text-white shadow-md shadow-blue-100 hover:bg-blue-700" @click="openSelectionModal(api.id)">
                      Select Operations
                    </button>
                  </div>
                </div>
              </article>
            </div>

            <section v-if="project.apis.length" class="rounded-[1.75rem] border border-cyan-200 bg-cyan-50 p-5 shadow-lg shadow-cyan-100">
              <div class="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
                <div>
                  <p class="text-xs font-black uppercase tracking-[0.3em] text-cyan-700">Ready</p>
                  <h3 class="mt-2 text-2xl font-black text-slate-950">{{ project.apis.length }} API{{ project.apis.length === 1 ? '' : 's' }} staged for APIM</h3>
                </div>
                <button class="rounded-2xl bg-cyan-500 px-6 py-3 font-black text-white shadow-lg shadow-cyan-200 hover:bg-cyan-600" @click="generateProject">
                  Generate Package
                </button>
              </div>
            </section>
          </div>

          <div v-else class="mx-auto max-w-7xl space-y-6">
            <div class="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
              <div>
                <p class="text-xs font-black uppercase tracking-[0.35em] text-blue-500">APIM Builder</p>
                <h2 class="mt-2 text-4xl font-black text-slate-950">Configure APIs for APIM</h2>
              </div>
              <div class="flex flex-row items-center gap-3">
                <button class="rounded-2xl border border-sky-200 bg-white px-5 py-3 font-black text-blue-700 hover:bg-sky-50" @click="builderMode = 'review'">
                  Back to Selection
                </button>
                <button class="rounded-2xl bg-blue-600 px-6 py-3 font-black text-white shadow-lg shadow-blue-200 hover:bg-blue-700" @click="generateProject">
                  Generate Package
                </button>
              </div>
            </div>

            <div class="grid gap-5">
              <article v-for="(api, apiIndex) in project.apis" :key="api.name + apiIndex" class="rounded-[1.75rem] border border-sky-100 bg-white p-5 shadow-lg shadow-sky-100">
                <div class="mb-5 flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                  <div>
                    <p class="text-xs font-black uppercase tracking-[0.25em] text-sky-500">APIM API</p>
                    <h3 class="mt-1 text-2xl font-black text-slate-950">{{ api.displayName || api.name || 'Untitled API' }}</h3>
                  </div>
                  <div class="flex flex-wrap gap-2">
                    <span class="rounded-full bg-blue-50 px-3 py-1 text-xs font-black text-blue-700">{{ api.operations.length }} operations</span>
                    <span class="rounded-full bg-cyan-50 px-3 py-1 text-xs font-black text-cyan-700">{{ api.specificationFormat }}</span>
                  </div>
                </div>

                <div class="grid gap-4 lg:grid-cols-3">
                  <label class="space-y-2">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">APIM Name</span>
                    <input v-model="api.name" class="w-full rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-sm font-black text-slate-950 outline-none ring-sky-300/30 focus:ring-4" />
                  </label>
                  <label class="space-y-2">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Display Name</span>
                    <input v-model="api.displayName" class="w-full rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-sm font-black text-slate-950 outline-none ring-sky-300/30 focus:ring-4" />
                  </label>
                  <label class="space-y-2">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">APIM Path</span>
                    <input v-model="api.path" class="w-full rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-sm font-black text-slate-950 outline-none ring-sky-300/30 focus:ring-4" />
                  </label>
                  <label class="space-y-2 lg:col-span-3">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Backend Service URL</span>
                    <input v-model="api.serviceUrl" class="w-full rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4" />
                  </label>
                  <label class="space-y-2">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">API Version</span>
                    <input v-model="api.apiVersion" placeholder="v1" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm font-black text-slate-950 outline-none ring-sky-300/30 focus:ring-4" />
                  </label>
                  <label class="space-y-2">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Version Set ID</span>
                    <input v-model="api.apiVersionSetId" placeholder="orders-version-set" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4" />
                  </label>
                  <label class="space-y-2">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Revision</span>
                    <input v-model="api.apiRevision" placeholder="1" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm font-black text-slate-950 outline-none ring-sky-300/30 focus:ring-4" />
                  </label>
                  <label class="space-y-2 lg:col-span-3">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Description</span>
                    <textarea v-model="api.description" rows="3" class="w-full resize-none rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-700 outline-none ring-sky-300/30 focus:ring-4"></textarea>
                  </label>
                  <label class="space-y-2">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Subscription Key Header</span>
                    <input v-model="api.subscriptionKeyParameterName" placeholder="Ocp-Apim-Subscription-Key" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4" />
                  </label>
                  <label class="space-y-2">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-transparent">Current revision</span>
                    <span class="flex h-12 items-center gap-3 rounded-2xl border border-sky-100 bg-white px-4 text-sm">
                      <input v-model="api.isCurrent" type="checkbox" class="h-4 w-4 rounded border-sky-300 text-blue-600 focus:ring-blue-500" />
                      <span class="text-sm font-black text-slate-700">Current revision</span>
                    </span>
                  </label>
                  <label class="space-y-2">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-transparent">Require subscription</span>
                    <span class="flex h-12 items-center gap-3 rounded-2xl border border-sky-100 bg-white px-4 text-sm">
                      <input v-model="api.subscriptionRequired" type="checkbox" class="h-4 w-4 rounded border-sky-300 text-blue-600 focus:ring-blue-500" />
                      <span class="text-sm font-black text-slate-700">Require subscription</span>
                    </span>
                  </label>
                  <label class="space-y-2 lg:col-span-3">
                    <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">API Policy XML</span>
                    <textarea v-model="api.policy" rows="10" class="w-full resize-y rounded-2xl border border-sky-100 bg-slate-950 px-4 py-3 font-mono text-xs text-sky-50 outline-none ring-sky-300/30 focus:ring-4"></textarea>
                  </label>
                </div>

                <div class="mt-6 rounded-3xl border border-sky-100 bg-sky-50 p-4">
                  <div class="mb-3 flex items-center justify-between">
                    <h4 class="text-lg font-black text-slate-950">Operations</h4>
                    <span class="text-sm font-black text-blue-700">{{ api.operations.length }} included</span>
                  </div>
                  <div class="space-y-2">
                    <div v-for="operation in api.operations" :key="operation.name" class="grid gap-3 rounded-2xl border border-sky-100 bg-white p-3 md:grid-cols-[7rem_minmax(0,1fr)_minmax(0,1fr)]">
                      <select v-model="operation.method" class="rounded-xl border border-sky-100 bg-sky-50 px-3 py-2 text-xs font-black text-blue-700 outline-none">
                        <option>GET</option>
                        <option>POST</option>
                        <option>PUT</option>
                        <option>PATCH</option>
                        <option>DELETE</option>
                        <option>HEAD</option>
                        <option>OPTIONS</option>
                      </select>
                      <input v-model="operation.urlTemplate" class="min-w-0 rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm font-black text-slate-800 outline-none focus:ring-2 focus:ring-sky-300" />
                      <input v-model="operation.displayName" class="min-w-0 rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm text-slate-700 outline-none focus:ring-2 focus:ring-sky-300" />
                      <textarea v-model="operation.policy" rows="4" placeholder="Operation policy XML, optional" class="md:col-span-3 w-full resize-y rounded-xl border border-sky-100 bg-slate-50 px-3 py-2 font-mono text-xs text-slate-700 outline-none focus:ring-2 focus:ring-sky-300"></textarea>
                    </div>
                  </div>
                </div>
              </article>
            </div>

            <div class="grid gap-5 xl:grid-cols-3">
              <section class="rounded-[1.75rem] border border-sky-100 bg-white p-5 shadow-lg shadow-sky-100">
                <div class="mb-4 flex items-center justify-between">
                  <h3 class="text-xl font-black text-slate-950">Products</h3>
                  <button class="rounded-xl bg-blue-600 px-3 py-2 text-xs font-black text-white" @click="openAddModal('product')">Add</button>
                </div>
                <div class="space-y-3">
                  <div v-for="product in project.products" :key="product.name" class="space-y-2 rounded-2xl bg-sky-50 p-3">
                    <input v-model="product.name" class="w-full rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm font-black" placeholder="product-name" />
                    <input v-model="product.displayName" class="w-full rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm" placeholder="Display name" />
                    <textarea v-model="product.description" rows="2" class="w-full resize-none rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm" placeholder="Description"></textarea>
                    <label class="flex items-center gap-2 text-sm font-bold text-slate-700">
                      <input v-model="product.published" type="checkbox" class="h-4 w-4 rounded border-sky-300 text-blue-600" />
                      Published
                    </label>
                  </div>
                </div>
              </section>

              <section class="rounded-[1.75rem] border border-sky-100 bg-white p-5 shadow-lg shadow-sky-100">
                <div class="mb-4 flex items-center justify-between">
                  <h3 class="text-xl font-black text-slate-950">Named Values</h3>
                  <button class="rounded-xl bg-blue-600 px-3 py-2 text-xs font-black text-white" @click="openAddModal('namedValue')">Add</button>
                </div>
                <div class="space-y-3">
                  <div v-for="namedValue in project.namedValues" :key="namedValue.name" class="space-y-2 rounded-2xl bg-sky-50 p-3">
                    <input v-model="namedValue.name" class="w-full rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm font-black" placeholder="name" />
                    <input v-model="namedValue.displayName" class="w-full rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm" placeholder="Display name" />
                    <input v-model="namedValue.value" class="w-full rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm" placeholder="Value or Key Vault reference" />
                    <label class="flex items-center gap-2 text-sm font-bold text-slate-700">
                      <input v-model="namedValue.secret" type="checkbox" class="h-4 w-4 rounded border-sky-300 text-blue-600" />
                      Secret
                    </label>
                  </div>
                </div>
              </section>

              <section class="rounded-[1.75rem] border border-sky-100 bg-white p-5 shadow-lg shadow-sky-100">
                <div class="mb-4 flex items-center justify-between">
                  <h3 class="text-xl font-black text-slate-950">Policy Fragments</h3>
                  <button class="rounded-xl bg-blue-600 px-3 py-2 text-xs font-black text-white" @click="openAddModal('policyFragment')">Add</button>
                </div>
                <div class="space-y-3">
                  <div v-for="fragment in project.policyFragments" :key="fragment.name" class="space-y-2 rounded-2xl bg-sky-50 p-3">
                    <input v-model="fragment.name" class="w-full rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm font-black" placeholder="fragment-name" />
                    <input v-model="fragment.description" class="w-full rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm" placeholder="Description" />
                    <textarea v-model="fragment.policyContent" rows="7" class="w-full resize-y rounded-xl border border-sky-100 bg-slate-950 px-3 py-2 font-mono text-xs text-sky-50"></textarea>
                  </div>
                </div>
              </section>
            </div>
          </div>
        </section>
      </main>

      <div v-if="showSelectionModal" class="fixed inset-0 z-50 bg-slate-950/45 p-3 backdrop-blur-sm sm:p-6">
        <div class="mx-auto flex h-full max-w-7xl flex-col overflow-hidden rounded-[2rem] border border-sky-100 bg-white shadow-2xl shadow-slate-900/20">
          <header class="border-b border-sky-100 bg-white px-5 py-4 sm:px-6">
            <div class="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
              <div>
                <p class="text-xs font-black uppercase tracking-[0.35em] text-blue-500">API Selection</p>
                <h2 class="mt-1 text-3xl font-black text-slate-950">Select APIs and operations</h2>
              </div>
              <div class="flex flex-wrap gap-3">
                <button class="rounded-2xl border border-sky-200 bg-white px-5 py-3 font-black text-blue-700 hover:bg-sky-50" @click="toggleAll(true)">
                  Select All
                </button>
                <button class="rounded-2xl border border-sky-200 bg-white px-5 py-3 font-black text-blue-700 hover:bg-sky-50" @click="toggleAll(false)">
                  Clear All
                </button>
                <button class="rounded-2xl border border-slate-200 bg-white px-5 py-3 font-black text-slate-700 hover:bg-slate-50" @click="cancelSelectionModal">
                  Cancel
                </button>
                <button class="rounded-2xl bg-blue-600 px-6 py-3 font-black text-white shadow-lg shadow-blue-200 hover:bg-blue-700" @click="acceptSelectionModal">
                  OK
                </button>
              </div>
            </div>
          </header>

          <div class="grid min-h-0 flex-1 overflow-hidden grid-cols-1 lg:grid-cols-[20rem_minmax(0,1fr)]">
            <aside class="border-b border-sky-100 bg-sky-50/70 p-4 lg:border-b-0 lg:border-r">
              <div class="mb-3 grid grid-cols-3 gap-2">
                <div class="rounded-2xl bg-white p-3 text-center">
                  <div class="text-xl font-black text-blue-700">{{ discoveredApis.length }}</div>
                  <div class="text-xs font-bold text-slate-500">APIs</div>
                </div>
                <div class="rounded-2xl bg-white p-3 text-center">
                  <div class="text-xl font-black text-blue-700">{{ selectedDiscoveryCount }}</div>
                  <div class="text-xs font-bold text-slate-500">Selected</div>
                </div>
                <div class="rounded-2xl bg-white p-3 text-center">
                  <div class="text-xl font-black text-blue-700">{{ selectedOperationCount }}</div>
                  <div class="text-xs font-bold text-slate-500">Ops</div>
                </div>
              </div>

              <div class="max-h-[calc(100vh-18rem)] space-y-2 overflow-y-auto pr-1">
                <button
                  v-for="api in discoveredApis"
                  :key="api.id"
                  :class="[
                    'w-full rounded-2xl border p-3 text-left transition',
                    activeSelectionApiId === api.id ? 'border-blue-300 bg-white shadow-md shadow-sky-100' : 'border-sky-100 bg-white/70 hover:bg-white'
                  ]"
                  @click="activeSelectionApiId = api.id"
                >
                  <div class="flex items-center justify-between gap-3">
                    <span class="min-w-0 truncate text-sm font-black text-slate-950">{{ api.displayName }}</span>
                    <span class="text-xs font-black text-blue-700">{{ selectedApiOperations(api).length }}/{{ api.operations.length }}</span>
                  </div>
                  <div class="mt-2 truncate text-xs font-semibold text-slate-500">{{ api.sourceName }}</div>
                </button>
              </div>
            </aside>

            <section class="min-h-0 overflow-hidden p-5 sm:p-6">
              <article v-if="activeSelectionApi" class="space-y-5">
                <div class="rounded-[1.75rem] border border-sky-100 bg-white p-5 shadow-lg shadow-sky-100">
                  <div class="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                    <div class="min-w-0">
                      <div class="mb-3 flex flex-wrap items-center gap-2">
                        <span class="rounded-full bg-blue-600 px-3 py-1 text-xs font-black text-white">{{ activeSelectionApi.format }}</span>
                        <span class="rounded-full bg-sky-100 px-3 py-1 text-xs font-black text-blue-700">{{ activeSelectionApi.version }}</span>
                        <span class="rounded-full bg-slate-100 px-3 py-1 text-xs font-black text-slate-600">{{ activeSelectionApi.sourceName }}</span>
                      </div>
                      <h3 class="truncate text-3xl font-black text-slate-950">{{ activeSelectionApi.displayName }}</h3>
                      <p class="mt-2 max-w-4xl text-sm leading-6 text-slate-600">{{ activeSelectionApi.description || 'No description provided.' }}</p>
                    </div>
                    <label class="relative inline-flex cursor-pointer items-center">
                      <input v-model="activeSelectionApi.selected" type="checkbox" class="peer sr-only" @change="toggleApiSelection(activeSelectionApi)" />
                      <span class="h-8 w-14 rounded-full bg-slate-200 after:absolute after:left-1 after:top-1 after:h-6 after:w-6 after:rounded-full after:bg-white after:shadow after:transition peer-checked:bg-blue-600 peer-checked:after:translate-x-6"></span>
                    </label>
                  </div>

                  <div class="mt-5 grid gap-3 sm:grid-cols-3">
                    <div class="rounded-2xl bg-sky-50 p-3">
                      <div class="text-xl font-black text-blue-700">{{ selectedApiOperations(activeSelectionApi).length }}/{{ activeSelectionApi.operations.length }}</div>
                      <div class="text-xs font-semibold text-slate-500">Ops selected</div>
                    </div>
                    <div class="rounded-2xl bg-sky-50 p-3">
                      <div class="truncate text-xl font-black text-blue-700">{{ activeSelectionApi.path || '/' }}</div>
                      <div class="text-xs font-semibold text-slate-500">APIM path</div>
                    </div>
                    <div class="rounded-2xl bg-sky-50 p-3">
                      <div class="text-xl font-black text-blue-700">{{ activeSelectionApi.schemaCount }}</div>
                      <div class="text-xs font-semibold text-slate-500">Schemas</div>
                    </div>
                  </div>
                </div>

                <div class="rounded-[1.75rem] border border-sky-100 bg-white p-4 shadow-lg shadow-sky-100">
                  <div class="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                    <h4 class="text-xl font-black text-slate-950">Operations</h4>
                    <div class="flex gap-2">
                      <button class="rounded-xl border border-sky-200 bg-white px-3 py-2 text-xs font-black text-blue-700 hover:bg-sky-50" @click="toggleApiOperations(activeSelectionApi, true)">
                        Select all
                      </button>
                      <button class="rounded-xl border border-sky-200 bg-white px-3 py-2 text-xs font-black text-blue-700 hover:bg-sky-50" @click="toggleApiOperations(activeSelectionApi, false)">
                        Clear
                      </button>
                    </div>
                  </div>

                  <div class="max-h-[calc(100vh-31rem)] space-y-2 overflow-y-auto pr-1">
                    <div
                      v-for="operation in activeSelectionApi.operations"
                      :key="operation.name"
                      :class="[
                        'grid items-center gap-3 rounded-2xl border px-3 py-3 md:grid-cols-[1.5rem_5rem_minmax(0,1.2fr)_minmax(0,1fr)]',
                        operation.selected ? 'border-sky-100 bg-white' : 'border-slate-100 bg-slate-50 opacity-70'
                      ]"
                    >
                      <input v-model="operation.selected" type="checkbox" class="h-4 w-4 rounded border-sky-300 text-blue-600 focus:ring-blue-500" @change="syncApiSelection(activeSelectionApi)" />
                      <span :class="['rounded-xl px-2 py-1 text-center text-xs font-black', methodClass(operation.method)]">{{ operation.method }}</span>
                      <span class="min-w-0 truncate text-sm font-black text-slate-800">{{ operation.urlTemplate }}</span>
                      <span class="min-w-0 truncate text-xs font-semibold text-slate-500">{{ operation.displayName }}</span>
                    </div>
                  </div>
                </div>
              </article>
            </section>
          </div>

          <footer class="border-t border-sky-100 bg-white px-5 py-4 shadow-[0_-12px_30px_rgba(14,165,233,0.08)] sm:px-6">
            <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <p class="text-sm font-semibold text-slate-600">
                {{ selectedOperationCount }} operation{{ selectedOperationCount === 1 ? '' : 's' }} selected across {{ selectedDiscoveryCount }} API{{ selectedDiscoveryCount === 1 ? '' : 's' }}.
              </p>
              <div class="flex gap-3">
                <button class="rounded-2xl border border-slate-200 bg-white px-6 py-3 font-black text-slate-700 hover:bg-slate-50" @click="cancelSelectionModal">
                  Cancel
                </button>
                <button class="rounded-2xl bg-blue-600 px-8 py-3 font-black text-white shadow-lg shadow-blue-200 hover:bg-blue-700" @click="acceptSelectionModal">
                  OK
                </button>
              </div>
            </div>
          </footer>
        </div>
      </div>

      <div v-if="artifactModalKind" class="fixed inset-0 z-[60] grid place-items-center bg-slate-950/45 p-4 backdrop-blur-sm">
        <div class="max-h-[calc(100vh-2rem)] w-full max-w-3xl overflow-hidden rounded-[2rem] border border-sky-100 bg-white shadow-2xl shadow-slate-900/20">
          <header class="border-b border-sky-100 bg-gradient-to-r from-white to-sky-50 px-6 py-5">
            <p class="text-xs font-black uppercase tracking-[0.35em] text-blue-500">{{ artifactModalEyebrow }}</p>
            <h2 class="mt-1 text-3xl font-black text-slate-950">{{ artifactModalTitle }}</h2>
          </header>

          <div class="max-h-[calc(100vh-15rem)] overflow-y-auto px-6 py-5">
            <div v-if="artifactModalKind === 'product'" class="grid gap-4">
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Product Name</span>
                <input v-model="productDraft.name" class="w-full rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-sm font-black text-slate-950 outline-none ring-sky-300/30 focus:ring-4" placeholder="starter-product" />
              </label>
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Display Name</span>
                <input v-model="productDraft.displayName" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4" placeholder="Starter Product" />
              </label>
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Description</span>
                <textarea v-model="productDraft.description" rows="4" class="w-full resize-none rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-700 outline-none ring-sky-300/30 focus:ring-4" placeholder="What this product exposes"></textarea>
              </label>
              <div class="grid gap-3 sm:grid-cols-3">
                <label class="flex h-12 items-center gap-3 rounded-2xl border border-sky-100 bg-white px-4 text-sm font-black text-slate-700">
                  <input v-model="productDraft.published" type="checkbox" class="h-4 w-4 rounded border-sky-300 text-blue-600 focus:ring-blue-500" />
                  Published
                </label>
                <label class="flex h-12 items-center gap-3 rounded-2xl border border-sky-100 bg-white px-4 text-sm font-black text-slate-700">
                  <input v-model="productDraft.subscriptionRequired" type="checkbox" class="h-4 w-4 rounded border-sky-300 text-blue-600 focus:ring-blue-500" />
                  Subscription
                </label>
                <label class="flex h-12 items-center gap-3 rounded-2xl border border-sky-100 bg-white px-4 text-sm font-black text-slate-700">
                  <input v-model="productDraft.approvalRequired" type="checkbox" class="h-4 w-4 rounded border-sky-300 text-blue-600 focus:ring-blue-500" />
                  Approval
                </label>
              </div>
            </div>

            <div v-else-if="artifactModalKind === 'namedValue'" class="grid gap-4">
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Name</span>
                <input v-model="namedValueDraft.name" class="w-full rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-sm font-black text-slate-950 outline-none ring-sky-300/30 focus:ring-4" placeholder="orders-base-url" />
              </label>
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Display Name</span>
                <input v-model="namedValueDraft.displayName" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4" placeholder="Orders Base URL" />
              </label>
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Value</span>
                <textarea v-model="namedValueDraft.value" rows="4" class="w-full resize-none rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-700 outline-none ring-sky-300/30 focus:ring-4" placeholder="Value or Key Vault reference"></textarea>
              </label>
              <label class="flex h-12 items-center gap-3 rounded-2xl border border-sky-100 bg-white px-4 text-sm font-black text-slate-700">
                <input v-model="namedValueDraft.secret" type="checkbox" class="h-4 w-4 rounded border-sky-300 text-blue-600 focus:ring-blue-500" />
                Secret value
              </label>
            </div>

            <div v-else class="grid gap-4">
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Fragment Name</span>
                <input v-model="fragmentDraft.name" class="w-full rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-sm font-black text-slate-950 outline-none ring-sky-300/30 focus:ring-4" placeholder="set-common-headers" />
              </label>
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Description</span>
                <input v-model="fragmentDraft.description" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4" placeholder="Reusable policy fragment" />
              </label>
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Policy XML</span>
                <textarea v-model="fragmentDraft.policyContent" rows="12" class="w-full resize-y rounded-2xl border border-sky-100 bg-slate-950 px-4 py-3 font-mono text-xs text-sky-50 outline-none ring-sky-300/30 focus:ring-4"></textarea>
              </label>
            </div>
          </div>

          <footer class="border-t border-sky-100 bg-white px-6 py-4">
            <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-end">
              <button class="rounded-2xl border border-slate-200 bg-white px-5 py-3 font-black text-slate-700 hover:bg-slate-50" @click="closeAddModal">
                Cancel
              </button>
              <button class="rounded-2xl border border-sky-200 bg-white px-5 py-3 font-black text-blue-700 hover:bg-sky-50" @click="saveArtifact(false)">
                Save & add another
              </button>
              <button class="rounded-2xl bg-blue-600 px-6 py-3 font-black text-white shadow-lg shadow-blue-200 hover:bg-blue-700" @click="saveArtifact(true)">
                Save
              </button>
            </div>
          </footer>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE' | 'HEAD' | 'OPTIONS' | 'TRACE'
type ArtifactModalKind = 'product' | 'namedValue' | 'policyFragment'

interface OperationDefinition {
  name: string
  displayName: string
  method: HttpMethod
  urlTemplate: string
  description?: string
  policy?: string
  selected: boolean
}

interface DiscoveredApi {
  id: string
  selected: boolean
  sourceName: string
  name: string
  displayName: string
  description: string
  path: string
  serviceUrl: string
  format: string
  version: string
  schemaCount: number
  operations: OperationDefinition[]
  specificationFormat: 'openapi' | 'swagger'
  specificationContent: string
}

const HTTP_METHODS = new Set(['get', 'post', 'put', 'patch', 'delete', 'head', 'options', 'trace'])
const DRAFT_STORAGE_KEY = 'apim-policy-studio-project-builder-draft'

const sampleSpec = `{
  "openapi": "3.0.0",
  "info": {
    "title": "Sample API",
    "version": "1.0.0",
    "description": "A sample API for testing APIM package generation"
  },
  "servers": [{ "url": "https://api.example.com/v1" }],
  "paths": {
    "/users": {
      "get": { "summary": "Get all users", "responses": { "200": { "description": "OK" } } },
      "post": { "summary": "Create user", "responses": { "201": { "description": "Created" } } }
    },
    "/users/{id}": {
      "get": { "summary": "Get user by ID", "responses": { "200": { "description": "OK" } } }
    }
  },
  "components": {
    "schemas": {
      "User": { "type": "object" },
      "UserInput": { "type": "object" }
    }
  }
}`

const projectName = ref('My APIM Project')
const projectDescription = ref('Generated from a reviewed OpenAPI or Swagger contract.')
const sourceName = ref('')
const rawSpec = ref('')
const parseError = ref('')
const hasAnalyzed = ref(false)
const builderMode = ref<'review' | 'builder'>('review')
const discoveredApis = ref<DiscoveredApi[]>([])
const showSelectionModal = ref(false)
const activeSelectionApiId = ref('')
const selectionSnapshot = ref('')
const artifactModalKind = ref<ArtifactModalKind | null>(null)
const draftStatus = ref('Draft autosaves locally in this browser.')
const isRestoringDraft = ref(false)
let draftSaveTimer: number | undefined

const project = reactive({
  apis: [] as any[],
  backends: [] as any[],
  namedValues: [] as any[],
  policyFragments: [] as any[],
  products: [] as any[],
  schemas: [] as any[]
})

const productDraft = reactive(createProductDraft())
const namedValueDraft = reactive(createNamedValueDraft())
const fragmentDraft = reactive(createPolicyFragmentDraft())

const pipelineSteps = ['Parse contract', 'Extract paths', 'Review import', 'Generate package']

const selectedDiscoveryCount = computed(() => discoveredApis.value.filter(api => api.selected).length)
const selectedOperationCount = computed(() =>
  discoveredApis.value.reduce((count, api) => count + (api.selected ? selectedApiOperations(api).length : 0), 0)
)
const activeSelectionApi = computed(() =>
  discoveredApis.value.find(api => api.id === activeSelectionApiId.value) ?? discoveredApis.value[0] ?? null
)
const discoverySummary = computed(() => {
  const operations = discoveredApis.value.reduce((count, api) => count + api.operations.length, 0)
  return `${discoveredApis.value.length} API candidate${discoveredApis.value.length === 1 ? '' : 's'} found with ${operations} operation${operations === 1 ? '' : 's'}. Keep adding swagger files, then approve only the APIs and operations that should be imported into APIM.`
})
const artifactModalEyebrow = computed(() => {
  if (artifactModalKind.value === 'product') return 'APIM Product'
  if (artifactModalKind.value === 'namedValue') return 'APIM Named Value'
  return 'APIM Policy Fragment'
})
const artifactModalTitle = computed(() => {
  if (artifactModalKind.value === 'product') return 'Add product'
  if (artifactModalKind.value === 'namedValue') return 'Add named value'
  return 'Add policy fragment'
})

onMounted(() => {
  restoreDraft()
})

watch(
  [projectName, projectDescription, sourceName, rawSpec, hasAnalyzed, builderMode, discoveredApis, project],
  scheduleDraftSave,
  { deep: true }
)

function triggerFilePicker() {
  const input = document.querySelector<HTMLInputElement>('input[type="file"]')
  input?.click()
}

async function handleFileUpload(event: Event) {
  const input = event.target as HTMLInputElement
  const files = Array.from(input.files ?? [])
  if (files.length === 0) return
  await readSpecFiles(files)
  input.value = ''
}

async function handleDrop(event: DragEvent) {
  const files = Array.from(event.dataTransfer?.files ?? [])
  if (files.length === 0) return
  await readSpecFiles(files)
}

async function readSpecFiles(files: File[]) {
  parseError.value = ''
  const imported: DiscoveredApi[] = []

  for (const file of files) {
    try {
      const content = await file.text()
      rawSpec.value = content
      imported.push(...discoverApis(JSON.parse(content), content, file.name))
    } catch (error) {
      parseError.value = `Could not read ${file.name}: ${error instanceof Error ? error.message : 'Invalid JSON.'}`
    }
  }

  if (imported.length > 0) {
    addDiscoveredApis(imported)
    builderMode.value = 'review'
    sourceName.value = `${files.length} file${files.length === 1 ? '' : 's'} imported`
  }
}

function loadSample() {
  sourceName.value = 'sample-api.json'
  rawSpec.value = sampleSpec
  analyzeSpec()
}

function analyzeSpec() {
  parseError.value = ''

  try {
    const spec = JSON.parse(rawSpec.value)
    const discovered = discoverApis(spec, rawSpec.value, sourceName.value || 'Pasted JSON')

    if (discovered.length === 0) {
      throw new Error('No paths were found in this OpenAPI or Swagger document.')
    }

    addDiscoveredApis(discovered)
    hasAnalyzed.value = true
  } catch (error) {
    parseError.value = error instanceof Error ? error.message : 'Unable to parse the swagger JSON.'
    hasAnalyzed.value = discoveredApis.value.length > 0
  }
}

function discoverApis(spec: any, content: string, importSourceName: string): DiscoveredApi[] {
  const paths = spec?.paths
  if (!paths || typeof paths !== 'object') return []

  const title = String(spec?.info?.title || 'Imported API')
  const version = String(spec?.info?.version || '1.0.0')
  const serviceUrl = getServiceUrl(spec)
  const basePath = normalizePath(spec?.basePath || inferPathFromUrl(serviceUrl) || slugify(title))
  const format = spec.swagger ? 'Swagger 2.0' : 'OpenAPI'
  const operations: OperationDefinition[] = []

  Object.entries(paths).forEach(([pathName, pathItem]) => {
    if (!pathItem || typeof pathItem !== 'object') return

    Object.entries(pathItem as Record<string, any>).forEach(([method, definition]) => {
      if (!HTTP_METHODS.has(method.toLowerCase())) return
      const displayName = String(definition?.summary || definition?.operationId || `${method.toUpperCase()} ${pathName}`)
      operations.push({
        name: slugify(String(definition?.operationId || `${method}-${pathName}`)),
        displayName,
        method: method.toUpperCase() as HttpMethod,
        urlTemplate: pathName,
        description: definition?.description || displayName,
        policy: '',
        selected: true
      })
    })
  })

  const apiName = makeUniqueApiName(slugify(title))

  return [{
    id: `${apiName}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
    selected: true,
    sourceName: importSourceName,
    name: apiName,
    displayName: title,
    description: String(spec?.info?.description || ''),
    path: basePath,
    serviceUrl,
    format,
    version,
    schemaCount: Object.keys(spec?.components?.schemas || spec?.definitions || {}).length,
    operations,
    specificationFormat: spec.swagger ? 'swagger' : 'openapi',
    specificationContent: content
  }]
}

function addDiscoveredApis(importedApis: DiscoveredApi[]) {
  discoveredApis.value.push(...importedApis)
  hasAnalyzed.value = discoveredApis.value.length > 0
  builderMode.value = 'review'

  if (projectName.value === 'My APIM Project' && importedApis[0]?.displayName) {
    projectName.value = `${importedApis[0].displayName} APIM Project`
  }
}

function getServiceUrl(spec: any) {
  if (Array.isArray(spec?.servers) && spec.servers[0]?.url) {
    return absolutizeServerUrl(String(spec.servers[0].url))
  }

  if (spec?.host) {
    const scheme = Array.isArray(spec.schemes) ? spec.schemes[0] : 'https'
    const basePath = spec.basePath || ''
    return `${scheme}://${spec.host}${basePath}`
  }

  return ''
}

function absolutizeServerUrl(url: string) {
  if (url.startsWith('http://') || url.startsWith('https://')) return url
  if (url.startsWith('//')) return `https:${url}`
  return url
}

function inferPathFromUrl(url: string) {
  try {
    const parsed = new URL(url)
    return parsed.pathname.replace(/^\/+|\/+$/g, '')
  } catch {
    return ''
  }
}

function normalizePath(value: string) {
  const cleaned = String(value || '').replace(/^\/+|\/+$/g, '')
  return cleaned || 'api'
}

function slugify(value: string) {
  return value
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .slice(0, 80) || 'api'
}

function toggleAll(selected: boolean) {
  discoveredApis.value.forEach(api => {
    api.selected = selected
    api.operations.forEach(operation => {
      operation.selected = selected
    })
  })
}

function openSelectionModal(apiId?: string) {
  selectionSnapshot.value = JSON.stringify(discoveredApis.value.map(api => ({
    id: api.id,
    selected: api.selected,
    operations: api.operations.map(operation => ({
      name: operation.name,
      selected: operation.selected
    }))
  })))
  activeSelectionApiId.value = apiId || discoveredApis.value[0]?.id || ''
  showSelectionModal.value = true
}

function acceptSelectionModal() {
  showSelectionModal.value = false
  selectionSnapshot.value = ''
}

function cancelSelectionModal() {
  if (selectionSnapshot.value) {
    const snapshot = JSON.parse(selectionSnapshot.value) as Array<{
      id: string
      selected: boolean
      operations: Array<{ name: string; selected: boolean }>
    }>

    snapshot.forEach(savedApi => {
      const api = discoveredApis.value.find(candidate => candidate.id === savedApi.id)
      if (!api) return

      api.selected = savedApi.selected
      savedApi.operations.forEach(savedOperation => {
        const operation = api.operations.find(candidate => candidate.name === savedOperation.name)
        if (operation) operation.selected = savedOperation.selected
      })
    })
  }

  showSelectionModal.value = false
  selectionSnapshot.value = ''
}

function toggleApiSelection(api: DiscoveredApi) {
  api.operations.forEach(operation => {
    operation.selected = api.selected
  })
}

function toggleApiOperations(api: DiscoveredApi, selected: boolean) {
  api.selected = selected
  api.operations.forEach(operation => {
    operation.selected = selected
  })
}

function syncApiSelection(api: DiscoveredApi) {
  api.selected = api.operations.some(operation => operation.selected)
}

function confirmImport() {
  const selected = discoveredApis.value
    .filter(api => api.selected)
    .map(api => ({
      ...api,
      operations: selectedApiOperations(api)
    }))
    .filter(api => api.operations.length > 0)

  if (selected.length === 0) {
    parseError.value = 'Select at least one API with at least one operation before moving to APIM Builder.'
    return
  }

  project.apis.splice(0, project.apis.length, ...selected.map(api => ({
    name: api.name,
    displayName: api.displayName,
    path: api.path,
    description: api.description,
    protocols: ['https'],
    serviceUrl: api.serviceUrl,
    subscriptionKeyParameterName: 'Ocp-Apim-Subscription-Key',
    apiVersion: 'v1',
    apiVersionSetId: `${api.name}-version-set`,
    apiRevision: '1',
    isCurrent: true,
    subscriptionRequired: true,
    policy: defaultApiPolicy(),
    operations: api.operations,
    specificationContent: api.specificationContent,
    specificationFormat: api.specificationFormat
  })))

  project.backends.splice(0, project.backends.length, ...selected
    .filter(api => api.serviceUrl)
    .map(api => ({
      name: `${api.name}-backend`,
      title: `${api.displayName} backend`,
      url: api.serviceUrl,
      protocol: api.serviceUrl.startsWith('https') ? 'https' : 'http',
      properties: {}
    })))

  project.products.splice(0, project.products.length, {
    name: 'starter-product',
    displayName: 'Starter Product',
    description: 'Generated product containing the selected imported APIs.',
    subscriptionRequired: true,
    approvalRequired: false,
    published: true,
    apis: selected.map(api => api.name)
  })

  builderMode.value = 'builder'
}

function selectedApiOperations(api: DiscoveredApi) {
  return api.operations.filter(operation => operation.selected)
}

function resetImports() {
  discoveredApis.value = []
  hasAnalyzed.value = false
  builderMode.value = 'review'
  project.apis.splice(0, project.apis.length)
  project.backends.splice(0, project.backends.length)
  project.namedValues.splice(0, project.namedValues.length)
  project.policyFragments.splice(0, project.policyFragments.length)
  project.products.splice(0, project.products.length)
  parseError.value = ''
}

function createProductDraft() {
  const index = project?.products?.length ? project.products.length + 1 : 1

  return {
    name: `product-${index}`,
    displayName: `Product ${index}`,
    description: '',
    subscriptionRequired: true,
    approvalRequired: false,
    published: true,
    apis: [] as string[]
  }
}

function createNamedValueDraft() {
  const index = project?.namedValues?.length ? project.namedValues.length + 1 : 1

  return {
    name: `named-value-${index}`,
    displayName: `Named Value ${index}`,
    value: '',
    secret: false,
    tags: [] as string[]
  }
}

function createPolicyFragmentDraft() {
  const index = project?.policyFragments?.length ? project.policyFragments.length + 1 : 1

  return {
    name: `policy-fragment-${index}`,
    description: '',
    policyContent: defaultPolicyFragment()
  }
}

function defaultPolicyFragment() {
  return `<fragment>
  <set-header name="x-apim-fragment" exists-action="override">
    <value>true</value>
  </set-header>
</fragment>`
}

function openAddModal(kind: ArtifactModalKind) {
  artifactModalKind.value = kind

  if (kind === 'product') {
    Object.assign(productDraft, createProductDraft())
  } else if (kind === 'namedValue') {
    Object.assign(namedValueDraft, createNamedValueDraft())
  } else {
    Object.assign(fragmentDraft, createPolicyFragmentDraft())
  }
}

function closeAddModal() {
  artifactModalKind.value = null
}

function saveArtifact(closeAfterSave: boolean) {
  if (artifactModalKind.value === 'product') {
    const product = {
      ...productDraft,
      name: productDraft.name.trim() || `product-${project.products.length + 1}`,
      displayName: productDraft.displayName.trim() || productDraft.name.trim() || `Product ${project.products.length + 1}`,
      apis: project.apis.map(api => api.name).filter(Boolean)
    }
    project.products.push(product)

    if (!closeAfterSave) Object.assign(productDraft, createProductDraft())
  } else if (artifactModalKind.value === 'namedValue') {
    const namedValue = {
      ...namedValueDraft,
      name: namedValueDraft.name.trim() || `named-value-${project.namedValues.length + 1}`,
      displayName: namedValueDraft.displayName.trim() || namedValueDraft.name.trim() || `Named Value ${project.namedValues.length + 1}`,
      tags: [...namedValueDraft.tags]
    }
    project.namedValues.push(namedValue)

    if (!closeAfterSave) Object.assign(namedValueDraft, createNamedValueDraft())
  } else if (artifactModalKind.value === 'policyFragment') {
    const fragment = {
      ...fragmentDraft,
      name: fragmentDraft.name.trim() || `policy-fragment-${project.policyFragments.length + 1}`,
      policyContent: fragmentDraft.policyContent.trim() || defaultPolicyFragment()
    }
    project.policyFragments.push(fragment)

    if (!closeAfterSave) Object.assign(fragmentDraft, createPolicyFragmentDraft())
  }

  if (closeAfterSave) closeAddModal()
}

function scheduleDraftSave() {
  if (isRestoringDraft.value || typeof window === 'undefined') return

  if (draftSaveTimer) {
    window.clearTimeout(draftSaveTimer)
  }

  draftSaveTimer = window.setTimeout(saveDraft, 350)
}

function saveDraft() {
  if (typeof window === 'undefined') return

  try {
    const draft = {
      projectName: projectName.value,
      projectDescription: projectDescription.value,
      sourceName: sourceName.value,
      rawSpec: rawSpec.value,
      hasAnalyzed: hasAnalyzed.value,
      builderMode: builderMode.value,
      discoveredApis: discoveredApis.value,
      project
    }

    window.localStorage.setItem(DRAFT_STORAGE_KEY, JSON.stringify(draft))
    draftStatus.value = 'Draft autosaved locally in this browser.'
  } catch {
    draftStatus.value = 'Draft is too large for localStorage. Generate the package or use fewer embedded specs.'
  }
}

function restoreDraft() {
  if (typeof window === 'undefined') return

  const savedDraft = window.localStorage.getItem(DRAFT_STORAGE_KEY)
  if (!savedDraft) return

  try {
    isRestoringDraft.value = true
    const draft = JSON.parse(savedDraft)

    projectName.value = draft.projectName || projectName.value
    projectDescription.value = draft.projectDescription || projectDescription.value
    sourceName.value = draft.sourceName || ''
    rawSpec.value = draft.rawSpec || ''
    hasAnalyzed.value = Boolean(draft.hasAnalyzed)
    builderMode.value = draft.builderMode === 'builder' ? 'builder' : 'review'
    discoveredApis.value = Array.isArray(draft.discoveredApis) ? draft.discoveredApis : []

    restoreArray(project.apis, draft.project?.apis)
    restoreArray(project.backends, draft.project?.backends)
    restoreArray(project.namedValues, draft.project?.namedValues)
    restoreArray(project.policyFragments, draft.project?.policyFragments)
    restoreArray(project.products, draft.project?.products)
    restoreArray(project.schemas, draft.project?.schemas)

    draftStatus.value = 'Draft restored from this browser.'
  } catch {
    draftStatus.value = 'Saved draft could not be restored.'
  } finally {
    isRestoringDraft.value = false
  }
}

function restoreArray(target: any[], value: unknown) {
  target.splice(0, target.length, ...(Array.isArray(value) ? value : []))
}

function makeUniqueApiName(baseName: string) {
  let candidate = baseName
  let suffix = 2
  const existingNames = new Set(discoveredApis.value.map(api => api.name))

  while (existingNames.has(candidate)) {
    candidate = `${baseName}-${suffix}`
    suffix += 1
  }

  return candidate
}

function defaultApiPolicy() {
  return `<policies>
  <inbound>
    <base />
  </inbound>
  <backend>
    <base />
  </backend>
  <outbound>
    <base />
  </outbound>
  <on-error>
    <base />
  </on-error>
</policies>`
}

async function generateProject() {
  if (project.apis.length === 0) {
    parseError.value = 'Confirm at least one API before generating the package.'
    return
  }

  syncProjectArtifacts()

  const projectData = {
    name: projectName.value,
    description: projectDescription.value,
    ...project
  }

  try {
    const response = await fetch('http://localhost:5000/api/project/download', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(projectData)
    })

    if (!response.ok) throw new Error('Generation failed')

    const blob = await response.blob()
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${projectName.value.replace(/\s+/g, '-').toLowerCase()}-apim-project.zip`
    a.click()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    parseError.value = error instanceof Error ? error.message : 'Failed to generate project.'
  }
}

function syncProjectArtifacts() {
  const apiNames = project.apis.map(api => api.name).filter(Boolean)
  project.products.forEach(product => {
    product.apis = apiNames
  })
}

function methodClass(method: string) {
  const classes: Record<string, string> = {
    GET: 'bg-emerald-300 text-emerald-950',
    POST: 'bg-cyan-300 text-cyan-950',
    PUT: 'bg-amber-300 text-amber-950',
    PATCH: 'bg-fuchsia-300 text-fuchsia-950',
    DELETE: 'bg-rose-300 text-rose-950'
  }

  return classes[method] || 'bg-slate-200 text-slate-950'
}
</script>

<style scoped>
::-webkit-scrollbar {
  height: 8px;
  width: 8px;
}

::-webkit-scrollbar-track {
  background: rgba(15, 23, 42, 0.6);
}

::-webkit-scrollbar-thumb {
  background: rgba(125, 211, 252, 0.55);
  border-radius: 999px;
}
</style>
