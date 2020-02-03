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
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            for (int i = 0; i < e.Changes.First().AddedLength; ++i)
            {
                TextBox.CaretPosition = TextBox.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward);
            }
            TextBox.TextChanged -= TextBox_TextChanged;
        }
    }
}
