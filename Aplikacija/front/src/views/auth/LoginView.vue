<script setup lang="ts">
import {reactive} from 'vue'
import { ArrowRight } from 'lucide-vue-next'
import AuthLogo from '../../components/AuthLogo.vue'
import PasswordField from '../../components/PasswordField.vue'
import type {LoginInput} from "@/types/auth/loginInput.ts";

import { useAuthStore } from "@/stores/authStore.ts";
import {UserType} from "@/types/user/userType.ts";
import router from "@/router";

const authStore = useAuthStore()

const loginForm = reactive<LoginInput>({ email: '', password: '' })

async function handleLogin() {
  try {
    await authStore.login(loginForm)
    const user = authStore.user!

    if (user.type === UserType.Customer) {
      await router.push({ name: 'customerDashboard' })
    } else if (user.type === UserType.SupportAgent) {
      await router.push({ name: 'supportAgentDashboard' });
    } else if (user.type === UserType.OrganizationAdmin) {
      await router.push({ name: 'organizationDashboard' });
    }

  } catch (e: any) {

  }
}
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 flex items-center justify-center px-4">
    <div class="w-full max-w-md">
      <AuthLogo />

      <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 shadow-sm p-8">
        <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-1.5 tracking-tight">
          Sign in to your SupportDesk account
        </h2>

        <form class="space-y-4" @submit.prevent="handleLogin">
          <div>
            <label
              for="login-email"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5"
            >
              Email address
            </label>
            <input
              id="login-email"
              v-model="loginForm.email"
              type="email"
              placeholder="your@email.com"
              class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all text-sm"
            />
          </div>

          <PasswordField
            id="login-password"
            v-model="loginForm.password"
            label="Password"
            placeholder="Enter your password"
          />

          <button
            type="submit"
            class="w-full flex items-center justify-center gap-2 bg-blue-500 hover:bg-blue-600 text-white py-2.5 rounded-xl font-medium transition-all shadow-sm mt-2 text-sm"
          >
            Sign In
            <ArrowRight :size="16" />
          </button>
        </form>
      </div>

      <p class="text-center text-sm text-gray-500 dark:text-gray-400 mt-5">
        Don't have an account?
        <RouterLink to="/register" class="text-blue-500 hover:text-blue-600 font-medium">
          Create one here
        </RouterLink>
      </p>
    </div>
  </div>
</template>
