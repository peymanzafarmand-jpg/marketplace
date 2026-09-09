# Security Header Policy — Next.js & ASP.NET Core

هدف: اعمال یکسان Header های امنیتی در هر دو لایه (Frontend Edge و Backend API) به‌عنوان دفاع Defense-in-Depth.

## هدرهای الزامی

| Header | مقدار پیشنهادی | هدف |
|---|---|---|
| `Content-Security-Policy` | پایین آمده | جلوگیری از XSS/Data Injection |
| `Strict-Transport-Security` | `max-age=63072000; includeSubDomains; preload` | اجبار HTTPS |
| `X-Content-Type-Options` | `nosniff` | جلوگیری از MIME Sniffing |
| `X-Frame-Options` | `DENY` | جلوگیری از Clickjacking |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | کنترل نشت URL در Referrer |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=(self), payment=(self)` | غیرفعال‌سازی API های مرورگر غیرضروری |
| `X-XSS-Protection` | `0` (غیرفعال — به CSP اتکا می‌شود، این هدر قدیمی خودش گاهی مشکل‌ساز است) | — |

---

## Next.js (`next.config.js`)

```js
// next.config.js
const isProd = process.env.NODE_ENV === "production";

const ContentSecurityPolicy = `
  default-src 'self';
  script-src 'self' ${isProd ? "" : "'unsafe-eval'"};
  style-src 'self' 'unsafe-inline';
  img-src 'self' data: https://cdn.marketplace.example;
  font-src 'self';
  connect-src 'self' ${process.env.NEXT_PUBLIC_API_BASE_URL};
  frame-ancestors 'none';
  base-uri 'self';
  form-action 'self';
  object-src 'none';
  upgrade-insecure-requests;
`.replace(/\s{2,}/g, " ").trim();

const securityHeaders = [
  { key: "Content-Security-Policy", value: ContentSecurityPolicy },
  { key: "Strict-Transport-Security", value: "max-age=63072000; includeSubDomains; preload" },
  { key: "X-Content-Type-Options", value: "nosniff" },
  { key: "X-Frame-Options", value: "DENY" },
  { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
  { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=(self), payment=(self)" },
  { key: "X-XSS-Protection", value: "0" },
];

/** @type {import('next').NextConfig} */
module.exports = {
  reactStrictMode: true,
  poweredByHeader: false, // remove "X-Powered-By: Next.js"
  async headers() {
    return [
      {
        source: "/:path*",
        headers: securityHeaders,
      },
    ];
  },
};
```

> نکته: `script-src` در Production باید بدون `'unsafe-inline'`/`'unsafe-eval'` باشد. اگر Next.js به Inline Script نیاز داشت (مثلاً برای Hydration Data)، از **Nonce-based CSP** (`next-safe` یا میان‌افزار سفارشی که Nonce تولید و به Header + Script تزریق می‌کند) استفاده شود.

---

## ASP.NET Core (`Program.cs` Middleware)

```csharp
// Program.cs
var app = builder.Build();

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;

    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=(), payment=()";
    headers["X-XSS-Protection"] = "0";

    // API معمولاً HTML رندر نمی‌کند، اما برای هر پاسخ HTML/Swagger هم اعمال می‌شود
    headers["Content-Security-Policy"] =
        "default-src 'none'; frame-ancestors 'none'; base-uri 'none';";

    if (context.Request.IsHttps)
    {
        headers["Strict-Transport-Security"] =
            "max-age=63072000; includeSubDomains; preload";
    }

    await next();
});

// اجبار HTTPS Redirect + HSTS داخلی ASP.NET Core
app.UseHsts();
app.UseHttpsRedirection();
```

### CORS محدود (نه Wildcard)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["App:FrontendOrigin"]! // e.g. https://marketplace.example
              )
              .AllowedToAllowWildcardSubdomains()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

---

## تفاوت CSP بین Frontend و Backend
- **Frontend (Next.js)**: CSP باید صفحات واقعی (تصویر، فونت، اتصال به API) را مجاز کند — Allow-list دقیق دامنه CDN و API.
- **Backend (ASP.NET Core API)**: چون API معمولاً JSON برمی‌گرداند نه HTML، سخت‌گیرانه‌ترین CSP ممکن (`default-src 'none'`) اعمال می‌شود؛ فقط در صورت وجود Swagger UI در محیط Development باید CSP موقتاً برای آن مسیر خاص شل‌تر شود (هرگز در Production فعال نباشد).

## تست
- بررسی خودکار هدرها با `securityheaders.com` یا اسکریپت CI که پاسخ `curl -I` هر Endpoint اصلی را در برابر لیست بالا Assert کند (پیشنهاد: افزودن Integration Test مخصوص در بخش 6).
