import {api} from "@/services/api.ts";
import type {CategoryListing} from "@/types/category/categoryListing.ts";

export const categoryService = {
  async listCategories(organizationId: string): Promise<CategoryListing[]> {
    const result = await api.get<any>(`/api/Organization/${organizationId}/categories`)
    return result.categories
  }
}
