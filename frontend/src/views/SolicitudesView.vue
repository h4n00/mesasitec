<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { listarSolicitudes, listarCategorias } from '../api/solicitudes'
import type { SolicitudLista, Categoria, Estado, Prioridad } from '../types/api'

const solicitudes = ref<SolicitudLista[]>([])
const categorias = ref<Categoria[]>([])

const cargando = ref(false)
const error = ref('')

const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const totalPaginas = ref(0)

const fEstado = ref<Estado | ''>('')
const fPrioridad = ref<Prioridad | ''>('')
const fCategoria = ref('')
const fVencidas = ref('')
const fBusqueda = ref('')

let temporizador: number | undefined

async function cargar(): Promise<void> {
  cargando.value = true
  error.value = ''

  try {
    const datos = await listarSolicitudes({
      estado: fEstado.value,
      prioridad: fPrioridad.value,
      categoriaId: fCategoria.value,
      q: fBusqueda.value,
      vencidas: fVencidas.value === '' ? null : fVencidas.value === 'true',
      page: page.value,
      pageSize: pageSize.value,
      sort: '-fechaCreacion'
    })

    solicitudes.value = datos.items
    total.value = datos.total
    totalPaginas.value = datos.totalPaginas
  } catch {
    error.value = 'No se pudo cargar el listado'
    solicitudes.value = []
  } finally {
    cargando.value = false
  }
}

function limpiarFiltros(): void {
  fEstado.value = ''
  fPrioridad.value = ''
  fCategoria.value = ''
  fVencidas.value = ''
  fBusqueda.value = ''
  page.value = 1
  cargar()
}

function irAPagina(destino: number): void {
  if (destino < 1 || destino > totalPaginas.value) return
  page.value = destino
  cargar()
}

function formatearFecha(iso: string): string {
  return new Date(iso).toLocaleString('es-GT')
}

// Los filtros de seleccion recargan de inmediato
watch([fEstado, fPrioridad, fCategoria, fVencidas], () => {
  page.value = 1
  cargar()
})

// La busqueda espera a que el usuario deje de escribir
watch(fBusqueda, () => {
  window.clearTimeout(temporizador)
  temporizador = window.setTimeout(() => {
    page.value = 1
    cargar()
  }, 400)
})

onMounted(async () => {
  try {
    categorias.value = await listarCategorias()
  } catch {
    categorias.value = []
  }
  await cargar()
})
</script>

<template>
  <div class="listado">
    <div class="encabezado">
      <h2>Solicitudes</h2>
      <RouterLink data-testid="btn-nueva-solicitud" :to="{ name: 'solicitud-nueva' }">
        Nueva solicitud
      </RouterLink>
    </div>

    <div class="filtros">
      <select v-model="fEstado" data-testid="filtro-estado">
        <option value="">Todos los estados</option>
        <option value="Nueva">Nueva</option>
        <option value="Asignada">Asignada</option>
        <option value="EnProceso">EnProceso</option>
        <option value="Resuelta">Resuelta</option>
        <option value="Cerrada">Cerrada</option>
        <option value="Cancelada">Cancelada</option>
      </select>

      <select v-model="fPrioridad" data-testid="filtro-prioridad">
        <option value="">Todas las prioridades</option>
        <option value="Baja">Baja</option>
        <option value="Media">Media</option>
        <option value="Alta">Alta</option>
        <option value="Critica">Critica</option>
      </select>

      <select v-model="fCategoria" data-testid="filtro-categoria">
        <option value="">Todas las categorias</option>
        <option v-for="c in categorias" :key="c.id" :value="c.id">
          {{ c.nombre }}
        </option>
      </select>

      <select v-model="fVencidas" data-testid="filtro-vencidas">
        <option value="">Todas</option>
        <option value="true">Solo vencidas</option>
        <option value="false">Solo en plazo</option>
      </select>

      <input
        v-model="fBusqueda"
        data-testid="filtro-busqueda"
        type="text"
        placeholder="Buscar por titulo, descripcion o codigo"
      />

      <button data-testid="btn-limpiar-filtros" @click="limpiarFiltros">
        Limpiar
      </button>
    </div>

    <p v-if="cargando" data-testid="listado-cargando">Cargando...</p>

    <p v-else-if="error" class="error">{{ error }}</p>

    <p v-else-if="solicitudes.length === 0" data-testid="listado-vacio">
      No hay solicitudes que coincidan con los filtros
    </p>

    <table v-else data-testid="tabla-solicitudes">
      <thead>
        <tr>
          <th>Codigo</th>
          <th>Titulo</th>
          <th>Estado</th>
          <th>Prioridad</th>
          <th>Categoria</th>
          <th>Agente</th>
          <th>Limite SLA</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="s in solicitudes"
          :key="s.id"
          data-testid="fila-solicitud"
          :data-codigo="s.codigo"
        >
          <td data-testid="celda-codigo">
            <RouterLink :to="{ name: 'solicitud-detalle', params: { id: s.id } }">
              {{ s.codigo }}
            </RouterLink>
          </td>
          <td>{{ s.titulo }}</td>
          <td data-testid="celda-estado">{{ s.estado }}</td>
          <td data-testid="celda-prioridad">{{ s.prioridad }}</td>
          <td>{{ s.categoria.nombre }}</td>
          <td>{{ s.agente ? s.agente.nombre : 'Sin asignar' }}</td>
          <td data-testid="celda-sla">
            {{ formatearFecha(s.fechaLimiteSla) }}
            <span v-if="s.vencida" data-testid="badge-vencida" class="vencida">
              Vencida
            </span>
          </td>
        </tr>
      </tbody>
    </table>

    <div class="paginacion">
      <button
        data-testid="paginacion-anterior"
        :disabled="page <= 1"
        @click="irAPagina(page - 1)"
      >
        Anterior
      </button>

      <span data-testid="paginacion-info">
        Página {{ page }} de {{ totalPaginas }} — {{ total }} resultados
      </span>

      <button
        data-testid="paginacion-siguiente"
        :disabled="page >= totalPaginas"
        @click="irAPagina(page + 1)"
      >
        Siguiente
      </button>
    </div>
  </div>
</template>

<style scoped>
.listado {
  padding: 16px;
  font-family: system-ui, sans-serif;
}
.encabezado {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.filtros {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  margin: 16px 0;
}
table {
  width: 100%;
  border-collapse: collapse;
}
th,
td {
  border: 1px solid #ddd;
  padding: 6px 8px;
  text-align: left;
  font-size: 14px;
}
th {
  background: #f4f4f4;
}
.vencida {
  background: #c00;
  color: #fff;
  padding: 1px 6px;
  border-radius: 3px;
  font-size: 12px;
  margin-left: 6px;
}
.paginacion {
  display: flex;
  gap: 12px;
  align-items: center;
  margin-top: 16px;
}
.error {
  color: #c00;
}
</style>