# Validación manual

Checklist recomendado antes de una entrega o merge a `main`.

## Reserva

- Crear una reserva válida.
- Verificar que solo aparezcan días abiertos.
- Verificar que no aparezcan horarios que no permitan completar la duración del servicio.
- Verificar que un horario ocupado no pueda reservarse.
- Probar teléfono con espacios, guiones y paréntesis.
- Realizar dos reservas seguidas sin abandonar `/reservar` y comprobar que los enlaces cambien.

## Concurrencia

Abrir dos sesiones o ventanas.

Intentar guardar el mismo servicio, día y horario casi al mismo tiempo.

Resultado esperado:

```text
una reserva se crea;
la otra se rechaza porque el horario ya se encuentra ocupado.
```

## Confirmación

- Crear turno Pendiente.
- Abrir su enlace de confirmación.
- Confirmar.
- Verificar estado Confirmado.
- Volver a abrir el enlace.
- Verificar que continúe Confirmado sin errores.

## Cancelación

- Crear un turno.
- Abrir enlace de cancelación.
- Verificar que abrirlo NO cancele automáticamente.
- Pulsar `Sí, cancelar turno`.
- Verificar estado Cancelado.
- Comprobar que el horario quede nuevamente disponible.

## Administración

- Login correcto.
- Login incorrecto.
- Panel.
- Turnos.
- Historial.
- Crear/editar/desactivar servicio.
- Configurar horarios.
- Guardar configuración.
- Subir logo.
- Eliminar logo.
- Cambiar datos de cuenta.

## Responsive

Probar al menos:

```text
Desktop
Tablet
412px aproximados
```

En móvil comprobar especialmente:

- menú hamburguesa;
- formulario de reserva;
- cards;
- páginas de confirmación/cancelación.

## Modo oscuro

Recorrer:

- Inicio.
- Reserva.
- Login.
- Panel.
- Turnos.
- Historial.
- Servicios.
- Horarios.
- Configuración.
- Mi cuenta.
- diálogos de confirmación.

No deben quedar inputs o cards con fondo blanco forzado cuando el modo oscuro está activo.

## Health check

Abrir:

```text
/health
```

Debe responder correctamente cuando SQL Server esté disponible.
