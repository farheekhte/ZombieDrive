# ساخت نسخه Windows بدون نصب Unity

Repository:
https://github.com/farheekhte/ZombieDrive

این پروژه برای Unity Build Automation آماده است و در مرحله Pre-export خودش HDRP را فعال می‌کند، Assetهای CC0 اختیاری را می‌گیرد، صحنه را می‌سازد و آن را وارد Build Settings می‌کند.

## Source Control

Repository عمومی است، بنابراین Unity Build Automation می‌تواند آن را به‌عنوان Git/GitHub source دریافت کند.

- Repository: `farheekhte/ZombieDrive`
- Branch: `main`
- Project subfolder: خالی / root

## Configuration پیشنهادی

- Platform: Windows
- Architecture: x86_64
- Unity version: Auto detect
- ProjectVersion source: `ProjectSettings/ProjectVersion.txt`
- Expected editor: `6000.3.10f1`
- Builder OS: Windows 11 24H2
- Windows backend: Mono برای اولین Prototype build
- Machine: Micro برای اولین Build
- Build mode: Release / non-development
- Auto-build: برای اولین Build خاموش

## Advanced Settings

Pre-export method:

`AliCloudBuild.PreExport`

این متد به‌ترتیب:
1. Linear color space را اعمال می‌کند.
2. HDRP Render Pipeline Asset را ایجاد/فعال می‌کند.
3. HDRP Global Settings را تضمین می‌کند.
4. Assetهای CC0 اختیاری را دانلود و Import می‌کند.
5. Scene بازی را می‌سازد.
6. Scene را به EditorBuildSettings اضافه می‌کند.

اگر یک منبع Asset خارجی موقتاً در دسترس نباشد، Build باید با fallbackهای داخلی ادامه پیدا کند.

## خروجی مورد انتظار

Artifact ویندوز شامل چیزی شبیه موارد زیر است:

- `Ali Zombie Drive.exe`
- `Ali Zombie Drive_Data/`
- Unity runtime files

برای اجرا باید کل پوشه Artifact کنار EXE باقی بماند؛ فقط فایل EXE به‌تنهایی کافی نیست.

## بعد از اولین Build موفق

به‌ترتیب این موارد ارزش ارتقا دارند:
1. باران و water spray روی آسفالت خیس
2. صدای موتور چندلایه + برخورد
3. tire smoke / sparks / collision VFX
4. vegetation و roadside dressing بیشتر
5. LOD و optimization
6. سپس Setup.exe

هدف این مرحله گرفتن یک Windows build قابل اجرا و ارزیابی گرافیک/کنترل واقعی است، نه اضافه‌کردن سیستم‌های بیشتر قبل از اینکه اولین Build روی سخت‌افزار واقعی دیده شود.
