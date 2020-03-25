using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Subtitle_Printer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TextBoxLineNumber.TextView = TextBox.TextArea.TextView;
            TextBoxLineNumber.VerticalTabs = TextBox.VerticalTabs;
            TextBox.Name = "Notitle";
            using (var reader = new XmlTextReader("ProjectZTexDef.xshd"))
            {
                TextBox.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var caret = TextBox.CaretOffset;
                TextBox.Text = TextBox.Text.Insert(TextBox.CaretOffset, "$$");
                TextBox.CaretOffset = caret + 1;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (TextBox.IsModified)
            {
                var u = new UnsavedWarningWindow(TextBox.Name);
                u.ShowDialog();
                switch (u.DialogResult)
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        //save
                        break;
                    case System.Windows.Forms.DialogResult.No:
                        return;
                    default:
                        e.Cancel = true;
                        break;
                }
            }
        }
    }
}
