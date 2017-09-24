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
using CoreTweet;

namespace Pwtter
{
    /// <summary>
    /// Tweet.xaml の相互作用ロジック
    /// </summary>
    public partial class Tweet : UserControl
    {
        public Tweet(Status tweet)
        {
            InitializeComponent();
            // icon
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(tweet.User.ProfileImageUrl);
            bitmapImage.EndInit();
            iconImage.Source = bitmapImage;
            // name
            nameTextBlock.Text = $"{tweet.User.Name} (@{tweet.User.ScreenName})";

            // text
            mainContent.Text = tweet.Text;

            // date
            dateTextBlock.Text = tweet.CreatedAt.AddHours(9).ToString();
        }

    }
}
