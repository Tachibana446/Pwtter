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
using Mastonet;
using System.Text.RegularExpressions;

namespace Pwtter
{
    /// <summary>
    /// Toot.xaml の相互作用ロジック
    /// </summary>
    public partial class Toot : UserControl
    {
        public Toot()
        {
            InitializeComponent();
        }

        public Toot(Mastonet.Entities.Status toot)
        {
            InitializeComponent();
            // Icon
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(toot.Account.AvatarUrl);
            bitmapImage.EndInit();

            iconImage.Source = bitmapImage;
            // Name
            nameTextBlock.Text = toot.Account.DisplayName;

            // Content
            mainContent.Text = ParseContent(toot.Content);

            // date 
            dateTextBlock.Text = toot.CreatedAt.ToString();
        }

        private static string ParseContent(string content)
        {
            content = Regex.Replace(content, @"<p>|</\s*?p>|</?a.*?>|</\s*?>|</?span.*?>", "");
            content = Regex.Replace(content, @"<br.*?>", "\n");
            return content;
        }
    }
}
