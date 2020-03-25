using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Subtitle_Printer
{
    class VerticalTabLineNumberMargin : LineNumberMargin
    {
        public IReadOnlyCollection<bool> VerticalTabs { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            TextView textView = this.TextView;
            Size renderSize = this.RenderSize;
            if (textView != null && textView.VisualLinesValid)
            {
                var foreground = (Brush)GetValue(Control.ForegroundProperty);
                var lineTexts = LineNumberCalc.Calc(textView.VisualLines.First().FirstDocumentLine.LineNumber, textView.VisualLines.Last().FirstDocumentLine.LineNumber, VerticalTabs);
                for (int i = 0; i < textView.VisualLines.Count; i++)
                {
                    var line = textView.VisualLines[i];
                    var lineText = lineTexts.ElementAt(i);
                    FormattedText text = CreateFormattedText(
                        this,
                        lineText,
                        typeface, emSize, foreground
                    );
                    double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);
                    drawingContext.DrawText(text, new Point(renderSize.Width - text.Width, y - textView.VerticalOffset));
                }
            }

            FormattedText CreateFormattedText(FrameworkElement element, string text, Typeface typeface, double? emSize, Brush foreground)
            {
                if (element == null)
                    throw new ArgumentNullException("element");
                if (text == null)
                    throw new ArgumentNullException("text");
                if (typeface == null)
                    typeface = element.CreateTypeface();
                if (emSize == null)
                    emSize = TextBlock.GetFontSize(element);
                if (foreground == null)
                    foreground = TextBlock.GetForeground(element);
#pragma warning disable CS0618 // 型またはメンバーが旧型式です
                return new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    emSize.Value,
                    foreground,
                    null,
                    TextOptions.GetTextFormattingMode(element)
                );
#pragma warning restore CS0618 // 型またはメンバーが旧型式です
            }
        }
    }

    internal static class ExtentionFromICSharpCode
    {
        public static Typeface CreateTypeface(this FrameworkElement fe)
        {
            return new Typeface((FontFamily)fe.GetValue(TextBlock.FontFamilyProperty),
                                (FontStyle)fe.GetValue(TextBlock.FontStyleProperty),
                                (FontWeight)fe.GetValue(TextBlock.FontWeightProperty),
                                (FontStretch)fe.GetValue(TextBlock.FontStretchProperty));
        }
    }
}
