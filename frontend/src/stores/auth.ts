import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import cliente from '../api/cliente'
import type { Usuario, LoginResponse } from '../types/api'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const usuario = ref<Usuario | null>(leerUsuarioGuardado())

  const autenticado = computed(() => token.value !== null)

  function leerUsuarioGuardado(): Usuario | null {
    const texto = localStorage.getItem('usuario')
    if (!texto) return null
    return JSON.parse(texto) as Usuario
  }

  async function login(email: string, password: string): Promise<void> {
    const respuesta = await cliente.post<LoginResponse>('/auth/login', {
      email,
      password
    })

    token.value = respuesta.data.accessToken
    usuario.value = respuesta.data.usuario

    localStorage.setItem('token', respuesta.data.accessToken)
    localStorage.setItem('usuario', JSON.stringify(respuesta.data.usuario))
  }

  function logout(): void {
    token.value = null
    usuario.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('usuario')
  }

  return { token, usuario, autenticado, login, logout }
})