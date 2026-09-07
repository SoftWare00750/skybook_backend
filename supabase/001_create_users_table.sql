-- Run this once in the Supabase SQL Editor (Project -> SQL Editor -> New query)
-- before starting the SkyBook.Api backend. It creates the "users" table
-- that AppDbContext.cs is mapped to.

create table if not exists public.users (
    id uuid primary key default gen_random_uuid(),
    full_name text not null,
    email text not null unique,
    phone text,
    password_hash text not null,
    created_at timestamptz not null default now()
);

create index if not exists idx_users_email on public.users (email);

-- Row Level Security: this table is only ever accessed via the .NET
-- backend using the postgres role (service-level access through the
-- connection string), never directly from the Flutter app, so RLS can
-- stay enabled with no public policies -- the API's Postgres role bypasses
-- RLS as the table owner. If you also want to browse this table from the
-- Supabase dashboard as a different role, add policies as needed.
alter table public.users enable row level security;
