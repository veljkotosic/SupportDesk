import { api } from '.././api'
import type {LoginInput} from "@/types/auth/loginInput.ts";
import type {RegisterCustomerInput} from "@/types/auth/registerCustomerInput.ts";
import type {RegisterOrganizationInput} from "@/types/auth/registerOrganizationInput.ts";
import type {RegisterSupportAgentInput} from "@/types/auth/registerSupportAgentInput.ts";
import type {Me} from "@/types/user/me.ts";

const BASE_URL: string = '/api/Auth'

export const authService = {
  async login(loginInput: LoginInput): Promise<void> {
    await api.post<any>(`${BASE_URL}/login`, loginInput)
  },

  async refreshLogin(): Promise<void> {
    await api.post<any>(`${BASE_URL}/refreshLogin`)
  },

  async registerCustomer(registerCustomerInput: RegisterCustomerInput): Promise<void> {
    await api.post<any>(`${BASE_URL}/registerCustomer`, registerCustomerInput)
  },

  async registerOrganization(registerOrganizationInput: RegisterOrganizationInput): Promise<void> {
    await api.post<any>(`${BASE_URL}/registerOrganization`, registerOrganizationInput)
  },

  async registerSupportAgent(registerSupportAgentInput: RegisterSupportAgentInput): Promise<void> {
    await api.post<any>(`${BASE_URL}/registerSupportAgent`, registerSupportAgentInput)
  },

  async logout(): Promise<void> {
    return api.delete<void>(`${BASE_URL}/logout`)
  },

  async logoutAll(): Promise<void> {
    return api.delete<void>(`${BASE_URL}/logoutAll`)
  },

  async getMe() : Promise<Me> {
    return await api.get<Me>(`${BASE_URL}/me`)
  },
}
