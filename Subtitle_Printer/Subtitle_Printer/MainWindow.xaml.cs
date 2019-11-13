using System;
using System.Collections.Generic;
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
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                this.DataContext = new { Text = "aa" };
            }
        }

        private void TextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            var d = e.Data.GetData(DataFormats.UnicodeText);

            if (((string)d).Contains("\r"))
            {
                d = "aa";
                e.Handled = true;
            }

            this.DataContext = new { Text = d };
        }
    }
}
