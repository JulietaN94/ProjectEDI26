# Changelog

Los cambios relevantes del proyecto se documentan en este archivo.

El formato toma como referencia Keep a Changelog y se utilizan commits con Conventional Commits.

## [Unreleased]

### Preparado

- Nuevo despliegue de producción desde `main`.

## [1.1.0] - 2026-11-02

### Agregado

- Botón flotante del carrito para resoluciones móviles y tablets.
- Contador reactivo de unidades en el acceso al carrito.
- Datos de demostración versionados para servicios y productos.
- Pruebas de reservas, pedidos, límites, transiciones de estado, CRUD, búsqueda y paginación.
- Documentación XML del servicio de catálogo.
- Badges y documentación de CI/CD en el README.

### Cambiado

- El despliegue automático de Azure se ejecuta desde `main`.
- La política de transiciones de turnos se centralizó en el dominio.
- El modelo de EF Core dejó de acoplar el campo de logo a un tipo exclusivo de SQL Server.

### Corregido

- El acceso flotante al carrito ya no activa el router ni navega al inicio.
- El catálogo móvil reserva espacio para que el botón flotante no tape contenido.

## [1.0.0] - 2026-10-23

### Agregado

- Catálogo público de productos con precio, marca, descripción e imagen opcional.
- Carrito con cantidades, subtotales y total estimado.
- Flujo de pedido en dos pasos sin registro previo del cliente.
- Administración de productos y pedidos.
- Estados de pedido con transiciones controladas.
- Productos destacados en el inicio según cantidad pedida.
- Migración `AgregarCatalogoPedidos`.

### Mejorado

- Componentes reutilizables para productos, detalle, carrito y pedidos.
- Diseño responsive y soporte de modo claro/oscuro en el catálogo.

## [0.9.0] - 2026-09-10

### Agregado

- Reservas públicas sin registro.
- Administración de servicios.
- Administración de horarios.
- Panel administrativo.
- Historial de turnos.
- Configuración del lavadero.
- ASP.NET Core Identity y rol Administrador.
- Logo configurable desde la aplicación.
- Favicon dinámico basado en el logo.
- Modo claro y oscuro con persistencia en navegador.
- Confirmación pública de turnos mediante token.
- Cancelación pública de turnos mediante token.
- Enlaces de confirmación y cancelación visibles después de reservar.
- Health check de base de datos.
- Logging estructurado en operaciones relevantes.

### Seguridad

- Tokens criptográficamente aleatorios.
- Índices únicos para tokens de confirmación y cancelación.
- Confirmación y cancelación con actualización atómica.
- Páginas con token marcadas `noindex` y `no-referrer`.
- Cookies administrativas `HttpOnly`, `Secure` y `SameSite=Strict`.
- Lockout de Identity.
- Protección de endpoints administrativos.
- Validación de mismo origen para operaciones del logo.

### Corregido

- Disponibilidad de horarios considerando duración completa del servicio.
- Liberación de horario al cancelar un turno.
- Días cerrados ocultos en la reserva.
- Validación de teléfonos.
- Normalización de teléfonos para evitar clientes duplicados por formato.
- Prevención de doble reserva simultánea con bloqueo de SQL Server.
- Actualización de enlaces al crear varias reservas sin abandonar la página.
- Persistencia del modo oscuro durante navegación.
- Estilos oscuros de Horarios, Mi cuenta y diálogos.
- Menú móvil.
- Botón "Ver servicios".
- Filtro de servicios por estado.
- Límites y mensajes de precio.
- Restricciones de fechas en Historial.

### Cambiado

- El precio del turno conserva el precio histórico del servicio.
- El logo pasó a gestionarse mediante endpoints HTTP específicos.
- WhatsApp manual utiliza enlaces `wa.me`.

### Eliminado

- Propiedades antiguas de logo que ya no se utilizaban.
- Función JavaScript obsoleta `abrirWhatsApp`.
