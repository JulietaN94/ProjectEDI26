# Configuración de desarrollo

## Requisitos

- .NET 8 SDK
- SQL Server
- Visual Studio 2022 o equivalente
- EF Core Tools si se utilizan comandos de migración desde Package Manager Console

## Cadena de conexión

Clave utilizada:

```text
ConnectionStrings:DefaultConnection
```

Ejemplo de desarrollo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LavaderoDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

La cadena debe adaptarse al SQL Server disponible en cada equipo.

## Administrador inicial

Los datos no sensibles pueden permanecer en configuración.

La contraseña debe mantenerse fuera del repositorio.

Configurar con:

```powershell
dotnet user-secrets set "AdminSeed:Password" "TU_PASSWORD_SEGURA" --project Lavadero.Web
```

El proyecto Web tiene configurado un `UserSecretsId`.

## Migraciones

Desde Package Manager Console:

```powershell
Add-Migration NombreMigracion -Project Lavadero.Infrastructure -StartupProject Lavadero.Web -Context LavaderoDbContext
```

Aplicar:

```powershell
Update-Database -Project Lavadero.Infrastructure -StartupProject Lavadero.Web -Context LavaderoDbContext
```

La aplicación también ejecuta las migraciones pendientes durante el arranque mediante `ApplyDatabaseMigrationsAsync()`.

## Ejecutar

```powershell
dotnet restore
dotnet build
dotnet run --project Lavadero.Web
```

## Health check

Con la aplicación iniciada:

```text
/health
```

Permite verificar que la aplicación puede comunicarse con la base de datos.

## Logo

Formatos admitidos por el endpoint actual:

```text
PNG
JPEG
WEBP
```

El archivo no debe superar 500 KB.

Endpoints:

```text
GET  /branding/logo
GET  /branding/favicon
POST /branding/logo/upload
POST /branding/logo/delete
```

Las operaciones de escritura requieren un administrador autenticado.
