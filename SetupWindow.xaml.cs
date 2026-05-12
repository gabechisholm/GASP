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
            // Save Settings
            settings.TargetHex = HexInput.Text;
            settings.Tolerance = (int)ToleranceSlider.Value;
            settings.CopyToClipboardByDefault = RadioClipboard.IsChecked == true;
            settings.Save();

            try
            {
                var installedPaths = InstallTool();
                if (installedPaths.Count > 0)
                {
                    MessageBox.Show("GASP Installed Successfully!\nRegistered in your Power BI External Tools.\n\nYou do NOT need to run this as Admin.", "Success");
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
            
            // Focus ONLY on User-level paths to avoid Admin prompts
            var targetPaths = new List<string> {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Power BI Desktop\External Tools"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.MicrosoftPowerBIDesktop_8wekyb3d8bbwe\LocalCache\Local\Microsoft\Power BI Desktop\External Tools")
            };

            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            string jsonContent = $@"{{
    ""version"": ""1.0"",
    ""name"": ""GASP"",
    ""description"": ""Gabe's Amazing Screenshot Program"",
    ""path"": ""{exePath.Replace("\\", "\\\\")}"",
    ""arguments"": ""%server% %database%""
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

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}
