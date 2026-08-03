# Tres decisiones técnicas



## Uso de IA

Usé Claude durante todo el proyecto. El código lo generó la IA y yo lo fui copiando al proyecto.

No dejé que armara todo de una sola vez, aunque era posible. Lo pedí por partes: primero las entidades, luego la conexión a la base, después un endpoint a la vez. En cada paso compilé y probé antes de seguir.

Lo hice así porque si el proyecto aparece terminado de una vez, no sé dónde está cada codigo. Trabajando por partes, cada archivo pasó por mis manos.

Lo que hice yo:

* Revisé el enunciado y verifiqué contra él cada pieza
* Elegí entre las opciones que se me plantearon: usar enums en vez de texto para los estados, dejar DTOs separados para crear y editar, no hacer el `docker-compose` opcional, usar el `Assert` que ya trae xUnit en vez de instalar otra librería
* Probé los 9 endpoints uno por uno en Swagger, incluyendo los errores 401, 403, 404, 409 y 422
* Comprobé el aislamiento entre organizaciones comparando lo que ve un usuario de Cooperativa Norte contra uno de Bufete Sur
* Recorrí el flujo completo de estados en la interfaz con los tres roles
* Probé el proyecto clonado en otra computadora para confirmar que arranca desde cero

Los errores que aparecieron los resolví consultándolos, pero partiendo de lo que veía en Visual Studio 2022 y en la consola del navegador.



### 1\. Las reglas de negocio están separadas de los controllers

`CalculadoraSla` y `MaquinaEstados` están en el proyecto `Dominio`. Son clases que solo hacen cálculos: no saben nada de HTTP ni de la base de datos.

Los controllers las llaman, pero no contienen reglas.

La ventaja se vio al escribir las pruebas: como son funciones normales, cada prueba es una llamada y una comparación. Si esas reglas estuvieran dentro de los controllers, habría que levantar un servidor y una base de datos falsa para probar la misma multiplicación.

### 2\. La base de datos se crea sola al arrancar

Uso `EnsureCreated()` en lugar de migraciones, y la semilla revisa si ya hay datos antes de insertar.

El objetivo era que quien clone el repositorio solo escriba `dotnet run` y tenga todo listo, sin instalar SQLite ni ejecutar comandos extra. Arrancar dos veces no duplica los datos.

La desventaja es que si cambio una entidad, hay que borrar el archivo `mesasitec.db` para que se vuelva a crear. En un proyecto real usaría migraciones de EF Core.

### 3\. El frontend repite la máquina de estados, pero el backend manda

El archivo `frontend/src/api/acciones.ts` tiene las mismas tablas de estados y permisos que el backend.

Lo hice así porque el enunciado pide que los botones que no corresponden no existan en el DOM. Para eso, el frontend tiene que saber qué mostrar antes de que el usuario haga clic.

Esto no debilita la seguridad: el backend sigue validando todo. Si alguien modificara el HTML o llamara directo a la API, recibiría un 403 o un 409 igual. El frontend decide qué se ve, el backend decide qué se permite.

## Qué haría con más tiempo

* **Migraciones de EF Core** en lugar de `EnsureCreated()`, para poder cambiar el modelo sin borrar la base.
* **Separar `SolicitudesView.vue` en componentes.** Tiene unas 250 líneas con los filtros, la tabla y la paginación juntos..
* **Generar los tipos de TypeScript desde el OpenAPI**.
* **Pruebas sobre los controllers**, no solo sobre el dominio.
* **`docker-compose.yml`.** Lo dejé fuera porque es opcional y preferí usar el tiempo en las reglas de negocio.

## Dónde me atasqué

**El login funcionaba pero `/me` daba 401.** El token se veía bien, pero el código no encontraba el id del usuario dentro de él. Resultó que .NET le cambia el nombre a ese dato al validar el token. Se resolvió con `MapInboundClaims = false`.

**La semilla fallaba al guardar las solicitudes.** Había puesto el código de solicitud como único en toda la tabla, pero las dos organizaciones generan `SOL-2026-00001` cada una. Cambié el índice para que la combinación única sea organización + código.

instale la version sdk 8 de .net y que sea compatible con cualquier version 8.

**El login desde el frontend fallaba sin explicación clara.** La consola del navegador mostraba que la petición era bloqueada por CORS. Agregué la política que permite el origen `localhost:5173`.

**Las fechas salían sin la Z.** El contrato pide formato UTC con Z al final, pero SQLite no guarda esa información. Se resolvió marcándolas como UTC al armar la respuesta.

**Swagger no abría desde la raíz.** Entrar a `http://localhost:5080` daba 404 porque no había nada definido ahí. Agregué una redirección a `/swagger`.

## Cosas que decidí por mi cuenta

El enunciado dice que un Solicitante solo puede editar sus propias solicitudes y solo en estado `Nueva`, pero no aclara si un Agente puede editar cualquier solicitud de su organización. Asumí que sí, porque los Agentes ya ven y gestionan todas las de su tenant.

También asumí que un Solicitante puede cerrar una solicitud propia que esté `Resuelta`, porque la tabla de permisos lo incluye en esa acción.

