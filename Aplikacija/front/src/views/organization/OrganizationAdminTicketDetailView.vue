<script setup lang="ts">
import { useRoute } from 'vue-router'
import OrganizationAdminLayout from '@/layouts/OrganizationAdminDashboardLayout.vue'
import OrganizationTicketDetail from '@/components/ticket/OrganizationTicketDetail.vue'

const route = useRoute()
</script>

<template>
  <OrganizationAdminLayout active-path="/organization/tickets">
    <OrganizationTicketDetail :ticket-id="route.params.id as string" back-path="/organization/tickets" />
  </OrganizationAdminLayout>
</template>
