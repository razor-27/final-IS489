# Guía de Despliegue — WariSalud

## Stack de producción
| Capa | Servicio |
|------|---------|
| Base de datos | Supabase (PostgreSQL) |
| Backend (.NET) | Render — Web Service |
| Frontend (React) | Render — Static Site |

---

## 1. Supabase — Configurar base de datos

1. Ve a [supabase.com](https://supabase.com) y crea un proyecto.
2. En **Settings → Database**, copia la **Connection string** modo `URI` (Transaction pooler):
   ```
   postgresql://postgres.[ref]:[password]@aws-0-us-east-1.pooler.supabase.com:6543/postgres
   ```
   > **NUNCA** la guardes en codigo. Solo como variable de entorno en Render.

---

## 2. Render — Backend (.NET API)

1. Crea un **Web Service** en [render.com](https://render.com) y conecta tu repo.
2. Configuracion del servicio:
   - **Build Command:** `dotnet publish WariSalud.API/WariSalud.API.csproj -c Release -o ./publish`
   - **Start Command:** `dotnet ./publish/WariSalud.API.dll`
3. **Variables de entorno** (Environment -> Add Environment Variable):

| Variable | Valor |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Tu URI de Supabase |
| `Jwt__Key` | Clave aleatoria 32+ chars |
| `Jwt__Issuer` | `WariSalud.API` |
| `Jwt__Audience` | `WariSalud.Clients` |
| `AllowedOrigins` | URL de tu frontend en Render |

---

## 3. Render — Frontend (React + Vite)

1. Crea un **Static Site** en Render con el mismo repo.
2. Configuracion:
   - **Root Directory:** `warisalud-frontend`
   - **Build Command:** `npm ci && npm run build`
   - **Publish Directory:** `dist`
3. **Variables de entorno:**

| Variable | Valor |
|----------|-------|
| `VITE_API_URL` | URL del backend en Render |

4. En **Redirects/Rewrites**: `/*` -> `/index.html` (Rewrite) para React Router.

---

## 4. Orden de despliegue

```
1. Crear proyecto en Supabase -> copiar connection string
2. Desplegar Backend en Render -> configurar todas las env vars
3. Copiar URL del Backend (ej: https://warisalud-api.onrender.com)
4. Desplegar Frontend en Render -> poner VITE_API_URL con la URL del paso 3
5. Copiar URL del Frontend -> volver al Backend y actualizar AllowedOrigins
6. Re-deploy del Backend para aplicar el CORS actualizado
```

---

## 5. Seguridad aplicada

- `appsettings.Development.json` y `appsettings.Production.json` en `.gitignore`
- `appsettings.json` solo tiene estructura vacia, sin secretos reales
- Todos los secretos se inyectan como variables de entorno en Render
- CORS configurado dinamicamente desde variable de entorno
- Swagger solo habilitado en `Development`
- `.env` y `.env.*` en `.gitignore` del frontend

---

## 6. Comandos utiles

```bash
# Build local del backend
dotnet build WariSalud.API

# Build local del frontend
cd warisalud-frontend && npm run build

# Nueva migracion (cuando cambies el modelo de datos)
dotnet ef migrations add NombreMigracion --project WariSalud.Infrastructure --startup-project WariSalud.API --output-dir Persistence/Migrations
```
