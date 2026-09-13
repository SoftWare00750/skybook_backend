-- Run this once in the Supabase SQL Editor, after 002 and 003.
-- Adds payment-tracking columns now that checkout goes through
-- POST /api/payments/simulate before creating the booking / wallet entry.

alter table public.bookings
    add column if not exists payment_method text,        -- 'card' | 'bank_transfer' | 'wallet' | 'other'
    add column if not exists payment_method_label text,   -- e.g. "Visa •••• 4242"
    add column if not exists payment_reference text;      -- e.g. "PAY-7F3K9QZL"

alter table public.wallet_transactions
    add column if not exists method text,     -- 'card' | 'bank_transfer' | 'other' | 'wallet' | null
    add column if not exists reference text;   -- matching payments.reference, when applicable
