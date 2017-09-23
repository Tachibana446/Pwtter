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

namespace Pwtter
{
    /// <summary>
    /// AuthWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AuthWindow : Window
    {
        string url;
        public string pin = "";
        public bool isOk = false;
        public bool isClosed = false;

        public AuthWindow(string url)
        {
            this.url = url;
            InitializeComponent();
            urlBox.Text = url;
        }

        private void AccessButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(url);
            ((Button)sender).IsEnabled = false;
            okButton.IsEnabled = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            pin = pinBox.Text.Trim();
            if (pin == "")
            {
                MessageBox.Show("PINを入力してください。");
                return;
            }
            isOk = true;
            isClosed = true;
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            isClosed = true;
        }
    }
}
