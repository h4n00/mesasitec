# Decisiones técnicas



## Uso de IA

Usé Claude durante todo el proyecto, y quiero ser preciso sobre cómo.

El código lo generó la IA y yo lo copié al proyecto. No trabajé con generación automática de archivos ni dejé que armara el proyecto completo de una sola vez, aunque era posible. Lo pedí por partes —primero las entidades, luego el DbContext, después un endpoint a la vez— y en cada paso compilé, ejecuté y probé el resultado antes de seguir.

La razón es concreta: si el proyecto aparece terminado de golpe, no sé dónde está cada cosa ni por qué está escrita así. Trabajando por partes, cada archivo pasó por mis manos y entiendo qué hace y en qué capa vive.

Lo que aporté yo:

* Leí el enunciado y verifiqué contra él cada pieza que iba quedando
* Tomé las decisiones que tenían más de una salida válida: enums en vez de strings para estados y prioridades, mantener DTOs separados para crear y editar, no incluir el `docker-compose` opcional, usar el `Assert` de xUnit en vez de agregar otra dependencia
* Probé los 9 endpoints uno por uno en Swagger, incluyendo los casos de error: 401, 403, 404, 409 y 422
* Verifiqué el aislamiento multi-tenant comparando las respuestas con tokens de organizaciones distintas
* Recorrí el flujo completo de estados en la interfaz con usuarios de los tres roles
* Detecté que el contrato de 9 endpoints no permitía poblar el selector de agentes del modal, y decidí agregar el endpoint adicional

Los errores que aparecieron durante el desarrollo los resolví consultándolos con la IA, pero el diagnóstico partía de lo que veía en Visual Studio y en la consola del navegador.

## Qué haría con más tiempo

* **Migraciones de EF Core** en lugar de `EnsureCreated()`, para poder evolucionar el esquema sin borrar la base.
* **Extraer componentes en el frontend.** `SolicitudesView.vue` ronda las 250 líneas y contiene filtros, tabla y paginación. Los separaría en `components/` para que cada pieza se pueda probar y reutilizar por separado.
* **Generar los tipos de TypeScript desde el OpenAPI** en vez de escribirlos a mano, para que el contrato no se desincronice.
* **Pruebas de integración sobre los controllers**, con una base SQLite en memoria, cubriendo los códigos de error de cada regla. Las 8 pruebas actuales cubren el dominio, no las rutas.
* **`docker-compose.yml`** para levantar backend y frontend con un solo comando. Lo dejé fuera a propósito: es opcional en el enunciado y preferí invertir el tiempo en cubrir las reglas de negocio.
* **Una sola fuente para las acciones disponibles**, devolviéndolas desde el endpoint de detalle en vez de duplicar la tabla en el cliente.



### 1\. La lógica de negocio vive en el Dominio, no en los controllers

`CalculadoraSla` y `MaquinaEstados` son clases estáticas en el proyecto `Dominio`, sin ninguna dependencia de Entity Framework ni de ASP.NET. Los controllers las llaman, pero no contienen reglas.

La consecuencia práctica fue que las 8 pruebas unitarias tomaron unos minutos: no hace falta levantar una base de datos ni simular peticiones HTTP para probarlas. Si esas reglas estuvieran dentro de los controllers, cada prueba habría necesitado una base en memoria y un token falso.

En la misma línea, `EstaVencida` recibe la fecha actual como parámetro en vez de leer `DateTime.UtcNow` internamente. Eso permite probarla con fechas fijas y que el resultado no dependa del día en que se ejecuten las pruebas.

### 2\. `EnsureCreated()` con una semilla idempotente, en lugar de migraciones

La base se crea al arrancar con `EnsureCreated()` y la semilla verifica `if (db.Tenants.Any()) return;` antes de insertar nada.

El objetivo era el requisito de arranque: quien clone el repositorio ejecuta `dotnet run` y tiene la base lista con datos, sin comandos adicionales ni instalar SQLite. Arrancar dos veces no duplica los datos.

La contrapartida es que `EnsureCreated()` no actualiza el esquema de una base que ya existe. Durante el desarrollo, cada cambio en el modelo obligó a borrar `mesasitec.db`. Para un proyecto con varios ambientes esto no serviría y usaría migraciones de EF Core; para una prueba técnica que debe levantar en un comando, el intercambio vale la pena.

### 3\. La máquina de estados está duplicada en el frontend, pero el backend es la única

El archivo `frontend/src/api/acciones.ts` replica las tablas de RN-02 y RN-03 para decidir qué botones renderizar. Es duplicación deliberada.

El frontend necesita saber qué mostrar antes de que el usuario haga clic; sin esa tabla, tendría que renderizar los seis botones y descubrir cuáles fallan. La regla 7.5 exige que los botones no aplicables ni siquiera existan en el DOM, así que la decisión tiene que tomarse en el cliente.

Eso no relaja la validación del servidor. Si alguien manipulara el DOM o llamara directo a la API, el backend responde igual con 403 o 409. El frontend decide qué se ve; el backend decide qué se permite.

El riesgo es que ambas tablas se desincronicen si las reglas cambian. Con más tiempo, expondría la lista de acciones disponibles desde el propio endpoint de detalle para tener una sola fuente.

## Dónde me atasqué

**El claim `sub` llegaba nulo.** El login funcionaba y devolvía un token válido, pero `GET /me` respondía 401. El token se veía correcto en jwt.io. Resultó que ASP.NET, por compatibilidad histórica, renombra los claims estándar de JWT a URLs largas de Microsoft, así que `User.FindFirst("sub")` no encontraba nada. Se resolvió con `options.MapInboundClaims = false`.

**El índice único del código de solicitud.** La semilla fallaba al guardar con un `DbUpdateException`. Había declarado `Codigo` como único global, pero RN-07 dice que el correlativo es por organización: Cooperativa Norte y Bufete Sur generan ambas `SOL-2026-00001`. Cambié el índice a compuesto sobre `TenantId + Codigo`.

**Versiones de paquetes NuGet.** Tengo instalado el SDK 9 y `dotnet add package` sin especificar versión instalaba la 10.0.10, incompatible con `net8.0`. Fijé cada paquete de Microsoft con `--version 8.0.11`.

**Compatibilidad del SDK.** El `global.json` apuntaba al parche exacto que tengo instalado, lo que habría roto la compilación en una máquina con otro parche de .NET 8. Lo cambié a `"version": "8.0.100"` con `"rollForward": "latestFeature"`, que acepta cualquier SDK de la version 8.

**CORS.** El login desde el frontend fallaba con un mensaje genérico. La consola del navegador mostraba que la petición era bloqueada por falta de `Access-Control-Allow-Origin`. Agregué una política que permite `http://localhost:5173`, colocando `UseCors` antes de `UseAuthentication` para que las respuestas de error también lleven las cabeceras.

**Fechas sin zona horaria.** SQLite no almacena el `Kind` de un `DateTime`, así que las fechas salían como `2026-01-15T08:00:00` sin la `Z` que pide el contrato. Se resolvió con `DateTime.SpecifyKind(..., DateTimeKind.Utc)` al mapear a los DTOs.

## Ambigüedades que resolví por mi cuenta

RN-03 detalla qué puede editar un Solicitante —solo las propias y solo en estado `Nueva`— pero no dice si un Agente puede editar título y descripción de cualquier solicitud de su organización. Asumí que sí, por coherencia con que Agente y Admin ven y gestionan todas las de su tenant.

También asumí que un Solicitante puede ejecutar `cerrar` sobre una solicitud propia que esté `Resuelta`, ya que la tabla de permisos lo incluye entre los roles autorizados para esa acción.

