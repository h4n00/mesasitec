<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useToastStore } from '../stores/toast'
import { useRoute, useRouter } from 'vue-router'
import {
  obtenerSolicitud,
  ejecutarTransicion,
  listarAgentes
} from '../api/solicitudes'
import {
  accionesDisponibles,
  puedeEditar,
  requiereAgente,
  requiereMotivo,
  minimoMotivo
} from '../api/acciones'
import { useAuthStore } from '../stores/auth'
import type { SolicitudDetalle, Referencia } from '../types/api'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const id = route.params.id as string

const solicitud = ref<SolicitudDetalle | null>(null)
const agentes = ref<Referencia[]>([])
const acciones = ref<string[]>([])

const cargando = ref(true)
const error = ref('')

const modalAbierto = ref(false)
const accionActual = ref('')
const agenteSeleccionado = ref('')
const motivo = ref('')
const errorModal = ref('')
const procesando = ref(false)
const toast = useToastStore()

function formatearFecha(iso: string | null): string {
  if (!iso) return '-'
  return new Date(iso).toLocaleString('es-GT')
}

function recalcularAcciones(): void {
  if (!solicitud.value || !auth.usuario) return
  acciones.value = accionesDisponibles(solicitud.value.estado, auth.usuario.rol)
}


function mostrarEditar(): boolean {
  if (!solicitud.value || !auth.usuario) return false
  return puedeEditar(solicitud.value.estado, auth.usuario.rol)
}

async function abrirModal(accion: string): Promise<void> {
  accionActual.value = accion
  agenteSeleccionado.value = ''
  motivo.value = ''
  errorModal.value = ''
  modalAbierto.value = true

  if (requiereAgente(accion) && agentes.value.length === 0) {
    try {
      agentes.value = await listarAgentes()
    } catch {
      errorModal.value = 'No se pudo cargar la lista de agentes'
    }
  }
}

function cerrarModal(): void {
  modalAbierto.value = false
}

async function confirmar(): Promise<void> {
  errorModal.value = ''
  const accion = accionActual.value

  if (requiereAgente(accion) && agenteSeleccionado.value === '') {
    errorModal.value = 'Selecciona un agente'
    return
  }

  if (requiereMotivo(accion) && motivo.value.trim().length < minimoMotivo(accion)) {
    errorModal.value = `El motivo debe tener al menos ${minimoMotivo(accion)} caracteres`
    return
  }

  procesando.value = true

  try {
    const actualizada = await ejecutarTransicion(id, {
      accion,
      agenteId: requiereAgente(accion) ? agenteSeleccionado.value : undefined,
      motivo: requiereMotivo(accion) ? motivo.value.trim() : undefined
    })

    solicitud.value = actualizada
    recalcularAcciones()
    toast.mostrar(`Accion "${accion}" ejecutada`)
    modalAbierto.value = false
  } catch {
    errorModal.value = 'No se pudo ejecutar la accion'
  } finally {
    procesando.value = false
  }
}

onMounted(async () => {
  try {
    solicitud.value = await obtenerSolicitud(id)
    recalcularAcciones()
  } catch {
    error.value = 'No se pudo cargar la solicitud'
  } finally {
    cargando.value = false
  }
})
</script>

<template>
  <div class="detalle">
    <p v-if="cargando">Cargando...</p>

    <p v-else-if="error" class="error">{{ error }}</p>

    <template v-else-if="solicitud">
      <h2 data-testid="detalle-codigo">{{ solicitud.codigo }}</h2>

      <p data-testid="detalle-titulo">{{ solicitud.titulo }}</p>
      <p data-testid="detalle-descripcion">{{ solicitud.descripcion }}</p>

      <dl>
        <dt>Estado</dt>
        <dd data-testid="detalle-estado">{{ solicitud.estado }}</dd>

        <dt>Prioridad</dt>
        <dd data-testid="detalle-prioridad">{{ solicitud.prioridad }}</dd>

        <dt>Categoria</dt>
        <dd data-testid="detalle-categoria">{{ solicitud.categoria.nombre }}</dd>

        <dt>Agente</dt>
        <dd data-testid="detalle-agente">
          {{ solicitud.agente ? solicitud.agente.nombre : 'Sin asignar' }}
        </dd>

        <dt>Creada</dt>
        <dd data-testid="detalle-fecha-creacion">
          {{ formatearFecha(solicitud.fechaCreacion) }}
        </dd>

        <dt>Limite SLA</dt>
        <dd data-testid="detalle-fecha-limite">
          {{ formatearFecha(solicitud.fechaLimiteSla) }}
        </dd>
      </dl>

      <p v-if="solicitud.vencida" data-testid="detalle-vencida" class="vencida">
        Vencida
      </p>

      <p
        v-if="solicitud.motivoResolucion || solicitud.motivoCancelacion"
        data-testid="detalle-motivo"
      >
        {{ solicitud.motivoResolucion || solicitud.motivoCancelacion }}
      </p>

      <div class="acciones">
        <RouterLink
          v-if="mostrarEditar()"
          data-testid="btn-editar"
          :to="{ name: 'solicitud-editar', params: { id: solicitud.id } }"
        >
          Editar
        </RouterLink>

        <button
          v-if="acciones.includes('asignar')"
          data-testid="btn-accion-asignar"
          @click="abrirModal('asignar')"
        >
          Asignar
        </button>

        <button
          v-if="acciones.includes('iniciar')"
          data-testid="btn-accion-iniciar"
          @click="abrirModal('iniciar')"
        >
          Iniciar
        </button>

        <button
          v-if="acciones.includes('resolver')"
          data-testid="btn-accion-resolver"
          @click="abrirModal('resolver')"
        >
          Resolver
        </button>

        <button
          v-if="acciones.includes('cerrar')"
          data-testid="btn-accion-cerrar"
          @click="abrirModal('cerrar')"
        >
          Cerrar
        </button>

        <button
          v-if="acciones.includes('reabrir')"
          data-testid="btn-accion-reabrir"
          @click="abrirModal('reabrir')"
        >
          Reabrir
        </button>

        <button
          v-if="acciones.includes('cancelar')"
          data-testid="btn-accion-cancelar"
          @click="abrirModal('cancelar')"
        >
          Cancelar
        </button>
      </div>

      <RouterLink :to="{ name: 'solicitudes' }">Volver al listado</RouterLink>

      <div v-if="modalAbierto" data-testid="modal-accion" class="modal">
        <div class="caja">
          <h3>{{ accionActual }}</h3>

          <template v-if="requiereAgente(accionActual)">
            <label>Agente</label>
            <select v-model="agenteSeleccionado" data-testid="modal-select-agente">
              <option value="">Selecciona un agente</option>
              <option v-for="a in agentes" :key="a.id" :value="a.id">
                {{ a.nombre }}
              </option>
            </select>
          </template>

          <template v-if="requiereMotivo(accionActual)">
            <label>Motivo (minimo {{ minimoMotivo(accionActual) }} caracteres)</label>
            <textarea v-model="motivo" data-testid="modal-motivo" rows="3" />
          </template>

          <p v-if="errorModal" data-testid="modal-error" class="error">
            {{ errorModal }}
          </p>

          <div class="acciones">
            <button
              data-testid="modal-confirmar"
              :disabled="procesando"
              @click="confirmar"
            >
              {{ procesando ? 'Procesando...' : 'Confirmar' }}
            </button>
            <button data-testid="modal-cancelar" @click="cerrarModal">Cerrar</button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.detalle {
  padding: 16px;
  font-family: system-ui, sans-serif;
}
dl {
  display: grid;
  grid-template-columns: 140px 1fr;
  gap: 4px 12px;
}
dt {
  font-weight: bold;
}
dd {
  margin: 0;
}
.acciones {
  display: flex;
  gap: 8px;
  margin: 16px 0;
  align-items: center;
}
.vencida {
  color: #c00;
  font-weight: bold;
}
.modal {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
}
.caja {
  background: #fff;
  padding: 20px;
  border-radius: 6px;
  min-width: 320px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.error {
  color: #c00;
}
button {
  padding: 6px 14px;
  cursor: pointer;
}
</style>