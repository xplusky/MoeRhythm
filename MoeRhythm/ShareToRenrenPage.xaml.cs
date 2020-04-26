using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;

namespace MoeRhythm
{
    public partial class ShareToRenrenPage : PhoneApplicationPage
    {
        public ShareToRenrenPage()
        {
            InitializeComponent();
        }

        private void ButtonSend_Click(object sender, System.EventArgs e)
        {
            App.renrenApi.PublishPhoto("share.jpg", ShareToRenenCallBack, TextboxMessage.Text, 0);
        }

        private void ShareToRenenCallBack(object sender, RenrenSDKLibrary.UploadPhotoCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //this.textBox2.Text = e.Error.ToString();
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                //this.textBox2.Text = e.Result.ToString();
                MessageBox.Show("上传成功");
                BitmapImage smallImage = new BitmapImage();
                smallImage.UriSource = new Uri(e.Result.src_small, UriKind.RelativeOrAbsolute);
                //image1.Source = smallImage;
            }
            Deployment.Current.Dispatcher.BeginInvoke(() => { NavigationService.GoBack(); });
        }

        private void ButtonCancel_Click(object sender, System.EventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => { NavigationService.GoBack(); });
        }

        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            
            TextboxMessage.Text = Message;

            try
            {
                BitmapImage bmp = new BitmapImage();
                bmp.CreateOptions = BitmapCreateOptions.BackgroundCreation;
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile("share.jpg", FileMode.Open, FileAccess.Read))
                {
                    bmp.SetSource(fileStream);
                }

                this.ImageShare.Source = bmp;
            }
            catch { }
            
            //this.ImageShare.Visibility = Visibility.Visible;
        }

        public static string Message { get; set; }

        public static BitmapImage ShareImage { get; set; }
        
    }
}