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
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public CoreTweet.Tokens TwitterTokens { get; set; } = null;
        public Mastonet.MastodonClient PawooClient { get; set; } = null;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        }


        /// <summary>
        /// メニューアイテムの認証→Twitterをクリックした時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AuthTwitterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var session = CoreTweet.OAuth.Authorize(Properties.Resources.TwConsumerKey, Properties.Resources.TwConsumerSecret);
                var authWindow = new AuthWindow(session.AuthorizeUri.ToString());
                authWindow.ShowDialog();
                if (authWindow.isOk)
                {
                    TwitterTokens = CoreTweet.OAuth.GetTokens(session, authWindow.pin);
                    // DEBUG
                    var stackPanel = new StackPanel();
                    foreach (var s in TwitterTokens.Statuses.HomeTimeline())
                    {
                        stackPanel.Children.Add(new TextBlock() { Text = s.Text });
                    }
                    tabsGrid.Children.Add(stackPanel);
                }
            }
            catch (CoreTweet.TwitterException ex)
            {
                MessageBox.Show(ex.Message, "エラー");
                return;
            }
        }

        /// <summary>
        /// メニューアイテムの認証→Pawooをクリックした時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AuthPawooMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var appRegistration = new Mastonet.Entities.AppRegistration
                {
                    Instance = "pawoo.net",
                    ClientId = Properties.Resources.PwClientKey,
                    ClientSecret = Properties.Resources.PwClientSecret
                };
                var authClient = new Mastonet.AuthenticationClient(appRegistration);
                var authWindow = new AuthWindow(authClient.OAuthUrl());
                authWindow.ShowDialog();
                if (authWindow.isOk)
                {
                    var accessToken = await authClient.ConnectWithCode(authWindow.pin);
                    PawooClient = new Mastonet.MastodonClient(appRegistration, accessToken);
                    // DEBUG
                    var sp = new StackPanel();
                    Grid.SetColumn(sp, 1);
                    tabsGrid.Children.Add(sp);
                    foreach (var s in await PawooClient.GetHomeTimeline())
                    {
                        sp.Children.Add(new TextBlock { Text = s.Content + "\n---" , TextWrapping = TextWrapping.Wrap});
                    }
                }
            }
            catch (Mastonet.ServerErrorException ex)
            {
                MessageBox.Show(ex.Message, "エラー");
            }
        }
    }
}
