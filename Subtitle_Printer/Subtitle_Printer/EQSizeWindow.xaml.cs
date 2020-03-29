using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Subtitle_Printer
{
    /// <summary>
    /// EQSizeWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EQSizeWindow : Window
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        public new DialogResult DialogResult { get { return result; } private set { result = value; this.Close(); } }
        private DialogResult result;
        private System.Drawing.Size picturebox1Size;
        readonly string eq = @"S_n = \sum_{k=1}^{n}f\left(t_k\right)\left(x_{k}-x_{k-1}\right)";
        public double EQSize { get; private set; }
        public bool AutoShrink { get; private set; }

        public EQSizeWindow(System.Drawing.Size pic1, double size, bool autoshrink)
        {
            InitializeComponent();
            picturebox1Size = pic1;
            EQSize = size;
            AutoShrink = autoshrink;
            CheckBox.IsChecked = autoshrink;
            pictureBox1.Stretch = Stretch.None;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            pictureBox1.MaxHeight = picturebox1Size.Height;
            pictureBox1.MaxWidth = picturebox1Size.Width;
            var bmp = SubtitleDrawer.ImageDrawer.TexPrinter(eq);
            if (bmp != null)
            {
                IntPtr hbitmap = bmp.GetHbitmap();
                pictureBox1.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(hbitmap);
            }
            else
            {
                pictureBox1.Source = null;
            }
            textBox1.Text = EQSize.ToString();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (Double.TryParse(textBox1.Text, out var size))
            {
                size++;
                EQSize = size;
                var bmp = SubtitleDrawer.ImageDrawer.TexPrinter(eq,EQSize);
                if (bmp != null)
                {
                    IntPtr hbitmap = bmp.GetHbitmap();
                    pictureBox1.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    DeleteObject(hbitmap);
                }
                else
                {
                    pictureBox1.Source = null;
                }
                textBox1.Text = EQSize.ToString();
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (Double.TryParse(textBox1.Text, out var size))
            {
                if (size == 1) return;
                size--;
                EQSize = size;
                var bmp = SubtitleDrawer.ImageDrawer.TexPrinter(eq, EQSize);
                if (bmp != null)
                {
                    IntPtr hbitmap = bmp.GetHbitmap();
                    pictureBox1.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    DeleteObject(hbitmap);
                }
                else
                {
                    pictureBox1.Source = null;
                }
                textBox1.Text = EQSize.ToString();
            }
        }

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (pictureBox1 == null) return;
            if (Double.TryParse(textBox1.Text, out var size))
            {
                EQSize = size;
                var bmp = SubtitleDrawer.ImageDrawer.TexPrinter(eq, EQSize);
                if (bmp != null)
                {
                    IntPtr hbitmap = bmp.GetHbitmap();
                    pictureBox1.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    DeleteObject(hbitmap);
                }
                else
                {
                    pictureBox1.Source = null;
                }
            }
            else if (textBox1.Text == "")
            {
                return;
            }
            else
            {
                textBox1.Text = EQSize.ToString();
                textBox1.SelectionStart = textBox1.Text.Length;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            AutoShrink = CheckBox.IsChecked.Value;
            DialogResult = DialogResult.OK;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
