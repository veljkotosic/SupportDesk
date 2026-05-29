import { defineStore } from 'pinia'

type Theme = 'light' | 'dark'

export const useAppStore = defineStore('app', {
  state: () => ({
    theme: 'light' as Theme,
  }),
  actions: {
    loadTheme() {
      const savedTheme = localStorage.getItem('theme') as Theme | null
      if (savedTheme) {
        this.theme = savedTheme
      } else if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
        this.theme = 'light'
      }
      this.applyTheme()
    },
    toggleTheme() {
      this.theme = this.theme === 'light' ? 'dark' : 'light'
      localStorage.setItem('theme', this.theme)
      document.documentElement.classList.toggle('dark', this.theme === 'dark')
    },
    applyTheme() {
      document.documentElement.classList.toggle('dark', this.theme === 'dark')
    }
  },
})
