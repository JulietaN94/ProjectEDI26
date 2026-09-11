# Arquitectura

## Visión general

Lavadero utiliza una separación por responsabilidades en cuatro proyectos.

```text
┌───────────────────────────┐
│       Lavadero.Web        │
│ Blazor / UI / Endpoints   │
└─────────────┬─────────────┘
              │
┌─────────────▼─────────────┐
│  Lavadero.Infrastructure  │
│ EF Core / Identity / SQL  │
└─────────────┬─────────────┘
              │
┌─────────────▼─────────────┐
│   Lavadero.Application    │
│ Interfaces / DTO / Models │
└─────────────┬─────────────┘
              │
┌─────────────▼─────────────┐
│      Lavadero.Domain      │
│ Entidades / Enums         │
└───────────────────────────┘
```

## Lavadero.Domain

Contiene el modelo central del negocio.

Entidades:

- `Cliente`
- `Auto`
- `Servicio`
- `Turno`
- `HorarioAtencion`
- `ConfiguracionLavadero`

También contiene `EstadoTurno`.

No depende de Entity Framework Core ni de la interfaz Web.

## Lavadero.Application

Define los contratos que utiliza la aplicación.

Interfaces actuales:

- `IClienteService`
- `IServicioService`
- `ITurnoService`
- `IHorarioAtencionService`
- `IConfiguracionLavaderoService`
- `IDashboardAdminService`

También contiene DTOs, modelos de entrada y `PagedResult<T>`.

## Lavadero.Infrastructure

Implementa los contratos de Application.

Responsabilidades principales:

- `LavaderoDbContext`
- configuraciones de Entity Framework Core;
- migraciones;
- acceso a SQL Server;
- servicios;
- ASP.NET Core Identity;
- seed del administrador;
- aplicación automática de migraciones.

La UI no accede directamente a SQL Server.

## Lavadero.Web

Contiene:

- páginas Razor;
- componentes reutilizables;
- layouts;
- CSS Isolation;
- JavaScript Interop;
- estado temporal de reservas;
- autenticación web;
- endpoints de branding;
- health check.

## Relaciones principales

```text
Cliente 1 ───── * Auto
Auto    1 ───── * Turno
Servicio 1 ──── * Turno
```

`Turno` conserva `PrecioServicio` como snapshot histórico. Si el administrador cambia posteriormente el precio de un servicio, los turnos ya creados mantienen el precio que tenían al reservar.

## Reserva

Flujo resumido:

```text
Servicio
   ↓
Día disponible
   ↓
Horario disponible
   ↓
Datos del cliente y vehículo
   ↓
Validación backend
   ↓
Bloqueo SQL por fecha
   ↓
Revalidación de disponibilidad
   ↓
Guardar Cliente/Auto/Turno
   ↓
Pendiente
```

El teléfono se normaliza para disminuir duplicados por diferencias de formato.

## Confirmación

```text
Turno Pendiente
      ↓
TokenConfirmacion
      ↓
/confirmar-turno/{token}
      ↓
Confirmar reserva
      ↓
Turno Confirmado
```

La confirmación registra `FechaConfirmacion`.

## Cancelación

```text
Pendiente o Confirmado
          ↓
TokenCancelacion
          ↓
/cancelar-turno/{token}
          ↓
Confirmación explícita
          ↓
Cancelado
```

Abrir la URL no cancela automáticamente. Esto evita que un previsualizador de enlaces pueda ejecutar la acción.

## Concurrencia

La creación de reservas utiliza una transacción y `sys.sp_getapplock`.

El bloqueo se crea por fecha:

```text
Lavadero:Reserva:yyyyMMdd
```

Después de obtenerlo se vuelve a consultar la disponibilidad antes de guardar.

## Autenticación

La administración utiliza `ApplicationUser` con ASP.NET Core Identity.

Las rutas `/admin/*` requieren el rol:

```text
Administrador
```

No existe registro público de administradores.
