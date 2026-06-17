<script setup lang="ts">
import { useRoute } from 'vue-router'
import SupportAgentDashboardLayout from '@/layouts/SupportAgentDashboardLayout.vue'
import OrganizationTicketDetail from '@/components/ticket/OrganizationTicketDetail.vue'

const route = useRoute()
</script>

<template>
  <SupportAgentDashboardLayout active-path="/supportAgent/dashboard">
    <OrganizationTicketDetail :ticket-id="route.params.id as string" back-path="/supportAgent/dashboard" />
  </SupportAgentDashboardLayout>
</template>
