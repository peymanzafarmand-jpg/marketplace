# Repo & Naming Contract

**وضعیت:** لازم‌الاجرا برای همه تیم‌ها (Backend، Frontend، Security/DevOps)
**نسخه:** v1.0 — TASK 1.4 (Correction Round، Sprint 1)
**مالک:** Claude 3 (Security/DevOps/QA)

این سند ساختار نهایی Repo، نام‌گذاری پروژه، و پورت‌ها را **قفل** می‌کند. هر Task بعدی (Backend، Frontend، Security) باید مطابق همین سند پیش برود؛ هرگونه انحراف باید ابتدا این سند را Update کند، نه برعکس.

---

## 1. نام رسمی پروژه

- نام رسمی: **`Marketplace`**
- استفاده در: Namespace کد C# (`Marketplace.Domain`, `Marketplace.Application`, `Marketplace.Infrastructure`, `Marketplace.API`)، نام Docker Image (`marketplace-api`, `marketplace-web`)، نام دیتابیس (`marketplace_dev` در Local/Dev، `marketplace_test` در CI)، نام کاربر DB (`marketplace`).
- نام قبلی `BabyMarket` که توسط تیم Security در Sprint 1 استفاده شده بود **منسوخ** است و در تمام فایل‌های این تیم به `Marketplace` تغییر یافت (این Task).
- توضیح محصول (نه نام فنی) در مستندات/README: «Marketplace محصولات نوزاد، کودک و مادر».

## 2. ساختار ریشه Repo

```
/
├── apps/
│   ├── api/                     ← Backend (ASP.NET Core, Modular Monolith, Clean Architecture)
│   │   ├── Dockerfile
│   │   ├── src/
│   │   │   ├── Core/
│   │   │   │   ├── Marketplace.Domain/
│   │   │   │   └── Marketplace.Application/
│   │   │   ├── Infrastructure/
│   │   │   │   └── Marketplace.Infrastructure/
│   │   │   └── Presentation/
│   │   │       └── Marketplace.API/         ← Entry point، تولید Marketplace.API.dll
│   │   └── tests/
│   │       ├── UnitTests/
│   │       └── IntegrationTests/
│   │
│   └── web/                     ← Frontend (Next.js)
│       ├── Dockerfile
│       ├── src/
│       └── tests/
│
├── e2e/                          ← Playwright E2E (مستقل از apps/*، به هر دو سرویس متصل می‌شود)
│
├── docker-compose.yml            ← تنها نسخه رسمی (مبنا: نسخه سخت‌شده Security)
├── .env.example                  ← تنها نسخه رسمی
├── .gitignore
├── .gitleaks.toml
│
├── .github/
│   └── workflows/ci.yml
│
└── docs/
    ├── repo-contract.md          ← همین سند
    ├── architecture/             ← اسناد معماری + ADR (Architecture Decision Record)
    ├── environments.md
    ├── monitoring-foundation.md
    └── security/
        ├── threat-model.md
        ├── secret-management.md
        ├── security-headers.md
        └── docker-security-checklist.md
```

### قواعد این ساختار
- **`apps/api`** جایگزین `backend/Marketplace/*` قبلی است — جابجایی پوشه، بدون تغییر منطق کد.
- **`apps/web`** جایگزین `frontend/frontend/*` قبلی است — جابجایی پوشه، بدون تغییر منطق کد.
- درون `apps/api/src`، Layering مطابق Clean Architecture سه پوشه اصلی دارد: `Core` (Domain + Application)، `Infrastructure`، `Presentation` (پروژه API/Entry point). این Layering توسط این سند برای هماهنگی با ساختار تست‌های Security فرض شده — **در صورت اختلاف با Solution واقعی Backend (`Marketplace.sln`)، این بخش باید در اولین Sync بعدی اصلاح و این سند Update شود** (به بخش «Open Assumption» در انتهای سند مراجعه کنید).
- هیچ کدی مستقیم زیر ریشه Repo (`/backend`, `/frontend`) باقی نمی‌ماند.

## 3. پورت‌ها

| سرویس | پورت داخلی (درون Container) | Mapping بیرونی (Local Dev، `docker-compose.yml`) |
|---|---|---|
| API (`apps/api`) | **`8080`** (ثابت، در `ASPNETCORE_URLS` و `EXPOSE`) | `8080:8080` |
| Web (`apps/web`) | `3000` | `3000:3000` |
| PostgreSQL | `5432` | بدون Mapping پیش‌فرض (فقط شبکه `internal`) |
| Redis | `6379` | بدون Mapping پیش‌فرض |
| RabbitMQ | `5672` (AMQP) / `15672` (Management UI) | بدون Mapping پیش‌فرض؛ در صورت نیاز به Debug لوکال از فایل `docker-compose.override.yml` جداگانه (که Commit نمی‌شود) استفاده شود |

**قاعده:** پورت داخلی API هرگز در هیچ فایلی (Dockerfile، `appsettings.json`، Health Check، Frontend `.env`) غیر از `8080` نیست. Mapping متفاوت (اگر لازم شد) فقط سمت چپ `docker-compose.yml` تغییر می‌کند، نه سمت راست.

## 4. نام دیتابیس/کاربر (یکپارچه)

| Environment | DB Name | DB User |
|---|---|---|
| local / development | `marketplace_dev` | `marketplace` |
| CI (`ci.yml` integration-test) | `marketplace_test` | `test` (مخصوص CI، موقت) |
| staging / production | نام‌گذاری واقعی توسط Secret Manager آن محیط تعیین می‌شود؛ الگو: `marketplace_staging` / `marketplace_prod` |

## 5. Docker Compose

`docker-compose.yml` ریشه Repo تنها نسخه رسمی است. مبنا: نسخه سخت‌شده تیم Security، شامل:
- Network Segmentation (`edge` برای `api`/`web`، `internal` برای `postgres`/`redis`/`rabbitmq` بدون دسترسی مستقیم اینترنت)
- `read_only: true` + `tmpfs: [/tmp]` روی `api`/`web`
- `security_opt: [no-new-privileges:true]` روی همه سرویس‌ها
- `cap_drop: [ALL]` روی `api`/`web`
- `mem_limit`/`cpus` روی همه سرویس‌ها
- نام سرویس‌ها: `api` و `web` (نه `backend`/`frontend`، برای هم‌خوانی با `apps/api`/`apps/web`)

## 6. CI Pipeline

`.github/workflows/ci.yml` (نسخه تیم Security) با مسیرهای اصلاح‌شده به `apps/api` و `apps/web` مبنای رسمی است. مراحل: `Lint → Build → Unit Test → Integration Test → Dependency Scan → Secret Scan → Container Scan`، با Gate نهایی `ci-success`.

## 7. .env.example

یک فایل واحد در ریشه Repo (نسخه Security، که کامل‌تر بود، حفظ و نام‌های دیتابیس/کاربر مطابق بخش 4 این سند اصلاح شد).

## 8. Dockerfile

Dockerfile سخت‌شده تیم Security (Multi-stage، Alpine، Non-root، HealthCheck) جایگزین رسمی برای `apps/api` و `apps/web` است. تنها تغییرات لازم نسبت به نسخه Sprint 1:
- Build Context: `apps/api` (به‌جای `backend`) و `apps/web` (به‌جای `frontend`)
- WORKDIR مرحله Build: `/src/src/Presentation/Marketplace.API` (مطابق Layering بخش 2)
- `ENTRYPOINT ["dotnet", "Marketplace.API.dll"]`

---

## Assumption — Closed (Task 1.7)

فرض قبلی این بخش (Layering دقیق `apps/api/src` و نام Entry point) با دسترسی مستقیم به کد واقعی `Marketplace.sln` در Task 1.7 راستی‌آزمایی شد و **درست از آب درآمد**: Layering سه‌پوشه‌ای (`Core/Marketplace.Domain`, `Core/Marketplace.Application`, `Infrastructure/Marketplace.Infrastructure`, `Presentation/Marketplace.API`) و نام پروژه `Marketplace.API` دقیقاً با فرض این سند یکی است. تمام مسیرهای وابسته (ProjectReference در تست‌ها، `WORKDIR` در Dockerfile) بدون نیاز به تغییر تأیید شدند.

**یک باگ واقعی هم در همین راستی‌آزمایی پیدا و رفع شد** (فراتر از فرض اولیه): `docker-compose.yml` انتظار `apps/api/Dockerfile` را داشت اما Dockerfile واقعی Backend در مسیر تودرتوی `apps/api/src/Presentation/Marketplace.API/Dockerfile` بود؛ Dockerfile سخت‌شده به مسیر درست منتقل و نسخه تکراری حذف شد.

**یک باگ واقعی دوم هم پیدا و رفع شد**: سرویس `api` در `docker-compose.yml` فقط `env_file: .env` داشت، بدون نگاشت به کلیدهای تودرتوی ASP.NET Core (که با `__` جدا می‌شوند). یعنی `POSTGRES_*`, `REDIS_*`, `RABBITMQ_*`, `JWT_*`, `PASSWORD_PEPPER` در `.env` هرگز واقعاً به `ConnectionStrings:Postgres`, `Jwt:PrivateKeyPath`, `PasswordHashing:Pepper` و... بایند نمی‌شدند و API در Startup واقعی Fail می‌کرد. یک بلوک `environment:` صریح در سرویس `api` اضافه شد که این نگاشت را انجام می‌دهد؛ کلید JWT هم از طریق یک Volume Mount (`./keys:/run/secrets:ro`) به Container می‌رسد نه از مسیر Host.

این بخش دیگر "Open" نیست؛ هر تغییر آینده در نام پروژه/Layering باید مستقیماً همین‌جا و در فایل‌های ذکرشده بالا به‌روزرسانی شود.

---

## Changelog

| نسخه | تاریخ | تغییر |
|---|---|---|
| v1.0 | Sprint 1 — Correction Round | ایجاد اولیه؛ یکپارچه‌سازی نام `Marketplace`، ساختار `apps/api`/`apps/web`، پورت `8080`، Docker Compose/CI/Dockerfile واحد |
| v1.1 | Sprint 1 — Task 1.7 | ادغام فیزیکی واقعی انجام شد؛ Open Assumption تأیید و بسته شد؛ دو باگ واقعی (مسیر Dockerfile، نگاشت متغیرهای محیطی `api`) پیدا و رفع شد؛ Fallback پورت اشتباه Frontend (`4000`) و تست `LoginForm` (email→phone) اصلاح شد. هنوز روی هیچ ماشینی با Docker/dotnet واقعی اجرا نشده بود. |
| v1.2 | Sprint 1 — Task 1.7 (بعد از اجرای واقعی روی سرور) | باگ واقعی سوم پیدا شد: `COPY src/**/*.csproj ./` در `apps/api/Dockerfile` مسیر تودرتوی پوشه‌ها را در Build واقعی از بین می‌برد (`dotnet restore` تمام ۶ پروژه را "not found" گزارش می‌کرد — تأیید‌شده با لاگ واقعی build روی سرور کاربر). با COPY صریح هر csproj در مسیر دقیق خودش + Restore فقط روی پروژه API (نه کل .sln، چون Image محصول نیازی به پروژه‌های تست ندارد) رفع شد. |
| v1.3 | Sprint 1 — Task 1.7 (بعد از اجرای واقعی روی سرور) | باگ واقعی چهارم پیدا شد: کامپایل واقعی C# شکست خورد — `UserConfiguration.cs` و `RoleConfiguration.cs` از `Microsoft.EntityFrameworkCore.ChangeTracking.PropertyAccessMode` استفاده می‌کردند (Namespace اشتباه، خطای CS0234)؛ enum واقعی در `Microsoft.EntityFrameworkCore` است، نه `.ChangeTracking`. با حذف پیشوند اضافی رفع شد (`using Microsoft.EntityFrameworkCore;` از قبل در هر دو فایل موجود بود). |
| v1.4 | Sprint 1 — Task 1.7 (بعد از اجرای واقعی روی سرور) | باگ واقعی پنجم: `HealthCheckExtensions.cs` از `AddDbContextCheck<ApplicationDbContext>()` استفاده می‌کرد، ولی پکیج NuGet ارائه‌دهنده آن (`Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`) در `Marketplace.API.csproj` Reference نشده بود (خطای CS1061). سه پکیج `AspNetCore.HealthChecks.*` موجود فقط Ping ساده Postgres/Redis/RabbitMQ را پوشش می‌دهند، نه بررسی واقعی اتصال EF Core به DbContext. پکیج گمشده با نسخه هماهنگ با EF Core 8.0.10 اضافه شد. |
| v1.5 | Sprint 1 — Task 1.7 (بعد از اجرای واقعی روی سرور) | دو باگ واقعی هم‌زمان با اولین اجرای کامل `docker compose up` پیدا شدند: (۱) `BuildRabbitMqUri()` در Health Check اصلاً VirtualHost را در URI نمی‌گذاشت، پس همیشه به vhost پیش‌فرض `/` وصل می‌شد نه `/marketplace` واقعی (خطای RabbitMQ `NOT_ALLOWED - vhost / not found`؛ نکته: Publisher واقعی پیام‌ها این باگ را نداشت، فقط مسیر Health Check). (۲) `ApplicationDbContext` هرگز `DomainEvent` را Ignore نکرده بود، پس EF Core تلاش می‌کرد `DomainEvents` (کالکشن روی `BaseEntity`) را به‌عنوان یک Entity واقعی Map کند و چون بدون Primary Key بود، هر Query‌ای (از جمله Health Check و Outbox Background Service) با Exception شکست می‌خورد. هر دو با یک تغییر کوچک (افزودن vhost به URI با Encode درست، و `modelBuilder.Ignore<DomainEvent>()`) رفع شدند. |
