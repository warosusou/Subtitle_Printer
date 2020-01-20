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

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

        private void TextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            
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
