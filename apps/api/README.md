# Marketplace Backend — Sprint 1.1 Foundation + Sprint 1.5 Security Corrections

Baby & Kids Marketplace — ASP.NET Core (.NET 8 LTS) backend, Clean Architecture,
**Modular Monolith** (Microservice extraction is explicitly deferred — see the
Architecture doc in `docs/`). Sprint 1.1 delivered **infrastructure/skeleton only**; Sprint
1.5 corrected three Security Gate violations found in Architecture Review (Argon2id instead
of BCrypt, RS256 instead of HS256, no direct identifiers in JWT claims) — see "Security
corrections (Sprint 1.5)" below for what changed and why. No module has a complete business
feature yet (by design — see "What this Sprint intentionally does NOT include" below).

## Solution layout

```
Marketplace.sln
├── src/
│   ├── Core/
│   │   ├── Marketplace.Domain/          Entities, Shared Kernel (BaseEntity, Result, Error, DomainEvent...)
│   │   │   └── Modules/{Identity,Users,Catalog,Sellers,Cart,Orders,Payments,
│   │   │                Inventory,Notifications,Reviews,Wishlist,Finance,CMS}/
│   │   └── Marketplace.Application/     CQRS (MediatR), validation/logging pipeline, interfaces
│   │       └── Modules/{...same 13 modules...}/
│   ├── Infrastructure/
│   │   └── Marketplace.Infrastructure/  EF Core + PostgreSQL, Redis, RabbitMQ, JWT/RBAC, Outbox, Serilog
│   └── Presentation/
│       └── Marketplace.API/             Controllers, middlewares, API versioning, Swagger, health checks
├── tests/
│   ├── Marketplace.UnitTests/
│   └── Marketplace.IntegrationTests/    WebApplicationFactory + Testcontainers (real Postgres/Redis/RabbitMQ)
└── docs/
```

Thirteen business modules (`Users`, `Catalog`, `Sellers`, `Cart`, `Orders`, `Payments`,
`Inventory`, `Notifications`, `Reviews`, `Wishlist`, `Finance`, `CMS` + `Identity`) each have
their own namespace/folder in **both** `Domain` and `Application` from day one. `Identity` is
the only module with real entities this sprint (`User`, `Role`, `Permission`, `RefreshToken`)
because JWT + RBAC infrastructure needs a concrete shape to compile against — every other
module is a marker class only, waiting for its own feature sprint. Keeping every module in
its own namespace now is what makes extracting a module into its own service later (see
Architecture doc §15) a namespace/project move instead of a redesign.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) + Docker Compose (for Postgres/Redis/RabbitMQ, and for
  Integration Tests, which use [Testcontainers](https://testcontainers.com/) — a Docker
  daemon must be reachable from wherever `dotnet test` runs)
- [OpenSSL](https://www.openssl.org/) (or any tool that can generate a PEM RSA keypair) — for
  the JWT signing key, see below

## Generating JWT keys (ADR-014 — RS256, asymmetric)

There is no symmetric JWT secret anymore. Generate a local RSA keypair once:

```bash
mkdir -p keys
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out keys/private.pem
openssl rsa -pubout -in keys/private.pem -out keys/public.pem
```
`keys/` is git-ignored (see `.gitignore`) — never commit these files. In every environment
beyond local dev, the private key belongs in a proper secret store (Vault, cloud KMS, etc.),
mounted read-only into the container — see the `backend.volumes` mapping in
`docker-compose.yml` and `JWT_KEYS_DIR` in `.env.example`.

## Running locally

```bash
# 1. Copy the env template and fill in real values (never commit .env)
cp .env.example .env

# 2. Generate the JWT keypair if you haven't already (see above)
mkdir -p keys && openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out keys/private.pem && openssl rsa -pubout -in keys/private.pem -out keys/public.pem

# 3. Start Postgres, Redis, RabbitMQ (and the API, containerized)
docker compose up -d --build

# 4. Apply the database migration (see "Migrations" below if you haven't generated one yet)
dotnet ef database update \
  --project src/Infrastructure/Marketplace.Infrastructure \
  --startup-project src/Presentation/Marketplace.API

# 5. Confirm it's alive
curl http://localhost:8080/health/live
curl http://localhost:8080/health/ready
curl "http://localhost:8080/api/v1/diagnostics/ping?message=hello"
```

### Running the API outside Docker (Postgres/Redis/RabbitMQ still in Docker)

```bash
docker compose up -d postgres redis rabbitmq
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=marketplace_dev;Username=marketplace;Password=<your .env password>"
export ConnectionStrings__Redis="localhost:6379,password=<your .env password>"
export RabbitMq__UserName=marketplace
export RabbitMq__Password=<your .env password>
export Jwt__PrivateKeyPath="$(pwd)/keys/private.pem"
export Jwt__PublicKeyPath="$(pwd)/keys/public.pem"
export PasswordHashing__Pepper="<generate with: openssl rand -base64 48>"

dotnet run --project src/Presentation/Marketplace.API
```
Swagger UI: `http://localhost:5xxx/swagger` (port printed on startup, Development only).

## Migrations

No migration is committed yet — generate the initial one locally once you have the SDK and
a reachable NuGet feed (see "A note on this environment" below):

```bash
dotnet tool install --global dotnet-ef   # first time only
dotnet ef migrations add InitialCreate \
  --project src/Infrastructure/Marketplace.Infrastructure \
  --startup-project src/Presentation/Marketplace.API \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project src/Infrastructure/Marketplace.Infrastructure \
  --startup-project src/Presentation/Marketplace.API
```
This creates: `users`, `roles`, `permissions`, `user_roles`, `role_permissions`,
`refresh_tokens` (including `device_id`/`user_agent` — device binding schema added in
Sprint 1.5, enforcement deferred to the Identity feature sprint), `outbox_messages`.

## Security corrections (Sprint 1.5)

Architecture Review flagged three Sprint 1 violations of already-approved ADRs; all three
are fixed as of this sprint:

| # | Was | Now | Why |
|---|---|---|---|
| 1 | `BcryptPasswordHasher` (BCrypt, work factor 12) | `Argon2idPasswordHasher` (Konscious.Security.Cryptography.Argon2, OWASP-recommended parameters) + server-side pepper via `PasswordHashing:Pepper` | Argon2id is memory-hard; BCrypt is not GPU/ASIC-resistant to the same degree. The pepper is a secret never stored in the DB, so a stolen DB dump alone can't be brute-forced even with per-row salts intact. |
| 2 | `HmacSha256` with one symmetric secret | `RsaSha256` (RS256), key pair loaded from `Jwt:PrivateKeyPath` / `Jwt:PublicKeyPath` (PEM) | ADR-014. Asymmetric signing means only the issuing service ever holds the private key; any future service that only needs to verify tokens gets the public key (or the JWKS endpoint — see the `TODO` in `TokenService`) without gaining signing capability. |
| 3 | `new Claim("phone_number", phoneNumber)` in the access token | Claim removed entirely; `ITokenService.GenerateAccessToken` no longer even takes a phone number parameter | Security doc §2.2 — a JWT is signed, not encrypted; its payload is trivially base64-decodable by anyone holding the token (client JS, browser devtools, a log line that captured an `Authorization` header). Only `sub`, `jti`, and `permission` claims are allowed. |

Two smaller items also landed this sprint:
- `BaseEntity.cs`'s comment wrongly implied `Guid.NewGuid()` produces sequential/v7-style
  IDs — it doesn't (it's random v4). Comment corrected; no behavior change. UUID v7 for
  high-volume append-heavy tables (`AuditLog`, `OrderStatusHistory` per ADR-017) is deferred
  until those tables actually get built.
- `RefreshToken` gained nullable `DeviceId`/`UserAgent` columns (device-binding schema only —
  enforcing a max of 5 concurrent sessions per user is deferred to the Identity feature
  sprint, once a real login endpoint exists to populate/enforce it meaningfully).



```bash
dotnet test tests/Marketplace.UnitTests
dotnet test tests/Marketplace.IntegrationTests   # requires a running Docker daemon (Testcontainers)
```

Definition-of-Done coverage:
- **Health Check test** → `Diagnostics/HealthCheckTests.cs` (asserts `/health`, `/health/ready`, `/health/live` all return 200)
- **Validation test** → `Application/PingQueryValidatorTests.cs` (unit) + `Diagnostics/PingEndpointTests.cs` (integration, asserts a 400 ProblemDetails)
- **Global Exception Handling test** → `Diagnostics/PingEndpointTests.cs::Ping_with_an_empty_message_returns_400_problem_details` (asserts `application/problem+json`, a `correlationId`, and an `errors` map — not a raw 500/stack trace)
- **Argon2id test** (Sprint 1.5) → `Infrastructure/Identity/Argon2idPasswordHasherTests.cs` (same password → different hash each time; correct/incorrect password verify; different pepper fails verify)
- **RS256 + no-phone-number test** (Sprint 1.5) → `Infrastructure/Identity/TokenServiceTests.cs` (token verifies against the matching RSA public key using RS256; explicit negative assertion that no `phone_number`/email/mobile claim is ever present)

## What this Sprint intentionally does NOT include (per the brief)

- No complete Product/Catalog feature, no complete Order feature, no Payment gateway integration, no frontend.
- No login/register HTTP endpoint yet — JWT issuance, refresh-token rotation, password hashing and RBAC/permission *infrastructure* all exist and compile, but there is no `/auth/*` controller yet; that is the first real Identity feature sprint.
- Migrations are not committed (see above) — generate `InitialCreate` locally as your first step.

## A note on this environment (read before assuming a red build is a bug)

This repository was authored in a sandboxed container **with no internet access to
nuget.org** (only a short allow-list of infra domains is reachable). I installed the .NET 8
SDK here (via `apt`, from `archive.ubuntu.com`) and confirmed `dotnet --version` → `8.0.104`,
but `dotnet restore` fails here with:
```
NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json.
Response status code does not indicate success: 403 (Forbidden).
```
So **I could not actually run `dotnet build`, `dotnet test`, or `docker compose up` in this
sandbox**, and I'm not going to claim otherwise. Every file was written by hand against the
real package APIs (EF Core 8, MediatR 12, FluentValidation 11, Serilog.AspNetCore 8,
RabbitMQ.Client 6.8, StackExchange.Redis via `Microsoft.Extensions.Caching.StackExchangeRedis`,
Testcontainers 3.10) and cross-checked for consistent namespaces/signatures, but a restore +
build on your machine (with normal NuGet access) is the real first test — please run it and
tell me what breaks, since I'd rather fix a real compiler error than guess at one.

## Remaining items before this Sprint is fully "Done"

1. **Run `dotnet restore && dotnet build`** on a machine with NuGet access — fix whatever surfaces (this sprint additionally pulls in `Konscious.Security.Cryptography.Argon2`, unverified against a real feed here — same caveat as everything else).
2. **Generate the `InitialCreate` migration** (needs the SDK + a live/throwaway Postgres — see above) and commit it — must now include the `device_id`/`user_agent` columns on `refresh_tokens`.
3. **Run `docker compose up -d --build`** end-to-end and confirm all four containers report healthy, with a real `keys/` RSA pair mounted.
4. **Run both test projects** and confirm green (Integration Tests need a local Docker daemon for Testcontainers).
5. **Reconcile `.env.example`** with the Security team's version — `JWT_PRIVATE_KEY_PATH`, `JWT_PUBLIC_KEY_PATH`, `PASSWORD_PEPPER` were added here matching the names described in the Task 1.5 brief, but I have not seen their actual file; confirm exact names/defaults match before merge.
6. **Publish JWKS** (`/.well-known/jwks.json`) — explicitly deferred (see `TODO` in `TokenService`), needed once a second service needs to verify these tokens.
7. Seed a first `SuperAdmin` role/permission set (currently only two bootstrap permission codes exist: `system.health.view`, `admin.access`) — needed once the first real `/auth/*` endpoint lands.
8. Enforce max-5-concurrent-sessions using the new `DeviceId`/`UserAgent` columns — deferred to the Identity feature sprint.
9. Wire CI (GitHub Actions or similar) to do steps 1–4 on every PR — not part of this brief, but the natural next step.
