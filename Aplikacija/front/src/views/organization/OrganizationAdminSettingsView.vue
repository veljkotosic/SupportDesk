<script setup lang="ts">
import { computed, onBeforeMount, onBeforeUnmount, reactive, ref } from 'vue'
import { Edit2, FileText, HelpCircle, Plus, Search, Tag, Trash2, X } from 'lucide-vue-next'
import OrganizationAdminLayout from '@/layouts/OrganizationAdminDashboardLayout.vue'
import { type SettingsTab, useOrganizationSettingsStore } from '@/stores/organizationSettingsStore.ts'
import ErrorBanner from '@/components/ErrorBanner.vue'

const settingsStore = useOrganizationSettingsStore()
const showForm = ref(false)
const editingId = ref<string | null>(null)
const form = reactive({ primary: '', secondary: '' })

const tabs: { id: SettingsTab; label: string; icon: typeof HelpCircle }[] = [
  { id: 'faqs', label: 'FAQ Items', icon: HelpCircle },
  { id: 'templates', label: 'Template Answers', icon: FileText },
  { id: 'categories', label: 'Categories', icon: Tag },
]

const actionLabel = computed(() => ({
  faqs: 'Add FAQ',
  templates: 'Add Template',
  categories: 'Add Category',
})[settingsStore.activeTab])

const formTitle = computed(() => {
  const action = editingId.value ? 'Edit' : 'New'
  return `${action} ${settingsStore.activeTab === 'faqs' ? 'FAQ Item' : settingsStore.activeTab === 'templates' ? 'Template Answer' : 'Category'}`
})

onBeforeMount(settingsStore.loadSettings)
onBeforeUnmount(settingsStore.clear)

function handleTabChange(tab: SettingsTab) {
  settingsStore.setActiveTab(tab)
  handleCloseForm()
}

function handleOpenForm() {
  editingId.value = null
  form.primary = ''
  form.secondary = ''
  showForm.value = true
}

function handleCloseForm() {
  showForm.value = false
  editingId.value = null
  form.primary = ''
  form.secondary = ''
}

function handleEditFaq(id: string, question: string, answer: string) {
  editingId.value = id
  form.primary = question
  form.secondary = answer
  showForm.value = true
}

function handleEditTemplate(id: string, title: string, text: string) {
  editingId.value = id
  form.primary = title
  form.secondary = text
  showForm.value = true
}

function handleEditCategory(id: string, name: string, description: string) {
  editingId.value = id
  form.primary = name
  form.secondary = description
  showForm.value = true
}

async function handleSave() {
  const primary = form.primary.trim()
  const secondary = form.secondary.trim()
  if (!primary || !secondary) {
    return
  }

  try {
    if (settingsStore.activeTab === 'faqs') {
      if (editingId.value) await settingsStore.updateFaq(editingId.value, primary, secondary)
      else await settingsStore.addFaq(primary, secondary)
    } else if (settingsStore.activeTab === 'templates') {
      if (editingId.value) await settingsStore.updateTemplate(editingId.value, primary, secondary)
      else await settingsStore.addTemplate(primary, secondary)
    } else {
      if (editingId.value) await settingsStore.updateCategory(editingId.value, primary, secondary)
      else await settingsStore.addCategory(primary, secondary)
    }

    handleCloseForm()
  } catch (e: any) {

  }
}

async function handleRemoveFaq(id: string) {
  try {
    await settingsStore.removeFaq(id)
  } catch (e: any) {

  }
}

async function handleRemoveTemplate(id: string) {
  try {
    await settingsStore.removeTemplate(id)
  } catch (e: any) {

  }
}

async function handleRemoveCategory(id: string) {
  try {
    await settingsStore.removeCategory(id)
  } catch (e: any) {

  }
}
</script>

<template>
  <OrganizationAdminLayout active-path="/organization/settings">
    <div class="p-6 max-w-4xl mx-auto">
      <div class="flex items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-2xl font-bold text-gray-900 dark:text-white tracking-tight">Settings</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage FAQs, template answers, and ticket categories
          </p>
        </div>
        <button
          type="button"
          class="flex items-center gap-2 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-xl text-sm font-medium transition-all shadow-sm"
          @click="handleOpenForm"
        >
          <Plus :size="15" />
          {{ actionLabel }}
        </button>
      </div>

      <div class="flex gap-1 p-1 bg-gray-100 dark:bg-gray-800 rounded-xl mb-6 w-fit max-w-full overflow-x-auto">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          type="button"
          class="flex items-center gap-1.5 px-4 py-2 rounded-lg text-sm font-medium whitespace-nowrap transition-all"
          :class="settingsStore.activeTab === tab.id
            ? 'bg-white dark:bg-gray-900 text-gray-900 dark:text-white shadow-sm'
            : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300'"
          @click="handleTabChange(tab.id)"
        >
          <component :is="tab.icon" :size="14" />
          {{ tab.label }}
        </button>
      </div>

      <div v-if="showForm" class="bg-white dark:bg-gray-900 rounded-2xl border border-blue-200 dark:border-blue-800 shadow-sm p-5 mb-5">
        <div class="flex items-center justify-between mb-4">
          <h2 class="font-semibold text-gray-900 dark:text-white">{{ formTitle }}</h2>
          <button type="button" class="p-1 text-gray-400 hover:text-gray-600" @click="handleCloseForm">
            <X :size="15" />
          </button>
        </div>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
              {{ settingsStore.activeTab === 'faqs' ? 'Question' : settingsStore.activeTab === 'templates' ? 'Title' : 'Category Name' }}
            </label>
            <input
              v-model="form.primary"
              type="text"
              class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
              {{ settingsStore.activeTab === 'faqs' ? 'Answer' : settingsStore.activeTab === 'templates' ? 'Response Content' : 'Description' }}
            </label>
            <textarea
              v-model="form.secondary"
              rows="4"
              class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all resize-none"
            />
          </div>
          <div class="flex gap-3">
            <button type="button" class="px-4 py-2 rounded-xl border border-gray-200 dark:border-gray-700 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800" @click="handleCloseForm">
              Cancel
            </button>
            <button type="button" class="px-5 py-2 rounded-xl bg-blue-500 hover:bg-blue-600 text-white text-sm font-medium" @click="handleSave">
              {{ editingId ? 'Save Changes' : actionLabel }}
            </button>
          </div>
        </div>
      </div>

      <ErrorBanner class="mb-4" :message="settingsStore.error" />

      <div class="relative mb-5">
        <Search :size="15" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-gray-400" />
        <input
          v-model="settingsStore.searchQuery"
          type="text"
          :placeholder="`Search ${settingsStore.activeTab === 'faqs' ? 'FAQs' : settingsStore.activeTab === 'templates' ? 'templates' : 'categories'}...`"
          class="w-full pl-9 pr-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-gray-900 dark:text-white placeholder-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all"
        />
      </div>

      <div v-if="settingsStore.activeTab === 'faqs'" class="space-y-3">
        <div v-if="settingsStore.filteredFaqs.length === 0" class="text-center py-10 text-gray-400">No FAQ items found</div>
        <div v-for="item in settingsStore.filteredFaqs" :key="item.id" class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-4">
          <div class="flex items-start justify-between gap-4">
            <div>
              <h3 class="font-medium text-gray-900 dark:text-white text-sm mb-2">{{ item.question }}</h3>
              <p class="text-sm text-gray-500 dark:text-gray-400 leading-relaxed">{{ item.answer }}</p>
            </div>
            <div class="flex gap-1 flex-shrink-0">
              <button type="button" class="p-1.5 rounded-lg text-gray-400 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-900/20" @click="handleEditFaq(item.id, item.question, item.answer)"><Edit2 :size="14" /></button>
              <button type="button" class="p-1.5 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20" @click="handleRemoveFaq(item.id)"><Trash2 :size="14" /></button>
            </div>
          </div>
        </div>
      </div>

      <div v-else-if="settingsStore.activeTab === 'templates'" class="space-y-3">
        <div v-if="settingsStore.filteredTemplateAnswers.length === 0" class="text-center py-10 text-gray-400">No template answers found</div>
        <div v-for="item in settingsStore.filteredTemplateAnswers" :key="item.id" class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-4">
          <div class="flex items-start justify-between gap-4">
            <div>
              <h3 class="font-medium text-gray-900 dark:text-white text-sm mb-2">{{ item.title }}</h3>
              <p class="text-sm text-gray-500 dark:text-gray-400 leading-relaxed">{{ item.text }}</p>
            </div>
            <div class="flex gap-1 flex-shrink-0">
              <button type="button" class="p-1.5 rounded-lg text-gray-400 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-900/20" @click="handleEditTemplate(item.id, item.title, item.text)"><Edit2 :size="14" /></button>
              <button type="button" class="p-1.5 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20" @click="handleRemoveTemplate(item.id)"><Trash2 :size="14" /></button>
            </div>
          </div>
        </div>
      </div>

      <div v-else class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div v-if="settingsStore.filteredCategories.length === 0" class="sm:col-span-2 text-center py-10 text-gray-400">No categories found</div>
        <div v-for="item in settingsStore.filteredCategories" :key="item.id" class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-4 flex items-start gap-3">
          <div class="w-10 h-10 rounded-xl bg-violet-500 flex items-center justify-center flex-shrink-0"><Tag :size="17" class="text-white" /></div>
          <div class="flex-1 min-w-0">
            <div class="flex items-start justify-between gap-2">
              <h3 class="font-medium text-gray-900 dark:text-white text-sm">{{ item.name }}</h3>
              <div class="flex gap-1 flex-shrink-0">
                <button type="button" class="p-1.5 rounded-lg text-gray-400 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-900/20" @click="handleEditCategory(item.id, item.name, item.description)"><Edit2 :size="13" /></button>
                <button
                  type="button"
                  class="p-1.5 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 disabled:opacity-30 disabled:cursor-not-allowed disabled:hover:text-gray-400 disabled:hover:bg-transparent"
                  :disabled="item.ticketCount > 0"
                  :title="item.ticketCount > 0 ? 'Categories with tickets cannot be deleted' : 'Delete category'"
                  @click="handleRemoveCategory(item.id)"
                >
                  <Trash2 :size="13" />
                </button>
              </div>
            </div>
            <p class="text-xs text-gray-500 dark:text-gray-400 leading-relaxed mt-1 mb-2">{{ item.description }}</p>
            <span class="text-xs font-medium text-gray-500 dark:text-gray-400 bg-gray-100 dark:bg-gray-800 px-2 py-0.5 rounded-md">{{ item.ticketCount }} tickets</span>
          </div>
        </div>
      </div>
    </div>
  </OrganizationAdminLayout>
</template>
