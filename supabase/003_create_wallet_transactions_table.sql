-- Run this once in the Supabase SQL Editor, after 001_create_users_table.sql.
-- Creates the "wallet_transactions" table that AppDbContext.cs is mapped to.
-- A user's balance is always the sum of their transactions' amount
-- (positive = credit, negative = debit) -- there is no separate balance
-- column to keep in sync.

create table if not exists public.wallet_transactions (
    id uuid primary key default gen_random_uuid(),
    user_id uuid not null references public.users (id) on delete cascade,
    label text not null,
    amount numeric(10, 2) not null,
    created_at timestamptz not null default now()
);

create index if not exists idx_wallet_transactions_user_id on public.wallet_transactions (user_id);

alter table public.wallet_transactions enable row level security;
