# Contribución y flujo Git

Este proyecto utiliza una estrategia Git simple, adecuada para un proyecto académico pequeño.

## Ramas

```text
main
```

Contiene versiones estables.

```text
develop
```

Integra el trabajo en desarrollo antes de pasarlo a `main`.

Para funcionalidades:

```text
feature/nombre-corto
```

Ejemplos:

```text
feature/confirmacion-turnos
feature/configuracion-horarios
```

Para correcciones:

```text
fix/nombre-corto
```

Ejemplos:

```text
fix/menu-movil
fix/modo-oscuro-horarios
```

## Flujo recomendado

```text
main
  └── develop
        ├── feature/...
        └── fix/...
```

Una funcionalidad se desarrolla en su rama y luego se integra en `develop`.

Cuando la versión está estable:

```text
develop -> main
```

## Conventional Commits

Formato:

```text
tipo(alcance): descripción
```

Tipos principales:

```text
feat      nueva funcionalidad
fix       corrección
refactor  mejora interna sin cambiar comportamiento
docs      documentación
style     cambios visuales/formato
chore     mantenimiento
```

Ejemplos del proyecto:

```text
feat(turnos): agregar confirmacion mediante enlace
feat(turnos): agregar cancelacion publica
fix(reservas): evitar doble reserva simultanea
fix(ui): corregir menu movil
fix(theme): corregir horarios en modo oscuro
refactor(configuracion): eliminar propiedades de logo obsoletas
docs(readme): documentar instalacion y arquitectura
```

## Antes de realizar un commit

Verificar:

```text
1. La solución compila.
2. No hay contraseñas ni secretos en archivos versionados.
3. No se agregaron bin/, obj/ ni .vs/.
4. La funcionalidad modificada fue probada manualmente.
5. El mensaje del commit explica qué cambió.
```

## Archivos sensibles

No versionar:

```text
secrets.json
appsettings.Local.json
*.pfx
*.p12
```

La contraseña del administrador debe configurarse con User Secrets o variables de entorno.
