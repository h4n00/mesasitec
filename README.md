# MesaSitec

Mesa de servicio multi-tenant. .NET 8 + SQLite en el backend, Vue 3 + TypeScript en el frontend.

## Arrancar

Dos terminales, desde la raíz del repositorio.

**Terminal 1 — API**


cd backend/src/Api
dotnet run


**Terminal 2 — Frontend**


cd frontend
npm install
npm run dev


Abre **http://localhost:5173** y entrar con `admin@norte.test` / `Sitec.2026`.

Listo. La base de datos se crea sola con los datos de prueba.

* Swagger: http://localhost:5080/swagger
* Health: http://localhost:5080/health

**Requisitos:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) y [Node 18+](https://nodejs.org). Nada más — SQLite es un archivo, no un servidor.

Para empezar de cero, borrar `backend/src/Api/mesasitec.db` y volver a ejecutar.

\---

## Credenciales

Contraseña de todos los usuarios: `Sitec.2026`

|Correo|Rol|Organización|
|-|-|-|
|`admin@norte.test`|Admin|Cooperativa Norte|
|`agente1@norte.test`|Agente|Cooperativa Norte|
|`agente2@norte.test`|Agente|Cooperativa Norte|
|`user1@norte.test`|Solicitante|Cooperativa Norte|
|`user2@norte.test`|Solicitante|Cooperativa Norte|
|`admin@sur.test`|Admin|Bufete Sur|
|`user1@sur.test`|Solicitante|Bufete Sur|

Para ver el aislamiento entre organizaciones, comparar lo que ve `admin@norte.test` contra `admin@sur.test`.

## Pruebas


cd backend
dotnet test


8 pruebas unitarias sobre el cálculo de SLA y la máquina de estados.

Verificación de tipos del frontend:


cd frontend
npx tsc --noEmit


## Variables de entorno

Ver `.env.example`. Ninguna es obligatoria: si no se definen, se usan los valores de `appsettings.json` pensados para desarrollo.

|Variable|Para qué sirve|
|-|-|
|`JWT\_SECRETO`|Clave de firma de los tokens. Mínimo 32 caracteres.|
|`SEED\_FECHA\_BASE`|Fecha base de la semilla, en formato ISO 8601 UTC.|

## Qué está implementado

**Backend**

* Los 9 endpoints del contrato, bajo `/api/v1`
* Las 7 reglas de negocio (RN-01 a RN-07)
* Autenticación JWT HS256, contraseñas con BCrypt
* Errores en `application/problem+json` con el campo `codigo`
* Filtrado, búsqueda, orden y paginación en el servidor
* 8 pruebas con xUnit

**Frontend**

* Las 5 vistas con sus `data-testid`
* Guard de rutas privadas y cliente HTTP centralizado que inyecta el token y redirige ante un 401
* Cada vista maneja los estados de carga, vacío y error
* Los botones de acción que no corresponden al estado o al rol no se renderizan en el DOM

## Qué no está implementado

* `docker-compose.yml` (opcional en el enunciado)
* Los tipos de TypeScript se escribieron a mano, no se generaron desde el OpenAPI
* No hay pruebas de integración sobre los controllers, solo unitarias sobre el dominio

## Endpoint adicional

`GET /api/v1/usuarios/agentes` no forma parte del contrato de 9 endpoints. Se agregó porque el modal de asignación necesita poblar su selector de agentes y el contrato no contempla ninguna forma de obtener esa lista. Está explicado en `DECISIONES.md`.

