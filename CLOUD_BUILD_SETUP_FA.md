# ساخت نسخه Windows بدون نصب Unity روی کامپیوتر

این پروژه برای Unity Build Automation آماده شده است.

## تنظیمات پیشنهادی Build Automation

- Source control: GitHub
- Branch: `main`
- Project subfolder: خالی (پروژه در ریشه Repository است)
- Unity version: Auto detect یا `6000.3.10f1`
- Platform: Windows Desktop / Standalone Windows 64-bit
- Builder OS: Windows
- Pre-export method: `AliCloudBuild.PreExport`
- Build output: Windows x86_64
- Auto-build: فعلاً خاموش؛ Build دستی برای اولین اجرا

`AliCloudBuild.PreExport` قبل از Build، صحنه بازی را به‌صورت خودکار می‌سازد و آن را وارد Build Settings می‌کند. بنابراین برای گرفتن EXE نیازی نیست Unity Editor را روی سیستم محلی باز کنید.

## روند

1. Repository را به Unity Dashboard > DevOps > Build Automation متصل کنید.
2. یک Configuration برای Windows بسازید.
3. در Advanced Settings مقدار Pre-export method را روی `AliCloudBuild.PreExport` قرار دهید.
4. Build را اجرا کنید.
5. پس از Success، Artifact خروجی Windows را دانلود کنید.

## خروجی مورد انتظار

Artifact ویندوز شامل فایل اجرایی بازی و پوشه Data خواهد بود، مشابه:

- `Ali Zombie Drive.exe`
- `Ali Zombie Drive_Data/`

## نکته درباره کیفیت گرافیکی

این Build یک Prototype فنی HDRP است. رسیدن به کیفیت تصویری نزدیک NFS Rivals نیازمند اضافه‌شدن Assetهای حرفه‌ای PBR، مدل ماشین High-poly، محیط، VFX و نورپردازی/Weather تکمیل‌شده است؛ Cloud Build فقط پروژه فعلی را به EXE تبدیل می‌کند و خودش کیفیت Art را افزایش نمی‌دهد.
