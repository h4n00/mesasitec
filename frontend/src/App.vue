<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from './stores/auth'
import { useToastStore } from './stores/toast'

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()

const mostrarNav = computed(() => auth.autenticado)

function salir(): void {
  auth.logout()
  router.push({ name: 'login' })
}
</script>

<template>
  <nav v-if="mostrarNav" data-testid="app-nav" class="nav">
    <RouterLink :to="{ name: 'solicitudes' }" class="marca">MesaSitec</RouterLink>

    <div class="usuario">
      <span data-testid="nav-usuario-nombre">{{ auth.usuario?.nombre }}</span>
      <span data-testid="nav-usuario-rol" class="rol">{{ auth.usuario?.rol }}</span>
      <button data-testid="btn-logout" @click="salir">Salir</button>
    </div>
  </nav>

  <p v-if="toast.mensaje" data-testid="toast-mensaje" class="toast">
    {{ toast.mensaje }}
  </p>

  <RouterView />
</template>

<style scoped>
.nav {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 16px;
  background: #24292f;
  color: #fff;
  font-family: system-ui, sans-serif;
}
.marca {
  color: #fff;
  text-decoration: none;
  font-weight: bold;
}
.usuario {
  display: flex;
  gap: 12px;
  align-items: center;
  font-size: 14px;
}
.rol {
  background: #444c56;
  padding: 2px 8px;
  border-radius: 10px;
}
.toast {
  margin: 0;
  padding: 10px 16px;
  background: #ffe9a8;
  font-family: system-ui, sans-serif;
}
</style>