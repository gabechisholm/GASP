using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Resources;

namespace PBIScreenshotter
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon _notifyIcon;
        private const int HOTKEY_ID = 9000;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeTrayIcon();

            if (e.Args.Length > 0)
            {
                new CaptureWindow().Show();
            }
            else
            {
                var hub = new SetupWindow();
                hub.Show();

                ComponentDispatcher.ThreadFilterMessage += (ref MSG msg, ref bool handled) => {
                    if (msg.message == 0x0312 && msg.wParam.ToInt32() == HOTKEY_ID)
                    {
                        new CaptureWindow().Show();
                        handled = true;
                    }
                };

                HotKeyManager.Register(hub, 0x53, HotKeyManager.MOD_ALT, HOTKEY_ID);
            }
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Text = "GASP - Screenshotter";
            
            try {
                // LOAD FROM EMBEDDED RESOURCE
                StreamResourceInfo sri = System.Windows.Application.GetResourceStream(new Uri("assets/baedc91c-cc61-4b6d-9d6c-76f0c747ae7f.png", UriKind.Relative));
                using (var stream = sri.Stream) {
                    var bitmap = (Bitmap)Image.FromStream(stream);
                    _notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
                }
            } catch {
                _notifyIcon.Icon = SystemIcons.Application;
            }

            _notifyIcon.Visible = true;
            _notifyIcon.DoubleClick += (s, e) => ShowHub();

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Open GASP Setup", null, (s, e) => ShowHub());
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => System.Windows.Application.Current.Shutdown());
        }

        private void ShowHub()
        {
            foreach (Window win in System.Windows.Application.Current.Windows)
            {
                if (win is SetupWindow)
                {
                    win.Show();
                    win.WindowState = WindowState.Normal;
                    win.Activate();
                    return;
                }
            }
            new SetupWindow().Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_notifyIcon != null) _notifyIcon.Dispose();
            base.OnExit(e);
        }
    }
}
