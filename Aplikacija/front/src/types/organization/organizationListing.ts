import type {CategoryListing} from "@/types/category/categoryListing.ts";

export interface OrganizationListing {
  organizationId: string;
  organizationName: string;
  categories?: CategoryListing[];
}
