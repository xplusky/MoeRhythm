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
using Alexis.WindowsPhone.Social;

namespace MoeRhythm
{
    public class Constants
    {
        public const string SHARE_IMAGE = "share.jpg";

        public static ClientInfo GetClient(SocialType type)
        {
            ClientInfo client = new ClientInfo();
            switch (type)
            {
                case SocialType.Weibo:
                    client.ClientId = "2771170504";
                    client.ClientSecret = "6a0c24df1abf9e6ee57c759110df414e";
                    //client.RedirectUri = "http://weibo.com/";//if not set,left this property empty
                    break;
                case SocialType.Tencent:
                    client.ClientId = "";
                    client.ClientSecret = "";
                    break;
                case SocialType.Renren:
                    client.ClientId = "";
                    client.ClientSecret = "";
                    break;
                default:
                    break;
            }
            return client;
        }
    }
}
