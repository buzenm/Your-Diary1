using Microsoft.Toolkit.Services.OneDrive;
using Microsoft.Toolkit.Services.Services.MicrosoftGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Your_Diary1.MyPages;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Your_Diary1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //表示你登没登陆 0==玫登录 1==登录
        //public static bool signState = false;
        public static MainPage current;
        public MainPage()
        {
            string appClientId = "d9f9e643-c8d6-4eb7-b38e-1878674fc7be";
            string[] scopes = new string[] { MicrosoftGraphScope.FilesReadWriteAll }; ;
            OneDriveService.Instance.Initialize
                (appClientId,
                 scopes,
                 null,
                 null);
            ApplicationData.Current.LocalSettings.CreateContainer("signStateContainer", ApplicationDataCreateDisposition.Always);
            if (!(ApplicationData.Current.LocalSettings.Containers["signStateContainer"].Values["signState"] is bool))
            {
                ApplicationData.Current.LocalSettings.Containers["signStateContainer"].Values["signState"] = false;
            }
            this.InitializeComponent();
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.Pink;
            titleBar.ButtonBackgroundColor = Colors.Pink;
            ContentFrame.Navigate(typeof(ContentPage));
            PaneFrame.Navigate(typeof(PanePage));
            current = this;
        }
    }
}
