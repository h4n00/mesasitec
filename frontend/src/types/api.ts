export type Rol = 'Admin' | 'Agente' | 'Solicitante'

export type Estado =
  | 'Nueva'
  | 'Asignada'
  | 'EnProceso'
  | 'Resuelta'
  | 'Cerrada'
  | 'Cancelada'

export type Prioridad = 'Baja' | 'Media' | 'Alta' | 'Critica'

export interface Referencia {
  id: string
  nombre: string
}

export interface Usuario {
  id: string
  nombre: string
  email: string
  rol: Rol
  tenantId: string
  tenantNombre: string
}

export interface LoginResponse {
  accessToken: string
  expiraEn: number
  usuario: Usuario
}

export interface Categoria {
  id: string
  nombre: string
  slaHoras: number
}

export interface SolicitudLista {
  id: string
  codigo: string
  titulo: string
  estado: Estado
  prioridad: Prioridad
  categoria: Referencia
  agente: Referencia | null
  fechaCreacion: string
  fechaLimiteSla: string
  vencida: boolean
}

export interface SolicitudDetalle extends SolicitudLista {
  descripcion: string
  solicitante: Referencia
  fechaResolucion: string | null
  motivoResolucion: string | null
  motivoCancelacion: string | null
}

export interface Pagina<T> {
  items: T[]
  page: number
  pageSize: number
  total: number
  totalPaginas: number
}

export interface ErrorApi {
  title: string
  status: number
  codigo: string
}