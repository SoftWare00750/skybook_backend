# SkyBook.Api

An ASP.NET Core 8 Web API that handles **user accounts only** — signup and
login — for the SkyBook Flutter app, backed by a **Supabase Postgres**
database via Entity Framework Core.

It intentionally does *not* handle flights, bookings, wallet, etc. — those
stay client-side (sample data) or come from aviationstack.com directly from
the Flutter app.

## Endpoints

| Method | Path              | Body                                                        | Returns                                  |
|--------|-------------------|--------------------------------------------------------------|-------------------------------------------|
| POST   | `/api/auth/signup`| `{ "fullName", "email", "phone", "password" }`               | `{ "token", "fullName", "email" }`        |
| POST   | `/api/auth/login` | `{ "emailOrPhone", "password" }`                              | `{ "token", "fullName", "email" }`        |

Passwords are hashed with BCrypt before storage. Successful auth returns a
JWT the Flutter app stores locally and can send as `Authorization: Bearer
<token>` on future authenticated calls (none are wired up yet since the
brief only asked for user info / login / signup).

## 1. Supabase project — already set up

The `skybook` Supabase project has been created for you and the `users`
table is live:

- Project ref: `iqyokfghzlbvpdzbgavt`
- Project URL: `https://iqyokfghzlbvpdzbgavt.supabase.co`
- Database host: `db.iqyokfghzlbvpdzbgavt.supabase.co`
- Region: `us-east-1`
- `public.users` table: created, with a unique index on `email`, matching
  `AppDbContext.cs` exactly (verified via `list_tables`)
- Row Level Security is enabled on `users` with no policies — that's
  intentional. This API talks to Postgres directly as the `postgres` role
  (via the connection string below), which owns the table and bypasses
  RLS entirely. RLS only matters for access through Supabase's PostgREST
  API / anon-key clients, which this backend doesn't use.

**One manual step required:** the database password isn't something an
API/automation can retrieve — Supabase only shows it once, at project
creation, and it wasn't visible to the tool that created this project.
Grab (or reset) it yourself:

1. Go to the [Supabase dashboard](https://supabase.com/dashboard/project/iqyokfghzlbvpdzbgavt) → **Project Settings → Database**.
2. Under **Database password**, either copy your saved password or click
   **Reset database password** to generate a new one.

Then drop it into the connection string below (already pre-filled with
the real host).

## 2. Configure the API

Open `appsettings.json` (or, better, use `dotnet user-secrets` / environment
variables so you never commit real credentials) and fill in:

```json
{
  "ConnectionStrings": {
    "Supabase": "Host=db.iqyokfghzlbvpdzbgavt.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_DB_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Jwt": {
    "Key": "a-long-random-string-at-least-32-characters"
  }
}
```

Recommended for local dev instead of editing appsettings.json directly:

```bash
cd SkyBook.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=db.iqyokfghzlbvpdzbgavt.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_DB_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "Jwt:Key" "a-long-random-string-at-least-32-characters"
```

## 3. Run it

```bash
cd SkyBook.Api
dotnet restore
dotnet run
```

By default it listens on `http://localhost:5236` (see
`Properties/launchSettings.json`) and serves Swagger UI at
`http://localhost:5236/swagger` in development.

The table is created directly via the SQL script above, so you don't need
to run EF Core migrations — but if you'd rather manage the schema through
EF Core migrations instead of raw SQL, you can scaffold them once the
project builds:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 4. Point the Flutter app at it

In the Flutter project, `lib/config/app_config.dart` defaults to
`http://localhost:5236`. Override it at run time if needed:

```bash
flutter run --dart-define=BACKEND_BASE_URL=http://localhost:5236
```

- **Android emulator**: use `http://10.0.2.2:5236` instead of `localhost`.
- **Physical device**: use your computer's LAN IP, e.g. `http://192.168.1.20:5236`,
  and make sure the device is on the same network.
- **Production**: deploy this API (Azure App Service, Fly.io, Render, a VM,
  etc.) and point `BACKEND_BASE_URL` at its public HTTPS URL.

## Security notes for going further

- Rotate the `Jwt:Key` and Supabase DB password before shipping — the
  placeholders in `appsettings.json` are not usable credentials.
- Add HTTPS termination (a reverse proxy or hosting platform typically
  handles this) before exposing this publicly.
- Consider rate-limiting `/api/auth/login` to slow down credential
  stuffing attempts.
