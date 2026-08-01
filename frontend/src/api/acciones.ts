import type { Estado, Rol } from '../types/api'

// Espejo de la maquina de estados del backend (RN-02)
const TRANSICIONES: Record<Estado, string[]> = {
  Nueva: ['asignar', 'cancelar'],
  Asignada: ['iniciar', 'asignar', 'cancelar'],
  EnProceso: ['resolver', 'asignar', 'cancelar'],
  Resuelta: ['cerrar', 'reabrir'],
  Cerrada: [],
  Cancelada: []
}

// Espejo de la tabla de permisos por rol (RN-03)
const PERMISOS: Record<string, Rol[]> = {
  asignar: ['Admin', 'Agente'],
  iniciar: ['Admin', 'Agente'],
  resolver: ['Admin', 'Agente'],
  reabrir: ['Admin', 'Agente'],
  cerrar: ['Admin', 'Agente', 'Solicitante'],
  cancelar: ['Admin']
}

export function accionesDisponibles(estado: Estado, rol: Rol): string[] {
  const porEstado = TRANSICIONES[estado]
  return porEstado.filter((accion) => PERMISOS[accion].includes(rol))
}

export function puedeEditar(estado: Estado, rol: Rol): boolean {
  if (rol === 'Solicitante') return estado === 'Nueva'
  return true
}

export function requiereAgente(accion: string): boolean {
  return accion === 'asignar'
}

export function requiereMotivo(accion: string): boolean {
  return accion === 'resolver' || accion === 'cancelar'
}

export function minimoMotivo(accion: string): number {
  if (accion === 'resolver') return 20
  if (accion === 'cancelar') return 10
  return 0
}