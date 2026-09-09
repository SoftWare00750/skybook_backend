-- Run this once in the Supabase SQL Editor, after 001_create_users_table.sql.
-- Creates the "bookings" table that AppDbContext.cs is mapped to.

create table if not exists public.bookings (
    id uuid primary key default gen_random_uuid(),
    user_id uuid not null references public.users (id) on delete cascade,
    booking_ref text not null,
    airline text not null,
    flight_code text not null,
    depart_code text not null,
    arrive_code text not null,
    depart_time text not null,
    arrive_time text not null,
    depart_date date not null,
    duration text,
    stops text,
    seat_number text,
    cabin_class text,
    passengers integer not null default 1,
    total_price numeric(10, 2) not null default 0,
    status text not null default 'Confirmed',
    created_at timestamptz not null default now()
);

create index if not exists idx_bookings_user_id on public.bookings (user_id);
create index if not exists idx_bookings_depart_date on public.bookings (depart_date);

-- Same rationale as users: accessed only via the .NET backend's postgres
-- role, which owns the table and bypasses RLS.
alter table public.bookings enable row level security;
