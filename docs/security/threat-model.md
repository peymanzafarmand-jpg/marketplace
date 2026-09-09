# Threat Model — Marketplace (نوزاد، کودک و مادر)

معماری هدف: ASP.NET Core Backend (Modular Monolith, Clean Architecture) + Next.js Frontend + PostgreSQL + Redis + RabbitMQ + Docker.

روش‌شناسی: STRIDE به‌ازای هر Trust Boundary + بررسی مجزا برای هر تهدید درخواستی.

## Trust Boundaries

```
[Browser] --HTTPS--> [Next.js Frontend / Edge]
                              |
                       --HTTPS/JWT-->
                              v
                    [ASP.NET Core API - Modular Monolith]
                    |         |          |
                    v         v          v
               [PostgreSQL] [Redis]  [RabbitMQ] --> [Worker Consumers]
                    |
                    v
            [Object Storage - Product Images]
                    |
                    v
          [Payment Gateway - External Service]
```

هر پیکان بالا یک Trust Boundary است؛ هیچ داده عبوری از یک مرز نباید بدون Validation/Authentication مستقل مورد اعتماد قرار گیرد.

---

## 1. SQL Injection

- **جایگاه در معماری**: لایه `Infrastructure` (EF Core / Dapper Repository ها) در Clean Architecture.
- **حمله**: تزریق از طریق فیلتر جستجوی محصول، Query Parameter صفحه‌بندی، فیلدهای فرم ثبت‌نام/پروفایل.
- **کنترل**:
  - استفاده اجباری از **EF Core** با LINQ (Parameterized by default) یا در صورت نیاز به Raw SQL، صرفاً با `FormattableString`/Parameter (`FromSqlInterpolated`)، هرگز `FromSqlRaw` با Concatenation.
  - Code Review Rule: هیچ String Concatenation در ساخت Query مجاز نیست (بررسی خودکار با SAST/Semgrep Rule سفارشی برای `FromSqlRaw`).
  - DB User برنامه (`marketplace`) بدون دسترسی `DROP`/`ALTER`/`GRANT`.

## 2. XSS

- **جایگاه در معماری**: Next.js — رندر نظرات محصول، توضیحات فروشنده، پیام‌های پشتیبانی.
- **حمله**: Stored XSS از طریق فیلد توضیحات محصول که توسط فروشنده پر می‌شود و برای همه بازدیدکنندگان رندر می‌شود.
- **کنترل**:
  - React/Next.js به‌صورت پیش‌فرض Output را Escape می‌کند؛ ممنوعیت استفاده از `dangerouslySetInnerHTML` مگر با Sanitizer (DOMPurify) روی محتوای از پیش پاک‌سازی‌شده سمت سرور.
  - CSP سخت‌گیرانه بدون `unsafe-inline` در Production (`docs/security/security-headers.md`).
  - Validation سمت Backend روی طول/محتوای فیلدهای متنی ورودی فروشنده (Application layer، نه فقط Frontend).

## 3. CSRF

- **جایگاه در معماری**: هر Endpoint حساس در ASP.NET Core API که وضعیت را تغییر می‌دهد (تغییر رمز، ثبت سفارش، تنظیمات فروشنده).
- **کنترل**:
  - چون Auth از طریق **Bearer JWT در Header** انجام می‌شود (نه Cookie خودکار مرورگر برای Access Token)، سطح ریسک CSRF کلاسیک عملاً پایین است.
  - Refresh Token در Cookie ذخیره می‌شود → برای Endpoint `refresh-token` از `SameSite=Strict` + بررسی `Origin` Header استفاده می‌شود.
  - برای هرگونه فرم آینده مبتنی بر Cookie Session، ASP.NET Core Antiforgery Middleware (`IAntiforgery`) فعال می‌شود.

## 4. IDOR

- **جایگاه در معماری**: `Application` layer — Use Case هایی مثل `GetOrderById`, `GetUserAddress`, `DownloadInvoice`.
- **حمله**: کاربر A با تغییر `orderId` در URL به سفارش کاربر B دسترسی پیدا کند.
- **کنترل**:
  - هر Query Handler (CQRS) باید `CurrentUserId`/`TenantId` را از Context امن (نه از Body/Query Param قابل دستکاری) بگیرد و در فیلتر Query اعمال کند، نه بعد از دریافت نتیجه.
  - استفاده از UUID v4 برای شناسه منابع حساس به‌جای ID عددی Sequential.
  - Integration Test اجباری برای هر Endpoint جدید Object-based (نمونه در `apps/api/tests/IntegrationTests/Orders/OrderAccessControlTests.cs`).
  - همین اصل برای Multi-tenancy فروشندگان: هر Query باید `SellerId` را در `WHERE` Clause اجباری داشته باشد (Repository Pattern مرکزی، نه پراکنده).

## 5. SSRF

- **جایگاه در معماری**: هر قابلیتی که از Backend یک URL خارجی Fetch می‌کند — مثلاً Import تصویر محصول از URL، Webhook Callback فروشنده، اعتبارسنجی لینک محصول مشابه.
- **کنترل**:
  - Allow-list دامنه مجاز (مثلاً فقط دامنه Payment Gateway مشخص برای Webhook).
  - مسدودسازی صریح IP Rangeهای داخلی/Metadata Endpoint (`127.0.0.0/8`, `169.254.169.254`, `10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16`) قبل از هر `HttpClient.GetAsync` روی URL کاربر-محور.
  - این منطق در یک `SafeHttpClient` مرکزی (Wrapper در لایه Infrastructure) پیاده می‌شود تا در همه Use Caseها یکسان اعمال شود، نه در هر جا جدا نوشته شود.

## 6. Brute Force

- **جایگاه در معماری**: Endpoint های `POST /auth/login`, `POST /auth/forgot-password`, `POST /auth/otp/verify`.
- **کنترل**:
  - Rate Limiting در سطح ASP.NET Core (`Microsoft.AspNetCore.RateLimiting`) به‌ازای IP + به‌ازای شناسه حساب (Sliding Window).
  - Redis برای نگه‌داری شمارنده تلاش ناموفق (Distributed، چون Backend ممکن است چند Instance داشته باشد).
  - Exponential Backoff + CAPTCHA بعد از N تلاش.

## 7. File Upload

- **جایگاه در معماری**: Endpoint آپلود تصویر محصول → Object Storage (از طریق Presigned URL) → Worker Consumer (RabbitMQ) برای پردازش Async.
- **کنترل** (تفصیل کامل در `docs/security/docker-security-checklist.md` مرتبط + سند اصلی معماری):
  - Presigned Upload مستقیم به Staging Bucket (فایل از Backend عبور نمی‌کند، فقط Metadata).
  - پیام RabbitMQ بعد از آپلود Trigger پردازش Worker می‌شود: بررسی Magic Number، Malware Scan (ClamAV)، Re-encode تصویر (حذف EXIF)، سپس انتقال به Bucket نهایی.
  - نام فایل نهایی همیشه UUID تولیدشده توسط سرور، نه نام اصلی کاربر.

## 8. Privilege Escalation

- **جایگاه در معماری**: تغییر `role`/`permissions` کاربر، مسیرهای Admin API.
- **کنترل**:
  - Endpoint تغییر نقش فقط با Permission اختصاصی `user:manage-role` که تنها به `super_admin` تعلق دارد.
  - JWT Claims (`role`, `permissions`) صرفاً Cache از وضعیت واقعی DB هستند؛ Middleware مرکزی Authorization در هر Request، Permission را از Source of Truth (DB/Redis Cache با Invalidation فوری) بررسی می‌کند، نه صرفاً به Claim امضاشده در توکن قدیمی اکتفا می‌کند برای عملیات حساس.
  - هر تغییر نقش/دسترسی در Audit Log با `before_state`/`after_state` ثبت می‌شود.

## 9. Payment Security

- **جایگاه در معماری**: ماژول جدا `Payments` در Modular Monolith (Bounded Context مستقل با دسترسی محدودتر به بقیه ماژول‌ها) + Payment Gateway خارجی.
- **کنترل**:
  - Backend هرگز PAN/CVV دریافت نمی‌کند (Redirect/iframe به Gateway — SAQ-A).
  - Webhook از RabbitMQ Consumer مجزا پردازش می‌شود؛ Signature (HMAC) و Idempotency Key قبل از هرگونه تغییر وضعیت سفارش بررسی می‌شوند.
  - جدول تراکنش Append-only (Event Sourcing سبک برای وضعیت پرداخت: `Created → Pending → Succeeded/Failed`، هر تغییر یک رکورد جدید، نه Update رکورد قبلی).

---

## خلاصه نگاشت تهدید ↔ لایه معماری

| تهدید | لایه اصلی مسئول دفاع |
|---|---|
| SQL Injection | Infrastructure (EF Core Repository) |
| XSS | Next.js Presentation + Application Validation |
| CSRF | API Auth Design (Bearer Token) + Antiforgery برای مسیرهای Cookie-based |
| IDOR | Application (Use Case Authorization) |
| SSRF | Infrastructure (SafeHttpClient Wrapper) |
| Brute Force | API Gateway/Middleware + Redis Rate Limit |
| File Upload | Presentation (Presigned URL) + Worker (Async Scan) |
| Privilege Escalation | Application (Authorization Middleware) + Audit |
| Payment Security | Payments Bounded Context + External Gateway |

## به‌روزرسانی این سند
این Threat Model باید هر بار که یک Bounded Context جدید (ماژول) به Modular Monolith اضافه می‌شود، یا یک External Integration جدید (Gateway/API خارجی) اضافه می‌شود، بازبینی و به‌روزرسانی شود.
