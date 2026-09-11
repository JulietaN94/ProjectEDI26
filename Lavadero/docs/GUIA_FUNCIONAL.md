# Guía funcional

## Área pública

### Inicio `/`

Muestra información general y los servicios activos.

El botón `Reservar turno` dirige a `/reservar`.

`Ver servicios` desplaza la página hasta los servicios sin cambiar el estado visual del tema.

### Reservar `/reservar`

El cliente:

1. selecciona un servicio;
2. selecciona un día habilitado del mes actual;
3. elige un horario disponible;
4. ingresa nombre y teléfono;
5. ingresa marca y modelo del vehículo;
6. confirma la reserva.

No necesita una cuenta.

La duración del servicio se tiene en cuenta al calcular los horarios.

Ejemplo: si un servicio dura dos horas y existe otro turno a las 13:00, no se ofrece un inicio a las 12:00 aunque las 12:00 aparezcan inicialmente libres como hora puntual.

### Confirmar turno

Ruta:

```text
/confirmar-turno/{token}
```

Muestra los datos del turno antes de realizar la confirmación.

### Cancelar turno

Ruta:

```text
/cancelar-turno/{token}
```

Muestra los datos y solicita una acción explícita antes de cancelar.

## Administración

### Login `/login`

Autenticación mediante ASP.NET Core Identity.

### Panel `/admin`

Presenta información resumida del funcionamiento del lavadero.

### Turnos `/admin/turnos`

Permite consultar y gestionar turnos vigentes.

Transiciones principales:

```text
Pendiente -> Confirmado
Pendiente -> Cancelado
Confirmado -> Completado
Confirmado -> Cancelado
```

### Historial `/admin/historial`

Permite consultar turnos históricos y filtrar por fechas.

### Servicios `/admin/servicios`

Permite:

- crear;
- editar;
- activar;
- desactivar;
- eliminar cuando las relaciones existentes lo permiten.

El precio almacenado en un turno es histórico y no cambia si luego se modifica el servicio.

### Horarios `/admin/horarios`

Define días de atención y horarios de apertura/cierre.

Un día deshabilitado no se muestra como opción reservable al cliente.

### Configuración `/admin/configuracion`

Permite configurar:

- nombre;
- teléfono;
- dirección;
- WhatsApp;
- logo.

El logo también se utiliza como favicon dinámico.

### Mi cuenta `/admin/cuenta`

Permite modificar información de la cuenta administrativa y contraseña de acuerdo con las reglas de Identity.

## Modo oscuro

La preferencia se guarda en `localStorage` con la clave:

```text
lavadero-theme
```

Los valores utilizados son:

```text
light
dark
```
