export interface FaqSetting {
  id: string
  question: string
  answer: string
}

export interface TemplateAnswerSetting {
  id: string
  title: string
  text: string
}

export interface CategorySetting {
  id: string
  name: string
  description: string
  ticketCount: number
}

export interface OrganizationSettings {
  organizationId: string
  faqs: FaqSetting[]
  templateAnswers: TemplateAnswerSetting[]
  categories: CategorySetting[]
}
