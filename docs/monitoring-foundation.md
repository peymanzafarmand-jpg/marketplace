# Monitoring Foundation

هدف این سند تعریف **زیرساخت پایه Monitoring** است (بدون ساخت Dashboard کامل)، تا از ابتدای توسعه، Observability به‌صورت Built-in در معماری وجود داشته باشد.

## 1. Structured Logs

- فرمت: **JSON** (نه Plain Text) روی هر دو لایه، برای قابلیت Query/Aggregate آسان.
- کتابخانه پیشنهادی Backend: **Serilog** با Sink به Console (stdout) — جمع‌آوری واقعی توسط Log Aggregator بیرونی (Loki/ELK) انجام می‌شود، نه توسط خود اپلیکیشن.
- فیلدهای اجباری هر Log Entry:
  ```json
  {
    "timestamp": "...",
    "level": "Information",
    "message": "...",
    "request_id": "correlation id",
    "user_id": "در صورت وجود Auth",
    "trace_id": "OpenTelemetry trace id",
    "service": "marketplace-api"
  }
  ```
- **ممنوعیت درج داده حساس در Log**: رمز عبور، Token کامل، شماره کارت، اطلاعات هویتی کودک هرگز Log نمی‌شوند (Redaction Middleware روی فیلدهای شناخته‌شده حساس).
- نمونه پیکربندی حداقلی Serilog (`Program.cs`):
  ```csharp
  builder.Host.UseSerilog((context, config) =>
  {
      config
          .Enrich.FromLogContext()
          .Enrich.WithProperty("service", "marketplace-api")
          .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
          .MinimumLevel.Is(context.HostingEnvironment.IsProduction()
              ? Serilog.Events.LogEventLevel.Warning
              : Serilog.Events.LogEventLevel.Debug);
  });
  ```

## 2. OpenTelemetry (Tracing + Metrics)

- استفاده از **OpenTelemetry SDK** برای .NET و Next.js، Export با پروتکل **OTLP** به یک Collector مرکزی (که بعداً می‌تواند به Jaeger/Tempo/Grafana وصل شود).
- هدف فاز فعلی: فقط **Instrumentation پایه** (Auto-instrumentation برای HTTP، Npgsql، Redis، RabbitMQ) — بدون نیاز به Dashboard کامل.
- نمونه پیکربندی Backend (`Program.cs`):
  ```csharp
  builder.Services.AddOpenTelemetry()
      .WithTracing(tracing => tracing
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddNpgsql()
          .AddOtlpExporter(o => o.Endpoint =
              new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]!)))
      .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddRuntimeInstrumentation()
          .AddOtlpExporter());
  ```
- هر Request باید یک `trace_id` واحد داشته باشد که در Structured Log هم درج می‌شود (ارتباط Log ↔ Trace).

## 3. Sentry (Error Tracking)

- فعال روی هر دو لایه (Backend .NET SDK + Frontend `@sentry/nextjs`).
- **Scrubbing خودکار** داده حساس قبل از ارسال به Sentry (فیلدهای `password`, `token`, `card_number` از Payload حذف شوند — تنظیم `beforeSend` Hook).
- سطح ارسال: فقط `Warning` به بالا در Production (جلوگیری از Noise)؛ `Error`+ به بالا Alert فوری.
- تفکیک Environment در Sentry (`SENTRY_ENVIRONMENT=production|staging|development`) تا خطاهای محیط تست با Production قاطی نشوند.

## 4. Prometheus Metrics

- Backend: کتابخانه `prometheus-net.AspNetCore` برای Expose خودکار Endpoint `/metrics` (فقط از شبکه داخلی قابل Scrape، نه عمومی).
- متریک‌های پایه (Auto): تعداد Request، Latency Histogram، Error Rate به تفکیک Endpoint.
- متریک‌های Custom پیشنهادی برای فاز بعدی (نه الزام فاز فعلی): `orders_created_total`, `payment_failures_total`, `login_failures_total`.
- Redis/RabbitMQ/Postgres هرکدام Exporter مستقل خود را دارند (`redis_exporter`, `rabbitmq_prometheus plugin`, `postgres_exporter`) که در Docker Compose به‌عنوان سرویس کمکی افزوده می‌شوند.

## 5. Health Check Monitoring

- ASP.NET Core Health Checks (`Microsoft.Extensions.Diagnostics.HealthChecks`) روی Endpoint `/health`:
  ```csharp
  builder.Services.AddHealthChecks()
      .AddNpgSql(connectionString, name: "postgres")
      .AddRedis(redisConnectionString, name: "redis")
      .AddRabbitMQ(rabbitConnectionString, name: "rabbitmq");

  app.MapHealthChecks("/health");
  ```
- Next.js: یک Route ساده `/api/health` که فقط `200 OK` برمی‌گرداند (برای Docker `HEALTHCHECK` و Load Balancer).
- این Endpointها توسط Docker `HEALTHCHECK` (بخش Docker) و در آینده توسط Orchestrator (Kubernetes Liveness/Readiness Probe) استفاده می‌شوند.
- `/health` نباید جزئیات داخلی حساس (نسخه دقیق کتابخانه، Connection String) را در پاسخ برگرداند — فقط وضعیت کلی `Healthy/Unhealthy` هر Dependency.

## جمع‌بندی فاز فعلی
در این فاز فقط **زیرساخت** (Instrumentation + Endpoint + Log Format) پیاده می‌شود؛ ساخت Dashboard Grafana کامل، تعریف Alert Rule دقیق، و SLO رسمی به فاز بعدی (بعد از شروع Coding واقعی و مشخص شدن ترافیک واقعی) موکول می‌شود.
