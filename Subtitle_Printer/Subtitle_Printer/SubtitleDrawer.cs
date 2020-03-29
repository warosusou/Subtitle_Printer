using System;
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
            internal static Size ImageFrame;
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
                PointF pt;
                var strfmt = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
                SizeF size = new SizeF(ImageFrame.Width, ImageFrame.Height);
                Bitmap canvas;
                bool gotsize = false;
                while (true)
                {
                    //描画先とするImageオブジェクトを作成する
                    canvas = new Bitmap((int)size.Width, (int)size.Height);
                    //ImageオブジェクトのGraphicsオブジェクトを作成する
                    using (var g = Graphics.FromImage(canvas))
                    {
                        using (var fnt = new Font(PrintingFont.Name, PrintingFont.Size))
                        {
                            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                            //画像サイズを取得
                            var s = g.MeasureString(text, fnt);
                            if (s.Width == 0) break;
                            size.Width = s.Width;
                            pt = new PointF(0,(ImageFrame.Height - s.Height) / 2);
                            //文字列を位置pt、黒で表示
                            g.DrawString(text, fnt, Brushes.Black, pt, strfmt);
                            if (gotsize) break;
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
                var width = bm.Width * (int)(bm.Height / (ImageFrame.Height));
                var result = new Bitmap(width, (int)(ImageFrame.Height));
                using (var g = Graphics.FromImage(result))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bm, 0, 0, width, (int)(ImageFrame.Height));
                }
                return result;
            }

            internal static Bitmap LineBitmap(string text)
            {
                Bitmap result = null;
                var sections = new List<Section>();
                text = text.Trim();
                if (text.EndsWith(":") || text.EndsWith("：")) text = "";
                if (text.Contains("%")) text = text.Split('%')[0];
                else if (text.Contains("％")) text = text.Split('％')[0];
                if (text == "") { return result; }
                if (text.Contains(beginTag) && text.Length > text.IndexOf(beginTag) + beginTag.Length && text.Substring(text.IndexOf(beginTag) + beginTag.Length).Contains(endTag))
                {
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
                }
                else
                {
                    sections.Add(new Section(text));
                }
                sections.RemoveAll(x => x.Image == null || x.Image.Width == 0);
                foreach (var s in sections)
                {
                    if (s.Image.Height > ImageFrame.Height && ImageDrawer.AutoShrink) { s.ShrinkImage(); }
                }
                if (sections.Sum(x => x.Image.Width) == 0) { return result; }
                result = new Bitmap(ImageFrame.Width, ImageFrame.Height);
                using (Graphics g = Graphics.FromImage(result))
                {
                    int xpos = 0;
                    switch (Alignment)
                    {
                        case Alignment.Left:
                            xpos = 0;
                            break;
                        case Alignment.Center:
                            xpos = (ImageFrame.Width - sections.Sum(x => x.Image.Width)) / 2;
                            break;
                        case Alignment.Right:
                            xpos = ImageFrame.Width;
                            sections.Reverse();
                            break;
                    }
                    foreach (var s in sections)
                    {
                        if (Alignment == Alignment.Right)
                            xpos -= s.Image.Width;
                        var ypos = (result.Height - s.Image.Height) / 2;
                        g.DrawImage(s.Image, xpos, ypos);
                        if (Alignment != Alignment.Right)
                            xpos += s.Image.Width;
                    }
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
