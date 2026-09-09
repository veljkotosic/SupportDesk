import { createRouter, createWebHistory } from 'vue-router'
import {useAuthStore} from "@/stores/authStore.ts";
import HomeView from '../views/HomeView.vue'
import LoginView from "@/views/auth/LoginView.vue";
import RegisterView from "@/views/auth/RegisterView.vue";
import CustomerDashboardView from "@/views/customer/CustomerDashboardView.vue";
import {UserRole} from "@/types/user/userRole.ts"
import OpenTicketView from "@/views/customer/OpenTicketView.vue";
import CustomerTicketView from "@/views/customer/CustomerTicketView.vue";
import OrganizationAdminDashboardView from "@/views/organization/OrganizationAdminDashboardView.vue";
import OrganizationAdminAgentsView from "@/views/organization/OrganizationAdminAgentsView.vue";
import OrganizationAdminTicketsView from "@/views/organization/OrganizationAdminTicketsView.vue";
import OrganizationAdminSettingsView from "@/views/organization/OrganizationAdminSettingsView.vue";
import OrganizationAdminTicketDetailView from "@/views/organization/OrganizationAdminTicketDetailView.vue";
import SupportAgentDashboardView from "@/views/supportAgent/SupportAgentDashboardView.vue";
import SupportAgentTicketDetailView from "@/views/supportAgent/SupportAgentTicketDetailView.vue";

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean;
    guestOnly?: boolean;
    allowedUsers?: UserRole[];
  }
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
      meta: {
        guestOnly: true,
      },
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: {
        guestOnly: true,
      },
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
      meta: {
        guestOnly: true,
      },
    },
    {
      path: '/customer/dashboard',
      name: 'customerDashboard',
      component: CustomerDashboardView,
      meta: {
        requiresAuth: true,
        allowedUsers: [UserRole.Customer]
      },
    },
    {
      path: '/customer/openTicket',
      name: 'customerOpenTicket',
      component: OpenTicketView,
      meta: {
        requiresAuth: true,
        allowedUsers: [UserRole.Customer]
      }
    },
    {
      path: '/customer/ticket/:id',
      name: 'customerTicket',
      component: CustomerTicketView,
      meta: {
        requiresAuth: true,
        allowedUsers: [UserRole.Customer]
      }
    },
    {
      path: '/organization/dashboard',
      name: 'organizationDashboard',
      component: OrganizationAdminDashboardView,
      meta: {
        requiresAuth: true,
        allowedUsers: [UserRole.OrganizationAdmin]
      }
    },
    {
      path: '/organization/agents',
      name: 'organizationAgents',
      component: OrganizationAdminAgentsView,
      meta: {
        requiresAuth: true,
        allowedUsers: [UserRole.OrganizationAdmin]
      }
    },
    {
      path: '/organization/tickets',
      name: 'organizationTickets',
      component: OrganizationAdminTicketsView,
      meta: {
        requiresAuth: true,
        allowedUsers: [UserRole.OrganizationAdmin]
      }
    },
    {
      path: '/organization/tickets/:id',
      name: 'organizationTicketDetail',
      component: OrganizationAdminTicketDetailView,
      meta: {
        requiresAuth: true,
        allowedUsers: [UserRole.OrganizationAdmin]
      }
    },
    {
      path: '/organization/settings',
      name: 'organizationSettings',
      component: OrganizationAdminSettingsView,
      meta: {
        requiresAuth: true,
        allowedUsers: [UserRole.OrganizationAdmin]
      }
    },
    {
      path: '/supportAgent/dashboard',
      name: 'supportAgentDashboard',
      component: SupportAgentDashboardView,
      meta: {
        requiresAuth: true,
        allowedUsers: [UserRole.SupportAgent]
      }
    },
    {
      path: '/supportAgent/tickets/:id',
      name: 'supportAgentTicketDetail',
      component: SupportAgentTicketDetailView,
      meta: {
        requiresAuth: true,
        allowedUsers: [UserRole.SupportAgent]
      }
    },
  ],
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()
  if (!auth.isInitialized) {
    await auth.initialize()
  }

  if (to.meta.guestOnly && auth.isAuthenticated) {
    if (auth.user?.role === UserRole.Customer) {
      return '/customer/dashboard'
    } else if (auth.user?.role === UserRole.SupportAgent) {
      return '/supportAgent/dashboard'
    } else if (auth.user?.role === UserRole.OrganizationAdmin) {
      return '/organization/dashboard'
    }
    return '/'
  }

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return {
      name: 'login',
      query: { redirect: to.fullPath }
    }
  }

  if (to.meta.allowedUsers && !to.meta.allowedUsers!.includes(auth.user?.role as UserRole)) {
    if (auth.user?.role === UserRole.Customer) {
      return '/customer/dashboard'
    } else if (auth.user?.role === UserRole.SupportAgent) {
      return '/supportAgent/dashboard'
    } else if (auth.user?.role === UserRole.OrganizationAdmin) {
      return '/organization/dashboard'
    }
  }
})

export default router
