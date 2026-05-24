<script setup lang="ts">
import { computed, ref } from 'vue'
import { Eye, EyeOff } from 'lucide-vue-next'

withDefaults(
  defineProps<{
    id?: string
    label: string
    modelValue: string
    placeholder?: string
    showToggle?: boolean
  }>(),
  {
    id: undefined,
    placeholder: '',
    showToggle: true,
  },
)

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const showPassword = ref(false)

const inputType = computed(() => (showPassword.value ? 'text' : 'password'))

function handleInput(event: Event) {
  emit('update:modelValue', (event.target as HTMLInputElement).value)
}

function togglePasswordVisibility() {
  showPassword.value = !showPassword.value
}
</script>

<template>
  <div>
    <label
      v-if="label"
      :for="id"
      class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5"
    >
      {{ label }}
    </label>
    <div class="relative">
      <input
        :id="id"
        :type="showToggle ? inputType : 'password'"
        :value="modelValue"
        :placeholder="placeholder"
        class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all text-sm"
        :class="{ 'pr-11': showToggle }"
        @input="handleInput"
      />
      <button
        v-if="showToggle"
        type="button"
        class="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
        aria-label="Toggle password visibility"
        @click="togglePasswordVisibility"
      >
        <EyeOff v-if="showPassword" :size="16" />
        <Eye v-else :size="16" />
      </button>
    </div>
  </div>
</template>
