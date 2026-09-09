# PRODUCTION SECURITY CHECKLIST
### Marketplace (نوزاد، کودک و مادر) — Pre-Launch Gate

> این چک‌لیست باید قبل از هر Deploy به `production` به‌طور کامل تایید شود. مسئول تایید نهایی: Security Engineer + DevOps Lead.

## 1. Transport & Network
- [ ] TLS 1.2+ اجباری روی همه Endpoint (Frontend + API + Admin Panel)
- [ ] HSTS فعال با `preload` (بعد از اطمینان کامل از HTTPS بودن همه Subdomain)
- [ ] WAF/Bot Protection در جلوی Edge فعال
- [ ] Backend/DB/Redis/RabbitMQ فقط روی شبکه Internal، بدون IP عمومی مستقیم

## 2. Secrets
- [ ] هیچ Secret در Git History (تایید با Gitleaks Full Scan)
- [ ] همه Secretهای Production در Vault/Secret Manager، نه در ENV پلین در `docker-compose.yml`
- [ ] JWT Signing Key اختصاصی Production (متفاوت از Staging/Dev)
- [ ] Rotation Policy برای DB Password / JWT Key مستند و زمان‌بندی‌شده

## 3. Authentication & Authorization
- [ ] Argon2id/bcrypt (Cost ≥ 12) برای Password Hashing فعال
- [ ] 2FA اجباری برای نقش Admin/Seller-Owner
- [ ] Refresh Token Rotation + Reuse Detection فعال و تست‌شده
- [ ] Rate Limiting روی Login/Register/Forgot-Password فعال
- [ ] RBAC + Ownership Check روی همه Endpoint حساس (تست IDOR در CI سبز)
- [ ] «خروج از همه دستگاه‌ها» تست عملکردی شده

## 4. API Security
- [ ] Schema Validation روی همه ورودی (رد Mass Assignment)
- [ ] CORS محدود به Origin واقعی Production (بدون Wildcard)
- [ ] Security Headers کامل (CSP, HSTS, X-Frame-Options, ...) روی Frontend و Backend تایید شده (اسکن `securityheaders.com`)
- [ ] Pagination اجباری روی همه List Endpoint

## 5. File Upload
- [ ] MIME + Magic Number Validation فعال
- [ ] Malware Scan (ClamAV) در Pipeline آپلود فعال
- [ ] Re-encode تصویر + حذف EXIF فعال
- [ ] Storage Bucket با ACL خصوصی پیش‌فرض، Signed URL برای محتوای غیرعمومی

## 6. Payment
- [ ] هیچ داده کارت خام (PAN/CVV) در DB/Log ذخیره نمی‌شود (تایید دستی + Code Review)
- [ ] Webhook Signature Verification فعال و تست‌شده
- [ ] Idempotency Key روی پردازش تراکنش فعال
- [ ] Reconciliation Job روزانه فعال

## 7. Logging & Audit
- [ ] Audit Log Append-only برای اقدامات حساس (Admin actions, Refund, Role Change)
- [ ] Log سطح Production روی `Warning`+ (بدون PII/Secret در Log — بررسی نمونه Log)
- [ ] Sentry با Scrubbing فعال (فیلدهای حساس Redact شده)

## 8. Backup & DR
- [ ] Backup روزانه DB فعال و **Restore Drill تست‌شده** در ماه اخیر
- [ ] WAL Archiving / PITR فعال
- [ ] RTO/RPO مستند (`docs/security/threat-model.md` یا سند DR جداگانه) و تایید شده توسط تیم

## 9. CI/CD
- [ ] Branch Protection روی `main`: نیازمند عبور همه Job های CI (`ci-success`)
- [ ] Container Scan (Trivy) بدون یافته Critical/High
- [ ] Dependency Scan (npm audit / dotnet vulnerable) بدون یافته Critical/High
- [ ] Secret Scan (Gitleaks) پاک
- [ ] Manual Approval Gate برای Deploy به Production فعال

## 10. Monitoring
- [ ] Health Check همه سرویس‌ها سبز
- [ ] Alert روی: Error Rate بالا، DB Down، Payment Failure Spike، Login Failure غیرعادی
- [ ] On-call مشخص و Runbook حوادث رایج آماده

## 11. Compliance / Privacy
- [ ] مکانیزم حذف/Export داده کاربر (حریم خصوصی) پیاده‌سازی شده
- [ ] سیاست جمع‌آوری داده کودک (در صورت وجود) بازبینی حقوقی شده و رضایت صریح دارد
- [ ] Data Retention Policy (Audit Log, Session, General Log) مستند و اعمال شده

## 12. Final Sign-off
- [ ] Penetration Test انجام شده، یافته‌های Critical/High رفع و تایید مجدد شده
- [ ] Load Test برای سناریوی ترافیک پیک انجام و ظرفیت تایید شده
- [ ] Rollback Plan برای این Release مشخص و در دسترس تیم On-call است
- [ ] این چک‌لیست توسط Security Engineer و DevOps Lead امضا/تایید شده است

---
**تاریخ آخرین بازبینی:** _(تکمیل شود)_
**تایید‌کننده:** _(تکمیل شود)_
