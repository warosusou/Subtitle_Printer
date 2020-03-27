﻿using ICSharpCode.AvalonEdit.Document;
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
        private Timer linePrintTimer = new Timer { Interval = 200 };

        public MainWindow()
        {
            InitializeComponent();
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
            ImageFrame.MinWidth = ImageGrid.ActualWidth - 10;
            ImageFrame.MinHeight = ImageGrid.ActualHeight;
            ImageFrame.MaxWidth = ImageFrame.MinWidth;
            ImageFrame.MaxHeight = ImageFrame.MinHeight;
            LeftAlignmentRadioButton.IsChecked = true;
            SubtitleDrawer.ImageDrawer.PrintingFont = new Font("メイリオ", 20);
            SubtitleDrawer.ImageDrawer.pictureBox1 = new System.Drawing.Size((int)ImageGrid.ActualWidth, (int)ImageGrid.ActualHeight);
            SubtitleDrawer.ImageDrawer.EQSize = SubtitleDrawer.ImageDrawer.PrintingFont.Size;
            SubtitleDrawer.ImageDrawer.AutoShrink = true;

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
                IntPtr hbitmap = bmp.GetHbitmap();
                ImageFrame.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
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
                        return;
                    default:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private bool SaveTextFile()
        {
            var result = TextBox.SaveVerticalTabText(FileName);
            if (result != "")
            {
                FileName = result;
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
                    Bitmap result = SubtitleDrawer.ImageDrawer.LineBitmap(lines[currentline]);
                    string text = lines[currentline].Trim();
                    if (text.EndsWith(":") || text.EndsWith("：")) continue;
                    if (text.Contains("%")) text = text.Split('%')[0];
                    else if (text.Contains("％")) text = text.Split('％')[0];
                    if (text == "") continue;
                    if (result != null) result.Save(String.Format("Line{0}.bmp", lineIndexes.ElementAt(currentline)));
                }
            }
        }

        private void EditorFontButton_Click(object sender, RoutedEventArgs e)
        {
            FontDialog fd = new FontDialog();
            fd.Font = new Font(TextBox.FontFamily.Source, (float)TextBox.FontSize);
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBox.FontFamily = new System.Windows.Media.FontFamily(fd.Font.Name);
                TextBox.FontSize = fd.Font.Size;
            }
        }

        private void PrintingFontButton_Click(object sender, RoutedEventArgs e)
        {
            FontDialog fd = new FontDialog();
            fd.Font = new Font(TextBox.FontFamily.Source, (float)TextBox.FontSize);
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImageDrawer.PrintingFont = fd.Font;
                ShowSubtitle();
            }
        }

        private void EQSizeButton_Click(object sender, RoutedEventArgs e)
        {
            var f = new EQSizeWindow(ImageDrawer.pictureBox1, ImageDrawer.EQSize, ImageDrawer.AutoShrink);
            f.ShowDialog();
            if (f.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                ImageDrawer.EQSize = f.EQSize;
                ImageDrawer.AutoShrink = f.AutoShrink;
                ShowSubtitle();
            }
        }

        private void AlignmentRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (LeftAlignmentRadioButton.IsChecked.Value)
            {
                SubtitleDrawer.ImageDrawer.Alignment = Alignment.Left;
            }
            else if (CenterAlignmentRadioButton.IsChecked.Value)
            {
                SubtitleDrawer.ImageDrawer.Alignment = Alignment.Center;
            }
            else if (RightAlignmentRadioButton.IsChecked.Value)
            {
                SubtitleDrawer.ImageDrawer.Alignment = Alignment.Right;
            }
            ShowSubtitle();
        }

        private void ImageResolutionButton_Click(object sender, RoutedEventArgs e)
        {
            var r = new ResolutionWindow(new System.Windows.Size(ImageFrame.MaxWidth, ImageFrame.MaxHeight));
            r.ShowDialog();
            if (r.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                ImageFrame.MinWidth = r.size.Width;
                ImageFrame.MinHeight = r.size.Height;
                ImageFrame.MaxWidth = r.size.Width;
                ImageFrame.MaxHeight = r.size.Height;
            }
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
