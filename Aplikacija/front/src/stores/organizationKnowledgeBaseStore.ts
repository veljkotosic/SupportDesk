import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import type {
  CategoryKnowledgeBase,
  FaqKnowledgeBase,
  TemplateAnswerKnowledgeBase
} from "@/types/organizationSettings/organizationKnowledgeBase.ts";
import {organizationService} from "@/services/organization/organizationService.ts";
import {faqService} from "@/services/faq/faqService.ts";
import {templateAnswerService} from "@/services/templateAnswer/templateAnswerService.ts";
import {categoryService} from "@/services/category/categoryService.ts";

export type KnowledgeBaseTab = 'faqs' | 'templates' | 'categories'

export const useOrganizationKnowledgeBaseStore = defineStore('organizationKnowledgeBase', () => {
  const organizationId = ref('')
  const faqs = ref<FaqKnowledgeBase[]>([])
  const templateAnswers = ref<TemplateAnswerKnowledgeBase[]>([])
  const categories = ref<CategoryKnowledgeBase[]>([])
  const activeTab = ref<KnowledgeBaseTab>('faqs')
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
      const result = await organizationService.getKnowledgeBase()
      organizationId.value = result.organizationId
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

  function setActiveTab(tab: KnowledgeBaseTab) {
    activeTab.value = tab
    searchQuery.value = ''
  }

  function clear() {
    organizationId.value = ''
    faqs.value = []
    templateAnswers.value = []
    categories.value = []
    activeTab.value = 'faqs'
    searchQuery.value = ''
    error.value = null
  }

  return {
    organizationId,
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
    addFaq: (question: string, answer: string) => mutate(() => faqService.addFaq(question, answer)),
    updateFaq: (id: string, question: string, answer: string) => mutate(() => faqService.updateFaq(id, question, answer)),
    removeFaq: (id: string) => mutate(() => faqService.removeFaq(id)),
    addTemplate: (title: string, text: string) => mutate(() => templateAnswerService.addTemplateAnswer(title, text)),
    updateTemplate: (id: string, title: string, text: string) => mutate(() => templateAnswerService.updateTemplateAnswer(id, title, text)),
    removeTemplate: (id: string) => mutate(() => templateAnswerService.removeTemplateAnswer(id)),
    addCategory: (name: string, description: string) => mutate(() => categoryService.addCategory(name, description)),
    updateCategory: (id: string, name: string, description: string) => mutate(() => categoryService.updateCategory(id, name, description)),
    removeCategory: (id: string) => mutate(() => categoryService.removeCategory(id)),
  }
})
