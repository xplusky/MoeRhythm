using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Tasks;
using Microsoft.Phone;
using System.Windows.Navigation;
using WeiboSdk;
using WeiboSdk.PageViews;
using System.IO.IsolatedStorage;

namespace MoeRhythm
{
    public class Info
    {
        static IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        public static void SendFeedback()
        {
            EmailComposeTask emailTask = new EmailComposeTask();
            emailTask.Subject = "关于萌音节拍的反馈";
            emailTask.To = "plusky@live.com";
            emailTask.Show();
            
        }

        public static void Evaluate()
        {
            MarketplaceReviewTask reviewTask = new MarketplaceReviewTask();
            reviewTask.Show();
        }

        public static void FollowWeibo()
        {
            WebBrowserTask webTask = new WebBrowserTask();
            webTask.Uri = new Uri("http://weibo.com/plusky");
            webTask.Show();
        }

        public static void Donate()
        {
            var webTask = new WebBrowserTask();
            webTask.Uri = new Uri("https://me.alipay.com/linchen1989");
            webTask.Show();
        }

        //Share Weibo
        public static void Share()
        {
            SdkData.AppKey = "2771170504";
            SdkData.AppSecret = "6a0c24df1abf9e6ee57c759110df414e";
            SdkData.RedirectUri = "http://moerhythm.sinaapp.com/";
            if (string.IsNullOrEmpty(SdkData.AppKey) || string.IsNullOrEmpty(SdkData.AppSecret)
                || string.IsNullOrEmpty(SdkData.RedirectUri))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("请在中MainPage.xmal.cs的构造函数中设置自己的appkey、appkeysecret、RedirectUri.");
                });
                return;
            }
            AuthenticationView.OAuth2VerifyCompleted = (e1, e2, e3) => ShareWeiboAuthCallback(e1, e2, e3);
            AuthenticationView.OBrowserCancelled = new EventHandler(cancleEvent);
            //其它通知事件...
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                App._MainPage.NavigationService.Navigate(new Uri("/WeiboSdk;component/PageViews/AuthenticationView.xaml"
                    , UriKind.Relative));
            });
        }

        static string AccessToken = "";
        static string RefleshToken = "";

        private static void ShareWeiboAuthCallback(bool isSucess, SdkAuthError errCode, SdkAuth2Res response)
        {
            MessageBox.Show(errCode.ToString());
            if (errCode.errCode == SdkErrCode.SUCCESS)
            {
                if (null != response)
                {
                    AccessToken = response.accesssToken;
                    RefleshToken = response.refleshToken;
                    MessageBox.Show(response.expriesIn);
                    settings["AccessToken"] = response.accesssToken;
                    settings["RefleshToken"] = response.refleshToken;
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ShowShareWindow();
                });
            }
            else if (errCode.errCode == SdkErrCode.NET_UNUSUAL)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("检查网络");
                });
            }
            else if (errCode.errCode == SdkErrCode.SERVER_ERR)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("服务器返回错误，错误代码:" + errCode.specificCode);
                });
            }

        }

        private static void cancleEvent(object sender, EventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                App._MainPage.NavigationService.GoBack();
            });
        }

        private static void ShowShareWindow()
        {
            SdkShare shareWindow = new SdkShare();
            shareWindow.AccessToken = AccessToken;
            //shareWindow.PicturePath = "";
            shareWindow.Message = "来自萌音节拍的微博测试 http://moerhythm.sinaapp.com/";
            shareWindow.Completed += new EventHandler<SendCompletedEventArgs>(ShowShareWindowCompleted);
            shareWindow.Show();

        }

        private static void ShowShareWindowCompleted(object sender, SendCompletedEventArgs e)
        {

        }


    }
}
