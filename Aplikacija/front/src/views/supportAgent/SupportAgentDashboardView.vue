<script setup lang="ts">
import SupportAgentDashboardLayout from '@/layouts/SupportAgentDashboardLayout.vue'
import OrganizationTicketList from '@/components/ticket/OrganizationTicketList.vue'
</script>

<template>
  <SupportAgentDashboardLayout active-path="/supportAgent/dashboard">
    <OrganizationTicketList title="My Tickets" detail-base-path="/supportAgent/tickets" />
  </SupportAgentDashboardLayout>
</template>
