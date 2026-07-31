import cliente from './cliente'
import type {
  Categoria,
  Pagina,
  SolicitudLista,
  SolicitudDetalle,
  Estado,
  Prioridad
} from '../types/api'

export interface FiltrosSolicitudes {
  estado?: Estado | ''
  prioridad?: Prioridad | ''
  categoriaId?: string
  q?: string
  vencidas?: boolean | null
  page: number
  pageSize: number
  sort: string
}

export async function listarSolicitudes(
  filtros: FiltrosSolicitudes
): Promise<Pagina<SolicitudLista>> {
  const params: Record<string, string | number | boolean> = {
    page: filtros.page,
    pageSize: filtros.pageSize,
    sort: filtros.sort
  }

  if (filtros.estado) params.estado = filtros.estado
  if (filtros.prioridad) params.prioridad = filtros.prioridad
  if (filtros.categoriaId) params.categoriaId = filtros.categoriaId
  if (filtros.q) params.q = filtros.q
  if (filtros.vencidas !== null && filtros.vencidas !== undefined) {
    params.vencidas = filtros.vencidas
  }

  const respuesta = await cliente.get<Pagina<SolicitudLista>>('/solicitudes', {
    params
  })
  return respuesta.data
}

export async function obtenerSolicitud(id: string): Promise<SolicitudDetalle> {
  const respuesta = await cliente.get<SolicitudDetalle>(`/solicitudes/${id}`)
  return respuesta.data
}

export async function listarCategorias(): Promise<Categoria[]> {
  const respuesta = await cliente.get<Categoria[]>('/categorias')
  return respuesta.data
}

export interface GuardarSolicitud {
  titulo: string
  descripcion: string
  categoriaId: string
  prioridad: Prioridad
}

export async function crearSolicitud(
  datos: GuardarSolicitud
): Promise<SolicitudDetalle> {
  const respuesta = await cliente.post<SolicitudDetalle>('/solicitudes', datos)
  return respuesta.data
}

export async function editarSolicitud(
  id: string,
  datos: GuardarSolicitud
): Promise<SolicitudDetalle> {
  const respuesta = await cliente.put<SolicitudDetalle>(`/solicitudes/${id}`, datos)
  return respuesta.data
}