using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using static Subtitle_Printer.SubtitleDrawer;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Subtitle_Printer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public string FileName { get { return filename; } private set { filename = value; this.Title = FileName; } }
        private string filename;
        private Config config;
        private const string configPath = "config.json";
        private Timer linePrintTimer = new Timer { Interval = 50 };

        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists(configPath))
            {
                config = Config.LoadConfig(configPath);
                config.ConfigAutoSave = true;
                config.ConfigSavePath = configPath;
            }
            else
            {
                config = new Config()
                {
                    EditorFont = TextBox.FontFamily,
                    EditorFontSize = TextBox.FontSize,
                    PrintingFont = new Font("メイリオ", 20),
                    EQSize = 30,
                    AutoShrink = true,
                    Alignment = Alignment.Left,
                    ImageResolution = new SizeF(),
                    ConfigAutoSave = true,
                    ConfigSavePath = configPath
                };
            }

            TextBoxLineNumber.TextView = TextBox.TextArea.TextView;
            TextBoxLineNumber.VerticalTabs = TextBox.VerticalTabs;
            FileName = "NoTitle.txt";
            using (var reader = new XmlTextReader("ProjectZTexDef.xshd"))
            {
                TextBox.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MinWidth = this.ActualWidth;
            this.MinHeight = 230;
            if (config.ImageResolution == new SizeF())
            {
                config.ImageResolution = new SizeF((float)ImageGrid.ActualWidth, (float)ImageGrid.ActualHeight);
                Config.SaveConfig(configPath, config);
            }

            TextBox.FontFamily = config.EditorFont;
            TextBox.FontSize = config.EditorFontSize;
            ImageDrawer.PrintingFont = config.PrintingFont;
            ImageDrawer.EQSize = config.EQSize;
            ImageDrawer.AutoShrink = config.AutoShrink;
            ImageDrawer.Alignment = config.Alignment;
            ImageDrawer.ImageFrame = new System.Drawing.Size((int)config.ImageResolution.Width, (int)config.ImageResolution.Height);
            SetImageFrameSize(config.ImageResolution.Width, config.ImageResolution.Height);
            SetAlignmentRadioButton(config.Alignment);
            statusBarLabel.Content = "";

            linePrintTimer.Tick += (s, et) =>
            {
                linePrintTimer.Enabled = false;
                ShowSubtitle();
            };
        }

        private void ShowSubtitle()
        {
            var bmp = SubtitleDrawer.ImageDrawer.LineBitmap(TextBox.Document.GetText(TextBox.Document.GetLineByOffset(TextBox.CaretOffset)));
            if (bmp != null)
            {
                ImageFrame.Source = null;
                IntPtr hbitmap = bmp.GetHbitmap();
                var s = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                ImageFrame.Source = s;
                DeleteObject(hbitmap);
            }
            else
            {
                ImageFrame.Source = null;
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSubtitles();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var caret = TextBox.CaretOffset;
                TextBox.Text = TextBox.Text.Insert(TextBox.CaretOffset, "$$");
                TextBox.CaretOffset = caret + 1;
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SaveTextFile();
                e.Handled = true;
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                linePrintTimer.Enabled = false;
                linePrintTimer.Enabled = true;
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            linePrintTimer.Enabled = false;
            linePrintTimer.Enabled = true;
        }
        
        private void TextBox_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            linePrintTimer.Enabled = false;
            linePrintTimer.Enabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (TextBox.IsModified)
            {
                var u = new UnsavedWarningWindow(FileName);
                u.ShowDialog();
                switch (u.DialogResult)
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        if (!SaveTextFile())
                            e.Cancel = true;
                        break;
                    case System.Windows.Forms.DialogResult.No:
                        break;
                    default:
                        e.Cancel = true;
                        break;
                }
                Config.SaveConfig(configPath, config);
            }
        }

        private bool SaveTextFile()
        {
            var result = TextBox.SaveVerticalTabText(FileName);
            if (result != "")
            {
                FileName = result;
                SetStatusBarLabelContent(String.Format("{0}を保存しました", result));
                return true;
            }
            else
            {
                return false;
            }
        }

        private void LoadTextFile()
        {
            FileName = TextBox.LoadVerticalTabText();
            SetStatusBarLabelContent(String.Format("{0}を読み込みました", FileName));
        }

        private void SaveSubtitles()
        {
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            List<string> files = new List<string>();
            Regex r = new Regex(@"Line\d*\.bmp");
            foreach (var f in di.GetFiles("*.bmp"))
            {
                if (r.IsMatch(f.Name))
                {
                    new FileInfo(f.FullName).Delete();
                }
            }
            var lines = TextBox.Text.Replace("\n", "").Split('\r');
            var lineIndexes = LineNumberCalc.Calc(1, lines.Count(), TextBox.VerticalTabs).ToArray();
            for (int currentline = 0; currentline < lines.Count(); currentline++)
            {
                if (lines[currentline].Length != 0)
                {
                    Bitmap result = ImageDrawer.LineBitmap(lines[currentline]);
                    string text = lines[currentline].Trim();
                    if (text.EndsWith(":") || text.EndsWith("：")) continue;
                    if (text.Contains("%")) text = text.Split('%')[0];
                    else if (text.Contains("％")) text = text.Split('％')[0];
                    if (text == "") continue;
                    if (result != null) result.Save(String.Format("Line{0}.bmp", lineIndexes.ElementAt(currentline)));
                }
            }
            SetStatusBarLabelContent(String.Format("字幕を保存しました"));
        }

        private void EditorFontButton_Click(object sender, RoutedEventArgs e)
        {
            FontDialog fd = new FontDialog();
            fd.Font = new Font(TextBox.FontFamily.Source, (float)TextBox.FontSize);
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBox.FontFamily = new System.Windows.Media.FontFamily(fd.Font.Name);
                TextBox.FontSize = fd.Font.Size;
                config.EditorFont = TextBox.FontFamily;
                config.EditorFontSize = TextBox.FontSize;
            }
        }

        private void PrintingFontButton_Click(object sender, RoutedEventArgs e)
        {
            FontDialog fd = new FontDialog();
            fd.Font = new Font(ImageDrawer.PrintingFont.Name, ImageDrawer.PrintingFont.Size);
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImageDrawer.PrintingFont = fd.Font;
                ShowSubtitle();
            }
        }

        private void EQSizeButton_Click(object sender, RoutedEventArgs e)
        {
            var f = new EQSizeWindow(ImageDrawer.ImageFrame, ImageDrawer.EQSize, ImageDrawer.AutoShrink);
            f.ShowDialog();
            if (f.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                ImageDrawer.EQSize = f.EQSize;
                ImageDrawer.AutoShrink = f.AutoShrink;
                config.EQSize = f.EQSize;
                config.AutoShrink = f.AutoShrink;
                ShowSubtitle();
            }
        }

        private void AlignmentRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (LeftAlignmentRadioButton.IsChecked.Value)
            {
                ImageDrawer.Alignment = Alignment.Left;
            }
            else if (CenterAlignmentRadioButton.IsChecked.Value)
            {
                ImageDrawer.Alignment = Alignment.Center;
            }
            else if (RightAlignmentRadioButton.IsChecked.Value)
            {
                ImageDrawer.Alignment = Alignment.Right;
            }
            ShowSubtitle();
        }

        private void ImageResolutionButton_Click(object sender, RoutedEventArgs e)
        {
            var r = new ResolutionWindow(new System.Windows.Size(ImageFrame.MaxWidth, ImageFrame.MaxHeight));
            r.ShowDialog();
            if (r.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                SetImageFrameSize(r.size.Width, r.size.Height);
                ImageDrawer.ImageFrame = new System.Drawing.Size((int)r.size.Width, (int)r.size.Height);
                config.ImageResolution = new SizeF((float)r.size.Width, (float)r.size.Height);
                ShowSubtitle();
            }
        }

        private void SetImageFrameSize(double width, double height)
        {
            ImageFieldGrid.MinHeight = height;
            ImageFieldGrid.MaxHeight = height;
            ImageGrid.MinWidth = width;
            ImageGrid.MinHeight = height;
            ImageGrid.MaxWidth = width;
            ImageGrid.MaxHeight = height;
            ImageFrame.MinWidth = width;
            ImageFrame.MinHeight = height;
            ImageFrame.MaxWidth = width;
            ImageFrame.MaxHeight = height;
            
            var thickness = MainGrid.Margin;
            thickness.Bottom = ImageFieldGrid.MaxHeight + statusBar.ActualHeight;
            MainGrid.Margin = thickness;
        }

        private void SetAlignmentRadioButton(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Left:
                    LeftAlignmentRadioButton.IsChecked = true;
                    break;
                case Alignment.Center:
                    CenterAlignmentRadioButton.IsChecked = true;
                    break;
                case Alignment.Right:
                    RightAlignmentRadioButton.IsChecked = true;
                    break;
            }
        }

        private void SetStatusBarLabelContent(string content)
        {
            statusBarLabel.Content = content;
            var t = new Timer { Interval = 5000, Enabled = true };
            t.Tick += (s, e) => { t.Enabled = false; statusBarLabel.Content = ""; };
        }

        private void LoadTextFileButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTextFile();
        }

        private void SaveTextFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveTextFile();
        }
    }
}
