-- Run this once in the Supabase SQL Editor, after 001_create_users_table.sql.
-- Creates the "payments" table that AppDbContext.cs is mapped to.
--
-- An audit log of every simulated charge made through
-- POST /api/payments/simulate — flight payments and wallet top-ups alike.
-- There's no real payment gateway wired up: the API always marks a charge
-- "succeeded" after a short simulated processing delay, which is enough to
-- drive a realistic checkout flow (card / bank transfer / wallet / other)
-- without handling real money. Whether a payment actually moves the
-- wallet balance is decided by the caller (bookings debit it only when
-- paid "from wallet"; top-ups credit it directly) — this table is just
-- the receipt trail.

create table if not exists public.payments (
    id uuid primary key default gen_random_uuid(),
    user_id uuid not null references public.users (id) on delete cascade,
    reference text not null,            -- e.g. "PAY-7F3K9QZL"
    method text not null,               -- 'card' | 'bank_transfer' | 'wallet' | 'other'
    method_label text not null,         -- e.g. "Visa •••• 4242"
    amount numeric(10, 2) not null,
    status text not null default 'succeeded',
    purpose text not null default 'flight_booking', -- 'flight_booking' | 'wallet_topup'
    created_at timestamptz not null default now()
);

create index if not exists idx_payments_user_id on public.payments (user_id);

alter table public.payments enable row level security;
