using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Subtitle_Printer
{
    /// <summary>
    /// ResolutionWindow.xaml の相互作用ロジック
    /// </summary>
    /// 
    public partial class ResolutionWindow : Window
    {
        public new DialogResult DialogResult { get { return result; } private set { result = value; this.Close(); } }
        private DialogResult result;
        public Size ImageSize { get; private set; }
        public ResolutionWindow()
        {
            InitializeComponent();
            this.ImageSize = new Size();
        }

        public ResolutionWindow(Size size)
        {
            InitializeComponent();
            this.ImageSize = size;
            textBox1.Text = size.Height.ToString();
            textBox2.Text = size.Width.ToString();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (Int32.TryParse(textBox1.Text, out int h) && Int32.TryParse(textBox2.Text, out int w))
            {
                this.ImageSize = new Size(w, h);
                DialogResult = DialogResult.OK;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("有効な数字を入力してください", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
