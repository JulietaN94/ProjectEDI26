# Migraciones EF Core

Esta carpeta debe contener migraciones generadas por EF Core.

Generar la primera migración desde la raíz de la solución:

```bash
dotnet ef migrations add CreacionInicial --project Lavadero.Infrastructure --startup-project Lavadero.Web --context LavaderoDbContext
```

No agregue `Migrations/` al `.gitignore`: las migraciones deben versionarse.
