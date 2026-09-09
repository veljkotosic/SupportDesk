export interface FaqKnowledgeBase {
  id: string
  question: string
  answer: string
}

export interface TemplateAnswerKnowledgeBase {
  id: string
  title: string
  text: string
}

export interface CategoryKnowledgeBase {
  id: string
  name: string
  description: string
  ticketCount: number
}

export interface OrganizationKnowledgeBase {
  organizationId: string
  faqs: FaqKnowledgeBase[]
  templateAnswers: TemplateAnswerKnowledgeBase[]
  categories: CategoryKnowledgeBase[]
}
