# Environment Strategy

چهار محیط: `local`، `development`، `staging`، `production`. هر محیط باید ایزوله باشد (DB/Redis/Queue/Secret جدا) تا خطا یا تست در یک محیط روی محیط دیگر اثر نگذارد.

## جدول کلی

| | **local** | **development** | **staging** | **production** |
|---|---|---|---|---|
| **هدف** | توسعه روی سیستم شخصی | محیط مشترک تیم برای QA زودهنگام | آینه Production برای تست نهایی/UAT | سرویس واقعی کاربران |
| **Environment Variables** | `.env` لوکال (کپی از `.env.example`) | GitHub Actions Environment `development` | GitHub Actions Environment `staging` + Secret Manager | GitHub Actions Environment `production` + Secret Manager (دسترسی محدودتر) |
| **Secret Strategy** | مقادیر Dummy/Local، هرگز Secret واقعی | Secret واقعی اما کم‌ریسک (Sandbox Payment Gateway) در Vault namespace `dev` | Secret شبه‌واقعی (Payment Gateway Sandbox با داده نزدیک Production) در Vault namespace `staging` | Secret واقعی، دسترسی فقط CI/CD Service Account + Super Admin، Rotation منظم |
| **Database Strategy** | Postgres در Docker Compose لوکال، Seed Data ساختگی | Postgres مشترک Dev، Migration خودکار در هر Deploy، امکان Reset دوره‌ای | Postgres مجزا، Snapshot دوره‌ای از داده Anonymized Production (اختیاری) برای تست واقعی‌تر | Postgres با Replica + PITR Backup، هرگز مستقیم توسط Developer Query نمی‌شود |
| **Logging Level** | `Debug`/`Trace` (جزئیات کامل برای دیباگ) | `Debug` | `Information` (با امکان موقت افزایش به Debug برای عیب‌یابی) | `Warning` برای اپلیکیشن عمومی، `Information` برای رویدادهای امنیتی/Audit (هرگز `Debug` در Production — ریسک درز داده حساس در Log) |
| **دسترسی عمومی** | فقط Localhost | پشت VPN/Auth ساده تیمی | پشت Auth (Basic Auth یا SSO) + IP Allow-list در صورت امکان | عمومی، پشت WAF/CDN |
| **Monitoring** | اختیاری (Console Log کافی) | Sentry فعال (سطح Warning+)، Metrics پایه | Sentry + Prometheus کامل، مشابه Production | Sentry + Prometheus + Alerting کامل + On-call |
| **Deploy Trigger** | دستی (`docker compose up`) | Push خودکار به branch `develop` | Push/Merge خودکار به `main` بعد از عبور CI | Manual Approval Gate بعد از تایید Staging (بخش CI/CD) |
| **Data Sensitivity** | داده ساختگی/Faker | داده تستی، بدون PII واقعی | داده Anonymized یا تستی — **هرگز PII واقعی کاربران کپی نشود** | داده واقعی کاربران، تحت کامل کنترل‌های امنیتی این سند |

## قواعد تکمیلی

1. **عدم اشتراک Secret بین Environmentها**: JWT Signing Key، DB Password، Payment API Key در هر محیط منحصربه‌فرد است. افشای Secret یک محیط نباید محیط دیگر را تحت تاثیر قرار دهد.
2. **Feature Flags**: قابلیت‌های نیمه‌کاره از طریق Feature Flag (نه Branch جدا) در `development`/`staging` فعال و در `production` به‌صورت کنترل‌شده (Gradual Rollout) باز می‌شوند.
3. **Data Seeding**: اسکریپت Seed مشترک برای `local`/`development` (داده Faker)؛ `staging` در صورت نیاز به داده واقعی‌تر باید از فرآیند **Anonymization** عبور کند (حذف/Hash ایمیل، تلفن، آدرس واقعی).
4. **Config Source of Truth**: `appsettings.{Environment}.json` فقط برای مقادیر غیرحساس (Feature Flag، Timeout، Log Level)؛ هر مقدار حساس از ENV/Secret Manager می‌آید، نه از فایل Commit‌شده.
5. **Promotion Path**: کد فقط در مسیر `local → development → staging → production` حرکت می‌کند؛ Hotfix اضطراری Production باید بلافاصله Backport به `develop` شود تا Drift ایجاد نشود.
