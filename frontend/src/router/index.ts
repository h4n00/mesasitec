import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue')
    },
    {
      path: '/solicitudes',
      name: 'solicitudes',
      component: () => import('../views/SolicitudesView.vue'),
      meta: { privada: true }
    },
    {
      path: '/solicitudes/nueva',
      name: 'solicitud-nueva',
      component: () => import('../views/SolicitudFormView.vue'),
      meta: { privada: true }
    },
    {
      path: '/solicitudes/:id',
      name: 'solicitud-detalle',
      component: () => import('../views/SolicitudDetalleView.vue'),
      meta: { privada: true }
    },
    {
      path: '/solicitudes/:id/editar',
      name: 'solicitud-editar',
      component: () => import('../views/SolicitudFormView.vue'),
      meta: { privada: true }
    },
    {
      path: '/',
      redirect: '/solicitudes'
    }
  ]
})

// Guard: bloquea las rutas privadas sin sesion
router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.privada && !auth.autenticado) {
    return { name: 'login' }
  }

  if (to.name === 'login' && auth.autenticado) {
    return { name: 'solicitudes' }
  }

  return true
})

export default router