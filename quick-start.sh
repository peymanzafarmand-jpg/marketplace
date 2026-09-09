#!/usr/bin/env bash
# quick-start.sh — راه‌اندازی خودکار کل Stack با یک دستور.
#
# پیش‌نیاز روی سیستم شما (این‌ها را خودِ اسکریپت نصب نمی‌کند):
#   - Docker Desktop / Docker Engine + Compose plugin (docker compose ...)
#   - openssl (روی مک/لینوکس معمولاً از قبل هست)
#   - dotnet SDK 8 (فقط برای ساخت Migration؛ اگر ندارید، این اسکریپت به‌جای شکست
#     خوردن، آن مرحله را رد می‌کند و به شما اطلاع می‌دهد)
#
# استفاده:
#   chmod +x quick-start.sh
#   ./quick-start.sh
#
# اجرای این اسکریپت جایگزین «راستی‌آزمایی واقعی» نیست — فقط مراحل تکراری و
# مستعد فراموشی (تولید Secret، ساخت کلید JWT، ترتیب بالا‌آمدن سرویس‌ها) را
# خودکار می‌کند. خروجی نهایی (docker compose ps, curl health) را همچنان خودتان
# باید ببینید و در گزارش Task 1.7 بیاورید — این اسکریپت چیزی را «قبول‌شده»
# اعلام نمی‌کند، فقط اجرا می‌کند و لاگ واقعی نشان می‌دهد.

set -euo pipefail
cd "$(dirname "$0")"

bold() { printf '\033[1m%s\033[0m\n' "$1"; }
ok()   { printf '  \033[32m✓\033[0m %s\n' "$1"; }
warn() { printf '  \033[33m!\033[0m %s\n' "$1"; }
err()  { printf '  \033[31m✗\033[0m %s\n' "$1"; }

# ---------- 0. پیش‌نیازها ----------
bold "0/6 — بررسی پیش‌نیازها"
command -v docker >/dev/null 2>&1 || { err "docker پیدا نشد. Docker Desktop را نصب کنید."; exit 1; }
docker compose version >/dev/null 2>&1 || { err "docker compose (plugin) پیدا نشد."; exit 1; }
ok "docker + compose موجود است"

HAS_OPENSSL=1
command -v openssl >/dev/null 2>&1 || HAS_OPENSSL=0
HAS_DOTNET=1
command -v dotnet >/dev/null 2>&1 || HAS_DOTNET=0

# ---------- 1. .env ----------
bold "1/6 — ساخت .env با مقادیر تصادفی امن"
if [ -f .env ]; then
  warn ".env از قبل وجود دارد — دست‌نخورده باقی می‌ماند. برای شروع تازه، آن را حذف کنید و دوباره اجرا کنید."
else
  cp .env.example .env
  rand() { python3 -c "import secrets; print(secrets.token_urlsafe(24))" 2>/dev/null || openssl rand -base64 24 | tr -d '=+/\n'; }
  for var in POSTGRES_PASSWORD REDIS_PASSWORD RABBITMQ_PASSWORD PASSWORD_PEPPER; do
    value=$(rand)
    # جایگزینی پرتابل برای Mac (BSD sed) و Linux (GNU sed)
    sed -i.bak "s|^${var}=.*|${var}=${value}|" .env && rm -f .env.bak
  done
  ok ".env ساخته شد و Secretهای تصادفی جایگزین CHANGE_ME_LOCAL_ONLY شدند"
fi

# ---------- 2. کلید RS256 ----------
bold "2/6 — ساخت جفت‌کلید JWT (RS256)"
mkdir -p keys
if [ -f keys/jwt-private.pem ] && [ -f keys/jwt-public.pem ]; then
  warn "keys/jwt-private.pem از قبل وجود دارد — رد شد."
elif [ "$HAS_OPENSSL" = "1" ]; then
  openssl genrsa -out keys/jwt-private.pem 2048 2>/dev/null
  openssl rsa -in keys/jwt-private.pem -pubout -out keys/jwt-public.pem 2>/dev/null
  ok "کلید خصوصی/عمومی در ./keys ساخته شد"
else
  warn "openssl پیدا نشد — ساخت کلید با یک Container موقت:"
  docker run --rm -v "$PWD/keys:/keys" alpine/openssl genrsa -out /keys/jwt-private.pem 2048
  docker run --rm -v "$PWD/keys:/keys" alpine/openssl rsa -in /keys/jwt-private.pem -pubout -out /keys/jwt-public.pem
  ok "کلید با Container موقت ساخته شد"
fi

# ---------- 3. زیرساخت پایه ----------
bold "3/6 — بالا آوردن Postgres/Redis/RabbitMQ"
docker compose up -d postgres redis rabbitmq
printf "  در انتظار healthy شدن..."
for i in $(seq 1 30); do
  if [ "$(docker compose ps --format '{{.Health}}' postgres redis rabbitmq 2>/dev/null | grep -c healthy)" = "3" ]; then
    printf "\n"; ok "هر سه سرویس healthy شدند"
    break
  fi
  printf "."
  sleep 2
  if [ "$i" = "30" ]; then printf "\n"; warn "بعد از ۶۰ ثانیه هنوز healthy نشدند — 'docker compose logs' را چک کنید."; fi
done

# ---------- 4. Migration اولیه ----------
bold "4/6 — ساخت اولین Migration دیتابیس (در صورت نبود)"
if [ -n "$(find apps/api/src/Infrastructure/Marketplace.Infrastructure/Persistence/Migrations -name '*.cs' 2>/dev/null)" ]; then
  warn "Migration از قبل موجود است — رد شد."
elif [ "$HAS_DOTNET" = "1" ]; then
  ( cd apps/api && \
    dotnet tool list --global | grep -q dotnet-ef || dotnet tool install --global dotnet-ef && \
    dotnet ef migrations add InitialCreate \
      --project src/Infrastructure/Marketplace.Infrastructure \
      --startup-project src/Presentation/Marketplace.API \
      --output-dir Persistence/Migrations ) \
  && ok "Migration ساخته شد" \
  || err "ساخت Migration شکست خورد — خروجی بالا را ببینید."
else
  warn "dotnet SDK پیدا نشد — این مرحله رد شد. بدون Migration، سرویس api در مرحله بعد شروع نمی‌شود."
  warn "راهنما: apps/api/README.md بخش Migrations (روش جایگزین با Container)."
fi

# ---------- 5. کل Stack ----------
bold "5/6 — بالا آوردن کل Stack (api + web)"
docker compose up -d --build
echo
docker compose ps

# ---------- 6. تست سلامت ----------
bold "6/6 — بررسی سلامت سرویس‌ها"
sleep 5
if curl -fsS http://localhost:8080/health >/dev/null 2>&1; then
  ok "API سالم است → http://localhost:8080/health"
else
  err "API پاسخ سالم نداد. 'docker compose logs api' را ببینید."
fi
if curl -fsS http://localhost:3000/api/health >/dev/null 2>&1; then
  ok "Web سالم است → http://localhost:3000/api/health"
else
  err "Web پاسخ سالم نداد. 'docker compose logs web' را ببینید."
fi

echo
bold "تمام شد. خروجی 'docker compose ps' بالا + نتیجه دو curl را عیناً در گزارش Task 1.7 کپی کنید."
echo "برای توقف کامل: docker compose down"
echo "برای تست‌های خودکار: apps/api → dotnet test | apps/web → npm ci && npm test | e2e → npm ci && npx playwright test"
