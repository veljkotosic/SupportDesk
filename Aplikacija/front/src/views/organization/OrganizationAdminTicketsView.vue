<script setup lang="ts">
import OrganizationAdminLayout from '@/layouts/OrganizationAdminDashboardLayout.vue'
import OrganizationTicketList from '@/components/ticket/OrganizationTicketList.vue'
</script>

<template>
  <OrganizationAdminLayout active-path="/organization/tickets">
    <OrganizationTicketList title="All Tickets" />
  </OrganizationAdminLayout>
</template>
