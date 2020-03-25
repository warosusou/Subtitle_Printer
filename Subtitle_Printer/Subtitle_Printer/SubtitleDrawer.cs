﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfMath;
using WpfMath.Exceptions;

namespace Subtitle_Printer
{
    static class SubtitleDrawer
    {
        private static string beginTag = "$";
        private static string endTag = "$";
        internal static class ImageDrawer
        {
            internal static Font PrintingFont;
            internal static Size pictureBox1;
            internal static Alignment Alignment;
            internal static double EQSize;
            internal static bool AutoShrink;

            internal static Bitmap TexPrinter(string latex)
            {
                return TexPrinter(latex, EQSize);
            }

            internal static Bitmap TexPrinter(string latex, double scale)
            {
                try
                {
                    latex = latex.Trim();
                    Bitmap bitmap = null;

                    var parser = new TexFormulaParser();
                    var formula = parser.Parse(latex);
                    formula.TextStyle = "{StaticResource ClearTypeFormula}";
                    var renderer = formula.GetRenderer(TexStyle.Display, scale, "Arial");
                    if (renderer.RenderSize.Width == 0 || renderer.RenderSize.Height == 0) { return bitmap; }
                    var bitmapsourse = renderer.RenderToBitmap(0, 0);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapsourse));
                    using (var ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        using (var temp = new Bitmap(ms))
                        {
                            bitmap = new Bitmap(temp);
                        }
                    }
                    return bitmap;
                }
                catch (Exception e) when (e is TexParseException || e is TexCharacterMappingNotFoundException)
                {
                    return Graphicer("!!TexError!!");
                }
            }

            internal static Bitmap Graphicer(string text)
            {
                string temp = text.Trim();
                if (temp == "") return null;
                PointF pt = new PointF(0, pictureBox1.Height / 2);
                var strfmt = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                switch (Alignment)
                {
                    case Alignment.Left:
                        strfmt.Alignment = StringAlignment.Near;
                        break;
                    case Alignment.Center:
                        strfmt.Alignment = StringAlignment.Center;
                        pt = new PointF(pictureBox1.Width / 2, pictureBox1.Height / 2);
                        break;
                    case Alignment.Right:
                        strfmt.Alignment = StringAlignment.Far;
                        pt = new PointF(pictureBox1.Width, pictureBox1.Height / 2);
                        break;
                }
                SizeF size = new SizeF(pictureBox1.Width, pictureBox1.Height);
                Bitmap canvas;
                bool gotsize = false;
                while (true)
                {
                    //描画先とするImageオブジェクトを作成する
                    canvas = new Bitmap((int)size.Width, pictureBox1.Height);
                    //ImageオブジェクトのGraphicsオブジェクトを作成する
                    using (var g = Graphics.FromImage(canvas))
                    {
                        using (var fnt = new Font(PrintingFont.Name, PrintingFont.Size))
                        {
                            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                            //文字列を位置pt、黒で表示
                            g.DrawString(text, fnt, Brushes.Black, pt, strfmt);
                            if (gotsize) break;
                            //画像サイズを取得
                            size = g.MeasureString(text, fnt);
                            if (size.Width == 0) break;
                            gotsize = true;
                            canvas.Dispose();
                        }
                    }
                }
                strfmt.Dispose();
                return canvas;
            }

            internal static Bitmap Shrink(Bitmap bm)
            {
                Bitmap result = null;
                var width = bm.Width * (int)(bm.Height / (pictureBox1.Height * 0.95));
                result = new Bitmap(width, (int)(pictureBox1.Height * 0.95));
                using (var g = Graphics.FromImage(result))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bm, 0, 0, width, (int)(pictureBox1.Height * 0.95));
                }
                return result;
            }

            internal static Bitmap LineBitmap(string text)
            {
                Bitmap result = null;
                text = text.Trim();
                if (text.EndsWith(":") || text.EndsWith("：")) text = "";
                if (text.Contains("%")) text = text.Split('%')[0];
                else if (text.Contains("％")) text = text.Split('％')[0];
                if (text == "") { return result; }
                if (text.Contains(beginTag) && text.Length > text.IndexOf(beginTag) + beginTag.Length && text.Substring(text.IndexOf(beginTag) + beginTag.Length).Contains(endTag))
                {
                    var sections = new List<Section>();
                    int result_width = 0;
                    int result_height = ImageDrawer.pictureBox1.Height;
                    while (text.Length != 0)
                    {
                        string section = "";
                        int begintagpos = text.IndexOf(beginTag);
                        if (begintagpos > 0)
                        {
                            section = text.Substring(0, begintagpos);
                            text = text.Substring(begintagpos);
                        }
                        else if (begintagpos == 0 && text.IndexOf(endTag, begintagpos + 1) > begintagpos)
                        {
                            section = text.Substring(0, text.IndexOf(endTag, begintagpos + 1) + endTag.Length);
                            if (text.IndexOf(endTag, begintagpos + 1) + endTag.Length < text.Length)
                            {
                                text = text.Substring(text.IndexOf(endTag, begintagpos + 1) + endTag.Length);
                            }
                            else
                            {
                                text = "";
                            }
                        }
                        else
                        {
                            section = text;
                            text = "";
                        }
                        if (section == "") { continue; }
                        sections.Add(new Section(section));
                    }
                    sections.RemoveAll(x => x.Image == null || x.Image.Width == 0);
                    foreach (var s in sections)
                    {
                        if (s.Image.Height > result_height && ImageDrawer.AutoShrink) { s.ShrinkImage(); }
                        result_width += s.Image.Width;
                    }
                    if (result_width == 0) { return result; }
                    result = new Bitmap(result_width, result_height);
                    using (Graphics g = Graphics.FromImage(result))
                    {
                        int xpos = 0;
                        foreach (var s in sections)
                        {
                            var ypos = (result.Height - s.Image.Height) / 2;
                            g.DrawImage(s.Image, xpos, ypos);
                            xpos += s.Image.Width;
                        }
                    }
                }
                else
                {
                    result = ImageDrawer.Graphicer(text);
                }
                return result;
            }
        }
        class Section
        {
            private string text;
            public Bitmap Image { get; private set; }
            public string Text
            {
                get
                {
                    return text;
                }
                set
                {
                    text = value;
                    if (text.Length >= beginTag.Length + endTag.Length && text.IndexOf(beginTag) == 0 && text.IndexOf(endTag, text.IndexOf(beginTag) + beginTag.Length) == text.Length - 1)
                    {
                        text = text.Remove(0, beginTag.Length).Remove(text.Length - 1 - endTag.Length, endTag.Length);
                        Image = ImageDrawer.TexPrinter(text);
                    }
                    else
                    {
                        Image = ImageDrawer.Graphicer(text);
                    }
                }
            }
            public Section(string text)
            {
                this.text = "";
                this.Image = null;
                Text = text;
            }

            public void ShrinkImage()
            {
                Image = ImageDrawer.Shrink(Image);
            }
        }
        internal enum Alignment
        {
            Left, Center, Right
        }
    }
}
