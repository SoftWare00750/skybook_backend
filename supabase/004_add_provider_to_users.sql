-- Run this once in the Supabase SQL Editor, after 001-003.
-- Adds the "provider" column to "users" so social sign-in (Google /
-- Facebook) can be told apart from password accounts. Existing rows all
-- default to 'password', which is correct for every account created
-- before this migration.

alter table public.users
    add column if not exists provider text not null default 'password';
