# Decisiones técnicas

Este documento resume decisiones relevantes para explicar y defender el proyecto.

## Blazor Interactive Server

Se utiliza Blazor Web App con Interactive Server.

Para el alcance del proyecto no se creó una API REST separada, evitando complejidad innecesaria. Los componentes interactivos se comunican con los servicios de la aplicación desde el servidor.

## SQL Server

SQL Server es la base principal del proyecto.

Esto permite utilizar Entity Framework Core y también mecanismos específicos de concurrencia como `sp_getapplock` para proteger la reserva frente a solicitudes simultáneas.

## Reserva sin registro

El cliente no necesita crear usuario.

Los datos mínimos requeridos son:

- nombre completo;
- teléfono;
- marca;
- modelo;
- servicio;
- fecha y horario.

La administración sí requiere autenticación.

## Identificación del cliente por teléfono

El teléfono se normaliza a dígitos antes de guardar nuevos clientes.

Esto evita que diferencias como espacios, guiones o paréntesis creen clientes duplicados innecesariamente.

## Precio histórico

`Turno.PrecioServicio` conserva el precio al momento de reservar.

No se calcula el historial utilizando el precio actual de `Servicio`, porque dicho precio puede cambiar posteriormente.

## Tokens públicos

No se expone el `Turno.Id` como mecanismo de autorización.

Cada turno recibe tokens independientes para:

- confirmar;
- cancelar.

Los tokens se generan con `RandomNumberGenerator`.

## Confirmar o cancelar requiere acción explícita

Un GET a `/confirmar-turno/{token}` o `/cancelar-turno/{token}` únicamente muestra la página.

El cambio de estado ocurre después de que el usuario pulsa el botón correspondiente.

Esto reduce el riesgo de que bots o previsualizadores de enlaces ejecuten acciones accidentalmente.

## Doble reserva

Comprobar disponibilidad antes de guardar no es suficiente ante concurrencia.

Dos solicitudes podrían validar simultáneamente el mismo horario.

Por eso la creación:

1. inicia una transacción;
2. adquiere un bloqueo SQL por fecha;
3. vuelve a validar disponibilidad;
4. guarda;
5. confirma la transacción.

## Logo en base de datos

El logo se guarda mediante la configuración del lavadero y se sirve a través de endpoints HTTP.

Esto permite utilizarlo tanto en la interfaz como en el favicon dinámico sin depender de una ruta física específica del equipo.

## WhatsApp

Actualmente existe apertura manual mediante `wa.me`.

La integración automática se pospone hasta contar con una URL pública y realizar el despliegue. Luego se incorporará WhatsApp Business API sin cambiar el diseño de tokens ya implementado.

## Modo oscuro

La preferencia visual se guarda en `localStorage`.

La interfaz utiliza variables CSS compartidas para mantener consistencia entre los modos claro y oscuro.
