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
using System.Windows.Shapes;
using System.Windows.Forms;

namespace Subtitle_Printer
{
    /// <summary>
    /// UnsavedWarningWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class UnsavedWarningWindow : Window
    {
        public new DialogResult DialogResult { get { return result; } private set { result = value; this.Close(); } }
        private DialogResult result;
        public UnsavedWarningWindow(string FileName)
        {
            InitializeComponent();
            label.Content = String.Format("{0}を保存しますか", FileName);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = DialogResult.Yes;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
