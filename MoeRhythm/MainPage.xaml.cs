using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Devices;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using WeiboSdk;
using WeiboSdk.PageViews;

namespace MoeRhythm
{
    public partial class MainPage
    {
        private readonly DispatcherTimer DetectTimer = new DispatcherTimer();
        private readonly DispatcherTimer MainTimer = new DispatcherTimer();
        private readonly IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private int _beat = 1;
        private double _bpmDecimal;
        private int _bpmInteger = 100;
        private double _bpmtemp;
        private int _detectTimes;
        private int _framePosition;
        private DateTime _startTime;
        private DateTime _timePoint;
        private DateTime _timePointTemp;
        private string _shareMessage = "";
        private int _strongBeat;

        public MainPage()
        {
            InitializeComponent();
            App._MainPage = this;
        }

        private TimeSpan BpmSpan
        {
            get { return TimeSpan.FromMilliseconds(15000/BPM); }
        }

        private double BPM
        {
            get { return _bpmInteger + Math.Round(_bpmDecimal, 3); }
        }

        private string defaultMessage
        {
            get { return "#萌音节拍#好棒啊，可以用来打节拍，还能够检测节拍，还有一个测手速的小游戏哦，按得手都疼了，大家来试试吧 http://moerhythm.sinaapp.com"; }
        }

        private void DetectTimer_Tick(object sender, EventArgs e)
        {
            if (_detectTimes > 3 && (_timePointTemp - _timePoint).TotalMilliseconds/(_detectTimes - 1) < 1000 && DateTime.Now - _timePointTemp > TimeSpan.FromSeconds(1.2))
                DetectStop();
            else if (DateTime.Now - _timePointTemp > TimeSpan.FromSeconds(2.3))
                DetectStop();
            else
            {
                TimeSpan ts = DateTime.Now - _timePoint;
                TextDetect.Text = string.Format("检测时间：{0}:{1}.{2}\r\n检测次数：{3}次\r\n", ts.Minutes, ts.Seconds, ts.Milliseconds, _detectTimes);
                if (_detectTimes > 3)
                {
                    TextDetect.Text += string.Format("测试结果：{0} BPM\r\n", Math.Round(_bpmtemp, 0));
                    string quickTip = "";
                    if (_bpmtemp > 300)
                    {
                        if (_bpmtemp < 380)
                            quickTip = "你的速度太慢啦！";
                        else if (_bpmtemp >= 380 && _bpmtemp < 450)
                            quickTip = "马马虎虎的速度";
                        else if (_bpmtemp >= 450 && _bpmtemp < 540)
                            quickTip = "飞一般的速度！";
                        else if (_bpmtemp >= 540 && _bpmtemp < 630)
                            quickTip = "闪电般的速度！！";
                        else if (_bpmtemp >= 630 && _bpmtemp < 750)
                            quickTip = "神一般的速度！！！";
                        else if (_bpmtemp >= 750)
                            quickTip = "神的速度！！！！";
                        double leftSec = 10 - Math.Floor((DateTime.Now - _timePoint).TotalSeconds);
                        TextDetect.Text += string.Format("{0}\r\n倒计时：{1}秒", quickTip, leftSec);
                        if (leftSec <= 0 && _bpmtemp > 300)
                        {
                            if (_bpmtemp < 380)
                                quickTip = "休闲玩家";
                            else if (_bpmtemp >= 380 && _bpmtemp < 450)
                                quickTip = "萌音战士";
                            else if (_bpmtemp >= 450 && _bpmtemp < 540)
                                quickTip = "快刀斗士";
                            else if (_bpmtemp >= 540 && _bpmtemp < 630)
                                quickTip = "闪电骑士";
                            else if (_bpmtemp >= 630 && _bpmtemp < 750)
                                quickTip = "帕金森患者";
                            else if (_bpmtemp >= 750)
                                quickTip = "神之手指";
                            TextDetectGame.Text = string.Format("游戏结束！\r\n你在10秒内按键次数为{0}次，BPM为{1}\r\n恭喜你获得“{2}”称号！", _detectTimes, Math.Round(_bpmtemp), quickTip);
                            StoryboardDetectGameOver.Begin();

                            if (Math.Round(_bpmtemp) > Convert.ToDouble(settings["HighestScore"]))
                            {
                                StoryboardDetectGameNewScore.Begin();
                                StoryboardDetectGameNewScoreZoom.Begin();
                                TextDetectGameScore.Text = string.Format("新纪录！{0} BPM\r\n原纪录：{1} BPM", Math.Round(_bpmtemp), settings["HighestScore"]);
                                settings["HighestScore"] = Math.Round(_bpmtemp);
                                settings.Save();
                                UpdateTile();
                            }
                            else
                            {
                                StoryboardDetectGameNewScore.Begin();
                                TextDetectGameScore.Text = string.Format("最高纪录：{0} BPM", settings["HighestScore"]);
                            }
                            BgRun.IsGameOver = true;
                            DetectStop();
                        }
                    }
                }
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            MainTimer.Interval = TimeSpan.FromMilliseconds(2);
            MainTimer.Tick += MainTimer_Tick;
            DetectTimer.Interval = TimeSpan.FromMilliseconds(5);
            DetectTimer.Tick += DetectTimer_Tick;
            _framePosition = 3;

            SliderIntegerBPM.ValueChanged += SliderIntegerBPM_ValueChanged;
            SliderDecimalBPM.ValueChanged += SliderDecimalBPM_ValueChanged;
            UpdateBpm();
            InitializeSettings();
            InitShare();
            UpdateTile();
        }

        private void UpdateTile()
        {
            var NewTileData = new StandardTileData
                              {
                                  Title = "萌音节拍",
                                  BackgroundImage = new Uri("Background.png", UriKind.Relative),
                                  Count = 0,
                                  BackTitle = "最高分",
                                  BackBackgroundImage = new Uri("BackBackground.png", UriKind.Relative),
                                  BackContent = settings["HighestScore"] + " BPM"
                              };

            ShellTile mainTile = ShellTile.ActiveTiles.First();
            if (mainTile != null)
            {
                mainTile.Update(NewTileData);
            }
        }

        private void InitShare()
        {
            SdkData.AppKey = "2771170504";
            SdkData.AppSecret = "6a0c24df1abf9e6ee57c759110df414e";
            SdkData.RedirectUri = "http://moerhythm.sinaapp.com/authok.html";
        }

        private void InitializeSettings()
        {
            if (settings.Contains("BPM"))
            {
                SliderIntegerBPM.Value = Math.Floor((double) settings["BPM"]);
                SliderDecimalBPM.Value = (double) settings["BPM"] - Math.Floor((double) settings["BPM"]);
            }

            if (settings.Contains("Volume")) SliderVolume.Value = (double) settings["Volume"];
            if (settings.Contains("Vebrate")) CheckBoxVibrate.IsChecked = (bool) settings["Vebrate"];
            if (!settings.Contains("HighestScore")) settings["HighestScore"] = 0;
            if (settings.Contains("EnergySaving")) CheckBoxEnergySaving.IsChecked = (bool) settings["EnergySaving"];
            if (settings.Contains("AccessToken")) App.AccessToken = settings["AccessToken"].ToString();
            if (settings.Contains("BeatSpan")) PickerBeat.SelectedIndex = (int) settings["BeatSpan"];
            settings.Save();
        }

        public void SaveSetting()
        {
            settings["BPM"] = _bpmInteger + _bpmDecimal;
            settings["Volume"] = SliderVolume.Value;
            settings["Vebrate"] = CheckBoxVibrate.IsChecked.Value;
            settings["EnergySaving"] = CheckBoxEnergySaving.IsChecked.Value;
            settings["BeatSpan"] = PickerBeat.SelectedIndex;
            settings.Save();
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now - _startTime >= TimeSpan.FromMilliseconds(15000*(double) _beat/BPM))
            {
                if (_framePosition == 8) _framePosition = 1;
                else _framePosition++;
                ImgMikuDance.Source = new BitmapImage(new Uri("Res/Image/MikuDance/" + _framePosition.ToString() + ".png", UriKind.Relative));
                if (_framePosition == 3 || _framePosition == 7)
                {
                    if (!CheckBoxEnergySaving.IsChecked.Value)
                    {
                        ImgMikuDance_Ghost.Source = ImgMikuDance.Source;
                        ImgMikuDance_Ghost_FadeOut.Begin();
                    }
                    PlaySound();
                }
                _beat++;
            }
        }

        private void DetectPlay()
        {
            _strongBeat = 0;
            if (BgRun.IsPlayed) Stop();
            BgRun.IsDetect = true;
            GridDetectHelper.Visibility = Visibility.Visible;
            _timePoint = DateTime.Now;
            _detectTimes = 0;
            DetectTimer.Start();
        }

        public void DetectStop()
        {
            BgRun.IsDetect = false;
            GridDetectHelper.Visibility = Visibility.Collapsed;
            DetectTimer.Stop();
        }

        public void Play()
        {
            _strongBeat = 0;
            if (BgRun.IsDetect)
            {
                DetectStop();
            }
            BgRun.IsGameOver = false;
            _startTime = DateTime.Now;
            _beat = 1;
            _timePoint = DateTime.Now;
            MainTimer.Start();
            BgRun.IsPlayed = true;
            ButtonPlay.Content = "停下吧……";
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
        }

        public void Stop()
        {
            MainTimer.Stop();
            BgRun.IsPlayed = false;
            ButtonPlay.Content = "给爷跳舞";
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
        }

        public void PlaySound()
        {
            if (_strongBeat > PickerBeat.SelectedIndex) _strongBeat = 0;

            using (Stream stream = TitleContainer.OpenStream("Res/Sound/rhythm.wav"))
            {
                SoundEffect effect = SoundEffect.FromStream(stream);
                SoundEffectInstance sei = effect.CreateInstance();
                if (_strongBeat == 0)
                    sei.Volume = (float) (SliderVolume.Value*1);
                else
                    sei.Volume = (float) (SliderVolume.Value*0.6);
                FrameworkDispatcher.Update();
                sei.Play();
            }
            _strongBeat++;

            if (CheckBoxVibrate.IsChecked.Value)
            {
                VibrateController vibController = VibrateController.Default;
                TimeSpan ts = TimeSpan.FromMilliseconds(50);
                vibController.Start(ts);
            }
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (BgRun.IsPlayed) Stop();
            else Play();
        }

        private void GridDetextBPM_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (BgRun.IsGameOver) return;
            if (!BgRun.IsDetect)
            {
                DetectPlay();
            }
            _timePointTemp = DateTime.Now;
            _detectTimes++;
            if (_detectTimes > 3)
            {
                _bpmtemp = 60000/((_timePointTemp - _timePoint).TotalMilliseconds/(_detectTimes - 1));
                SliderIntegerBPM.Value = Math.Round(_bpmtemp, 0);
                SliderDecimalBPM.Value = 0;
            }
            PlaySound();
            if (!CheckBoxEnergySaving.IsChecked.Value) GridDetextBPM_Ghost_FadeOut.Begin();
            ImageSource TempIs = ImageDetectMiku1.Source;
            ImageDetectMiku1.Source = ImageDetectMiku2.Source;
            ImageDetectMiku2.Source = TempIs;
        }

        private void SliderIntegerBPM_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _bpmInteger = (int) SliderIntegerBPM.Value;
            UpdateBpm();
        }

        private void SliderDecimalBPM_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _bpmDecimal = Math.Round(SliderDecimalBPM.Value, 3);
            UpdateBpm();
        }

        private void UpdateBpm()
        {
            if (BgRun.IsPlayed)
            {
                Play();
            }
            TextBPM.Text = string.Format("速度：{0} BPM", BPM);
        }

        private void BottonDetectGameOverOk_Click(object sender, RoutedEventArgs e)
        {
            BgRun.IsGameOver = false;
            StoryboardDetectGameOver.Stop();
            StoryboardDetectGameNewScore.Stop();
            StoryboardDetectGameNewScoreZoom.Stop();
        }

        private void ButtonSendFeedback_Click(object sender, RoutedEventArgs e)
        {
            Info.SendFeedback();
        }

        private void ButtonFollowWeibo_Click(object sender, RoutedEventArgs e)
        {
            Info.FollowWeibo();
        }

        private void ButtonEvaluate_Click(object sender, RoutedEventArgs e)
        {
            Info.Evaluate();
        }

        private void ButtonDonate_Click(object sender, RoutedEventArgs e)
        {
            Info.Donate();
        }

        private void ButtonShare_Click(object sender, RoutedEventArgs e)
        {
            ScrollInfo.ScrollToVerticalOffset(0);
            Share("");
        }

        private void Share(string message)
        {
            if (message == "")
                _shareMessage = defaultMessage;
            else
                _shareMessage = message;
            GridShareHelper.Visibility = Visibility.Visible;
        }

        public void ShareToWeibo(string message)
        {
            if ((settings.Contains("WeiboAuthDate") && settings.Contains("ExpriesIn")) && (DateTime) settings["WeiboAuthDate"] + TimeSpan.FromSeconds(Convert.ToInt32(settings["ExpriesIn"].ToString())) > DateTime.Now)
            {
                ShowShareWindow(message);
                return;
            }

            if (string.IsNullOrEmpty(SdkData.AppKey) || string.IsNullOrEmpty(SdkData.AppSecret)
                || string.IsNullOrEmpty(SdkData.RedirectUri))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("请在中MainPage.xmal.cs的构造函数中设置自己的appkey、appkeysecret、RedirectUri."); });
                return;
            }
            AuthenticationView.OAuth2VerifyCompleted = (isSucess, errCode, response) =>
                                                       {
                                                           if (errCode.errCode == SdkErrCode.SUCCESS)
                                                           {
                                                               if (null != response)
                                                               {
                                                                   App.AccessToken = response.accesssToken;
                                                                   App.RefleshToken = response.refleshToken;
                                                                   Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                                                                             {
                                                                                                                 settings["AccessToken"] = response.accesssToken;
                                                                                                                 settings["ExpriesIn"] = response.expriesIn;
                                                                                                                 settings["WeiboAuthDate"] = DateTime.Now;
                                                                                                                 settings.Save();
                                                                                                             });
                                                               }
                                                               Deployment.Current.Dispatcher.BeginInvoke(() => { ShowShareWindow(""); });
                                                           }
                                                           else if (errCode.errCode == SdkErrCode.NET_UNUSUAL)
                                                               Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("检查网络"); });
                                                           else if (errCode.errCode == SdkErrCode.SERVER_ERR)
                                                               Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("服务器返回错误，错误代码:" + errCode.specificCode); });
                                                           else
                                                               Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("Other Err."); });
                                                       };
            AuthenticationView.OBrowserCancelled = cancleEvent;
            //其它通知事件...

            Deployment.Current.Dispatcher.BeginInvoke(() => { NavigationService.Navigate(new Uri("/WeiboSdk;component/PageViews/AuthenticationView.xaml", UriKind.Relative)); });
        }

        private void cancleEvent(object sender, EventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => { NavigationService.GoBack(); });
        }

        private void ShowShareWindow(string message)
        {
            var shareWindow = new SdkShare();
            shareWindow.AccessToken = App.AccessToken;
            shareWindow.PicturePath = "share.jpg";
            shareWindow.Message = message;
            shareWindow.Completed += (sender, ea) =>
                                     {
                                         if (ea.IsSendSuccess)
                                             MessageBox.Show("发送成功");
                                         else if (ea.Response.ToLower() == "auth faild2!")
                                         {
                                             Deployment.Current.Dispatcher.BeginInvoke(() => { ShareToWeibo(message); });
                                         }
                                         else
                                         {
                                             MessageBoxResult result = MessageBox.Show("发送失败\r\n" + ea.Response + "\r\n" + "是否重新授权？", "", MessageBoxButton.OKCancel);
                                             if (result == MessageBoxResult.OK)
                                                 Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                                                           {
                                                                                               settings.Remove("WeiboAuthDate");
                                                                                               settings.Remove("ExpriesIn");
                                                                                               settings.Remove("AccessToken");
                                                                                               settings.Save();
                                                                                               ShareToWeibo(message);
                                                                                           });
                                         }
                                     };
            shareWindow.Show();
        }

        //private void ShareToRenen()
        //{
        //    //List<string> scope = new List<string> { "publish_feed", "publish_blog", "read_user_album", "create_album", "photo_upload" };
        //    List<string> scope = new List<string> {  "photo_upload" };
        //    App.renrenApi.Login(this, scope, (sender, e) =>
        //    {
        //        if (e.Error == null)
        //        {
        //            //App.renrenApi.PublishPhoto("share.jpg", ShareToRenenCallBack, shareMessage == "" ? "test1" : shareMessage, 0);
        //            ShareToRenrenPage.ShareImage = new BitmapImage(new Uri("/share.jpg", UriKind.Relative));
        //            ShareToRenrenPage.Message = shareMessage;
        //            NavigationService.Navigate(new Uri("/ShareToRenrenPage.xaml", UriKind.Relative));
        //        }

        //        else
        //            MessageBox.Show(e.Error.Message);
        //    });

        //}

        private void SnapShot()
        {
            string FILE_NAME = "share.jpg";
            try
            {
                var bitmap = new WriteableBitmap(480, 800);
                bitmap.Render(Application.Current.RootVisual, null);
                bitmap.Invalidate();

                App.ShareImage = bitmap;
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(FILE_NAME))
                    {
                        store.DeleteFile(FILE_NAME);
                    }
                    using (IsolatedStorageFileStream stream = store.OpenFile(FILE_NAME, FileMode.OpenOrCreate))
                    {
                        try
                        {
                            bitmap.SaveJpeg(stream, 480, 800, 0, 80);
                        }
                        catch
                        {}
                    }
                }
            }
            catch
            {}
        }

        private void BottonDetectGameOverShare_Click(object sender, RoutedEventArgs e)
        {
            Share("玩了#萌音节拍#以后，头也不疼了，腰也不酸了，就是手有点抽筋。来和我比比手速吧  http://moerhythm.sinaapp.com");
        }

        private void ButtomShareToWeibo_Click(object sender, RoutedEventArgs e)
        {
            GridShareHelper.Visibility = Visibility.Collapsed;
            SnapShot();
            ShareToWeibo(_shareMessage);
        }

        private void ButtonShareToRenren_Click(object sender, RoutedEventArgs e)
        {
            //GridShareHelper.Visibility = Visibility.Collapsed;
            //SnapShot();
            //ShareToRenen();
        }

        private void ButtonShareViaMail_Click(object sender, RoutedEventArgs e)
        {
            var emailTask = new EmailComposeTask();
            emailTask.Body = defaultMessage;
            emailTask.Show();
        }

        private void ButtomShareCancel_Click(object sender, RoutedEventArgs e)
        {
            GridShareHelper.Visibility = Visibility.Collapsed;
        }

        private void MenuScore_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("是否要删除最高纪录（将清零）", "", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                settings["HighestScore"] = 0;
            }
        }

        private void ButtonSnapshot_Click(object sender, RoutedEventArgs e)
        {
            GridShareHelper.Visibility = Visibility.Collapsed;
            var bitmap = new WriteableBitmap(480, 800);
            bitmap.Render(Application.Current.RootVisual, null);
            bitmap.Invalidate();
            using (var stream = new MemoryStream())
            {
                bitmap.SaveJpeg(stream, 480, 800, 0, 80);
                stream.Position = 0;
                using (var mediaLib = new MediaLibrary())
                {
                    DateTime datetime = DateTime.Now;
                    string filename = string.Format("Capture-{0}-{1}-{2}-{3}-{4}-{5}", datetime.Year%100, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second);
                    mediaLib.SavePicture(filename, stream);
                }
            }
            MessageBox.Show("截图成功，以保存到照片库");
        }
    }
}