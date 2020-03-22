using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Util = Subtitle_Printer.RichTextBoxUtil;

namespace Subtitle_Printer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainView view;
        public MainWindow()
        {
            InitializeComponent();
            view = new MainView();
            this.DataContext = view;
            TextBox_VerticalTabsChanged(TextBox, new VerticalTabsChangedEventArgs(false, new List<int> { 0 }));
            LineNumberTextBox.Width = new FormattedText(
                "0123456789",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(
                    LineNumberTextBox.FontFamily,
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal
                    ),
                LineNumberTextBox.FontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip
                ).Width;
        }

        private void TextBox_VerticalTabsChanged(object sender, VerticalTabsChangedEventArgs e)
        {
            int paragraphNum = 1;
            int inlineNum = 0;
            var paragraph = new Paragraph();
            var tabs = TextBox.VerticalTabs;
            var fd = new FlowDocument();
            for (int i = 0; ; i++)
            {
                if (tabs[i] == true)
                {
                    if (paragraph.Inlines.Count != 0)
                        paragraph.Inlines.Add(new LineBreak());
                    paragraph.Inlines.Add(new Run(String.Format("{0}-{1}", paragraphNum, alphabetCalc(inlineNum))));
                    inlineNum++;
                }
                else
                {
                    if (i > 0 && tabs[i - 1] == true)
                    {
                        if (paragraph.Inlines.Count != 0)
                            paragraph.Inlines.Add(new LineBreak());
                        paragraph.Inlines.Add(new Run(String.Format("{0}-{1}", paragraphNum, alphabetCalc(inlineNum))));
                        fd.Blocks.Add(paragraph);
                        paragraph = new Paragraph();
                        inlineNum = 0;
                        paragraphNum++;
                    }
                    else
                    {
                        paragraph = new Paragraph(new Run(String.Format("{0}", paragraphNum)));
                        fd.Blocks.Add(paragraph);
                        paragraph = new Paragraph();
                        inlineNum = 0;
                        paragraphNum++;
                    }
                }
                if (i >= tabs.Count - 1)
                    break;
            }
            LineNumberTextBox.Document = fd;

            string alphabetCalc(int num)
            {
                string result = "";
                //A == 0, Z == 25
                while (true)
                {
                    char c = (char)((num % 26) + 'A');
                    result = result.Insert(0, c.ToString());
                    num = num / 26 - 1;
                    if (num == -1) break;
                }
                return result;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var run = TextBox.CaretPosition.Parent as Run;
            if (TextBox.Selection.Text == "\r\n")
            {
                run = TextBox.CaretPosition.GetNextInsertionPosition(LogicalDirection.Backward).Parent as Run;
                TextBox.CaretPosition = run.ElementEnd;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.T)
            {
                var caretPos = TextBox.CaretPosition;
                var symbol = view.MathSymbol.ToString();
                var run = caretPos.Parent as Run;
                var newRunLeft = new Run();
                var newRunRight = new Run();
                var insert = symbol;
                e.Handled = true;
                if (caretPos.GetOffsetToPosition(run.ContentStart) != 0 && caretPos.GetOffsetToPosition(run.ContentEnd) != 0)
                {
                    var i = run;
                    var forword = new TextRange(run.ContentStart, caretPos);
                    var behind = new TextRange(caretPos, run.ContentEnd);
                    caretPos.Paragraph.Inlines.InsertAfter(i, new Run(forword.Text));
                    i = i.NextInline as Run;
                    caretPos.Paragraph.Inlines.InsertAfter(i, new Run(behind.Text));
                    caretPos.Paragraph.Inlines.Remove(run);
                    run = i;
                    caretPos = run.ContentEnd;
                }
                newRunLeft.Text = insert;
                newRunLeft.Background = view.MathBrushBack;
                newRunLeft.Foreground = view.MathBrushFore;
                newRunRight.Text = insert;
                newRunRight.Background = view.MathBrushBack;
                newRunRight.Foreground = view.MathBrushFore;

                if (caretPos.IsAtLineStartPosition)
                {
                    if (caretPos.Paragraph.Inlines.FirstInline.Background == view.MathBrushBack)
                        caretPos.Paragraph.Inlines.Remove(caretPos.Paragraph.Inlines.FirstInline);
                    caretPos.Paragraph.Inlines.InsertBefore(run, newRunLeft);
                    caretPos.Paragraph.Inlines.InsertBefore(newRunLeft, newRunRight);
                    caretPos.Paragraph.Inlines.InsertBefore(newRunRight, new Run { Background = TextBox.Background });
                    TextBox.CaretPosition = caretPos.GetNextInsertionPosition(LogicalDirection.Backward);
                }
                else
                {
                    caretPos.Paragraph.Inlines.InsertAfter(run, newRunLeft);
                    caretPos.Paragraph.Inlines.InsertAfter(newRunLeft, newRunRight);
                    caretPos.Paragraph.Inlines.InsertAfter(newRunRight, new Run { Background = TextBox.Background });
                    TextBox.CaretPosition = caretPos.GetNextInsertionPosition(LogicalDirection.Forward);
                }
                InputMethod.Current.ImeSentenceMode = ImeSentenceModeValues.Conversation;
            }
            /*
            if (e.Key == Key.Enter)
            {
                var caretPosParent = TextBox.CaretPosition.Parent as Run;
                if (caretPosParent == null || caretPosParent.Text != view.MathSymbol.ToString()) return;
                var left = caretPosParent.PreviousInline as Run;
                var right = caretPosParent.NextInline as Run;
                if (left == null || right == null) return;
                if (
                    (left.Background == view.MathBrushBack && TextBox.CaretPosition.GetOffsetToPosition(caretPosParent.ContentStart) == 0) ||
                    (right.Background == view.MathBrushBack && TextBox.CaretPosition.GetOffsetToPosition(caretPosParent.ContentEnd) == 0)
                    )
                {
                    e.Handled = true;
                }
            }*/
        }

        private void TextBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            Debug.WriteLine(1);
            if (e.Text == view.MathSymbol.ToString())
            {
                var caretPosParent = TextBox.CaretPosition.Parent as Run;
                if (caretPosParent == null) return;
                var split = caretPosParent.Text.Split(view.MathSymbol);
                caretPosParent.Text = split[0];
                var mathRun = new Run(view.MathSymbol.ToString()) { Foreground = view.MathBrushFore, Background = view.MathBrushBack };
                var surplusRun = new Run(split[1]);
                TextBox.CaretPosition.Paragraph.Inlines.InsertAfter(caretPosParent, mathRun);
                TextBox.CaretPosition.Paragraph.Inlines.InsertAfter(mathRun, surplusRun);
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                System.Windows.IDataObject clipboardData = System.Windows.Clipboard.GetDataObject();
                if (clipboardData.GetDataPresent(System.Windows.DataFormats.Text))
                {
                    string clipboardText = (string)clipboardData.GetData(System.Windows.DataFormats.Text);
                    int lineCount = clipboardText.Where(x => x == '\v' || x == '\n').Count();
                    var currentPos = TextBox.CaretPosition;
                    for (int i = 0; i < lineCount; i++)
                    {
                        var next = TextBox.CaretPosition.GetLineStartPosition(-1).GetNextInsertionPosition(LogicalDirection.Forward);
                        if (next != TextBox.CaretPosition)
                        {
                            TextBox.CaretPosition = next;
                            ColorCoordinator();
                        }
                        else
                        {
                            TextBox.CaretPosition = TextBox.CaretPosition.GetLineStartPosition(-1);
                        }
                    }
                    TextBox.CaretPosition = currentPos;
                }
            }
            ColorCoordinator();
        }

        private void ColorCoordinator()
        {
            var lineStart = TextBox.CaretPosition.GetLineStartPosition(0).GetNextInsertionPosition(LogicalDirection.Forward);
            if (lineStart == null) return;
            var lineEnd = TextBox.CaretPosition.GetLineStartPosition(1);
            if (lineEnd == null)
            {
                if (lineStart.Paragraph.Inlines.LastInline as Run == null)
                    lineEnd = ((lineStart.Paragraph.Inlines.LastInline as Span).Inlines.LastInline as Run).ContentEnd;
                else
                    lineEnd = (lineStart.Paragraph.Inlines.LastInline as Run).ContentEnd;
            }
            else
                lineEnd = lineEnd.GetNextInsertionPosition(LogicalDirection.Backward);
            var index = lineStart.Parent as Run;
            var last = lineEnd.Parent as Run;
            bool isInsideMarker = false;
            while (true)
            {
                if (index == null)
                    break;
                if (index.Text.Contains(view.MathSymbol.ToString()) && index.Text.Length != 1)
                {
                    var split = index.Text.Split(view.MathSymbol);
                    index.Text = split[0];
                    for (int i = split.Length - 2; i >= 0; i--)
                    {
                        var math = new Run(view.MathSymbol.ToString()) { Foreground = view.MathBrushFore, Background = view.MathBrushBack };
                        var surplus = new Run(split[i + 1]);
                        TextBox.CaretPosition.Paragraph.Inlines.InsertAfter(index, math);
                        TextBox.CaretPosition.Paragraph.Inlines.InsertAfter(math, surplus);
                    }
                    lineEnd = TextBox.CaretPosition.GetLineStartPosition(1);
                    if (lineEnd == null)
                        lineEnd = (lineStart.Paragraph.Inlines.LastInline as Run).ContentEnd;
                    else
                        lineEnd = lineEnd.GetNextInsertionPosition(LogicalDirection.Backward);
                    last = lineEnd.Parent as Run;

                }
                if (index.Text.StartsWith(view.MathSymbol.ToString()) && index.Text.EndsWith(view.MathSymbol.ToString()) && index.Background != view.MathBrushBack)
                {
                    index.Background = view.MathBrushBack;
                    index.Foreground = view.MathBrushFore;
                }
                else if (!index.Text.Contains(view.MathSymbol) && index.Background != TextBox.Background)
                {
                    index.Background = TextBox.Background;
                    index.Foreground = TextBox.Foreground;
                }
                if (index != lineStart.Parent as Run && index != last)
                {
                    if (index.Text == view.MathSymbol.ToString() && isInsideMarker == false)
                    {
                        isInsideMarker = true;
                        index.Background = view.MathBrushBack;
                        index.Foreground = view.MathBrushFore;
                    }
                    else if (index.Text == view.MathSymbol.ToString() && isInsideMarker == true)
                    {
                        isInsideMarker = false;
                        index.Background = view.MathBrushBack;
                        index.Foreground = view.MathBrushFore;
                    }
                }
                if (isInsideMarker)
                {
                    index.Background = view.MathBrushBack;
                    index.Foreground = view.MathBrushFore;
                }
                else if (index.Text != view.MathSymbol.ToString() && index.Text != (view.MathSymbol.ToString() + view.MathSymbol.ToString()))
                {
                    index.Background = TextBox.Background;
                    index.Foreground = TextBox.Foreground;
                }
                if (index == last)
                    break;
                index = index.NextInline as Run;
            }
        }

        private void TextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LineNumberTextBox.Height = TextBox.ActualHeight;
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            BindableRichTextBox.VerticalTabsModifier(TextBox, Key.X, Key.LeftCtrl);
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            BindableRichTextBox.VerticalTabsModifier(TextBox, Key.V, Key.LeftCtrl);
        }

        private void TextBoxScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            LineNumberScrollViewer.ScrollToVerticalOffset(TextBoxScrollViewer.VerticalOffset);
        }
    }
}
