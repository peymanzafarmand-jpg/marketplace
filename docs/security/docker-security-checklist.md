# Docker Security Checklist

این چک‌لیست باید روی هر Dockerfile جدید/تغییریافته در Repo (Backend و Frontend) اعمال و در Code Review بررسی شود.

## Build

- [ ] **Multi-stage Build** استفاده شده (مرحله Build جدا از مرحله Runtime؛ SDK/Toolchain در Image نهایی وجود ندارد)
- [ ] **Base Image Minimal**: `alpine` یا معادل (`aspnet:8.0-alpine`, `node:20-alpine`) به‌جای Image کامل
- [ ] Base Image با **Tag مشخص** (نه `latest`) Pin شده؛ ترجیحاً با Digest (`@sha256:...`) برای Reproducibility
- [ ] لایه‌های Dockerfile به ترتیب کم‌تغییرترین → پرتغییرترین چیده شده‌اند (Cache Efficiency)

## Runtime User

- [ ] یک کاربر Non-root ساخته و با `USER` انتخاب شده (هرگز Container به‌عنوان `root` اجرا نمی‌شود)
- [ ] مالکیت فایل‌های کپی‌شده به همان کاربر (`--chown=appuser:appgroup`) تنظیم شده
- [ ] در صورت نیاز به Bind به پورت < 1024، از Port ≥ 1024 (مثل 8080) استفاده و در جلوی Reverse Proxy Map می‌شود (نه اجرای Container به‌عنوان root فقط برای Bind پورت 80/443)

## Secrets

- [ ] هیچ `ARG`/`ENV` حاوی Secret واقعی در Dockerfile نیست (فقط مقادیر Public build-time مثل `NEXT_PUBLIC_API_BASE_URL`)
- [ ] هیچ فایل `.env`/کلید خصوصی با `COPY` وارد Image نمی‌شود
- [ ] بررسی `docker history <image>` بعد از Build برای اطمینان از عدم درز Secret در لایه‌های میانی
- [ ] Secretهای Runtime (DB Password، JWT Key) فقط از طریق ENV Var تزریق‌شده در زمان اجرا (Docker Secrets/Vault Agent) در دسترس‌اند، نه Bake شده در Image

## Image Size / Attack Surface

- [ ] ابزارهای غیرضروری (`curl`, `git`, compiler) در Image نهایی نیستند مگر برای Health Check لازم باشند (مثال: `curl` فقط برای healthcheck نصب شده)
- [ ] `.dockerignore` تعریف شده و `node_modules`, `.git`, `bin/obj`, `.env*` را از Context Build حذف می‌کند
- [ ] بسته‌های نصب‌شده حداقلی هستند (`--no-cache` در Alpine برای جلوگیری از باقی‌ماندن Package Index)

## Filesystem / Runtime Hardening

- [ ] در `docker-compose`/Deployment، **Read-only Root Filesystem** فعال است در صورت امکان:
  ```yaml
  read_only: true
  tmpfs:
    - /tmp
  ```
- [ ] `no-new-privileges` فعال:
  ```yaml
  security_opt:
    - no-new-privileges:true
  ```
- [ ] محدودیت منابع (`mem_limit`, `cpus`) تعریف شده برای جلوگیری از Resource Exhaustion
- [ ] Capability های غیرضروری حذف شده‌اند (`cap_drop: [ALL]` و فقط موارد لازم `cap_add`)

## Networking

- [ ] Container های Backend/DB/Queue فقط روی شبکه `internal` (بدون دسترسی مستقیم اینترنت) قرار دارند
- [ ] فقط `api-gateway`/`frontend` روی شبکه `edge` (متصل به اینترنت) هستند
- [ ] پورت‌های غیرضروری `EXPOSE` نشده‌اند

## Health & Observability

- [ ] `HEALTHCHECK` تعریف شده (برای Orchestrator جهت تشخیص Container ناسالم)
- [ ] Logging Container به `stdout/stderr` می‌رود (نه فایل داخل Container) تا توسط Log Aggregator جمع‌آوری شود

## CI Enforcement

- [ ] هر Image در CI با **Trivy** اسکن می‌شود؛ وجود آسیب‌پذیری **Critical/High** باعث Fail شدن Pipeline می‌شود (بخش CI)
- [ ] Hadolint (Dockerfile Linter) برای رعایت Best Practiceهای نگارش Dockerfile اجرا می‌شود (پیشنهادی، اختیاری در فاز اول)
