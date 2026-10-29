using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

// Voice Typer
//  - اجرای فایل از هر مسیر غیر از پوشه نصب  => Setup
//  - اجرای نسخه نصب‌شده                       => برنامه پس‌زمینه (Tray)
//  - VoiceTyper.exe /uninstall                => حذف کامل
static class Program
{
    const string AppName = "Voice Typer";
    const string ExeName = "VoiceTyper.exe";
    const string RegApp = @"Software\VoiceTyper";
    const string RegUninstall = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\VoiceTyper";
    const string RegRun = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public const int VK_F9 = 0x78;
    public const int VK_SCROLL = 0x91;

    const int SampleRate = 16000;
    const int MinBytes = 16000;
    const string ApiKey = "AIzaSyBOti4mM-6x9WDnZIjIeyEU21OpBXqWBgw";

    [DllImport("user32.dll")] static extern short GetAsyncKeyState(int vKey);
    [DllImport("user32.dll")] static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);

    static Recorder rec = new Recorder();
    static NotifyIcon tray;
    static Form host;
    static System.Windows.Forms.Timer timer;
    static int hotkey = VK_F9;
    static bool recording, busy, wasDown;

    // ---------- مسیرها ----------
    static string InstallDir
    {
        get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VoiceTyper"); }
    }
    static string InstalledExe { get { return Path.Combine(InstallDir, ExeName); } }
    static string StartMenuLink
    {
        get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), AppName + ".lnk"); }
    }

    public static string KeyName(int vk)
    {
        if (vk == VK_SCROLL) return "Scroll Lock";
        if (vk == 0x13) return "Pause";
        if (vk == 0x2D) return "Insert";
        return ((Keys)vk).ToString();
    }

    public static bool IsToggleKey(int vk) { return vk == VK_SCROLL; }

    // ---------- تنظیمات ----------
    static int ReadHotkey()
    {
        try
        {
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(RegApp))
            {
                if (k != null)
                {
                    object o = k.GetValue("Hotkey");
                    if (o is int) return (int)o;
                }
            }
        }
        catch { }
        return VK_F9;
    }

    static void WriteHotkey(int vk)
    {
        using (RegistryKey k = Registry.CurrentUser.CreateSubKey(RegApp))
            k.SetValue("Hotkey", vk, RegistryValueKind.DWord);
    }

    static bool IsAutoStart()
    {
        try
        {
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(RegRun))
                return k != null && k.GetValue("VoiceTyper") != null;
        }
        catch { return false; }
    }

    static void SetAutoStart(bool on)
    {
        using (RegistryKey k = Registry.CurrentUser.CreateSubKey(RegRun))
        {
            if (on) k.SetValue("VoiceTyper", "\"" + InstalledExe + "\"");
            else k.DeleteValue("VoiceTyper", false);
        }
    }

    static void MakeShortcut(string lnk, string target)
    {
        try
        {
            Type t = Type.GetTypeFromProgID("WScript.Shell");
            object sh = Activator.CreateInstance(t);
            object l = t.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, sh, new object[] { lnk });
            Type lt = l.GetType();
            lt.InvokeMember("TargetPath", BindingFlags.SetProperty, null, l, new object[] { target });
            lt.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, l, new object[] { InstallDir });
            lt.InvokeMember("Save", BindingFlags.InvokeMethod, null, l, null);
        }
        catch { }
    }

    static void KillOthers()
    {
        int me = Process.GetCurrentProcess().Id;
        foreach (Process p in Process.GetProcessesByName("VoiceTyper"))
        {
            if (p.Id == me) continue;
            try { p.Kill(); p.WaitForExit(2000); } catch { }
        }
    }

    // ---------- ورودی ----------
    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();

        if (args.Length > 0 && args[0].Equals("/uninstall", StringComparison.OrdinalIgnoreCase))
        {
            Uninstall(args.Length < 2);
            return;
        }

        bool installed = string.Equals(Path.GetFullPath(Application.ExecutablePath),
                                       Path.GetFullPath(InstalledExe), StringComparison.OrdinalIgnoreCase);
        if (installed) RunApp();
        else Setup();
    }

    // ---------- نصب ----------
    static void Setup()
    {
        bool already = File.Exists(InstalledExe);
        using (KeyDialog dlg = new KeyDialog(ReadHotkey(), true, already ? IsAutoStart() : true))
        {
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                KillOthers();
                Directory.CreateDirectory(InstallDir);
                File.Copy(Application.ExecutablePath, InstalledExe, true);

                WriteHotkey(dlg.VkCode);
                SetAutoStart(dlg.AutoStart);

                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(RegUninstall))
                {
                    k.SetValue("DisplayName", AppName);
                    k.SetValue("DisplayIcon", InstalledExe);
                    k.SetValue("UninstallString", "\"" + InstalledExe + "\" /uninstall");
                    k.SetValue("InstallLocation", InstallDir);
                    k.SetValue("Publisher", AppName);
                    k.SetValue("NoModify", 1, RegistryValueKind.DWord);
                    k.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                }

                MakeShortcut(StartMenuLink, InstalledExe);
                Process.Start(InstalledExe);

                string how = IsToggleKey(dlg.VkCode)
                    ? "Press " + KeyName(dlg.VkCode) + " once to start recording and again to stop."
                    : "Hold " + KeyName(dlg.VkCode) + " to speak, release it to type.";
                MessageBox.Show("Installed successfully.\n\n" + how + "\nThe app runs in the system tray.",
                    AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Installation failed:\n" + ex.Message, AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ---------- حذف ----------
    static void Uninstall(bool ask)
    {
        if (ask && MessageBox.Show("Remove Voice Typer from this computer?", AppName,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        KillOthers();
        try { Registry.CurrentUser.DeleteSubKeyTree(RegApp, false); } catch { }
        try { Registry.CurrentUser.DeleteSubKeyTree(RegUninstall, false); } catch { }
        try { SetAutoStart(false); } catch { }
        try { if (File.Exists(StartMenuLink)) File.Delete(StartMenuLink); } catch { }

        MessageBox.Show("Voice Typer has been removed.", AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);

        try
        {
            // حذف پوشه بعد از خروج همین پروسس
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe",
                "/c ping 127.0.0.1 -n 3 >nul & rmdir /s /q \"" + InstallDir + "\"");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }
        catch { }
    }

    // ---------- برنامه پس‌زمینه ----------
    static void RunApp()
    {
        bool created;
        using (Mutex m = new Mutex(true, "VoiceTyper_SingleInstance", out created))
        {
            if (!created) return;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // TLS 1.2
            hotkey = ReadHotkey();

            host = new Form();
            host.ShowInTaskbar = false;
            host.WindowState = FormWindowState.Minimized;
            host.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            host.Opacity = 0;
            host.Load += delegate { host.Hide(); };
            IntPtr h = host.Handle; // ساخت handle برای BeginInvoke

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Change hotkey...", null, delegate { ChangeHotkey(); });
            menu.Items.Add("Uninstall...", null, delegate { StartUninstallFromTray(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, delegate { tray.Visible = false; Application.Exit(); });

            tray = new NotifyIcon();
            tray.Icon = SystemIcons.Application;
            tray.ContextMenuStrip = menu;
            tray.Text = IdleText();
            tray.Visible = true;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 25;
            timer.Tick += Tick;
            timer.Start();

            Application.Run();
        }
    }

    static string IdleText()
    {
        return AppName + " - " + (IsToggleKey(hotkey) ? "press " : "hold ") + KeyName(hotkey);
    }

    static void ChangeHotkey()
    {
        timer.Stop();
        using (KeyDialog d = new KeyDialog(hotkey, false, false))
        {
            if (d.ShowDialog() == DialogResult.OK)
            {
                hotkey = d.VkCode;
                try { WriteHotkey(hotkey); } catch { }
                tray.Text = IdleText();
            }
        }
        wasDown = (GetAsyncKeyState(hotkey) & 0x8000) != 0;
        timer.Start();
    }

    static void StartUninstallFromTray()
    {
        if (MessageBox.Show("Remove Voice Typer from this computer?", AppName,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        tray.Visible = false;
        Process.Start(Application.ExecutablePath, "/uninstall /y");
        Application.Exit();
    }

    static void Tick(object s, EventArgs e)
    {
        bool down = (GetAsyncKeyState(hotkey) & 0x8000) != 0;
        bool edge = down && !wasDown;
        wasDown = down;

        if (recording) rec.Poll(true);

        if (IsToggleKey(hotkey))
        {
            // حالت روشن/خاموش: هر بار فشردن کلید
            if (edge)
            {
                if (recording) StopRec();
                else StartRec();
            }
        }
        else
        {
            // حالت نگه‌داشتن
            if (edge && !recording) StartRec();
            else if (recording && !down) StopRec();
        }
    }

    static void StartRec()
    {
        if (busy) return;
        if (rec.Start())
        {
            recording = true;
            tray.Text = AppName + " - recording...";
        }
        else tray.Text = AppName + " - mic error";
    }

    static void StopRec()
    {
        recording = false;
        byte[] pcm = rec.Stop();
        if (pcm.Length < MinBytes) { tray.Text = IdleText(); return; }

        busy = true;
        tray.Text = AppName + " - recognizing...";
        ThreadPool.QueueUserWorkItem(delegate { Process_(pcm); });
    }

    static void Process_(byte[] pcm)
    {
        string text = "";
        try
        {
            string fa = "", en = "";
            Thread tf = new Thread(delegate () { fa = Recognize(pcm, "fa-IR"); });
            Thread te = new Thread(delegate () { en = Recognize(pcm, "en-US"); });
            tf.Start(); te.Start(); tf.Join(); te.Join();

            if (fa.Length > 0 && Regex.IsMatch(fa, "[\u0600-\u06FF]")) text = fa;
            else if (en.Length > 0 && Regex.IsMatch(en, @"^[a-zA-Z0-9\s.,!?'""()\-:;]+$")) text = en;
            else text = fa.Length > 0 ? fa : en;
        }
        catch { }

        host.BeginInvoke((MethodInvoker)delegate
        {
            try
            {
                if (text.Trim().Length > 0)
                {
                    Clipboard.SetText(text);
                    Thread.Sleep(100);
                    keybd_event(0x11, 0, 0, UIntPtr.Zero);   // Ctrl down
                    keybd_event(0x56, 0, 0, UIntPtr.Zero);   // V down
                    keybd_event(0x56, 0, 2, UIntPtr.Zero);   // V up
                    keybd_event(0x11, 0, 2, UIntPtr.Zero);   // Ctrl up
                }
            }
            catch { }
            if (text.Trim().Length == 0)
                tray.ShowBalloonTip(1500, AppName, "No speech recognized (or no internet).", ToolTipIcon.Info);
            busy = false;
            tray.Text = IdleText();
        });
    }

    static string Recognize(byte[] pcm, string lang)
    {
        try
        {
            string url = "https://www.google.com/speech-api/v2/recognize?output=json&lang=" + lang +
                         "&key=" + ApiKey + "&maxAlternatives=1&client=chromium";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.Timeout = 20000;
            req.ContentType = "audio/l16; rate=" + SampleRate + "; channels=1";
            req.ContentLength = pcm.Length;
            using (Stream s = req.GetRequestStream()) s.Write(pcm, 0, pcm.Length);

            string body;
            using (WebResponse r = req.GetResponse())
            using (StreamReader sr = new StreamReader(r.GetResponseStream(), System.Text.Encoding.UTF8))
                body = sr.ReadToEnd();

            string result = "";
            foreach (Match m in Regex.Matches(body, "\"transcript\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\""))
                result = Regex.Unescape(m.Groups[1].Value);
            return result;
        }
        catch { return ""; }
    }
}

// ---------- پنجره انتخاب کلید (برای Setup و Change hotkey) ----------
class KeyDialog : Form
{
    class CaptureBox : TextBox
    {
        protected override bool IsInputKey(Keys keyData) { return true; }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) { return false; }
    }

    CaptureBox box;
    Label modeLabel;
    CheckBox autoBox;
    public int VkCode;
    public bool AutoStart { get { return autoBox != null && autoBox.Checked; } }

    public KeyDialog(int vk, bool setup, bool autostart)
    {
        VkCode = vk;
        Text = setup ? "Voice Typer Setup" : "Change hotkey";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(390, setup ? 200 : 168);

        Label info = new Label();
        info.Text = "Click the box below, then press the key you want to use\n(F1-F24, Scroll Lock, Pause or Insert):";
        info.SetBounds(16, 14, 358, 36);
        Controls.Add(info);

        box = new CaptureBox();
        box.ReadOnly = true;
        box.BackColor = SystemColors.Window;
        box.TextAlign = HorizontalAlignment.Center;
        box.Font = new Font(Font.FontFamily, 12f, FontStyle.Bold);
        box.SetBounds(16, 56, 358, 28);
        box.KeyDown += OnKey;
        Controls.Add(box);

        modeLabel = new Label();
        modeLabel.SetBounds(16, 92, 358, 36);
        Controls.Add(modeLabel);

        int y = 134;
        if (setup)
        {
            autoBox = new CheckBox();
            autoBox.Text = "Start with Windows";
            autoBox.Checked = autostart;
            autoBox.SetBounds(16, 130, 358, 22);
            Controls.Add(autoBox);
            y = 162;
        }

        Button ok = new Button();
        ok.Text = setup ? "Install" : "Save";
        ok.DialogResult = DialogResult.OK;
        ok.SetBounds(194, y, 86, 28);
        Controls.Add(ok);

        Button cancel = new Button();
        cancel.Text = "Cancel";
        cancel.DialogResult = DialogResult.Cancel;
        cancel.SetBounds(288, y, 86, 28);
        Controls.Add(cancel);

        AcceptButton = ok;
        CancelButton = cancel;
        Refresh_(null);
    }

    static bool Allowed(int vk)
    {
        return (vk >= 0x70 && vk <= 0x87) || vk == Program.VK_SCROLL || vk == 0x13 || vk == 0x2D;
    }

    void OnKey(object s, KeyEventArgs e)
    {
        e.Handled = true;
        e.SuppressKeyPress = true;
        int vk = e.KeyValue;
        if (Allowed(vk))
        {
            VkCode = vk;
            Refresh_(null);
        }
        else Refresh_("Unsupported key. Please use F1-F24, Scroll Lock, Pause or Insert.");
    }

    void Refresh_(string warning)
    {
        box.Text = Program.KeyName(VkCode);
        modeLabel.Text = warning != null ? warning :
            (Program.IsToggleKey(VkCode)
                ? "Mode: Toggle - press once to start recording, press again to stop."
                : "Mode: Hold - hold the key to speak, release it to type.");
    }
}

// ---------- ضبط صدا با winmm (بدون نیاز به SoX) ----------
class Recorder
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct WAVEFORMATEX
    {
        public ushort wFormatTag, nChannels;
        public uint nSamplesPerSec, nAvgBytesPerSec;
        public ushort nBlockAlign, wBitsPerSample, cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WAVEHDR
    {
        public IntPtr lpData;
        public uint dwBufferLength, dwBytesRecorded;
        public IntPtr dwUser;
        public uint dwFlags, dwLoops;
        public IntPtr lpNext, reserved;
    }

    [DllImport("winmm.dll")] static extern int waveInOpen(out IntPtr h, uint dev, ref WAVEFORMATEX f, IntPtr cb, IntPtr inst, uint flags);
    [DllImport("winmm.dll")] static extern int waveInPrepareHeader(IntPtr h, IntPtr hdr, int size);
    [DllImport("winmm.dll")] static extern int waveInUnprepareHeader(IntPtr h, IntPtr hdr, int size);
    [DllImport("winmm.dll")] static extern int waveInAddBuffer(IntPtr h, IntPtr hdr, int size);
    [DllImport("winmm.dll")] static extern int waveInStart(IntPtr h);
    [DllImport("winmm.dll")] static extern int waveInStop(IntPtr h);
    [DllImport("winmm.dll")] static extern int waveInReset(IntPtr h);
    [DllImport("winmm.dll")] static extern int waveInClose(IntPtr h);

    const int Count = 6, BufSize = 3200; // 6 بافر × 100ms
    static readonly int HdrSize = Marshal.SizeOf(typeof(WAVEHDR));

    IntPtr dev;
    IntPtr[] hdrs = new IntPtr[Count];
    MemoryStream data;

    public bool Start()
    {
        WAVEFORMATEX f = new WAVEFORMATEX();
        f.wFormatTag = 1; f.nChannels = 1; f.nSamplesPerSec = 16000;
        f.wBitsPerSample = 16; f.nBlockAlign = 2; f.nAvgBytesPerSec = 32000;

        if (waveInOpen(out dev, 0xFFFFFFFF, ref f, IntPtr.Zero, IntPtr.Zero, 0) != 0) return false;
        data = new MemoryStream();

        for (int i = 0; i < Count; i++)
        {
            WAVEHDR h = new WAVEHDR();
            h.lpData = Marshal.AllocHGlobal(BufSize);
            h.dwBufferLength = BufSize;
            hdrs[i] = Marshal.AllocHGlobal(HdrSize);
            Marshal.StructureToPtr(h, hdrs[i], false);
            waveInPrepareHeader(dev, hdrs[i], HdrSize);
            waveInAddBuffer(dev, hdrs[i], HdrSize);
        }
        waveInStart(dev);
        return true;
    }

    public void Poll(bool rearm)
    {
        for (int i = 0; i < Count; i++)
        {
            if (hdrs[i] == IntPtr.Zero) continue;
            WAVEHDR h = (WAVEHDR)Marshal.PtrToStructure(hdrs[i], typeof(WAVEHDR));
            if ((h.dwFlags & 1) == 0 || (h.dwFlags & 2) == 0) continue; // WHDR_DONE و WHDR_PREPARED
            if (h.dwBytesRecorded > 0)
            {
                byte[] tmp = new byte[h.dwBytesRecorded];
                Marshal.Copy(h.lpData, tmp, 0, tmp.Length);
                data.Write(tmp, 0, tmp.Length);
            }
            if (rearm) waveInAddBuffer(dev, hdrs[i], HdrSize);
            else
            {
                h.dwFlags &= ~1u; // جلوگیری از خواندن مجدد
                Marshal.StructureToPtr(h, hdrs[i], false);
            }
        }
    }

    public byte[] Stop()
    {
        waveInStop(dev);
        waveInReset(dev);   // همه بافرها (حتی ناقص) برگردانده می‌شوند
        Poll(false);

        for (int i = 0; i < Count; i++)
        {
            WAVEHDR h = (WAVEHDR)Marshal.PtrToStructure(hdrs[i], typeof(WAVEHDR));
            waveInUnprepareHeader(dev, hdrs[i], HdrSize);
            Marshal.FreeHGlobal(h.lpData);
            Marshal.FreeHGlobal(hdrs[i]);
            hdrs[i] = IntPtr.Zero;
        }
        waveInClose(dev);
        return data.ToArray();
    }
}
