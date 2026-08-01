import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useToastStore = defineStore('toast', () => {
  const mensaje = ref('')
  let temporizador: number | undefined

  function mostrar(texto: string): void {
    mensaje.value = texto
    window.clearTimeout(temporizador)
    temporizador = window.setTimeout(() => {
      mensaje.value = ''
    }, 4000)
  }

  return { mensaje, mostrar }
})
