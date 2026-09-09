import {api} from "@/services/api";
import type {CategoryListing} from "@/types/category/categoryListing.ts";

const BASE_URL = '/api/v1/category'

export const categoryService = {
  async listCategories(organizationId: string): Promise<CategoryListing[]> {
    const result = await api.get<any>(`/api/v1/organization/${organizationId}/categories`)
    return result.categories
  },

  async addCategory(name: string, description: string): Promise<void> {
    await api.post(BASE_URL, { name, description })
  },

  async updateCategory(categoryId: string, name: string | undefined, description: string | undefined): Promise<void> {
    await api.patch(`${BASE_URL}/${categoryId}`, { name, description })
  },

  async removeCategory(categoryId: string): Promise<void> {
    await api.delete(`${BASE_URL}/${categoryId}`)
  },
}
