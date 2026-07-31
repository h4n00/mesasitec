<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()

const email = ref('')
const password = ref('')
const error = ref('')
const cargando = ref(false)

async function enviar(): Promise<void> {
  error.value = ''
  cargando.value = true

  try {
    await auth.login(email.value, password.value)
    router.push({ name: 'solicitudes' })
  } catch {
    error.value = 'Correo o contrasena incorrectos'
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <div class="login">
    <h1>MesaSitec</h1>

    <label>Correo</label>
    <input
      v-model="email"
      data-testid="login-email"
      type="email"
      autocomplete="username"
    />

    <label>Contrasena</label>
    <input
      v-model="password"
      data-testid="login-password"
      type="password"
      autocomplete="current-password"
      @keyup.enter="enviar"
    />

    <button data-testid="login-submit" :disabled="cargando" @click="enviar">
      {{ cargando ? 'Entrando...' : 'Entrar' }}
    </button>

    <p v-if="error" data-testid="login-error" class="error">{{ error }}</p>
  </div>
</template>

<style scoped>
.login {
  max-width: 320px;
  margin: 80px auto;
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-family: system-ui, sans-serif;
}
input {
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
}
button {
  padding: 10px;
  margin-top: 12px;
  cursor: pointer;
}
.error {
  color: #c00;
}
</style>