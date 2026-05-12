using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace PBIScreenshotter
{
    public class VisualInfo
    {
        public Rect Bounds { get; set; }
        public string Name { get; set; }
    }

    public class VisualDetector
    {
        private static AppSettings settings = AppSettings.Load();

        public static Rect SeekVisualEdges(System.Windows.Point clickPoint)
        {
            // Refresh settings on each seek to ensure we use latest saved config
            settings = AppSettings.Load();

            int startX = (int)clickPoint.X;
            int startY = (int)clickPoint.Y;
            int screenW = (int)SystemParameters.PrimaryScreenWidth;
            int screenH = (int)SystemParameters.PrimaryScreenHeight;

            using (Bitmap screen = new Bitmap(screenW, screenH))
            {
                using (Graphics g = Graphics.FromImage(screen))
                {
                    g.CopyFromScreen(0, 0, 0, 0, screen.Size);
                }

                int top = startY;
                for (int y = startY; y > 0; y--) {
                    if (IsSelectionLine(screen, startX, y, true)) { top = y; break; }
                }

                int bottom = startY;
                for (int y = startY; y < screenH - 1; y++) {
                    if (IsSelectionLine(screen, startX, y, true)) { bottom = y; break; }
                }

                int left = startX;
                for (int x = startX; x > 0; x--) {
                    if (IsSelectionLine(screen, x, startY, false)) { left = x; break; }
                }

                int right = startX;
                for (int x = startX; x < screenW - 1; x++) {
                    if (IsSelectionLine(screen, x, startY, false)) { right = x; break; }
                }

                if (right <= left || bottom <= top) return new Rect(startX - 150, startY - 150, 300, 300);
                return new Rect(left, top, right - left, bottom - top);
            }
        }

        private static bool IsSelectionLine(Bitmap bmp, int x, int y, bool horizontalScan)
        {
            int count = 0;
            int checkRange = 2; 
            for (int i = -checkRange; i <= checkRange; i++)
            {
                int cx = horizontalScan ? x + i : x;
                int cy = horizontalScan ? y : y + i;
                if (cx < 0 || cx >= bmp.Width || cy < 0 || cy >= bmp.Height) continue;
                if (IsSelectionColor(bmp.GetPixel(cx, cy))) count++;
            }
            return count >= 3; 
        }

        private static bool IsSelectionColor(System.Drawing.Color c)
        {
            var target = settings.GetColor();
            // Tolerance is 0-50, we use it as absolute diff per channel
            return Math.Abs(c.R - target.R) <= settings.Tolerance && 
                   Math.Abs(c.G - target.G) <= settings.Tolerance && 
                   Math.Abs(c.B - target.B) <= settings.Tolerance;
        }

        public static List<VisualInfo> GetAllVisuals()
        {
            var results = new List<VisualInfo>();
            try {
                var processes = Process.GetProcessesByName("PBIDesktop");
                foreach (var process in processes) {
                    var root = AutomationElement.FromHandle(process.MainWindowHandle);
                    var visuals = root.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, "VisualContainer"));
                    foreach (AutomationElement v in visuals) {
                        var r = v.Current.BoundingRectangle;
                        if (r.Width > 50 && r.Width < SystemParameters.PrimaryScreenWidth * 0.8)
                            results.Add(new VisualInfo { Bounds = r, Name = v.Current.Name });
                    }
                }
            } catch {}
            return results;
        }
    }
}
