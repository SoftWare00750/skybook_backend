-- Run this once in the Supabase SQL Editor, after 001_create_users_table.sql.
-- Creates the "payment_methods" table that AppDbContext.cs is mapped to.
--
-- A payment method saved to a user's wallet (Wallet -> Payment Methods),
-- so checkout can offer "pay with a saved card" instead of re-entering
-- details every time. Only the last 4 digits of a card/account are ever
-- stored — never the full number — matching how a real payment
-- processor's tokenized reference would be handled.

create table if not exists public.payment_methods (
    id uuid primary key default gen_random_uuid(),
    user_id uuid not null references public.users (id) on delete cascade,
    type text not null,                 -- 'card' | 'bank_transfer' | 'other'
    label text not null,                -- e.g. "Visa •••• 4242"

    -- card-specific
    brand text,                         -- 'Visa' | 'Mastercard' | 'Amex' | 'Discover' | 'Card'
    last4 text,
    expiry_month text,
    expiry_year text,

    -- bank_transfer-specific
    bank_name text,
    account_last4 text,

    -- other-specific
    other_provider text,                -- e.g. "PayPal", "Apple Pay", "Google Pay"

    is_default boolean not null default false,
    created_at timestamptz not null default now()
);

create index if not exists idx_payment_methods_user_id on public.payment_methods (user_id);

-- Same rationale as the other tables: accessed only via the .NET
-- backend's postgres role, which owns the table and bypasses RLS.
alter table public.payment_methods enable row level security;
