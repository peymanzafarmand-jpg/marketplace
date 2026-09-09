# Marketplace — Repo ادغام‌شده (Task 1.7)

## 🚀 نصب سریع (یک دستور)

```bash
unzip marketplace-merged-repo.zip -d marketplace && cd marketplace
chmod +x quick-start.sh
./quick-start.sh
```

این اسکریپت مراحل ۱ تا ۵ زیر را خودکار انجام می‌دهد (ساخت `.env` با Secret تصادفی، ساخت کلید JWT، بالا آوردن زیرساخت، Migration، بالا آوردن کل Stack، تست سلامت). خروجی ترمینال را عیناً نگه دارید — همان چیزی است که در گزارش Task 1.7 لازم است. اگر خواستید مرحله‌به‌مرحله و دستی هم پیش بروید، راهنمای کامل زیر همچنان موجود است.

این Repo حاصل ادغام فیزیکی سه Deliverable (Backend، Frontend، Security/DevOps) است. جزئیات کامل تصمیمات در `docs/repo-contract.md` آمده.

## ⚠️ وضعیت راستی‌آزمایی — مهم، قبل از هر کاری بخوانید

این Repo **در یک Container بدون Docker، بدون dotnet SDK، و بدون دسترسی به اینترنت** ساخته شده. یعنی:

- ✅ ساختار پوشه‌ها، مسیرهای Reference (`csproj`, `Marketplace.sln`, `Dockerfile`)، Syntax فایل‌های YAML/JSON، و منطق نگاشت متغیرهای محیطی — همه به‌صورت **دستی و خط‌به‌خط** بررسی و تأیید شدند.
- ❌ **هیچ `dotnet build`، `npm install`، یا `docker compose up` واقعی اجرا نشده.** یعنی احتمال وجود حداقل یک خطای کوچک (Typo، نسخه ناسازگار پکیج، و مشابه) که فقط با اجرای واقعی مشخص می‌شود، هنوز صفر نیست.

مراحل زیر را **دقیقاً به همین ترتیب** روی سیستمی با Docker Desktop (یا Docker Engine + Compose plugin) نصب‌شده اجرا کنید و نتیجه هر مرحله را یادداشت کنید — این همان چیزی است که باید در گزارش نهایی Task 1.7 (`STATUS: DONE`) بیاید.

---

## مرحله ۱ — استخراج و آماده‌سازی متغیرهای محیطی

```bash
unzip marketplace-merged-repo.zip -d marketplace
cd marketplace
cp .env.example .env
```

فایل `.env` را باز کنید و مقادیر `CHANGE_ME_LOCAL_ONLY` را با مقادیر دلخواه محلی (فقط برای توسعه، نه واقعی) جایگزین کنید — حداقل این‌ها:
`POSTGRES_PASSWORD`, `REDIS_PASSWORD`, `RABBITMQ_PASSWORD`, `PASSWORD_PEPPER` (حداقل ۳۲ کاراکتر تصادفی).

## مرحله ۲ — ساخت جفت‌کلید RS256 برای JWT

بک‌اند برای امضای توکن به یک جفت‌کلید RSA نیاز دارد (طبق ADR-014). این فایل‌ها Commit نمی‌شوند و باید محلی ساخته شوند:

```bash
mkdir -p keys
openssl genrsa -out keys/jwt-private.pem 2048
openssl rsa -in keys/jwt-private.pem -pubout -out keys/jwt-public.pem
```

اگر `openssl` نصب نیست: `docker run --rm -v "$PWD/keys:/keys" alpine/openssl genrsa -out /keys/jwt-private.pem 2048` و دستور دوم را مشابه با Image همین تطبیق دهید.

## مرحله ۳ — بالا آوردن زیرساخت (بدون API، برای ساخت Migration)

قبل از اولین اجرای کامل، باید یک Migration اولیه دیتابیس ساخته شود (طبق `apps/api/README.md`، این Migration در Sprint قبل عمداً Commit نشده بود):

```bash
docker compose up -d postgres redis rabbitmq
# صبر کنید تا هر سه healthy شوند:
docker compose ps
```

اگر روی ماشین شما .NET SDK 8 و ابزار `dotnet-ef` نصب است:

```bash
cd apps/api
dotnet tool install --global dotnet-ef   # اگر قبلاً نصب نیست
dotnet ef migrations add InitialCreate \
  --project src/Infrastructure/Marketplace.Infrastructure \
  --startup-project src/Presentation/Marketplace.API \
  --output-dir Persistence/Migrations
cd ../..
```

اگر SDK محلی ندارید، این مرحله را داخل یک Container موقت انجام دهید (نمونه دستور در `apps/api/README.md`، بخش Migrations).

**نتیجه این مرحله را یادداشت کنید:** آیا Migration بدون خطا ساخته شد؟ چند فایل تولید شد؟

## مرحله ۴ — بالا آوردن کل Stack

```bash
docker compose up --build
```

نتایج مورد انتظار:
- هر ۵ سرویس (`postgres`, `redis`, `rabbitmq`, `api`, `web`) باید در نهایت `healthy` شوند (`docker compose ps`).
- اگر `api` بالا نیامد یا Unhealthy ماند: `docker compose logs api` را چک کنید — رایج‌ترین علت احتمالی، Migration اعمال‌نشده (مرحله ۳) یا مقدار اشتباه در `.env` است.

**نتیجه این مرحله را دقیقاً یادداشت کنید** (خروجی `docker compose ps` را Copy کنید) — این همان «لاگ واقعی» است که Task 1.7 برای `STATUS: DONE` می‌خواهد.

## مرحله ۵ — تست دستی سلامت هر سرویس

```bash
curl -i http://localhost:8080/health
curl -i http://localhost:3000/api/health
```

هر دو باید `200 OK` برگردانند.

## مرحله ۶ — اجرای تست‌های خودکار

```bash
# Backend
cd apps/api
dotnet test
cd ../..

# Frontend
cd apps/web
npm ci
npm test
cd ../..

# E2E (نیاز به Stack بالا در مرحله ۴)
cd e2e
npm ci
npx playwright install --with-deps chromium
npm test
cd ..
```

## مرحله ۷ — اجرای CI به‌صورت واقعی

ساده‌ترین راه: این Repo را در یک مخزن Git واقعی (مثلاً GitHub) Push کنید — `.github/workflows/ci.yml` از قبل آماده اجراست و به‌محض Push، Pipeline (`Lint → Build → Unit Test → Integration Test → Dependency Scan → Secret Scan → Container Scan`) به‌طور خودکار اجرا می‌شود. نتیجه واقعی (سبز/قرمز هر Job) همان چیزی است که در گزارش نهایی Task 1.7 باید بیاید.

---

## اگر جایی خطا گرفتید

خطای واقعی (پیام کامل + کدام مرحله) را دقیقاً همان‌طور که هست کپی کنید و به‌عنوان `KNOWN ISSUES` در گزارش Task 1.7 برگردانید — دقیقاً طبق قانون پروژه: **هیچ لاگ ساختگی یا نتیجه فرضی گزارش نشود.**
