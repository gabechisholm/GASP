using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;

namespace PBIScreenshotter
{
    public partial class SaveDialog : Window
    {
        private Bitmap capturedBitmap;

        public SaveDialog(Bitmap bmp)
        {
            InitializeComponent();
            SetIcon();
            capturedBitmap = bmp;
            PreviewImage.Source = BitmapToImageSource(bmp);

            this.Loaded += (s, e) => {
                this.Dispatcher.BeginInvoke(new Action(() => {
                    Save_Click(null, null);
                }));
            };
        }

        private void SetIcon()
        {
            try {
                this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/assets/baedc91c-cc61-4b6d-9d6c-76f0c747ae7f.png"));
            } catch { }
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            try {
                Clipboard.SetImage(BitmapToImageSource(capturedBitmap));
                MessageBox.Show("Image copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) { MessageBox.Show("Failed to copy: " + ex.Message); }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            string ext = "png";
            if (FormatSelector.SelectedItem != null) {
                ext = (FormatSelector.SelectedItem as System.Windows.Controls.ComboBoxItem).Content.ToString().ToLower();
            }

            sfd.Filter = $"{ext.ToUpper()} Image|*.{ext}";
            sfd.FileName = "PowerBI_Visual_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    if (ext == "svg") {
                        SaveAsSvg(sfd.FileName, capturedBitmap);
                    } else {
                        ImageFormat format = ImageFormat.Png;
                        if (ext == "jpg") format = ImageFormat.Jpeg;
                        if (ext == "bmp") format = ImageFormat.Bmp;
                        capturedBitmap.Save(sfd.FileName, format);
                    }
                    this.Close();
                }
                catch (Exception ex) { MessageBox.Show("Failed to save: " + ex.Message); }
            }
        }

        private void SaveAsSvg(string filePath, Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                string base64 = Convert.ToBase64String(ms.ToArray());
                string svg = $@"<svg xmlns=""http://schemas.microsoft.com/2000/svg"" viewBox=""0 0 {bitmap.Width} {bitmap.Height}"">
  <image width=""{bitmap.Width}"" height=""{bitmap.Height}"" href=""data:image/png;base64,{base64}"" />
</svg>";
                File.WriteAllText(filePath, svg);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
