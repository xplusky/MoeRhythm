using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Media;
using System.Diagnostics;

namespace MoeRhythmWpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer MainTimer = new DispatcherTimer();
        DispatcherTimer DetectTimer = new DispatcherTimer();
        int FramePosition;
        DateTime StartTime;
        int beat = 1;
        DateTime TimePoint;
        DateTime TimePointTemp;
        TimeSpan BpmSpan { get { return TimeSpan.FromMilliseconds(15000 / BPM); } }
        double BPM { get { return (double)BpmInteger + Math.Round(BpmDecimal, 3); } }
        int BpmInteger = 100;
        double BpmDecimal = 0;
        int DetectTimes = 0;
        double bpmtemp;
        int strongBeat = 0;
        Storyboard StoryboardMikuDanceFade, StoryboardDetectGameOver;
        SoundPlayer beatPlayer = new SoundPlayer(System.IO.Path.GetDirectoryName(Application.ResourceAssembly.Location) + @"\Res\Sound\rhythm.wav");
        SoundPlayer beatPlayer2 = new SoundPlayer(System.IO.Path.GetDirectoryName(Application.ResourceAssembly.Location) + @"\Res\Sound\rhythm2.wav");
        AppSetting settings = new AppSetting();

        public MainWindow()
        {
            InitializeComponent();
            App._MainPage = this;
            MainTimer.Interval = System.TimeSpan.FromMilliseconds(2);
            MainTimer.Tick += new EventHandler(MainTimer_Tick);
            DetectTimer.Interval = TimeSpan.FromMilliseconds(5);
            DetectTimer.Tick += new EventHandler(DetectTimer_Tick);
            FramePosition = 3;
        }

        void MainTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now - StartTime >= TimeSpan.FromMilliseconds(15000 * (double)beat / BPM))
            {
                if (FramePosition == 8) FramePosition = 1;
                else FramePosition++;
                ImgMikuDance.Source = new BitmapImage(new Uri("Res/Image/MikuDance/" + FramePosition.ToString() + ".png", UriKind.Relative));
                if (FramePosition == 3 || FramePosition == 7)
                {
                    if (!CheckBoxEnergySaving.IsChecked.Value)
                    {
                        ImgMikuDance_Ghost.Source = ImgMikuDance.Source;
                        StoryboardMikuDanceFade.Begin();
                    }
                    PlaySound();
                }
                beat++;
            }
        }

        void DetectTimer_Tick(object sender, EventArgs e)
        {
            if (DetectTimes > 3 && (TimePointTemp - TimePoint).TotalMilliseconds / (DetectTimes - 1) < 1000 && DateTime.Now - TimePointTemp > TimeSpan.FromSeconds(1.2))
                DetectStop();
            else if (DateTime.Now - TimePointTemp > TimeSpan.FromSeconds(2.3))
                DetectStop();
            else
            {
                TimeSpan ts = DateTime.Now - TimePoint;
                TextDetect.Text = string.Format("检测时间：{0}:{1}.{2}\r\n检测次数：{3}次\r\n", ts.Minutes, ts.Seconds, ts.Milliseconds, DetectTimes);
                if (DetectTimes > 3)
                {
                    TextDetect.Text += string.Format("测试结果：{0} BPM\r\n", Math.Round(bpmtemp, 0));
                    string quickTip = "";
                    if (bpmtemp > 300)
                    {
                        if (bpmtemp < 380)
                            quickTip = "你的速度太慢啦！";
                        else if (bpmtemp >= 380 && bpmtemp < 450)
                            quickTip = "马马虎虎的速度";
                        else if (bpmtemp >= 450 && bpmtemp < 540)
                            quickTip = "飞一般的速度！";
                        else if (bpmtemp >= 540 && bpmtemp < 630)
                            quickTip = "闪电般的速度！！";
                        else if (bpmtemp >= 630 && bpmtemp < 750)
                            quickTip = "神一般的速度！！！";
                        else if (bpmtemp >= 750)
                            quickTip = "神的速度！！！！";
                        double leftSec = 10 - Math.Floor((DateTime.Now - TimePoint).TotalSeconds);
                        TextDetect.Text += string.Format("{0}\r\n倒计时：{1}秒", quickTip, leftSec);
                        if (leftSec == 0 && bpmtemp > 300)
                        {
                            if (bpmtemp < 380)
                                quickTip = "休闲玩家";
                            else if (bpmtemp >= 380 && bpmtemp < 450)
                                quickTip = "萌音战士";
                            else if (bpmtemp >= 450 && bpmtemp < 540)
                                quickTip = "快刀斗士";
                            else if (bpmtemp >= 540 && bpmtemp < 630)
                                quickTip = "闪电骑士";
                            else if (bpmtemp >= 630 && bpmtemp < 750)
                                quickTip = "帕金森患者";
                            else if (bpmtemp >= 750)
                                quickTip = "神之手指";
                            TextDetectGame.Text = string.Format("游戏结束！\r\n你在10秒内按键次数为{0}次，BPM为{1}\r\n恭喜你获得“{2}”称号！", DetectTimes, Math.Round(bpmtemp), quickTip);
                            StoryboardDetectGameOver.Begin();

                            //if (Math.Round(bpmtemp) > Convert.ToDouble(settings["HighestScore"]))
                            //{
                            //    StoryboardDetectGameNewScore.Begin();
                            //    StoryboardDetectGameNewScoreZoom.Begin();
                            //    TextDetectGameScore.Text = string.Format("新纪录！{0} BPM\r\n原纪录：{1} BPM", Math.Round(bpmtemp), settings["HighestScore"]);
                            //    settings["HighestScore"] = Math.Round(bpmtemp);
                            //    settings.Save();
                            //    UpdateTile();

                            //}
                            //else
                            //{
                            //    StoryboardDetectGameNewScore.Begin();
                            //    TextDetectGameScore.Text = string.Format("最高纪录：{0} BPM", settings["HighestScore"]);
                            //}
                            DetectStop();
                        }
                    }
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            InitSetting();
            InitVisual();
            InitStoryboard();
        }

        private void InitStoryboard()
        {
            StoryboardMikuDanceFade = (Storyboard)FindResource("StoryboardMikuDanceFade");
            StoryboardDetectGameOver = (Storyboard)FindResource("StoryboardDetectGameOver");
        }

        private void InitVisual()
        {
            UpdateBpm();
            GridSetting.Visibility = Visibility.Hidden;
            GridDetect.Visibility = Visibility.Hidden;
            GridHelperInner.Visibility = Visibility.Hidden;
            TextDetectGame.Visibility = Visibility.Hidden;
            GridAbout.Visibility = Visibility.Hidden;
        }

        private void InitSetting()
        {
            SliderIntegerBPM.ValueChanged+=new RoutedPropertyChangedEventHandler<double>(SliderIntegerBPM_ValueChanged);
            SliderDecimalBPM.ValueChanged+=new RoutedPropertyChangedEventHandler<double>(SliderDecimalBPM_ValueChanged);

            SliderIntegerBPM.Value = Math.Floor((double)settings["BPM"]);
            SliderDecimalBPM.Value = (double)settings["BPM"] - Math.Floor((double)settings["BPM"]);
            CheckBoxEnergySaving.IsChecked = (bool)settings["EnergySaving"];
            ComboboxBeatSpan.SelectedIndex = (int)settings["BeatSpan"];
            
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                
            }
        }

        private void UpdateBpm()
        {
            if (BgRun.IsPlayed)
            {
                Play();
            }
            TextBPM.Text = string.Format("速度：{0} BPM", BPM);
        }

        public void Play()
        {
            strongBeat = 0;
            if (BgRun.IsDetect)
            {
                DetectStop();
            }
            GridDetect.Visibility = Visibility.Collapsed;
            StartTime = DateTime.Now;
            beat = 1;
            TimePoint = DateTime.Now;
            MainTimer.Start();
            BgRun.IsPlayed = true;
            ButtonPlay.Content = "停下";
        }

        public void Stop()
        {
            MainTimer.Stop();
            BgRun.IsPlayed = false;
            ButtonPlay.Content = "跳舞";
        }

        private void DetectPlay()
        {
            strongBeat = 0;
            if (BgRun.IsPlayed) Stop();
            BgRun.IsDetect = true;
            GridDetect.Visibility = Visibility.Visible;
            TimePoint = DateTime.Now;
            DetectTimes = 0;
            DetectTimer.Start();

        }

        public void DetectStop()
        {
            BgRun.IsDetect = false;
            
            DetectTimer.Stop();
        }

        public void PlaySound()
        {
            if (strongBeat > ComboboxBeatSpan.SelectedIndex) strongBeat = 0;
            
            if (strongBeat == 0)
            {
                beatPlayer.Play();
            }
            else
            {
                beatPlayer2.Play();
            }
            strongBeat++;

        }

        private void ButtonSetting_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GridSetting.Visibility = Visibility.Visible;
        }

        private void SliderIntegerBPM_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            BpmInteger = (int)SliderIntegerBPM.Value;
            UpdateBpm();
        }

        private void SliderDecimalBPM_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            BpmDecimal = Math.Round(SliderDecimalBPM.Value, 3);
            UpdateBpm();
        }

        private void ButtonPlay_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (BgRun.IsPlayed) Stop();
            else Play();
        }

        private void GridDetextBPM_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!BgRun.IsDetect)
            {
                DetectPlay();
            }
            TimePointTemp = DateTime.Now;
            DetectTimes++;
            if (DetectTimes > 3)
            {
                bpmtemp = 60000 / ((TimePointTemp - TimePoint).TotalMilliseconds / (DetectTimes - 1));
                SliderIntegerBPM.Value = Math.Round(bpmtemp, 0);
                SliderDecimalBPM.Value = 0;
            }
            PlaySound();
            //if (!CheckBoxEnergySaving.IsChecked.Value) GridDetextBPM_Ghost_FadeOut.Begin();
            ImageSource TempIs = ImageDetectMiku.Source;
            ImageDetectMiku.Source = ImageDetectMiku2.Source;
            ImageDetectMiku2.Source = TempIs;
        }

        private void ButtonDetect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GridDetect.Visibility = Visibility.Visible;
        }

        private void ButtonGridDetectClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GridDetect.Visibility = Visibility.Hidden;
        }

        private void ButtonGridSettingClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GridSetting.Visibility = Visibility.Hidden;
        }

        private void BottonDetectGameOverOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StoryboardDetectGameOver.Stop();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            settings["BPM"] = BpmInteger + BpmDecimal;
            settings["EnergySaving"] = CheckBoxEnergySaving.IsChecked.Value;
            settings["BeatSpan"] = ComboboxBeatSpan.SelectedIndex;
            settings.Save();
        }

        private void ButtonExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonAbout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GridAbout.Visibility = Visibility.Visible;
        }

        private void ButtonWeb_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("http://weibo.com/313458881");
        }

        private void ButtonGridAboutClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GridAbout.Visibility = Visibility.Hidden;
        }

        private void ButtonTopmost_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Topmost = !Topmost;
        }

    }
}
