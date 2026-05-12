using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Drawing;
using System.Windows.Threading;

namespace PBIScreenshotter
{
    public partial class CaptureWindow : Window
    {
        private AppSettings settings = AppSettings.Load();

        public CaptureWindow()
        {
            InitializeComponent();
            SetIcon();
            this.Loaded += Window_Loaded;
        }

        private void SetIcon()
        {
            try {
                this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/assets/baedc91c-cc61-4b6d-9d6c-76f0c747ae7f.png"));
            } catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var allVisuals = VisualDetector.GetAllVisuals();
            foreach (var v in allVisuals)
            {
                var debugBorder = new Border {
                    BorderBrush = System.Windows.Media.Brushes.Red,
                    BorderThickness = new Thickness(1),
                    Width = v.Bounds.Width, Height = v.Bounds.Height,
                    Opacity = 0.2, IsHitTestVisible = false
                };
                Canvas.SetLeft(debugBorder, v.Bounds.X);
                Canvas.SetTop(debugBorder, v.Bounds.Y);
                DetectionBordersCanvas.Children.Add(debugBorder);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            ActionTooltip.Visibility = Visibility.Visible;
            Canvas.SetLeft(ActionTooltip, pos.X + 15);
            Canvas.SetTop(ActionTooltip, pos.Y + 15);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(this);
            var screenPoint = this.PointToScreen(mousePos);

            Rect targetBounds = VisualDetector.SeekVisualEdges(screenPoint);

            HighlightBorder.Visibility = Visibility.Visible;
            HighlightBorder.Width = 2;
            HighlightBorder.Height = 2;
            Canvas.SetLeft(HighlightBorder, mousePos.X);
            Canvas.SetTop(HighlightBorder, mousePos.Y);

            var duration = TimeSpan.FromMilliseconds(400);
            var ease = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6 };

            var wAnim = new DoubleAnimation(targetBounds.Width, duration) { EasingFunction = ease };
            var hAnim = new DoubleAnimation(targetBounds.Height, duration) { EasingFunction = ease };
            var lAnim = new DoubleAnimation(targetBounds.X, duration) { EasingFunction = ease };
            var tAnim = new DoubleAnimation(targetBounds.Y, duration) { EasingFunction = ease };

            lAnim.Completed += (s, ev) => CaptureRegion(targetBounds);

            HighlightBorder.BeginAnimation(WidthProperty, wAnim);
            HighlightBorder.BeginAnimation(HeightProperty, hAnim);
            HighlightBorder.BeginAnimation(Canvas.LeftProperty, lAnim);
            HighlightBorder.BeginAnimation(Canvas.TopProperty, tAnim);
        }

        private void CaptureRegion(Rect bounds)
        {
            Dispatcher.BeginInvoke(new Action(async () => {
                await System.Threading.Tasks.Task.Delay(100);
                this.Hide();
                await System.Threading.Tasks.Task.Delay(200);

                try {
                    using (Bitmap bmp = new Bitmap((int)bounds.Width, (int)bounds.Height))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.CopyFromScreen((int)bounds.X, (int)bounds.Y, 0, 0, bmp.Size);
                        }

                        if (settings.CopyToClipboardByDefault)
                        {
                            System.Windows.Clipboard.SetImage(BitmapToImageSource(bmp));
                        }
                        else
                        {
                            var saveDialog = new SaveDialog(bmp);
                            saveDialog.ShowDialog();
                        }
                    }
                } catch (Exception ex) {
                    MessageBox.Show("Capture failed: " + ex.Message);
                }
                this.Close();
            }), DispatcherPriority.Background);
        }

        private System.Windows.Media.Imaging.BitmapSource BitmapToImageSource(Bitmap bitmap)
        {
            var handle = bitmap.GetHbitmap();
            try {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            } finally { DeleteObject(handle); }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }
    }
}
