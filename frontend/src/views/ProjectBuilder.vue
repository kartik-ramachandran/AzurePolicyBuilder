<template>
  <div class="project-builder min-h-[calc(100vh-7rem)] text-slate-950">
    <div :class="['relative isolate min-h-[calc(100vh-7rem)] overflow-hidden', hasAnalyzed ? 'review-shell' : 'welcome-surface']">

      <main v-if="!hasAnalyzed" class="welcome-stage">
        <section class="welcome-copy">
          <div class="welcome-kicker">
            <span class="welcome-kicker__dot"></span>
            From contract to gateway
          </div>
          <h1 class="welcome-title">
            Shape your APIs.<br />
            <span>Ship with confidence.</span>
          </h1>
          <p class="welcome-description">
            Turn OpenAPI contracts into a clean, reviewable APIM package—without losing control of what gets deployed.
          </p>

          <div class="welcome-actions">
            <button class="welcome-primary" @click="triggerFilePicker">
              <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 16V4m0 0 4 4m-4-4-4 4M4 20h16" />
              </svg>
              Choose contract
            </button>
            <button class="welcome-secondary" @click="loadSample">
              Explore a sample
              <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m9 5 7 7-7 7" /></svg>
            </button>
          </div>

          <button class="mt-4 self-start text-sm font-bold text-blue-700 underline decoration-sky-300 underline-offset-4 hover:text-blue-800" @click="startManually">
            No contract? Compose the package manually →
          </button>

          <div class="welcome-capabilities">
            <span v-for="item in ['OpenAPI 3.x', 'Swagger 2.0', 'Selective import']" :key="item">
              <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m5 12 4 4L19 6" /></svg>
              {{ item }}
            </span>
          </div>
        </section>

        <section class="contract-card">
          <div class="contract-card__header">
            <div>
              <p>Contract input</p>
              <h2>Import OpenAPI</h2>
            </div>
            <span class="contract-format"><i></i> JSON</span>
          </div>

          <label class="contract-dropzone" @dragover.prevent @drop.prevent="handleDrop">
            <input class="hidden" type="file" accept=".json,application/json" multiple @change="handleFileUpload" />
            <span class="contract-dropzone__icon">
              <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8" d="M12 16V4m0 0 4 4m-4-4-4 4M4 20h16" /></svg>
            </span>
            <span class="min-w-0 flex-1">
              <strong>{{ sourceName || 'Drop your contract here' }}</strong>
              <small>{{ sourceName ? 'Ready to inspect' : 'or click to browse · multiple files supported' }}</small>
            </span>
            <span class="contract-browse">Browse</span>
          </label>

          <div class="contract-code">
            <div class="contract-code__bar">
              <span><i></i><i></i><i></i></span>
              <span>openapi.json</span>
              <span>{{ rawSpec ? 'Loaded' : 'Paste JSON' }}</span>
            </div>
            <textarea
              v-model="rawSpec"
              spellcheck="false"
              placeholder="{&#10;  &quot;openapi&quot;: &quot;3.0.1&quot;,&#10;  &quot;info&quot;: { ... }&#10;}"
            ></textarea>
          </div>

          <p v-if="parseError" class="contract-error">{{ parseError }}</p>

          <div class="contract-card__footer">
            <span>{{ rawSpec ? 'Contract ready for analysis' : 'Paste or upload a JSON contract' }}</span>
            <button :disabled="!rawSpec.trim()" @click="analyzeSpec">
              Analyze contract
              <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m9 5 7 7-7 7" /></svg>
            </button>
          </div>
        </section>

        <div class="workflow-line">
          <div v-for="(step, index) in pipelineSteps" :key="step" class="workflow-step">
            <span>{{ index + 1 }}</span>
            <div><strong>{{ step }}</strong><small>{{ ['Validate the source', 'Map APIs & operations', 'Approve the scope', 'Export for Azure'][index] }}</small></div>
          </div>
        </div>
      </main>

      <main v-else class="min-h-[calc(100vh-8rem)]">
        <aside class="hidden">
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
              <div class="mt-3 flex items-center justify-between gap-2">
                <p class="text-xs font-semibold text-slate-500">{{ draftStatus }}</p>
                <button
                  class="shrink-0 text-xs font-black text-rose-500 transition hover:text-rose-600"
                  title="Remove the locally cached draft and reset the builder"
                  @click="clearDraft"
                >
                  Clear draft
                </button>
              </div>
            </section>
          </div>
        </aside>

        <section class="review-content overflow-y-auto">
          <div v-if="builderMode === 'review'" class="review-page">
            <header class="review-page__header">
              <div>
                <div class="review-eyebrow"><span></span> Import review</div>
                <h1>Choose what moves forward.</h1>
                <p>Review the discovered APIs and fine-tune the operations before they enter your APIM package.</p>
              </div>
              <div class="review-progress" aria-label="Import progress">
                <span class="is-complete"><i>1</i> Contract</span>
                <b></b>
                <span class="is-current"><i>2</i> Review</span>
                <b></b>
                <span><i>3</i> Configure</span>
                <b></b>
                <span><i>4</i> Export</span>
              </div>
            </header>

            <div class="source-ribbon">
              <input class="hidden" type="file" accept=".json,application/json" multiple @change="handleFileUpload" />
              <span class="source-ribbon__icon">
                <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8" d="M8 3h5l5 5v13H6V3h2Zm5 0v5h5M9 13h6m-6 4h4" /></svg>
              </span>
              <span class="min-w-0 flex-1">
                <strong>{{ discoveredApis[0]?.sourceName || sourceName }}</strong>
                <small>{{ discoveredApis.length }} API · {{ totalOperationCount }} operations discovered</small>
              </span>
              <span class="source-ribbon__status"><i></i> Parsed successfully</span>
              <button @click="triggerFilePicker">Add contract</button>
            </div>

            <div class="review-layout">
              <section class="api-review-list">
                <div class="api-review-list__heading">
                  <div>
                    <h2>APIs in scope</h2>
                    <p>{{ selectedDiscoveryCount }} of {{ discoveredApis.length }} selected</p>
                  </div>
                  <button @click="openSelectionModal()">
                    Review all operations
                    <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m9 5 7 7-7 7" /></svg>
                  </button>
                </div>

                <article
                  v-for="api in discoveredApis"
                  :key="api.id"
                  :class="['api-review-card', { 'is-disabled': !api.selected }]"
                >
                  <div class="api-review-card__main">
                    <div class="api-monogram">{{ api.displayName.slice(0, 2).toUpperCase() }}</div>
                    <div class="min-w-0 flex-1">
                      <div class="api-review-card__meta">
                        <span>{{ api.format }}</span>
                        <span>v{{ api.version }}</span>
                        <span>{{ api.path || '/' }}</span>
                      </div>
                      <h3>{{ api.displayName }}</h3>
                      <p>{{ api.description || 'No description was supplied in this contract.' }}</p>
                    </div>
                    <label class="sleek-switch" title="Include this API">
                      <input v-model="api.selected" type="checkbox" @change="toggleApiSelection(api)" />
                      <span></span>
                    </label>
                  </div>

                  <div class="api-review-card__footer">
                    <div class="api-inline-stats">
                      <span><strong>{{ selectedApiOperations(api).length }}</strong>/{{ api.operations.length }} operations</span>
                      <i></i>
                      <span><strong>{{ api.schemaCount }}</strong> schemas</span>
                    </div>
                    <button @click="openSelectionModal(api.id)">
                      Configure operations
                      <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m9 5 7 7-7 7" /></svg>
                    </button>
                  </div>
                </article>
              </section>

              <aside class="import-summary">
                <div class="import-summary__glow"></div>
                <p class="import-summary__eyebrow">Import scope</p>
                <div class="import-summary__count">
                  <strong>{{ selectedOperationCount }}</strong>
                  <span>operations ready</span>
                </div>
                <div class="import-summary__progress"><span :style="{ width: `${selectionPercentage}%` }"></span></div>
                <p class="import-summary__caption">{{ selectionPercentage }}% of discovered operations selected</p>

                <dl>
                  <div><dt>APIs</dt><dd>{{ selectedDiscoveryCount }} / {{ discoveredApis.length }}</dd></div>
                  <div><dt>Operations</dt><dd>{{ selectedOperationCount }} / {{ totalOperationCount }}</dd></div>
                  <div><dt>Schemas</dt><dd>{{ selectedSchemaCount }}</dd></div>
                </dl>

                <button class="import-summary__primary" :disabled="selectedDiscoveryCount === 0" @click="confirmImport">
                  Continue to configure
                  <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m9 5 7 7-7 7" /></svg>
                </button>
                <button class="import-summary__secondary" @click="openSelectionModal()">Fine-tune selection</button>
                <button class="import-summary__reset" @click="resetImports">Start over with a new contract</button>
              </aside>
            </div>
          </div>

          <div v-else class="builder-config mx-auto max-w-7xl space-y-6">
            <div class="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
              <div>
                <p class="text-[11px] font-semibold uppercase tracking-[0.14em] text-blue-600">APIM Builder</p>
                <h2 class="mt-2 text-3xl font-bold tracking-tight text-slate-950">Configure APIs for APIM</h2>
              </div>
              <div class="flex flex-row items-center gap-3">
                <button v-if="discoveredApis.length" class="builder-action builder-action--back" @click="builderMode = 'review'">
                  <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m15 18-6-6 6-6" /></svg>
                  Back to Selection
                </button>
                <button class="builder-action builder-action--secondary" @click="openAddModal('api')">
                  + Add API
                </button>
                <button class="builder-action builder-action--primary" @click="generateProject">
                  Generate Package
                </button>
              </div>
            </div>

            <div class="grid gap-5">
              <article v-for="(api, apiIndex) in project.apis" class="rounded-2xl border border-sky-100 bg-white p-5 shadow-sm">
                <div class="mb-5 flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                  <div>
                    <p class="text-[11px] font-semibold uppercase tracking-[0.12em] text-sky-600">APIM API</p>
                    <h3 class="mt-1 text-xl font-bold tracking-tight text-slate-950">{{ api.displayName || api.name || 'Untitled API' }}</h3>
                  </div>
                  <div class="flex flex-wrap items-center gap-2">
                    <span class="rounded-full bg-blue-50 px-3 py-1 text-xs font-black text-blue-700">{{ api.operations.length }} operations</span>
                    <span class="rounded-full bg-cyan-50 px-3 py-1 text-xs font-black text-cyan-700">{{ api.specificationFormat }}</span>
                    <button class="rounded-full bg-rose-50 px-3 py-1 text-xs font-black text-rose-600 hover:bg-rose-100" title="Remove this API from the package" @click="removeApi(apiIndex)">
                      Remove
                    </button>
                  </div>
                </div>

                <div class="grid gap-4 lg:grid-cols-3">
                  <label class="space-y-2">
                    <span class="text-[11px] font-semibold uppercase tracking-[0.08em] text-slate-500">APIM Name</span>
                    <input v-model="api.name" class="w-full rounded-xl border border-sky-100 bg-sky-50/70 px-4 py-3 text-sm font-medium text-slate-950 outline-none ring-sky-300/30 focus:ring-4" />
                  </label>
                  <label class="space-y-2">
                    <span class="text-[11px] font-semibold uppercase tracking-[0.08em] text-slate-500">Display Name</span>
                    <input v-model="api.displayName" class="w-full rounded-xl border border-sky-100 bg-sky-50/70 px-4 py-3 text-sm font-medium text-slate-950 outline-none ring-sky-300/30 focus:ring-4" />
                  </label>
                  <label class="space-y-2">
                    <span class="text-[11px] font-semibold uppercase tracking-[0.08em] text-slate-500">APIM Path</span>
                    <input v-model="api.path" class="w-full rounded-xl border border-sky-100 bg-sky-50/70 px-4 py-3 text-sm font-medium text-slate-950 outline-none ring-sky-300/30 focus:ring-4" />
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
                    <div class="flex items-center gap-3">
                      <span class="text-sm font-black text-blue-700">{{ api.operations.length }} included</span>
                      <button class="rounded-xl bg-blue-600 px-3 py-2 text-xs font-black text-white hover:bg-blue-700" @click="addOperation(api)">
                        + Add operation
                      </button>
                    </div>
                  </div>
                  <div class="space-y-2">
                    <div v-for="(operation, operationIndex) in api.operations" :key="operation.name + operationIndex" class="grid gap-3 rounded-2xl border border-sky-100 bg-white p-3 md:grid-cols-[7rem_minmax(0,1fr)_minmax(0,1fr)_auto]">
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
                      <button class="grid h-9 w-9 place-items-center self-center rounded-xl text-slate-400 hover:bg-rose-50 hover:text-rose-600" title="Remove operation" @click="api.operations.splice(operationIndex, 1)">
                        <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
                      </button>
                      <textarea v-model="operation.policy" rows="4" placeholder="Operation policy XML, optional" class="md:col-span-4 w-full resize-y rounded-xl border border-sky-100 bg-slate-50 px-3 py-2 font-mono text-xs text-slate-700 outline-none focus:ring-2 focus:ring-sky-300"></textarea>
                    </div>
                    <div v-if="api.operations.length === 0" class="rounded-2xl border border-dashed border-sky-200 bg-white px-4 py-6 text-center text-sm font-semibold text-slate-500">
                      No operations yet. Add at least one so this API is usable in APIM.
                    </div>
                  </div>
                </div>
              </article>
            </div>

            <div class="grid gap-5 xl:grid-cols-3">
              <section class="rounded-[1.75rem] border border-sky-100 bg-white p-5 shadow-lg shadow-sky-100">
                <div class="mb-4 flex items-center justify-between">
                  <h3 class="text-xl font-black text-slate-950">Backends</h3>
                  <button class="rounded-xl bg-blue-600 px-3 py-2 text-xs font-black text-white" @click="openAddModal('backend')">Add</button>
                </div>
                <div class="space-y-3">
                  <div v-for="(backend, backendIndex) in project.backends" :key="backendIndex" class="space-y-2 rounded-2xl bg-sky-50 p-3">
                    <div class="flex items-center justify-between">
                      <span class="text-xs font-black uppercase tracking-[0.12em] text-sky-600">Backend</span>
                      <button class="text-slate-400 hover:text-rose-600" title="Remove backend" @click="project.backends.splice(backendIndex, 1)">
                        <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
                      </button>
                    </div>
                    <input v-model="backend.name" class="w-full rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm font-black" placeholder="backend-name" />
                    <input v-model="backend.title" class="w-full rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm" placeholder="Title" />
                    <input v-model="backend.url" class="w-full rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm" placeholder="https://backend.example.com" />
                    <select v-model="backend.protocol" class="w-full rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm">
                      <option value="https">https</option>
                      <option value="http">http</option>
                    </select>
                  </div>
                  <p v-if="project.backends.length === 0" class="rounded-xl border border-dashed border-sky-200 px-3 py-4 text-center text-xs font-semibold text-slate-500">
                    No backends yet.
                  </p>
                </div>
              </section>

              <section class="rounded-[1.75rem] border border-sky-100 bg-white p-5 shadow-lg shadow-sky-100">
                <div class="mb-4 flex items-center justify-between">
                  <h3 class="text-xl font-black text-slate-950">Products</h3>
                  <button class="rounded-xl bg-blue-600 px-3 py-2 text-xs font-black text-white" @click="openAddModal('product')">Add</button>
                </div>
                <div class="space-y-3">
                  <div v-for="(product, productIndex) in project.products" :key="productIndex" class="space-y-2 rounded-2xl bg-sky-50 p-3">
                    <div class="flex items-center justify-between">
                      <span class="text-xs font-black uppercase tracking-[0.12em] text-sky-600">Product</span>
                      <button class="text-slate-400 hover:text-rose-600" title="Remove product" @click="project.products.splice(productIndex, 1)">
                        <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
                      </button>
                    </div>
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
                  <div v-for="(namedValue, namedValueIndex) in project.namedValues" :key="namedValueIndex" class="space-y-2 rounded-2xl bg-sky-50 p-3">
                    <div class="flex items-center justify-between">
                      <span class="text-xs font-black uppercase tracking-[0.12em] text-sky-600">Named Value</span>
                      <button class="text-slate-400 hover:text-rose-600" title="Remove named value" @click="project.namedValues.splice(namedValueIndex, 1)">
                        <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
                      </button>
                    </div>
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

              <section class="rounded-[1.75rem] border border-sky-100 bg-white p-5 shadow-lg shadow-sky-100 xl:col-span-3">
                <div class="mb-4 flex items-center justify-between">
                  <h3 class="text-xl font-black text-slate-950">Policy Fragments</h3>
                  <button class="rounded-xl bg-blue-600 px-3 py-2 text-xs font-black text-white" @click="openAddModal('policyFragment')">Add</button>
                </div>
                <div class="grid gap-3 xl:grid-cols-3">
                  <div v-for="(fragment, fragmentIndex) in project.policyFragments" :key="fragmentIndex" class="space-y-2 rounded-2xl bg-sky-50 p-3">
                    <div class="flex items-center justify-between">
                      <span class="text-xs font-black uppercase tracking-[0.12em] text-sky-600">Fragment</span>
                      <button class="text-slate-400 hover:text-rose-600" title="Remove fragment" @click="project.policyFragments.splice(fragmentIndex, 1)">
                        <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
                      </button>
                    </div>
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

      <Teleport to="body">
        <div v-if="showSelectionModal" class="operation-modal-backdrop" @click.self="cancelSelectionModal">
          <div class="operation-modal" role="dialog" aria-modal="true" aria-labelledby="operation-modal-title">
            <header class="operation-modal__header">
              <div>
                <p>Import scope</p>
                <h2 id="operation-modal-title">Select operations</h2>
              </div>
              <div class="operation-modal__header-summary">
                <span><strong>{{ selectedOperationCount }}</strong> selected</span>
                <button aria-label="Close" @click="cancelSelectionModal">
                  <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8" d="M6 6l12 12M18 6 6 18" /></svg>
                </button>
              </div>
            </header>

            <div class="operation-modal__body">
              <aside class="operation-api-rail">
                <div class="operation-api-rail__summary">
                  <span><strong>{{ selectedDiscoveryCount }}</strong>/{{ discoveredApis.length }} APIs</span>
                  <span><strong>{{ selectedOperationCount }}</strong>/{{ totalOperationCount }} ops</span>
                </div>
                <p class="operation-api-rail__label">Contracts</p>
                <button
                  v-for="api in discoveredApis"
                  :key="api.id"
                  :class="['operation-api-item', { 'is-active': activeSelectionApiId === api.id }]"
                  @click="activeSelectionApiId = api.id"
                >
                  <span class="operation-api-item__mark">{{ api.displayName.slice(0, 2).toUpperCase() }}</span>
                  <span class="min-w-0 flex-1">
                    <strong>{{ api.displayName }}</strong>
                    <small>{{ selectedApiOperations(api).length }} of {{ api.operations.length }} selected</small>
                  </span>
                  <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m9 5 7 7-7 7" /></svg>
                </button>
              </aside>

              <section v-if="activeSelectionApi" class="operation-workspace">
                <div class="operation-api-header">
                  <div class="min-w-0 flex-1">
                    <div class="operation-api-header__meta">
                      <span>{{ activeSelectionApi.format }}</span><span>v{{ activeSelectionApi.version }}</span><span>{{ activeSelectionApi.path || '/' }}</span>
                    </div>
                    <h3>{{ activeSelectionApi.displayName }}</h3>
                    <p>{{ activeSelectionApi.description || 'No description provided.' }}</p>
                  </div>
                  <label class="sleek-switch" title="Include this API">
                    <input v-model="activeSelectionApi.selected" type="checkbox" @change="toggleApiSelection(activeSelectionApi)" />
                    <span></span>
                  </label>
                </div>

                <div class="operation-toolbar">
                  <div class="operation-search">
                    <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8" d="m21 21-4.35-4.35m2.35-5.15a7.5 7.5 0 1 1-15 0 7.5 7.5 0 0 1 15 0Z" /></svg>
                    <input v-model="operationSearch" type="search" placeholder="Filter operations…" />
                  </div>
                  <div class="operation-toolbar__actions">
                    <button @click="toggleApiOperations(activeSelectionApi, true)">Select all</button>
                    <button @click="toggleApiOperations(activeSelectionApi, false)">Clear</button>
                  </div>
                </div>

                <div class="operation-list">
                  <label
                    v-for="operation in filteredActiveOperations"
                    :key="operation.name"
                    :class="['operation-row', { 'is-unselected': !operation.selected }]"
                  >
                    <input v-model="operation.selected" type="checkbox" @change="syncApiSelection(activeSelectionApi)" />
                    <span :class="['operation-method', methodClass(operation.method)]">{{ operation.method }}</span>
                    <span class="operation-path">{{ operation.urlTemplate }}</span>
                    <span class="operation-name">{{ operation.displayName }}</span>
                  </label>
                  <div v-if="filteredActiveOperations.length === 0" class="operation-empty">No operations match “{{ operationSearch }}”.</div>
                </div>
              </section>
            </div>

            <footer class="operation-modal__footer">
              <p><strong>{{ selectedOperationCount }}</strong> operations across <strong>{{ selectedDiscoveryCount }}</strong> APIs will be imported.</p>
              <div>
                <button class="operation-cancel" @click="cancelSelectionModal">Cancel</button>
                <button class="operation-apply" @click="acceptSelectionModal">
                  Apply selection
                  <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m9 5 7 7-7 7" /></svg>
                </button>
              </div>
            </footer>
          </div>
        </div>
      </Teleport>

      <div v-if="artifactModalKind" class="fixed inset-0 z-[60] grid place-items-center bg-slate-950/45 p-4 backdrop-blur-sm">
        <div class="max-h-[calc(100vh-2rem)] w-full max-w-3xl overflow-hidden rounded-[2rem] border border-sky-100 bg-white shadow-2xl shadow-slate-900/20">
          <header class="border-b border-sky-100 bg-gradient-to-r from-white to-sky-50 px-6 py-5">
            <p class="text-xs font-black uppercase tracking-[0.35em] text-blue-500">{{ artifactModalEyebrow }}</p>
            <h2 class="mt-1 text-3xl font-black text-slate-950">{{ artifactModalTitle }}</h2>
          </header>

          <div class="max-h-[calc(100vh-15rem)] overflow-y-auto px-6 py-5">
            <div v-if="artifactModalKind === 'api'" class="grid gap-4">
              <div class="grid gap-4 sm:grid-cols-2">
                <label class="space-y-2">
                  <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">APIM Name</span>
                  <input v-model="apiDraft.name" class="w-full rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-sm font-black text-slate-950 outline-none ring-sky-300/30 focus:ring-4" placeholder="orders-api" />
                </label>
                <label class="space-y-2">
                  <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Display Name</span>
                  <input v-model="apiDraft.displayName" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4" placeholder="Orders API" />
                </label>
              </div>
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">APIM Path</span>
                <input v-model="apiDraft.path" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4" placeholder="/orders/v1" />
              </label>
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Backend Service URL</span>
                <input v-model="apiDraft.serviceUrl" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4" placeholder="https://orders.internal.example.com" />
              </label>
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Description</span>
                <textarea v-model="apiDraft.description" rows="3" class="w-full resize-none rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-700 outline-none ring-sky-300/30 focus:ring-4" placeholder="What this API does"></textarea>
              </label>
              <p class="rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-xs font-semibold text-slate-600">
                Operations can be added on the API card after saving. A backend entry is created automatically when a service URL is provided.
              </p>
            </div>

            <div v-else-if="artifactModalKind === 'backend'" class="grid gap-4">
              <div class="grid gap-4 sm:grid-cols-2">
                <label class="space-y-2">
                  <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Name</span>
                  <input v-model="backendDraft.name" class="w-full rounded-2xl border border-sky-100 bg-sky-50 px-4 py-3 text-sm font-black text-slate-950 outline-none ring-sky-300/30 focus:ring-4" placeholder="orders-backend" />
                </label>
                <label class="space-y-2">
                  <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Title</span>
                  <input v-model="backendDraft.title" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4" placeholder="Orders backend" />
                </label>
              </div>
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">URL</span>
                <input v-model="backendDraft.url" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4" placeholder="https://orders.internal.example.com" />
              </label>
              <label class="space-y-2">
                <span class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Protocol</span>
                <select v-model="backendDraft.protocol" class="w-full rounded-2xl border border-sky-100 bg-white px-4 py-3 text-sm text-slate-800 outline-none ring-sky-300/30 focus:ring-4">
                  <option value="https">https</option>
                  <option value="http">http</option>
                </select>
              </label>
            </div>

            <div v-else-if="artifactModalKind === 'product'" class="grid gap-4">
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
import { projectService } from '@/services/api'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE' | 'HEAD' | 'OPTIONS' | 'TRACE'
type ArtifactModalKind = 'api' | 'backend' | 'product' | 'namedValue' | 'policyFragment'

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
const operationSearch = ref('')
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

const apiDraft = reactive(createApiDraft())
const backendDraft = reactive(createBackendDraft())
const productDraft = reactive(createProductDraft())
const namedValueDraft = reactive(createNamedValueDraft())
const fragmentDraft = reactive(createPolicyFragmentDraft())

const pipelineSteps = ['Parse contract', 'Extract paths', 'Review import', 'Generate package']

const selectedDiscoveryCount = computed(() => discoveredApis.value.filter(api => api.selected).length)
const selectedOperationCount = computed(() =>
  discoveredApis.value.reduce((count, api) => count + (api.selected ? selectedApiOperations(api).length : 0), 0)
)
const totalOperationCount = computed(() =>
  discoveredApis.value.reduce((count, api) => count + api.operations.length, 0)
)
const selectedSchemaCount = computed(() =>
  discoveredApis.value.reduce((count, api) => count + (api.selected ? api.schemaCount : 0), 0)
)
const selectionPercentage = computed(() =>
  totalOperationCount.value === 0 ? 0 : Math.round((selectedOperationCount.value / totalOperationCount.value) * 100)
)
const activeSelectionApi = computed(() =>
  discoveredApis.value.find(api => api.id === activeSelectionApiId.value) ?? discoveredApis.value[0] ?? null
)
const filteredActiveOperations = computed(() => {
  const query = operationSearch.value.trim().toLowerCase()
  const operations = activeSelectionApi.value?.operations ?? []
  if (!query) return operations
  return operations.filter(operation =>
    `${operation.method} ${operation.urlTemplate} ${operation.displayName}`.toLowerCase().includes(query)
  )
})
const artifactModalEyebrow = computed(() => {
  if (artifactModalKind.value === 'api') return 'APIM API'
  if (artifactModalKind.value === 'backend') return 'APIM Backend'
  if (artifactModalKind.value === 'product') return 'APIM Product'
  if (artifactModalKind.value === 'namedValue') return 'APIM Named Value'
  return 'APIM Policy Fragment'
})
const artifactModalTitle = computed(() => {
  if (artifactModalKind.value === 'api') return 'Add API manually'
  if (artifactModalKind.value === 'backend') return 'Add backend'
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
  operationSearch.value = ''
  showSelectionModal.value = true
}

function acceptSelectionModal() {
  showSelectionModal.value = false
  selectionSnapshot.value = ''
  operationSearch.value = ''
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
  operationSearch.value = ''
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

function removeApi(index: number) {
  project.apis.splice(index, 1)
  syncProjectArtifacts()
}

function addOperation(api: any) {
  const operationNumber = api.operations.length + 1
  api.operations.push({
    name: `custom-operation-${Date.now()}-${operationNumber}`,
    displayName: `New operation ${operationNumber}`,
    method: 'GET',
    urlTemplate: `/operation-${operationNumber}`,
    description: '',
    policy: '',
    selected: true
  })
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

function startManually() {
  parseError.value = ''
  hasAnalyzed.value = true
  builderMode.value = 'builder'
  if (project.apis.length === 0) {
    openAddModal('api')
  }
}

function createApiDraft() {
  const index = project?.apis?.length ? project.apis.length + 1 : 1

  return {
    name: `api-${index}`,
    displayName: `API ${index}`,
    path: '',
    serviceUrl: '',
    description: ''
  }
}

function createBackendDraft() {
  const index = project?.backends?.length ? project.backends.length + 1 : 1

  return {
    name: `backend-${index}`,
    title: `Backend ${index}`,
    url: '',
    protocol: 'https'
  }
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

  if (kind === 'api') {
    Object.assign(apiDraft, createApiDraft())
  } else if (kind === 'backend') {
    Object.assign(backendDraft, createBackendDraft())
  } else if (kind === 'product') {
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
  if (artifactModalKind.value === 'api') {
    const name = apiDraft.name.trim() || `api-${project.apis.length + 1}`
    const displayName = apiDraft.displayName.trim() || name
    const serviceUrl = apiDraft.serviceUrl.trim()

    project.apis.push({
      name,
      displayName,
      path: apiDraft.path.trim(),
      description: apiDraft.description,
      protocols: ['https'],
      serviceUrl,
      subscriptionKeyParameterName: 'Ocp-Apim-Subscription-Key',
      apiVersion: 'v1',
      apiVersionSetId: `${name}-version-set`,
      apiRevision: '1',
      isCurrent: true,
      subscriptionRequired: true,
      policy: defaultApiPolicy(),
      operations: [] as any[],
      specificationFormat: 'openapi',
      specificationContent: ''
    })

    if (serviceUrl) {
      project.backends.push({
        name: `${name}-backend`,
        title: `${displayName} backend`,
        url: serviceUrl,
        protocol: serviceUrl.startsWith('https') ? 'https' : 'http',
        properties: {}
      })
    }

    if (!closeAfterSave) Object.assign(apiDraft, createApiDraft())
  } else if (artifactModalKind.value === 'backend') {
    project.backends.push({
      ...backendDraft,
      name: backendDraft.name.trim() || `backend-${project.backends.length + 1}`,
      title: backendDraft.title.trim() || backendDraft.name.trim() || `Backend ${project.backends.length + 1}`,
      properties: {}
    })

    if (!closeAfterSave) Object.assign(backendDraft, createBackendDraft())
  } else if (artifactModalKind.value === 'product') {
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

function clearDraft() {
  if (typeof window === 'undefined') return
  if (!confirm('Remove the locally saved draft and reset the builder?')) return

  if (draftSaveTimer) {
    window.clearTimeout(draftSaveTimer)
    draftSaveTimer = undefined
  }
  window.localStorage.removeItem(DRAFT_STORAGE_KEY)

  isRestoringDraft.value = true
  projectName.value = 'My APIM Project'
  projectDescription.value = 'Generated from a reviewed OpenAPI or Swagger contract.'
  sourceName.value = ''
  rawSpec.value = ''
  parseError.value = ''
  hasAnalyzed.value = false
  builderMode.value = 'review'
  discoveredApis.value = []
  project.apis.splice(0)
  project.backends.splice(0)
  project.namedValues.splice(0)
  project.policyFragments.splice(0)
  project.products.splice(0)
  project.schemas.splice(0)
  draftStatus.value = 'Draft cleared. Autosave resumes when you make changes.'

  // Release the guard after the deep watcher has flushed so the reset itself isn't re-saved
  window.setTimeout(() => {
    isRestoringDraft.value = false
  }, 0)
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
    const blob = await projectService.downloadProject(projectData)
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
