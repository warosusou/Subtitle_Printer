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
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

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

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            /*var caretpos = TextBox.CaretPosition;
            if (e.Text == "@")
            {
                e.Handled = true;
                caretpos.InsertTextInRun("@@");
                var range = new TextRange(caretpos, caretpos.GetPositionAtOffset("@@".Length));
                range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Black);
                TextBox.CaretPosition = caretpos.GetNextInsertionPosition(LogicalDirection.Forward);
            }
            else
            {
                e.Handled = true;
                //TextBox.CaretPosition.GetNextContextPosition(LogicalDirection.Backward).GetNextContextPosition(LogicalDirection.Backward).InsertTextInRun(e.Text);
                var right = TextBox.CaretPosition.GetNextContextPosition(LogicalDirection.Forward).GetNextContextPosition(LogicalDirection.Forward);
                if (RichTextBoxUtil.GetLineIndex(right) != RichTextBoxUtil.GetLineIndex(caretpos) && !caretpos.IsAtLineStartPosition)
                {
                    TextBox.CaretPosition.InsertTextInRun(e.Text);
                    var range = new TextRange(caretpos, caretpos.GetPositionAtOffset(e.Text.Length));
                    range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
                    if (caretpos.GetNextInsertionPosition(LogicalDirection.Forward) != null)
                        TextBox.CaretPosition = caretpos.GetNextInsertionPosition(LogicalDirection.Forward);
                }
            }*/
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
    }
}
