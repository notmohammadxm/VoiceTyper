<div align="center">

# Voice Typer

### Speak naturally. Type instantly.

**A lightweight Windows speech-to-text utility that runs quietly in the system tray and types recognized speech into the active application.**

[![Version](https://img.shields.io/badge/version-1.0.0-2563eb?style=for-the-badge)](#release)
[![Platform](https://img.shields.io/badge/platform-Windows-0078D4?style=for-the-badge&logo=windows&logoColor=white)](#requirements)
[![Language](https://img.shields.io/badge/language-C%23-68217A?style=for-the-badge&logo=csharp&logoColor=white)](#technology)
[![Interface](https://img.shields.io/badge/interface-WinForms-475569?style=for-the-badge)](#technology)

[English](#english) · [فارسی](#فارسی)

</div>

---

<a id="english"></a>

## English

Voice Typer is a small Windows desktop utility for dictating text without repeatedly opening a terminal or switching to a separate transcription window. After installation, it can start with Windows, stay in the system tray, listen while its hotkey is activated, recognize speech, and insert the resulting text into the currently focused application.

> **Version 1.0.0 — Initial Release**

## Contents

- [Features](#features)
- [How It Works](#how-it-works)
- [Installation](#installation)
- [Using Voice Typer](#using-voice-typer)
- [Hotkey Modes](#hotkey-modes)
- [System Tray Menu](#system-tray-menu)
- [Requirements](#requirements)
- [Technology](#technology)
- [Privacy and Security](#privacy-and-security)
- [Troubleshooting](#troubleshooting)
- [Uninstall](#uninstall)
- [Known Limitations](#known-limitations)
- [Release](#release)
- [فارسی](#فارسی)

## Features

- **Speech-to-text:** Converts recorded speech into text using an online recognition endpoint.
- **Persian and English recognition:** Tries Persian (`fa-IR`) and US English (`en-US`) recognition and selects a result using basic language checks.
- **Global hotkey:** Activate dictation from other applications without switching windows.
- **Configurable shortcut:** Choose from F1–F24, Scroll Lock, Pause, or Insert.
- **Default F9 hotkey:** New installations use F9 unless another key is selected during setup.
- **Hold-to-talk mode:** Hold the selected key while speaking; release it to finish recording and type the result.
- **Toggle mode:** Scroll Lock can be configured to start recording with one press and stop it with another.
- **System tray operation:** Runs in the background with a tray icon and status text.
- **Launch at Windows sign-in:** Optionally adds itself to the current user's Windows startup entries.
- **Simple setup flow:** Running the executable from outside its installation directory opens the installer.
- **Built-in uninstaller:** Remove the application through the tray menu or the `/uninstall` command-line switch.
- **No separate audio recorder executable:** Audio capture uses the Windows `winmm.dll` multimedia API.

## How It Works

1. Activate the configured hotkey.
2. Voice Typer records mono, 16-bit audio at 16 kHz from the default recording device.
3. When recording stops, the captured audio is sent to an online speech-recognition endpoint for Persian and English recognition.
4. The application selects a suitable transcript.
5. The recognized text is copied to the Windows clipboard and pasted into the focused application using simulated `Ctrl+V` input.

**Important:** Speech recognition requires a working internet connection. The destination application must be focused and able to accept pasted text.

## Installation

1. Open the project's **Releases** page and download the published executable or release package.
2. Run `VoiceTyper.exe`.
3. Select a hotkey in the setup dialog.
4. Leave **Start with Windows** enabled if you want Voice Typer to launch automatically at sign-in.
5. Select **Install** and wait for the confirmation message.

The application installs under:

```text
%LOCALAPPDATA%\VoiceTyper\
```

The installed executable is:

```text
%LOCALAPPDATA%\VoiceTyper\VoiceTyper.exe
```

A Start Menu shortcut is also created. The startup option is registered for the current Windows user; administrator privileges are not normally required.

## Using Voice Typer

1. Open the application where you want the text to appear.
2. Place the text cursor in an editable field.
3. Use the configured hotkey and speak clearly.
4. Stop recording according to the selected hotkey mode.
5. Wait for recognition; the transcript will be pasted into the focused field.

Keep the target field focused while recognition is running. If the focus changes before the result is inserted, the text may be pasted into another application.

## Hotkey Modes

| Key selection | Behavior |
| --- | --- |
| F1–F24 | Hold the key to record; release it to stop and process speech |
| Pause | Hold the key to record; release it to stop and process speech |
| Insert | Hold the key to record; release it to stop and process speech |
| Scroll Lock | Press once to start recording; press again to stop |

To change the shortcut after installation, right-click the Voice Typer tray icon and select **Change hotkey...**. The new selection is saved for the current Windows user.

## System Tray Menu

Right-click the Voice Typer icon in the system tray to access:

- **Change hotkey...** — select and save a different shortcut.
- **Uninstall...** — launch the removal flow.
- **Exit** — close the background application.

The tray tooltip reports the current shortcut or the current recording/recognition state.

## Requirements

- Windows desktop environment.
- A working microphone or recording input device.
- Internet access for online speech recognition.
- Permission to use the microphone and clipboard.
- A focused application that accepts pasted text.

## Technology

| Component | Implementation |
| --- | --- |
| Language | C# |
| Desktop UI | Windows Forms |
| Audio capture | Windows Multimedia API (`winmm.dll`) |
| Keyboard state | Win32 `GetAsyncKeyState` |
| Text insertion | Windows clipboard + simulated `Ctrl+V` |
| Preferences | Current-user Windows Registry |
| Startup | Current-user `Run` registry key |
| Recognition | HTTPS request to a speech-recognition endpoint |

## Privacy and Security

Voice Typer sends recorded audio to an online speech-recognition service for transcription. **Do not dictate passwords, authentication codes, confidential business information, or other sensitive content unless you have reviewed and accepted the service provider's data-handling terms.**

Additional notes:

- An internet connection is required to recognize speech.
- The transcript is temporarily placed on the Windows clipboard before it is pasted.
- Startup and hotkey preferences are stored in the current user's Windows Registry.
- This application does not provide offline speech recognition.
- Before publishing or redistributing the source, review the configured recognition API key. Do not publish private credentials. Use a key you are authorized to use, restrict it according to the provider's guidance, and rotate it if it has been exposed.

## Troubleshooting

### No text appears

- Confirm that the intended text field is focused.
- Check that the microphone is connected and available to Windows.
- Speak for long enough to produce a usable recording.
- Verify internet connectivity and speech-recognition service availability.
- Check whether the tray tooltip has returned to its idle state.

### The microphone does not start

- Check Windows microphone permissions and the default recording device.
- Close other applications that may be holding exclusive access to the device.
- Try another supported hotkey if the selected key is intercepted by another application.

### Recognition fails or returns no text

- Check the internet connection.
- Try speaking more clearly and reducing background noise.
- The online recognition endpoint may be unavailable, rate-limited, or incompatible with the configured credentials.

### Voice Typer does not start with Windows

- Re-run setup and enable **Start with Windows**, or reinstall the application.
- Confirm that the current user's startup entry has not been removed by system-cleanup software or organizational policy.

## Uninstall

Use either method:

- Right-click the tray icon and select **Uninstall...**, then confirm.
- Run the installed executable with the `/uninstall` argument:

```bat
"%LOCALAPPDATA%\VoiceTyper\VoiceTyper.exe" /uninstall
```

The uninstaller removes the application's registry settings, startup entry, and Start Menu shortcut, then schedules the installation folder for removal after the process exits.

## Known Limitations

- Online recognition depends on network connectivity and the external recognition service.
- Persian/English selection uses basic transcript checks; mixed-language speech may not always be classified as expected.
- Text is pasted into whichever application is focused when insertion occurs.
- The shortcut is polled while the background application is running; it is not registered through the Windows `RegisterHotKey` API.
- The application currently exposes a compact tray interface rather than a full settings window.

## Release

**Current release:** `v1.0.0` — Initial Release

See the repository's **Releases** section for available downloads and version history.

---

<a id="فارسی"></a>

## فارسی

**Voice Typer** یک ابزار سبک برای ویندوز است که گفتار را به متن تبدیل می‌کند و متن تشخیص‌داده‌شده را مستقیماً در برنامه‌ای که در حال استفاده از آن هستید وارد می‌کند. پس از نصب، برنامه می‌تواند همراه ویندوز اجرا شود، در System Tray باقی بماند و بدون نیاز به باز نگه‌داشتن CMD یا اجرای دستی در هر بار استفاده، در پس‌زمینه کار کند.

> **نسخه 1.0.0 — اولین انتشار**

## فهرست مطالب

- [قابلیت‌ها](#قابلیتهای-اصلی)
- [نحوه عملکرد](#نحوه-عملکرد)
- [نصب](#نصب)
- [روش استفاده](#روش-استفاده)
- [حالت‌های کلید میانبر](#حالتهای-کلید-میانبر)
- [منوی System Tray](#منوی-system-tray)
- [نیازمندی‌ها](#نیازمندیها)
- [فناوری‌های استفاده‌شده](#فناوریهای-استفادهشده)
- [حریم خصوصی و امنیت](#حریم-خصوصی-و-امنیت)
- [عیب‌یابی](#عیبیابی)
- [حذف برنامه](#حذف-برنامه)
- [محدودیت‌های شناخته‌شده](#محدودیتهای-شناختهشده)
- [انتشار](#انتشار)

## قابلیت‌های اصلی

- **تبدیل گفتار به متن:** ارسال صدای ضبط‌شده به سرویس آنلاین تشخیص گفتار.
- **پشتیبانی از فارسی و انگلیسی:** تلاش برای تشخیص فارسی (`fa-IR`) و انگلیسی آمریکایی (`en-US`) و انتخاب نتیجه با بررسی‌های پایه زبانی.
- **کلید میانبر سراسری:** شروع تایپ صوتی از داخل برنامه‌های دیگر، بدون نیاز به جابه‌جایی بین پنجره‌ها.
- **انتخاب کلید دلخواه:** پشتیبانی از کلیدهای F1 تا F24، به‌همراه Scroll Lock، Pause و Insert.
- **کلید پیش‌فرض F9:** در نصب‌های جدید، F9 به‌عنوان کلید پیش‌فرض انتخاب شده است؛ البته می‌توان هنگام نصب کلید دیگری تعیین کرد.
- **حالت نگه‌داشتن کلید:** تا زمانی که کلید میانبر را نگه می‌دارید ضبط انجام می‌شود و با رهاکردن آن، ضبط پایان می‌یابد.
- **حالت روشن/خاموش:** با انتخاب Scroll Lock، یک بار فشردن کلید ضبط را شروع می‌کند و فشردن دوباره آن ضبط را متوقف می‌کند.
- **اجرای پس‌زمینه:** برنامه در System Tray ویندوز اجرا می‌شود و وضعیت خود را نمایش می‌دهد.
- **اجرای خودکار با ویندوز:** امکان فعال‌کردن اجرای خودکار هنگام ورود کاربر به ویندوز.
- **فرایند نصب ساده:** اجرای فایل از خارج از پوشه نصب، پنجره نصب را باز می‌کند.
- **حذف‌کننده داخلی:** حذف برنامه از طریق منوی Tray یا دستور `/uninstall`.
- **بدون نیاز به برنامه ضبط صدای جانبی:** ضبط صدا با API داخلی ویندوز یعنی `winmm.dll` انجام می‌شود.

## نحوه عملکرد

1. کلید میانبر انتخاب‌شده را فشار دهید.
2. برنامه صدای ورودی پیش‌فرض را به‌صورت Mono، با نرخ نمونه‌برداری 16 kHz و عمق 16 بیت ضبط می‌کند.
3. پس از پایان ضبط، صدا برای تشخیص گفتار فارسی و انگلیسی به سرویس آنلاین ارسال می‌شود.
4. برنامه یکی از متن‌های تشخیص‌داده‌شده را بر اساس بررسی‌های ساده انتخاب می‌کند.
5. متن در Clipboard ویندوز قرار می‌گیرد و با شبیه‌سازی `Ctrl+V` در برنامه فعال وارد می‌شود.

**نکته مهم:** تشخیص گفتار به اینترنت نیاز دارد. هنگام واردشدن متن، برنامه مقصد باید فعال باشد و امکان Paste کردن متن را داشته باشد.

## نصب

1. وارد بخش **Releases** مخزن گیت‌هاب شوید و فایل اجرایی یا بسته انتشار را دریافت کنید.
2. فایل `VoiceTyper.exe` را اجرا کنید.
3. در پنجره نصب، کلید میانبر موردنظر را انتخاب کنید.
4. اگر می‌خواهید برنامه با ورود به ویندوز اجرا شود، گزینه **Start with Windows** را فعال نگه دارید.
5. روی **Install** کلیک کنید و منتظر پیام موفقیت نصب بمانید.

مسیر نصب:

```text
%LOCALAPPDATA%\VoiceTyper\
```

مسیر فایل اجرایی نصب‌شده:

```text
%LOCALAPPDATA%\VoiceTyper\VoiceTyper.exe
```

همچنین یک میانبر در منوی Start ساخته می‌شود. تنظیم اجرای خودکار برای کاربر فعلی ویندوز ثبت می‌شود و معمولاً به دسترسی Administrator نیاز نیست.

## روش استفاده

1. برنامه‌ای را باز کنید که می‌خواهید متن در آن وارد شود.
2. نشانگر تایپ را داخل یک فیلد متنی قرار دهید.
3. کلید میانبر را مطابق حالت انتخاب‌شده فشار دهید و صحبت کنید.
4. ضبط را با روش مربوط به همان کلید متوقف کنید.
5. منتظر پایان تشخیص گفتار بمانید؛ متن در فیلد فعال Paste می‌شود.

تا زمان واردشدن متن، فیلد مقصد را فعال نگه دارید. اگر فوکوس به پنجره دیگری منتقل شود، ممکن است متن در برنامه دیگری وارد شود.

## حالت‌های کلید میانبر

| کلید | عملکرد |
| --- | --- |
| F1 تا F24 | با نگه‌داشتن کلید ضبط شروع می‌شود و با رهاکردن آن متوقف می‌شود |
| Pause | با نگه‌داشتن کلید ضبط شروع می‌شود و با رهاکردن آن متوقف می‌شود |
| Insert | با نگه‌داشتن کلید ضبط شروع می‌شود و با رهاکردن آن متوقف می‌شود |
| Scroll Lock | یک بار برای شروع ضبط و بار دیگر برای توقف ضبط |

برای تغییر کلید پس از نصب، روی آیکون Voice Typer در System Tray راست‌کلیک کنید و گزینه **Change hotkey...** را انتخاب کنید. کلید جدید در تنظیمات کاربر فعلی ویندوز ذخیره می‌شود.

## منوی System Tray

با راست‌کلیک روی آیکون Voice Typer به این گزینه‌ها دسترسی دارید:

- **Change hotkey...** — تغییر و ذخیره کلید میانبر.
- **Uninstall...** — شروع فرایند حذف برنامه.
- **Exit** — بستن برنامه پس‌زمینه.

متن راهنمای آیکون، کلید میانبر فعلی یا وضعیت ضبط و تشخیص گفتار را نشان می‌دهد.

## نیازمندی‌ها

- سیستم‌عامل ویندوز با محیط دسکتاپ.
- میکروفون یا دستگاه ورودی ضبط صدا.
- اتصال اینترنت برای تشخیص گفتار آنلاین.
- دسترسی لازم به میکروفون و Clipboard.
- برنامه مقصدی که متن Paste‌شده را بپذیرد.

## فناوری‌های استفاده‌شده

| بخش | فناوری |
| --- | --- |
| زبان برنامه‌نویسی | C# |
| رابط دسکتاپ | Windows Forms |
| ضبط صدا | Windows Multimedia API (`winmm.dll`) |
| بررسی وضعیت کلید | Win32 `GetAsyncKeyState` |
| ورود متن | Clipboard ویندوز و شبیه‌سازی `Ctrl+V` |
| تنظیمات | رجیستری کاربر فعلی ویندوز |
| اجرای خودکار | کلید `Run` در رجیستری کاربر |
| تشخیص گفتار | درخواست HTTPS به سرویس تشخیص گفتار |

## حریم خصوصی و امنیت

برای تبدیل گفتار به متن، صدای ضبط‌شده به یک سرویس آنلاین ارسال می‌شود. **تا زمانی که شرایط پردازش داده‌های سرویس را بررسی نکرده‌اید، اطلاعات حساس مانند رمز عبور، کدهای ورود یا اطلاعات محرمانه کاری را دیکته نکنید.**

نکات دیگر:

- برای تشخیص گفتار به اینترنت نیاز است.
- متن تشخیص‌داده‌شده پیش از Paste شدن، موقتاً در Clipboard ویندوز قرار می‌گیرد.
- تنظیمات کلید میانبر و اجرای خودکار در رجیستری کاربر فعلی ذخیره می‌شوند.
- این نسخه قابلیت تشخیص گفتار آفلاین ندارد.
- پیش از انتشار عمومی سورس‌کد، کلید API تنظیم‌شده را بررسی کنید. اطلاعات محرمانه را در مخزن عمومی قرار ندهید؛ از کلیدی استفاده کنید که مجاز به استفاده از آن هستید، محدودیت‌های لازم را اعمال کنید و در صورت افشاشدن کلید، آن را تعویض کنید.

## عیب‌یابی

### متنی وارد نمی‌شود

- مطمئن شوید فیلد مقصد فعال است.
- اتصال و دسترسی میکروفون را در ویندوز بررسی کنید.
- به‌اندازه کافی صحبت کنید تا صدای قابل‌استفاده ضبط شود.
- اتصال اینترنت و در دسترس بودن سرویس تشخیص گفتار را بررسی کنید.
- وضعیت آیکون برنامه در System Tray را بررسی کنید.

### ضبط صدا شروع نمی‌شود

- مجوز دسترسی به میکروفون و دستگاه ضبط پیش‌فرض ویندوز را بررسی کنید.
- برنامه‌هایی را که ممکن است دسترسی انحصاری به میکروفون گرفته باشند ببندید.
- اگر برنامه دیگری کلید میانبر را کنترل می‌کند، کلید دیگری انتخاب کنید.

### تشخیص گفتار ناموفق است یا متنی برنمی‌گرداند

- اتصال اینترنت را بررسی کنید.
- واضح‌تر و با صدای محیطی کمتر صحبت کنید.
- ممکن است سرویس آنلاین در دسترس نباشد، محدودیت درخواست داشته باشد یا اعتبارنامه تنظیم‌شده با آن سازگار نباشد.

### برنامه با ویندوز اجرا نمی‌شود

- برنامه نصب را دوباره اجرا کنید و گزینه **Start with Windows** را فعال کنید؛ در صورت نیاز نصب را تکرار کنید.
- بررسی کنید که ورودی اجرای خودکار توسط ابزارهای پاک‌سازی سیستم یا سیاست‌های سازمانی حذف نشده باشد.

## حذف برنامه

از یکی از روش‌های زیر استفاده کنید:

- روی آیکون Tray راست‌کلیک کنید، گزینه **Uninstall...** را بزنید و حذف را تأیید کنید.
- فایل اجرایی نصب‌شده را با آرگومان `/uninstall` اجرا کنید:

```bat
"%LOCALAPPDATA%\VoiceTyper\VoiceTyper.exe" /uninstall
```

حذف‌کننده تنظیمات رجیستری، ورودی اجرای خودکار و میانبر منوی Start را حذف می‌کند و حذف پوشه نصب را به پس از پایان اجرای برنامه موکول می‌کند.

## محدودیت‌های شناخته‌شده

- تشخیص گفتار آنلاین به اتصال شبکه و در دسترس بودن سرویس خارجی وابسته است.
- انتخاب زبان فارسی/انگلیسی بر اساس بررسی‌های ساده متن انجام می‌شود؛ گفتار ترکیبی ممکن است همیشه درست تشخیص داده نشود.
- متن در برنامه‌ای وارد می‌شود که هنگام درج متن فعال است.
- کلید میانبر در زمان اجرای برنامه به‌صورت دوره‌ای بررسی می‌شود و از API ویندوز `RegisterHotKey` استفاده نمی‌شود.
- رابط برنامه فعلاً به منوی System Tray محدود است و پنجره تنظیمات کامل ندارد.

## انتشار

**نسخه فعلی:** `v1.0.0` — اولین انتشار

برای دریافت فایل‌ها و مشاهده نسخه‌های منتشرشده، بخش **Releases** مخزن را ببینید.

---

<div align="center">

**Voice Typer · v1.0.0**

Made for faster, more convenient voice typing on Windows.

</div>
