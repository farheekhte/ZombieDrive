# Ali Zombie Drive — Unity HDRP

پروتوتایپ سه‌بعدی رانندگی شبانه با زامبی، ارتقای سپر و سکوهای پرش؛ هدف طراحی، حس آرکید سریع و سینمایی روی PC است، نه شبیه‌ساز خشک.

## نسخه‌ی هدف
- Unity 6000.3.10f1 (Unity 6.3 LTS)
- HDRP 17.3.0
- Input System 1.16.0
- Unity glTFast 6.20.0
- Windows x86_64
- کنترل اصلی: Xbox / PlayStation Gamepad

## مسیر Cloud Build
برای Build ویندوز روی سیستم محلی Unity لازم نیست. در Unity Build Automation متد `AliCloudBuild.PreExport` را به‌عنوان Pre-export method تنظیم کن. این متد:
1. Assetهای CC0 اختیاری را دانلود و Import می‌کند.
2. صحنه‌ی بازی را خودش می‌سازد.
3. Scene را وارد Build Settings می‌کند.
4. Build حتی در صورت قطع بودن یکی از منابع Asset با fallbackهای داخلی ادامه پیدا می‌کند.

## Assetهای رایگان فعلی
- ماشین اصلی: Mid-engine Sports Car از 3DAssets.dev، CC0، حدود 48.7k triangles و متریال‌های چندگانه.
- زامبی عادی: Infected Runner از 3DAssets.dev، CC0، حدود 12.6k triangles.
- زامبی سطح 5 به بالا: Brute Infected از 3DAssets.dev، CC0.
- آسفالت PBR، Street Lamp، Rock Face، Concrete Barrier و HDRI: Poly Haven، CC0.

Assetهای دانلودی داخل Git ذخیره نمی‌شوند و در `Assets/AliZombieDrive/GeneratedAssets/` ساخته می‌شوند.

## کنترل
- Left Stick: فرمان
- RT / R2: گاز
- LT / L2: ترمز / دنده عقب
- کیبورد جایگزین: WASD یا Arrow Keys

## وضعیت گیم‌پلی
- Rigidbody arcade handling
- Chase Camera و FOV وابسته به سرعت
- جاده‌ی procedural با پیچ و ارتفاع
- زامبی‌های پراکنده با قدرت افزایشی
- برخورد فیزیکی، پرتاب/چرخش بدن و cleanup
- مدل Brute برای زامبی‌های قوی
- Ram Upgrade
- Jump Ramp + مانع واقعی بعد از سکو
- نور شب، چراغ خودرو، چراغ جاده، Bloom، ACES، Fog و HDRI Sky
- آسفالت PBR با ظاهر wet-night

## هدف بصری
این پروژه از Asset یا کد اختصاصی Need for Speed استفاده نمی‌کند. جهت بصری فقط الهام‌گرفته از رانندگی سینمایی مدرن است. جهش بعدی کیفیت از VFX باران/ذرات، صدای موتور چندلایه، LOD/occlusion، vegetation و کیفیت animation می‌آید؛ نه از پیچیده‌ترکردن سیستم‌های پایه.
