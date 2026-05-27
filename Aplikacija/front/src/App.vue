<script setup lang="ts">
import {useAppStore} from "@/stores/appStore.ts";
import {useAuthStore} from "@/stores/authStore.ts";
import {onMounted} from "vue";

const app = useAppStore()
const auth = useAuthStore()

onMounted(() => {
  auth.initialize()
  app.loadTheme()
})
</script>

<template>
  <RouterView />
</template>

<style scoped>

</style>
