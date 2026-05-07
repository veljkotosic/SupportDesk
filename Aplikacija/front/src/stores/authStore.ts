import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import { authService } from '@/services/auth/authService'
import type { Me } from '@/types/user/me'
import type { LoginInput } from '@/types/auth/loginInput'
import type { RegisterCustomerInput } from '@/types/auth/registerCustomerInput'
import type { RegisterOrganizationInput } from '@/types/auth/registerOrganizationInput'
import type { RegisterSupportAgentInput } from '@/types/auth/registerSupportAgentInput'
import router from "@/router";

export const useAuthStore = defineStore('auth', () => {
  const user = ref<Me | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => !!user.value)

  async function initialize() {
    isLoading.value = true
    error.value = null
    try {
      user.value = await authService.getMe()
    } catch (e) {
      user.value = null
    } finally {
      isLoading.value = false
    }
  }

  async function login(loginInput: LoginInput) {
    isLoading.value = true
    error.value = null
    try {
      await authService.login(loginInput)
      user.value = await authService.getMe()
    } catch (e: any) {
      error.value = e?.message ?? 'Login failed'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function registerCustomer(registerCustomerInput: RegisterCustomerInput) {
    isLoading.value = true
    error.value = null
    try {
      await authService.registerCustomer(registerCustomerInput)
      user.value = await authService.getMe()
    } catch (e: any) {
      error.value = e?.message ?? 'Signup failed'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function registerOrganization(registerOrganizationInput: RegisterOrganizationInput) {
    isLoading.value = true
    error.value = null
    try {
      await authService.registerOrganization(registerOrganizationInput)
      user.value = await authService.getMe()
    } catch (e: any) {
      error.value = e?.message ?? 'Signup failed'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function registerSupportAgent(registerSupportAgentInput: RegisterSupportAgentInput) {
    isLoading.value = true
    error.value = null
    try {
      await authService.registerSupportAgent(registerSupportAgentInput)
      user.value = await authService.getMe()
    } catch (e: any) {
      error.value = e?.message ?? 'Signup failed'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function logout() {
    isLoading.value = true
    try {
      await authService.logout()
    } catch (e) {
      console.warn('Logout request failed', e)
    } finally {
      user.value = null
      isLoading.value = false
      await router.push('/login')
    }
  }

  return {
    user,
    isAuthenticated,
    isLoading,
    error,
    initialize,
    registerCustomer,
    registerOrganization,
    registerSupportAgent,
    login,
    logout,
  }
})
