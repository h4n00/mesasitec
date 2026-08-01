<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useToastStore } from '../stores/toast'
import { useRoute, useRouter } from 'vue-router'
import {
  listarCategorias,
  obtenerSolicitud,
  crearSolicitud,
  editarSolicitud
} from '../api/solicitudes'
import type { Categoria, Prioridad } from '../types/api'

const route = useRoute()
const router = useRouter()

const id = computed(() => route.params.id as string | undefined)
const esEdicion = computed(() => id.value !== undefined)

const categorias = ref<Categoria[]>([])

const titulo = ref('')
const descripcion = ref('')
const categoriaId = ref('')
const prioridad = ref<Prioridad>('Media')

const errorTitulo = ref('')
const errorDescripcion = ref('')
const errorCategoria = ref('')
const errorGeneral = ref('')

const cargando = ref(false)
const guardando = ref(false)
const toast = useToastStore()

function validar(): boolean {
  errorTitulo.value = ''
  errorDescripcion.value = ''
  errorCategoria.value = ''

  if (titulo.value.trim().length < 5) {
    errorTitulo.value = 'El titulo debe tener al menos 5 caracteres'
  }

  if (descripcion.value.trim().length < 10) {
    errorDescripcion.value = 'La descripcion debe tener al menos 10 caracteres'
  }

  if (categoriaId.value === '') {
    errorCategoria.value = 'Selecciona una categoria'
  }

  return (
    errorTitulo.value === '' &&
    errorDescripcion.value === '' &&
    errorCategoria.value === ''
  )
}

async function guardar(): Promise<void> {
  errorGeneral.value = ''
  if (!validar()) return

  guardando.value = true

  try {
    const datos = {
      titulo: titulo.value.trim(),
      descripcion: descripcion.value.trim(),
      categoriaId: categoriaId.value,
      prioridad: prioridad.value
    }

    const resultado = esEdicion.value
      ? await editarSolicitud(id.value as string, datos)
      : await crearSolicitud(datos)

    toast.mostrar(esEdicion.value ? 'Solicitud actualizada' : 'Solicitud creada')

    router.push({ name: 'solicitud-detalle', params: { id: resultado.id } })
  } catch {
    errorGeneral.value = 'No se pudo guardar la solicitud'
  } finally {
    guardando.value = false
  }
}

function cancelar(): void {
  if (esEdicion.value) {
    router.push({ name: 'solicitud-detalle', params: { id: id.value as string } })
  } else {
    router.push({ name: 'solicitudes' })
  }
}

onMounted(async () => {
  cargando.value = true

  try {
    categorias.value = await listarCategorias()

    if (esEdicion.value) {
      const s = await obtenerSolicitud(id.value as string)
      titulo.value = s.titulo
      descripcion.value = s.descripcion
      categoriaId.value = s.categoria.id
      prioridad.value = s.prioridad
    }
  } catch {
    errorGeneral.value = 'No se pudo cargar la informacion'
  } finally {
    cargando.value = false
  }
})
</script>

<template>
  <div class="formulario">
    <h2>{{ esEdicion ? 'Editar solicitud' : 'Nueva solicitud' }}</h2>

    <p v-if="cargando">Cargando...</p>

    <template v-else>
      <label>Titulo</label>
      <input v-model="titulo" data-testid="form-titulo" type="text" />
      <p v-if="errorTitulo" data-testid="error-titulo" class="error">
        {{ errorTitulo }}
      </p>

      <label>Descripcion</label>
      <textarea v-model="descripcion" data-testid="form-descripcion" rows="4" />
      <p v-if="errorDescripcion" data-testid="error-descripcion" class="error">
        {{ errorDescripcion }}
      </p>

      <label>Categoria</label>
      <select v-model="categoriaId" data-testid="form-categoria">
        <option value="">Selecciona una categoria</option>
        <option v-for="c in categorias" :key="c.id" :value="c.id">
          {{ c.nombre }} ({{ c.slaHoras }} h)
        </option>
      </select>
      <p v-if="errorCategoria" data-testid="error-categoria" class="error">
        {{ errorCategoria }}
      </p>

      <label>Prioridad</label>
      <select v-model="prioridad" data-testid="form-prioridad">
        <option value="Baja">Baja</option>
        <option value="Media">Media</option>
        <option value="Alta">Alta</option>
        <option value="Critica">Critica</option>
      </select>

      <p v-if="errorGeneral" class="error">{{ errorGeneral }}</p>

      <div class="acciones">
        <button data-testid="form-submit" :disabled="guardando" @click="guardar">
          {{ guardando ? 'Guardando...' : 'Guardar' }}
        </button>
        <button data-testid="form-cancelar" @click="cancelar">Cancelar</button>
      </div>
    </template>
  </div>
</template>

<style scoped>
.formulario {
  max-width: 520px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-family: system-ui, sans-serif;
}
input,
textarea,
select {
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
}
.acciones {
  display: flex;
  gap: 8px;
  margin-top: 16px;
}
button {
  padding: 8px 16px;
  cursor: pointer;
}
.error {
  color: #c00;
  font-size: 13px;
  margin: 0;
}
</style>