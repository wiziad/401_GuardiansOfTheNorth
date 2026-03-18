# Guardians Of The North Backend

This folder contains the standalone backend service for the Unity game client. It is built with Next.js route handlers, Prisma, PostgreSQL, and JWT-based authentication.

The backend currently supports:

- user registration
- user login
- current-user lookup
- create save slot
- list all save slots for the current user
- read one save slot
- update one save slot
- health check endpoint
- periodic database keep-alive ping
- database backup and restore scripts
- unit, route, and real database integration tests

## Project Architecture

This backend is designed as a single deployable service that talks directly to a Render PostgreSQL database.

Runtime flow:

```text
Unity Client
  -> HTTP/JSON requests
Next.js Backend
  -> auth, validation, save logic
Prisma
  -> SQL queries
Render PostgreSQL
```

### Main architectural decisions

- `Next.js app router` is used as an API server through route handlers in `app/api`
- `Prisma` handles database access and schema management
- `JWT` is used for stateless authentication
- `Zod` validates request payloads
- `Vitest` runs unit and integration tests
- `Render` deploys the backend service, while Render Postgres stores persistent game data

## Folder Structure

```text
backend/
  app/
    api/
      auth/
        login/
        me/
        register/
      health/
      saves/
        [id]/
  lib/
    auth.ts
    current-user.ts
    env.ts
    keep-alive.ts
    prisma.ts
    response.ts
    validators.ts
  prisma/
    schema.prisma
  scripts/
    backup-db.sh
    common.sh
    list-db-tables.sh
    restore-db.sh
  tests/
    auth.test.ts
    database.integration.test.ts
    saves.test.ts
    validators.test.ts
  FRONTEND_INTEGRATION.md
  README.md
```

### What each part does

- `app/api`: HTTP endpoints that the Unity client calls
- `lib/auth.ts`: password hashing and JWT signing/verification
- `lib/current-user.ts`: extracts the current user from the Bearer token
- `lib/env.ts`: environment variable handling and fallback values
- `lib/keep-alive.ts`: periodic `SELECT 1` database ping
- `lib/prisma.ts`: shared Prisma client instance
- `lib/response.ts`: consistent JSON response helpers
- `lib/validators.ts`: request validation rules
- `prisma/schema.prisma`: database schema definition
- `scripts/`: operational utilities for backup, restore, and inspection
- `tests/`: automated tests, including real database integration coverage

## Database Schema

The main application tables currently used by this backend are:

- `users`
- `save_slots`

Prisma also manages:

- `_prisma_migrations`

### `users`

- `id`: unique user id
- `email`: unique email
- `username`: unique username
- `passwordHash`: hashed password
- `createdAt`
- `updatedAt`

### `save_slots`

- `id`: unique save id
- `userId`: owner user id
- `slotName`: display name for the slot
- `mapId`: current map identifier
- `playerX`
- `playerY`
- `hp`
- `mana`
- `level`
- `createdAt`
- `updatedAt`

Notes:

- one user can have multiple save slots
- the current API limits each user to 3 save slots
- tables have already been created in the connected Render PostgreSQL database

## Authentication

The backend uses Bearer tokens.

Protected routes require:

```text
Authorization: Bearer <token>
```

`JWT_SECRET` is optional in this project because the backend has a fallback secret. That means the service can still run even if Render does not define `JWT_SECRET`.

Important tradeoff:

- convenient for class/demo deployment
- less secure for a real public production system

If this project becomes public-facing later, set a real `JWT_SECRET` in Render.

## API Overview

Base path:

```text
/api
```

Routes:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/saves`
- `POST /api/saves`
- `GET /api/saves/:id`
- `PUT /api/saves/:id`
- `GET /api/health`

### Response format

Successful responses:

```json
{
  "success": true,
  "data": {}
}
```

Error responses:

```json
{
  "success": false,
  "message": "Invalid or expired token"
}
```

## Environment Variables

Copy `.env.example` to `.env`.

Required for normal backend use:

- `DATABASE_URL`

Optional:

- `JWT_SECRET`
- `CORS_ORIGIN`
- `ENABLE_DB_KEEP_ALIVE`
- `KEEP_ALIVE_INTERVAL_MINUTES`

Example:

```env
DATABASE_URL="postgresql://username:password@host:5432/database"
JWT_SECRET="optional"
CORS_ORIGIN="*"
ENABLE_DB_KEEP_ALIVE="true"
KEEP_ALIVE_INTERVAL_MINUTES="30"
```

## Local Setup

1. Copy `.env.example` to `.env`
2. Put your PostgreSQL connection string into `DATABASE_URL`
3. Run `npm install`
4. Run `npx prisma db push`
5. Start the backend with `npm run dev`
6. Run tests with `npm test`

## Testing

The test suite covers three layers:

- validation tests for request payloads
- route tests with mocked Prisma
- real database integration test against the configured PostgreSQL database

Commands:

- `npm test`: run everything, including the real database integration test
- `npm run test:watch`: watch mode for development

### Real database integration coverage

`tests/database.integration.test.ts` connects to the real database from `DATABASE_URL` and verifies:

- real connection works
- a user can be inserted
- a save slot can be inserted
- the inserted records can be queried
- the save slot can be updated
- test data is cleaned up after the run

## Database Backup And Recovery

The project includes shell scripts for operational recovery.

Commands:

- `npm run db:list`
- `npm run db:backup`
- `npm run db:restore -- ./backups/<file.dump>`
- `npm run db:restore -- ./backups/<file.sql>`

What they do:

- `db:list`: show the public tables in the current database
- `db:backup`: create a full custom PostgreSQL dump and a schema-only SQL backup
- `db:restore`: restore from a `.dump` or `.sql` backup file

Backups are stored in:

```text
backend/backups/
```

These scripts read `DATABASE_URL` from `backend/.env`.

## Keep-Alive Behavior

The backend includes a lightweight keep-alive mechanism that runs a database `SELECT 1` query on startup and then every 30 minutes by default.

Purpose:

- reduce cold idle gaps from the application side
- keep database connectivity warm
- expose database reachability through `/api/health`

Config:

- `ENABLE_DB_KEEP_ALIVE=true`
- `KEEP_ALIVE_INTERVAL_MINUTES=30`

Note:

- this helps from the application side
- platform-level sleeping behavior is still ultimately controlled by Render

## Render Deployment

Render configuration is defined in the repository root file [render.yaml](/Users/huojunyu/Downloads/401_GuardiansOfTheNorth/render.yaml).

That file stays in the root because it is a Render Blueprint file, and it already points Render to deploy only the backend service through:

```yaml
rootDir: backend
```

Current deployment behavior:

- only `backend/` is deployed
- build command: `npm install && npm run build`
- start command: `npm run start`

Render environment variables you should set:

- `DATABASE_URL`
- `CORS_ORIGIN`

Optional:

- `JWT_SECRET`
- `ENABLE_DB_KEEP_ALIVE`
- `KEEP_ALIVE_INTERVAL_MINUTES`

## Frontend Integration

Frontend usage details are documented in [FRONTEND_INTEGRATION.md](/Users/huojunyu/Downloads/401_GuardiansOfTheNorth/backend/FRONTEND_INTEGRATION.md).

That file explains:

- how Unity should log in
- how tokens should be stored and sent
- how to load saves
- how to create or update saves
- what request and response shapes to expect

## Current Status

This backend is already in a usable state:

- code compiles successfully
- API routes exist
- Prisma schema exists
- Render Postgres tables exist
- automated tests pass
- real database integration test passes
- backup and restore scripts exist
