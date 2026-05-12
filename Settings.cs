using System;
using System.IO;
using System.Text.Json;
using System.Drawing;

namespace PBIScreenshotter
{
    public class AppSettings
    {
        public string TargetHex { get; set; } = "#C8C8C8";
        public int Tolerance { get; set; } = 0;
        public bool CopyToClipboardByDefault { get; set; } = false;

        private static string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GASP", "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json);
                }
            }
            catch { }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string json = JsonSerializer.Serialize(this);
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }

        public Color GetColor()
        {
            try {
                return ColorTranslator.FromHtml(TargetHex);
            } catch { return Color.FromArgb(200, 200, 200); }
        }
    }
}
