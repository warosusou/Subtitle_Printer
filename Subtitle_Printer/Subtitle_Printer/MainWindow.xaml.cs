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
            /*
            if (e.Text == "") return;
            if (e.Text.Length > 1)
            {
                e.Handled = true;
                for (int i = 0; i < e.Text.Length; i++)
                {
                    var composition = new TextComposition(InputManager.Current, (BindableRichTextBox)sender, e.Text[i].ToString());
                    var ne = new TextCompositionEventArgs(e.Device, composition);
                    ne.RoutedEvent = e.RoutedEvent;
                    ne.Source = e.Source;
                    TextBox_PreviewTextInput(sender, ne);
                }
            }
            var caretPos = TextBox.CaretPosition;
            var symbol = view.MathSymbol.ToString();
            var run = caretPos.Parent as Run;
            var newRun = new Run();
            if (TextBox.Selection.Text == "\r\n")
            {
                run = caretPos.GetNextInsertionPosition(LogicalDirection.Backward).Parent as Run;
                caretPos = run.ElementEnd;
            }
            if (e.Text == symbol || e.Text == view.MathSymbolAlias.ToString())
            {
                var insert = symbol + symbol;
                e.Handled = true;
                if (e.Text == view.MathSymbolAlias.ToString())
                    caretPos.DeleteTextInRun(-1);
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
                newRun.Text = insert;
                newRun.Foreground = view.MathBrushFore;
                if (caretPos.IsAtLineStartPosition)
                {
                    if (caretPos.Paragraph.Inlines.FirstInline.Foreground == view.MathBrushFore)
                        caretPos.Paragraph.Inlines.Remove(caretPos.Paragraph.Inlines.FirstInline);
                    caretPos.Paragraph.Inlines.InsertBefore(run, newRun);
                    caretPos.Paragraph.Inlines.InsertBefore(newRun, new Run { Foreground = TextBox.Foreground });
                    TextBox.CaretPosition = caretPos.GetNextInsertionPosition(LogicalDirection.Backward);
                }
                else
                {
                    caretPos.Paragraph.Inlines.InsertAfter(run, newRun);
                    caretPos.Paragraph.Inlines.InsertAfter(newRun, new Run { Foreground = TextBox.Foreground });
                    TextBox.CaretPosition = caretPos.GetNextInsertionPosition(LogicalDirection.Forward);
                }
                InputMethod.Current.ImeSentenceMode = ImeSentenceModeValues.Conversation;
            }
            else
            {
                if (run.Text.StartsWith(symbol) && run.Text.EndsWith(symbol) && run.Text.Count(x => x == view.MathSymbol) == 2)
                {
                    if (caretPos.GetOffsetToPosition(run.ContentStart) == 0)
                    {
                        if (run.PreviousInline == null || Util.GetLineIndex(run.ContentStart) != Util.GetLineIndex(run.PreviousInline.ContentStart))
                        {
                            newRun.Foreground = TextBox.Foreground;
                            newRun.Text = e.Text;
                            e.Handled = true;
                            caretPos.Paragraph.Inlines.InsertBefore(run, newRun);
                            TextBox.CaretPosition = newRun.ElementEnd;
                        }
                        else if (run.PreviousInline != null && (run.PreviousInline as Run).Text.StartsWith(symbol) && (run.PreviousInline as Run).Text.EndsWith(symbol))
                        {
                            newRun.Foreground = TextBox.Foreground;
                            newRun.Text = e.Text;
                            e.Handled = true;
                            caretPos.Paragraph.Inlines.InsertBefore(run, newRun);
                            TextBox.CaretPosition = newRun.ElementEnd;
                        }
                        else
                        {
                            TextBox.CaretPosition = run.PreviousInline.ElementEnd;
                        }
                    }
                    else if (caretPos.GetOffsetToPosition(run.ContentEnd) == 0)
                    {
                        if (run.NextInline == null || Util.GetLineIndex(run.ContentStart) != Util.GetLineIndex(run.NextInline.ContentStart))
                        {
                            newRun.Foreground = TextBox.Foreground;
                            newRun.Text = e.Text;
                            e.Handled = true;
                            caretPos.Paragraph.Inlines.InsertAfter(run, newRun);
                            TextBox.CaretPosition = newRun.ElementStart;
                        }
                        else if (run.NextInline != null && (run.NextInline as Run).Text.StartsWith(symbol) && (run.NextInline as Run).Text.EndsWith(symbol))
                        {
                            newRun.Foreground = TextBox.Foreground;
                            newRun.Text = e.Text;
                            e.Handled = true;
                            caretPos.Paragraph.Inlines.InsertAfter(run, newRun);
                            TextBox.CaretPosition = newRun.ElementEnd;
                        }
                        else
                        {
                            TextBox.CaretPosition = run.NextInline.ElementStart;
                        }
                    }
                }
                else if (run.Text.Count(x => x == view.MathSymbol) > 2)
                {
                    e.Handled = true;
                    var i = run;
                    var forword = new TextRange(run.ContentStart, caretPos);
                    var behind = new TextRange(caretPos, run.ContentEnd);
                    if (!forword.Text.EndsWith(symbol) && !behind.Text.StartsWith(symbol)) return;
                    caretPos.Paragraph.Inlines.InsertAfter(i, new Run(forword.Text) { Foreground = view.MathBrushFore });
                    i = i.NextInline as Run;
                    caretPos.Paragraph.Inlines.InsertAfter(i, new Run(e.Text));
                    i = i.NextInline as Run;
                    caretPos.Paragraph.Inlines.InsertAfter(i, new Run(behind.Text) { Foreground = view.MathBrushFore });
                    caretPos.Paragraph.Inlines.Remove(run);
                    run = i;
                    TextBox.CaretPosition = run.ContentEnd;
                }
            }
        */
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var run = TextBox.CaretPosition.Parent as Run;
            if (run == null) return;
            if (run.Text.Contains(view.MathSymbol))
            {
                if (TextBox.CaretPosition == run.ElementStart)
                {
                    if (run.PreviousInline == null)
                        TextBox.CaretPosition.Paragraph.Inlines.InsertBefore(run, new Run() { Foreground = TextBox.Foreground });
                    var previous = run.PreviousInline;
                    TextBox.CaretPosition = previous.ElementEnd;
                }
                else
                {
                    if (run.NextInline == null)
                        TextBox.CaretPosition.Paragraph.Inlines.InsertAfter(run, new Run() { Foreground = TextBox.Foreground });
                    var next = run.NextInline;
                    TextBox.CaretPosition = next.ElementStart;
                }
            }
            ColorCoordinator();
            if (InputMethod.Current.ImeState == InputMethodState.On && (e.ImeProcessedKey == Key.Oem3 || (e.ImeProcessedKey == Key.D2 && Keyboard.Modifiers == ModifierKeys.Shift)))
            {
                InputMethod.Current.ImeSentenceMode = ImeSentenceModeValues.None;
            }
        }

        private void ColorCoordinator()
        {
            var lineStart = TextBox.CaretPosition.GetLineStartPosition(0).GetNextInsertionPosition(LogicalDirection.Forward);
            var lineEnd = TextBox.CaretPosition.GetLineStartPosition(1);
            if (lineEnd == null)
                lineEnd = lineStart.DocumentEnd;
            else
                lineEnd = lineEnd.GetNextInsertionPosition(LogicalDirection.Backward);
            var index = lineStart.Parent as Run;
            var last = lineEnd.Parent as Run;
            while (true)
            {
                if (index == null)
                    break;
                if (index.Text.StartsWith(view.MathSymbol.ToString()) && index.Text.EndsWith(view.MathSymbol.ToString()) && index.Foreground != view.MathBrushFore)
                    index.Foreground = view.MathBrushFore;
                else if (!index.Text.Contains(view.MathSymbol) && index.Foreground != TextBox.Foreground)
                    index.Foreground = TextBox.Foreground;
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
            Debug.WriteLine(TextBox.VerticalTabs.Count.ToString());
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
