import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { organizationSettingsService } from '@/services/organizationAdmin/organizationSettingsService.ts'
import type {
  CategorySetting,
  FaqSetting,
  TemplateAnswerSetting,
} from '@/types/organizationSettings/organizationSettings.ts'

export type SettingsTab = 'faqs' | 'templates' | 'categories'

export const useOrganizationSettingsStore = defineStore('organizationSettings', () => {
  const faqs = ref<FaqSetting[]>([])
  const templateAnswers = ref<TemplateAnswerSetting[]>([])
  const categories = ref<CategorySetting[]>([])
  const activeTab = ref<SettingsTab>('faqs')
  const searchQuery = ref('')
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const normalizedSearch = computed(() => searchQuery.value.trim().toLowerCase())
  const filteredFaqs = computed(() => faqs.value.filter(item =>
    !normalizedSearch.value ||
    item.question.toLowerCase().includes(normalizedSearch.value) ||
    item.answer.toLowerCase().includes(normalizedSearch.value),
  ))
  const filteredTemplateAnswers = computed(() => templateAnswers.value.filter(item =>
    !normalizedSearch.value ||
    item.title.toLowerCase().includes(normalizedSearch.value) ||
    item.text.toLowerCase().includes(normalizedSearch.value),
  ))
  const filteredCategories = computed(() => categories.value.filter(item =>
    !normalizedSearch.value ||
    item.name.toLowerCase().includes(normalizedSearch.value) ||
    item.description.toLowerCase().includes(normalizedSearch.value),
  ))

  async function loadSettings() {
    isLoading.value = true
    error.value = null
    try {
      const result = await organizationSettingsService.getSettings()
      faqs.value = result.faqs
      templateAnswers.value = result.templateAnswers
      categories.value = result.categories
    } catch (e: any) {
      error.value = e?.message ?? 'Failed to load settings'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function mutate(action: () => Promise<void>) {
    error.value = null
    try {
      await action()
      await loadSettings()
    } catch (e: any) {
      error.value = e?.message ?? 'Failed to save settings'
      throw e
    }
  }

  function setActiveTab(tab: SettingsTab) {
    activeTab.value = tab
    searchQuery.value = ''
  }

  function clear() {
    faqs.value = []
    templateAnswers.value = []
    categories.value = []
    activeTab.value = 'faqs'
    searchQuery.value = ''
    error.value = null
  }

  return {
    faqs,
    templateAnswers,
    categories,
    activeTab,
    searchQuery,
    isLoading,
    error,
    filteredFaqs,
    filteredTemplateAnswers,
    filteredCategories,
    loadSettings,
    setActiveTab,
    clear,
    addFaq: (question: string, answer: string) => mutate(() => organizationSettingsService.addFaq(question, answer)),
    updateFaq: (id: string, question: string, answer: string) => mutate(() => organizationSettingsService.updateFaq(id, question, answer)),
    removeFaq: (id: string) => mutate(() => organizationSettingsService.removeFaq(id)),
    addTemplate: (title: string, text: string) => mutate(() => organizationSettingsService.addTemplateAnswer(title, text)),
    updateTemplate: (id: string, title: string, text: string) => mutate(() => organizationSettingsService.updateTemplateAnswer(id, title, text)),
    removeTemplate: (id: string) => mutate(() => organizationSettingsService.removeTemplateAnswer(id)),
    addCategory: (name: string, description: string) => mutate(() => organizationSettingsService.addCategory(name, description)),
    updateCategory: (id: string, name: string, description: string) => mutate(() => organizationSettingsService.updateCategory(id, name, description)),
    removeCategory: (id: string) => mutate(() => organizationSettingsService.removeCategory(id)),
  }
})
