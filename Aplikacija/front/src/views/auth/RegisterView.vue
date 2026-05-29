<script setup lang="ts">
import {type Component, computed, reactive, ref} from 'vue'
import {ArrowRight, Building2, User, UserCheck} from 'lucide-vue-next'
import AuthLogo from '../../components/AuthLogo.vue'
import PasswordField from '../../components/PasswordField.vue'
import {useAuthStore} from "@/stores/authStore.ts";
import {AccountType} from "@/types/account/accountType.ts";
import type {RegisterCustomerInput} from "@/types/auth/registerCustomerInput.ts";
import type {RegisterSupportAgentInput} from "@/types/auth/registerSupportAgentInput.ts";
import type {RegisterOrganizationInput} from "@/types/auth/registerOrganizationInput.ts";
import {useRoute, useRouter} from "vue-router";

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const accountType = ref<AccountType>(AccountType.Customer)
const codeError = ref('')

const registerForm = reactive({
  name: '',
  email: '',
  organizationName: '',
  inviteCode: '',
  password: '',
  confirmPassword: '',
})

const tabs: Array<{
  id: AccountType
  label: string
  icon: Component
}> = [
  { id: AccountType.Customer, label: 'Customer', icon: User },
  { id: AccountType.SupportAgent, label: 'Support Agent', icon: UserCheck },
  { id: AccountType.Organization, label: 'Organization', icon: Building2 },
]

const emailPlaceholder = computed(() =>
  accountType.value === AccountType.SupportAgent ? 'company@email.com' : 'your@email.com',
)

const submitLabel = computed(() => {
  if (accountType.value === AccountType.Organization) return 'Register Organization'
  if (accountType.value === AccountType.SupportAgent) return 'Join as Support Agent'
  return 'Create Account'
})

function selectAccountType(type: AccountType) {
  accountType.value = type
  codeError.value = ''
}

async function handleRegister() {
    if (accountType.value === AccountType.Customer) {
      await handleRegisterCustomer()
    } else if (accountType.value === AccountType.SupportAgent) {
      await handleRegisterSupportAgent()
    } else if (accountType.value === AccountType.Organization) {
      await handleRegisterOrganization()
    }
}

async function handleRegisterCustomer() {
  try {
    await authStore.registerCustomer({
      email: registerForm.email,
      username: registerForm.name,
      password: registerForm.password,
    } as RegisterCustomerInput)

    const redirectTo = route.query.redirect as string
    if (redirectTo) {
      await router.push(redirectTo)
    } else {
      await router.push({ name: 'customerDashboard' })
    }
  } catch (e: any) {

  }
}

async function handleRegisterSupportAgent() {
  try {
    await authStore.registerSupportAgent({
      email: registerForm.email,
      name: registerForm.name,
      password: registerForm.password,
      inviteCode: registerForm.inviteCode,
    } as RegisterSupportAgentInput)
  } catch (e: any) {

  }
}

async function handleRegisterOrganization() {
  try {
    await authStore.registerCustomer({
      email: registerForm.email,
      username: registerForm.name,
      password: registerForm.password,
      organizationName: registerForm.organizationName
    } as RegisterOrganizationInput)
  } catch (e: any) {

  }
}
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 flex items-center justify-center px-4 py-8">
    <div class="w-full max-w-md">
      <AuthLogo />

      <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 shadow-sm p-8">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-1.5 tracking-tight">
          Create your account
        </h1>
        <p v-if="accountType === AccountType.Customer" class="text-sm text-gray-500 dark:text-gray-400 mb-6">
          Create account as a customer to open tickets.
        </p>
        <p v-else-if="accountType === AccountType.SupportAgent" class="text-sm text-gray-500 dark:text-gray-400 mb-6">
          Join organization as a support agent.
        </p>
        <p v-else-if="accountType === AccountType.Organization" class="text-sm text-gray-500 dark:text-gray-400 mb-6">
          Register your organization and solve problems for your customers.
        </p>

        <div class="grid grid-cols-3 gap-1.5 mb-6 p-1 bg-gray-100 dark:bg-gray-800 rounded-xl">
          <button
            v-for="tab in tabs"
            :key="tab.id"
            type="button"
            class="py-2 px-1 rounded-lg text-xs font-medium transition-all flex items-center justify-center gap-1.5"
            :class="
              accountType === tab.id
                ? 'bg-white dark:bg-gray-900 text-gray-900 dark:text-white shadow-sm'
                : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300'
            "
            @click="selectAccountType(tab.id)"
          >
            <component :is="tab.icon" :size="13" />
            {{ tab.label }}
          </button>
        </div>

        <form class="space-y-4" @submit.prevent="handleRegister">
          <div>
            <label
              for="register-name"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5"
            >
              Full Name
            </label>
            <input
              id="register-name"
              v-model="registerForm.name"
              type="text"
              placeholder="Your name"
              class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all text-sm"
            />
          </div>

          <div v-if="accountType === AccountType.Organization">
            <label
              for="register-organization"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5"
            >
              Organization Name
            </label>
            <input
              id="register-organization"
              v-model="registerForm.organizationName"
              type="text"
              placeholder="Organization name"
              class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all text-sm"
            />
          </div>

          <div>
            <label
              for="register-email"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5"
            >
              Email address
            </label>
            <input
              id="register-email"
              v-model="registerForm.email"
              type="email"
              :placeholder="emailPlaceholder"
              class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all text-sm"
            />
          </div>

          <div v-if="accountType === AccountType.SupportAgent">
            <label
              for="register-invite-code"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5"
            >
              Invite Code
            </label>
            <input
              id="register-invite-code"
              v-model="registerForm.inviteCode"
              type="text"
              placeholder="XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"
              maxlength="9"
              spellcheck="false"
              class="w-full px-4 py-2.5 rounded-xl border bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 transition-all text-sm font-mono tracking-widest uppercase"
              :class="
                codeError
                  ? 'border-red-400 dark:border-red-600 focus:ring-red-400/30 focus:border-red-400'
                  : 'border-gray-200 dark:border-gray-700 focus:ring-blue-500/30 focus:border-blue-500'
              "
              @input="codeError = ''"
            />
            <p v-if="codeError" class="text-xs text-red-500 mt-1.5">
              {{ codeError }}
            </p>
            <p v-else class="text-xs text-gray-400 mt-1.5">
              Enter the code you received from your organization admin.
            </p>
          </div>

          <PasswordField
            id="register-password"
            v-model="registerForm.password"
            label="Password"
            placeholder="Min. 8 characters"
          />

          <PasswordField
            id="register-confirm-password"
            v-model="registerForm.confirmPassword"
            label="Confirm Password"
            placeholder="Repeat your password"
            :show-toggle="false"
          />

          <div
            v-if="accountType === AccountType.SupportAgent"
            class="flex items-start gap-2.5 p-3 rounded-xl bg-blue-50 dark:bg-blue-900/20 border border-blue-100 dark:border-blue-800"
          >
            <UserCheck :size="15" class="text-blue-500 flex-shrink-0 mt-0.5" />
            <p class="text-xs text-blue-700 dark:text-blue-300 leading-relaxed">
              Your account will be linked to the organization that issued your invite code.
              Make sure you use the same email address the admin registered.
            </p>
          </div>

          <button
            type="submit"
            class="w-full flex items-center justify-center gap-2 bg-blue-500 hover:bg-blue-600 text-white py-2.5 rounded-xl font-medium transition-all shadow-sm text-sm"
          >
            {{ submitLabel }}
            <ArrowRight :size="16" />
          </button>
        </form>
      </div>

      <p class="text-center text-sm text-gray-500 dark:text-gray-400 mt-5">
        Already have an account?
        <RouterLink
          :to="{ path: '/login', query: { redirect: route.query.redirect } }"
          class="text-blue-500 hover:text-blue-600 font-medium"
        >
          Sign in
        </RouterLink>
      </p>
    </div>
  </div>
</template>
