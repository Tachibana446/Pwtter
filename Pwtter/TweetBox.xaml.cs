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

namespace Pwtter
{
    /// <summary>
    /// TweetBox.xaml の相互作用ロジック
    /// </summary>
    public partial class TweetBox : UserControl
    {
        public TweetBox()
        {
            InitializeComponent();
        }

        private async void submitButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)twitterCheckBox.IsChecked && MainWindow.Instance.TwitterTokens == null)
            {
                MessageBox.Show("まずはTwitterのアカウントを認証してください。");
                return;
            }
            if ((bool)pawooCheckBox.IsChecked && MainWindow.Instance.PawooClient == null)
            {
                MessageBox.Show("まずはPawooのアカウントを認証してください。");
                return;
            }

            if (!(bool)twitterCheckBox.IsChecked && !(bool)pawooCheckBox.IsChecked)
            {
                MessageBox.Show("TwitterかPawooのうち、投稿する方どちらかあるいは両方にチェックを入れてください");
                return;
            }
            var text = messageTextBox.Text.Trim();
            if (text == "")
            {
                MessageBox.Show("投稿する文章が空です");
                return;
            }

            if ((bool)twitterCheckBox.IsChecked && !(bool)canOverLengthText.IsChecked && text.Length > 140)
            {
                MessageBox.Show("Twitterの上限である140字を超えています。");
                return;
            }
            if (text.Length > 500)
            {
                MessageBox.Show("Pawooの上限である500字を超えています。");
                return;
            }
            submitButton.IsEnabled = false;
            messageTextBox.Text = "";

            // Twitter
            try
            {
                if ((bool)twitterCheckBox.IsChecked)
                {
                    int c = 0;
                    while (c < text.Length)
                    {
                        string updateText = text.Substring(c, Math.Min(140, text.Length));
                        c += 140;
                        MainWindow.Instance.TwitterTokens.Statuses.Update(status: updateText);
                    }
                }
            }
            catch (CoreTweet.TwitterException ex)
            {
                MessageBox.Show(ex.Message);
            }

            // Pawoo
            try
            {
                if ((bool)pawooCheckBox.IsChecked)
                {
                    await MainWindow.Instance.PawooClient.PostStatus(text, Mastonet.Visibility.Public);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            submitButton.IsEnabled = true;

        }
    }
}
