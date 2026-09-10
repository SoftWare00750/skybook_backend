# SkyBook.Api

An ASP.NET Core 8 Web API for the SkyBook Flutter app, backed by a
**Supabase Postgres** database via Entity Framework Core. It handles user
accounts (signup/login), bookings, and the in-app wallet. Flight search
itself still comes straight from aviationstack.com on the Flutter side —
that's a flight-status/schedule API, not something a booking backend needs
to proxy.

## Endpoints

| Method | Path                | Auth | Body                                                          | Returns                                             |
|--------|---------------------|------|-----------------------------------------------------------------|-------------------------------------------------------|
| POST   | `/api/auth/signup`  | —    | `{ "fullName", "email", "phone", "password" }`                  | `{ "token", "fullName", "email" }`                     |
| POST   | `/api/auth/login`   | —    | `{ "emailOrPhone", "password" }`                                 | `{ "token", "fullName", "email" }`                     |
| POST   | `/api/auth/google`  | —    | `{ "idToken" }` (from Google Sign-In on the Flutter app)         | `{ "token", "fullName", "email" }`                     |
| POST   | `/api/auth/facebook`| —    | `{ "accessToken" }` (from Facebook Login on the Flutter app)     | `{ "token", "fullName", "email" }`                     |
| GET    | `/api/profile`      | JWT  | —                                                                | Profile + trip counts                                  |
| PUT    | `/api/profile`      | JWT  | `{ "fullName", "phone" }`                                        | Updated profile                                        |
| GET    | `/api/bookings`     | JWT  | —                                                                | This user's bookings (newest first)                    |
| GET    | `/api/bookings/{id}`| JWT  | —                                                                | A single booking                                       |
| POST   | `/api/bookings`     | JWT  | Flight/seat/price details (see `CreateBookingRequest`)           | The created booking; also debits the wallet             |
| GET    | `/api/wallet`       | JWT  | —                                                                | `{ "balance", "transactions": [...] }`                  |
| POST   | `/api/wallet/topup` | JWT  | `{ "amount", "label" }`                                          | The new transaction                                     |

Passwords are hashed with BCrypt before storage. Successful auth returns a
JWT the Flutter app stores locally and sends as `Authorization: Bearer
<token>` on every authenticated call above. Guest sessions (no token) never
hit the JWT-protected endpoints — the app keeps guests on local/sample data
for those screens, since there's no account to attach a booking or wallet
entry to.

### Google / Facebook sign-in

`/api/auth/google` and `/api/auth/facebook` never trust the Flutter app's
word for who signed in — they take the raw token the native SDK produced
and verify it server-side (Google via `oauth2.googleapis.com/tokeninfo`,
Facebook via a Graph API `/me` call using the token itself), then find or
create a user by the verified email and issue our own JWT exactly like
`/login` does. If an email from Google/Facebook matches an existing
password account, that same account is signed into — one person, one
account, regardless of how they choose to sign in each time.

**This will 401 until you configure real OAuth credentials** — there's no
way around that, since Google and Facebook only issue tokens for apps
registered under *your* developer accounts. Checklist:

1. **Google**: In [Google Cloud Console](https://console.cloud.google.com/apis/credentials),
   create an OAuth consent screen, then three OAuth client IDs: one
   **Web application** (this is the "server client ID" — put it in both
   this API's `Google:ClientId` config *and* the Flutter app's
   `AppConfig.googleServerClientId`), one **Android** (needs your app's
   package name + SHA-1 signing fingerprint), and one **iOS** (needs your
   bundle ID).
2. **Facebook**: Create an app at [Meta for Developers](https://developers.facebook.com/apps),
   add the **Facebook Login** product, and fill in your Android package
   name + key hash and iOS bundle ID under Settings. Copy the App ID /
   Client Token into the Flutter app's Android `strings.xml` and iOS
   `Info.plist` (see `frontend/README.md`). The backend doesn't need a
   Facebook secret for this flow — verifying via the Graph API only needs
   the access token the client already has.
3. Setting `Google:ClientId` here is optional but recommended — without
   it, this backend accepts a valid Google ID token issued for *any*
   Google app, not just yours.

A booking's `Upcoming` vs `Past` status is derived from its `depart_date`
compared to "now", not stored — so it can never drift out of sync. Same
idea for the wallet: the balance returned by `GET /api/wallet` is always
the live sum of that user's transactions, not a separately stored number.

## 1. Supabase project — already set up

The `skybook` Supabase project has been created for you:

- Project ref: `iqyokfghzlbvpdzbgavt`
- Project URL: `https://iqyokfghzlbvpdzbgavt.supabase.co`
- Database host: `db.iqyokfghzlbvpdzbgavt.supabase.co`
- Region: `us-east-1`

Run the SQL migrations **in order** in the Supabase SQL editor before
starting the API:

1. `supabase/001_create_users_table.sql`
2. `supabase/002_create_bookings_table.sql`
3. `supabase/003_create_wallet_transactions_table.sql`
4. `supabase/004_add_provider_to_users.sql`

Each mirrors its matching `DbSet<T>` in `AppDbContext.cs` exactly. Row
Level Security is enabled with no policies on all three — intentional,
since this API talks to Postgres directly as the `postgres` role via the
connection string, which owns the tables and bypasses RLS. RLS only
matters for access through Supabase's PostgREST API / anon-key clients,
which this backend doesn't use.

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

The tables are created directly via the SQL scripts above, so you don't need
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
