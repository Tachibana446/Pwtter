using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        const string TokenFilePathTwitter = "./tokens/twitter.txt";
        const string TokenFilePathPawoo = "./tokens/pawoo.txt";

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            // Create Directory
            if (!System.IO.Directory.Exists("./tokens"))
                System.IO.Directory.CreateDirectory("./tokens");

            // Login
            if (System.IO.File.Exists(TokenFilePathTwitter)) AuthTwitterFromFile();
            if (System.IO.File.Exists(TokenFilePathPawoo)) AuthPawooFromFile();
            Task.Run(async () => await SetMyAccounts());
        }



        /// <summary>
        /// メニューアイテムの認証→Twitterをクリックした時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AuthTwitterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var session = CoreTweet.OAuth.Authorize(Properties.Resources.TwConsumerKey, Properties.Resources.TwConsumerSecret);
                var authWindow = new AuthWindow(session.AuthorizeUri.ToString());
                authWindow.ShowDialog();
                if (authWindow.isOk)
                {
                    TwitterTokens = CoreTweet.OAuth.GetTokens(session, authWindow.pin);
                    // Save Token
                    using (var sw = new System.IO.StreamWriter(TokenFilePathTwitter))
                    {
                        sw.WriteLine(TwitterTokens.AccessToken);
                        sw.WriteLine(TwitterTokens.AccessTokenSecret);
                    }
                    // タイムラインのロード
                    foreach (var s in await TwitterTokens.Statuses.HomeTimelineAsync())
                    {
                        twitterTimelineStackPanel.Children.Add(new Tweet(s));
                    }
                }
            }
            catch (CoreTweet.TwitterException ex)
            {
                MessageBox.Show(ex.Message, "エラー");
                return;
            }
            await SetMyAccounts();
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
                    // Save Token
                    using (var sw = new System.IO.StreamWriter(TokenFilePathPawoo))
                    {
                        sw.WriteLine(accessToken.AccessToken);
                    }
                    // タイムラインのロード
                    foreach (var s in await PawooClient.GetHomeTimeline())
                    {
                        pawooTimelineStackPanel.Children.Add(new Toot(s));
                    }
                }
            }
            catch (Mastonet.ServerErrorException ex)
            {
                MessageBox.Show(ex.Message, "エラー");
                return;
            }
            await SetMyAccounts();
        }

        /// <summary>
        /// 自分のアカウントを取得し、メニューバーに表示
        /// </summary>
        /// <returns></returns>
        private async Task SetMyAccounts()
        {
            try
            {
                if (TwitterTokens != null)
                {
                    var tweet = (await TwitterTokens.Statuses.UserTimelineAsync())[0];
                    var text = $"{tweet.User.Name}(@{tweet.User.ScreenName})".Replace("_", "__"); // アンダーバーをエスケープ
                    Dispatcher.Invoke(() => twitterAccountMenuItem.Header = text);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Twitterのアカウントを読み込む際にエラーがでました。\n再認証してください。\n" + e.Message);
                TwitterTokens = null;
            }

            try
            {
                if (PawooClient != null)
                {
                    var user = await PawooClient.GetCurrentUser();
                    var text = $"{user.DisplayName}(@{user.AccountName})".Replace("_", "__"); // アンダーバーをエスケープ
                    Dispatcher.Invoke(() => pawooAccountMenuItem.Header = text);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Pawooのアカウントを読み込む際にエラーが出ました。\n再認証してください。\n" + e.Message);
                PawooClient = null;
            }
        }

        private void AuthTwitterFromFile()
        {
            var lines = System.IO.File.ReadLines(TokenFilePathTwitter).ToList();
            if (lines.Count() != 2) return;
            TwitterTokens = CoreTweet.Tokens.Create(
                Properties.Resources.TwConsumerKey,
                Properties.Resources.TwConsumerSecret,
                lines[0],
                lines[1]
            );

        }

        private void AuthPawooFromFile()
        {
            var accessToken = System.IO.File.ReadLines(TokenFilePathPawoo).ToArray()[0];

            var appRegistration = new Mastonet.Entities.AppRegistration
            {
                Instance = "pawoo.net",
                ClientId = Properties.Resources.PwClientKey,
                ClientSecret = Properties.Resources.PwClientSecret
            };
            PawooClient = new Mastonet.MastodonClient(appRegistration, new Mastonet.Entities.Auth { AccessToken = accessToken });

        }

        /// <summary>
        /// タイムラインを更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReloadMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PawooClient != null)
            {
                foreach (var s in await PawooClient.GetHomeTimeline())
                {
                    pawooTimelineStackPanel.Children.Add(new Toot(s));
                }
            }

            if (TwitterTokens != null)
            {
                foreach (var s in await TwitterTokens.Statuses.HomeTimelineAsync())
                {
                    twitterTimelineStackPanel.Children.Add(new Tweet(s));
                }
            }
        }
    }
}
