using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Security.Principal;
using System.Collections.Generic;

namespace PBIScreenshotter
{
    public partial class SetupWindow : Window
    {
        private AppSettings settings;

        public SetupWindow()
        {
            InitializeComponent();
            settings = AppSettings.Load();
            HexInput.Text = settings.TargetHex;
            ToleranceSlider.Value = settings.Tolerance;
            ToleranceText.Text = settings.Tolerance + "%";
            
            if (settings.CopyToClipboardByDefault) RadioClipboard.IsChecked = true;
            else RadioSave.IsChecked = true;

            try {
                this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/assets/baedc91c-cc61-4b6d-9d6c-76f0c747ae7f.png"));
            } catch { }
        }

        private void ToleranceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ToleranceText != null)
                ToleranceText.Text = (int)e.NewValue + "%";
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            settings.TargetHex = HexInput.Text;
            settings.Tolerance = (int)ToleranceSlider.Value;
            settings.CopyToClipboardByDefault = RadioClipboard.IsChecked == true;
            settings.Save();

            try
            {
                var installedPaths = InstallTool();
                if (installedPaths.Count > 0)
                {
                    MessageBox.Show("GASP Installed Successfully!\nRegistered in your Power BI External Tools.", "Success");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Installation failed: " + ex.Message);
            }
        }

        private List<string> InstallTool()
        {
            var results = new List<string>();
            var targetPaths = new List<string> {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Power BI Desktop\External Tools"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.MicrosoftPowerBIDesktop_8wekyb3d8bbwe\LocalCache\Local\Microsoft\Power BI Desktop\External Tools")
            };

            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            
            // Base64 Icon Data for Power BI Ribbon
            string iconBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAMMSURBVFhH7ZbfS1NhGMdNveumLu0i0Ipwy2yZDqVpQdZFIygvzAuLSIggEIqg3wUZSFpWdiH0Q6MflJJuYVOb0xRtWeEf4Ar6YWqQm9bmZnaeb7znnB3P9nY2t6JudvHh7Dx73vf9vM/7nvOeJCLC/yQpPPCvSQgkBCIKuD0ejI2Py0yITLrdXF4Q9p+YOzbfxu32cHlqNAVsXd1Yoc9BeqYB6br1yJDJyt0IEvh8BvtPnct+r1yzAbZOO5cbRFOguvYKso2FokinvUfGgYEXL7ncIIPOITEnmM/aZhuLxL7Cc4NEELgK05btXFyTSSdowgb6OhASNxWb4xcoLI5BoGMZyLII9HRpSLzwjwRiqUBHmiTQviQkzqoYl0Bt3XVs3raDiyt8sYM+NIFGW6R7JtDGC7A+aurq+fYymgJ+vz/yI+TQgyzJECwpIBJAnWkgazInwPqY8fv59jKaAjyCCgJ166SSW1UCv1mCaCxYQBjcCrKkgp4sBv3wghw6acbWVJUAX4FoLFiA+vKlNWaznPWoBOQKaGzCaMQmwAZgs2QC4UvABLqWg4b38W0joCkwNTWNj59G52PhAuImZPcpIEGA4DTj/cgwTp27gONnzuPk2SqMuN5y/YajKXDxcj02qR/DvgJ5CdQCyaChEvH13HCjEaXl+5VzgLGztBwNNxvh6O3n+o8qIL0JzfOx/gJ5xsE9oBf3gPDZAp0hXzq0Mg04cOgwmu4+QOXRE4oIO5B8vhlujKgCIWfBWBvIVQN6dw0kzIFGH4FctRC+u8QB0nUGHDl2Gs2PLTDvKsPtO/dRVX1JOR2np79xY8QmoIEgCKIAG6Tneb90JGcaxJhz6LVSmbgE1uaa8LClVZxVc6tVvHY8c/AC+hxR4FbTPVQcrESGzoCSsr1otbQryxCzANs4q7PzRHvWYXAm64xFIR8k6gqws9/WZcerN8Po7u1DnqlY+TCJWYARmJ2F1+uD1+dTroFAgMvbvacCq7JyNciDuaQMc3M/uXZRBWKBVUKL8Fw1f00gXhICCYFftdSYQ0x3F4YAAAAASUVORK5CYII=";

            string jsonContent = $@"{{
    ""version"": ""1.0"",
    ""name"": ""GASP"",
    ""description"": ""Gabe's Amazing Screenshot Program"",
    ""path"": ""{exePath.Replace("\\", "\\\\")}"",
    ""arguments"": ""%server% %database%"",
    ""iconData"": ""data:image/png;base64,{iconBase64}""
}}";

            foreach (var dir in targetPaths)
            {
                try {
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(Path.Combine(dir, "gasp.pbitool.json"), jsonContent);
                    results.Add(dir);
                } catch { }
            }
            return results;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized) this.Hide();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}
