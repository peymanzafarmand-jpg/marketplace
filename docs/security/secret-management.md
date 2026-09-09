# Secret Management Strategy

## هدف
هیچ Secret واقعی (Password، JWT Signing Key، API Key، Connection String تولید) هرگز نباید داخل Git Commit، Docker Image، یا Log قرار بگیرد.

## دسته‌بندی Secretها
- Database credentials (`POSTGRES_PASSWORD`, `ConnectionStrings__Default`)
- JWT signing keys (RS256 private key)
- `PASSWORD_PEPPER`
- Redis / RabbitMQ credentials
- Payment gateway API key + webhook signing secret
- Object storage access/secret key
- Sentry DSN (حساسیت پایین‌تر، اما همچنان کلید مخصوص پروژه است)

## استراتژی به تفکیک Environment

| Environment | محل نگه‌داری Secret | تزریق به Runtime |
|---|---|---|
| **local** | فایل `.env` لوکال (در `.gitignore`)، از `.env.example` کپی می‌شود | Docker Compose `env_file` |
| **development** | GitHub Actions Environment Secrets یا Vault Dev namespace | CI Job env / Vault Agent Sidecar |
| **staging** | Vault / Cloud Secret Manager (namespace `staging`) | Injected at container start (Vault Agent / CSI driver) |
| **production** | Vault / Cloud Secret Manager (namespace `production`)، دسترسی محدود به CI/CD Service Account + Super Admin | Injected at container start؛ هیچ Secret در Image یا ENV تعریف‌شده در `docker-compose.yml` Commit‌شده |

> پیشنهاد ابزار: **HashiCorp Vault** (self-hosted) یا معادل Cloud (AWS Secrets Manager / Azure Key Vault) بسته به Provider نهایی. تا زمان راه‌اندازی Vault، حداقل GitHub Actions **Encrypted Secrets** + `.env` غیرقابل Commit استفاده می‌شود.

## قواعد اجباری
1. `.env` هرگز Commit نمی‌شود — فقط `.env.example` با مقادیر Placeholder.
2. کلید امضای JWT (Private Key) هرگز در Repo قرار نمی‌گیرد؛ مسیر فایل از طریق ENV مشخص می‌شود و فایل واقعی از Secret Manager Mount می‌شود.
3. Rotation دوره‌ای: JWT Signing Key هر 90 روز، DB Password هر 180 روز یا در صورت افشای احتمالی فوراً.
4. Gitleaks در هر Commit/PR اجرا می‌شود (بخش CI) تا نشت تصادفی Secret قبل از Merge شناسایی شود.
5. Secretهای Production فقط برای CI/CD Service Account و نقش `super_admin` DevOps قابل مشاهده‌اند (Least Privilege + Audit Log دسترسی در Vault).
6. هیچ Secret در `docker history` یا لایه‌های Image باقی نمی‌ماند (بررسی در Docker Security Checklist).
7. در صورت افشای Secret: Rotation فوری + بررسی Audit Log برای سوءاستفاده + مستندسازی Incident.
