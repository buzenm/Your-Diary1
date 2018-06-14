using Microsoft.Toolkit.Services.OneDrive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Your_Diary1.MyClasses;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Your_Diary1.MyPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DiaryContentPage : Page
    {
        public string weather = string.Empty;
        public Diary oneDiary = new Diary();
        public DiaryContentPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            base.OnNavigatedTo(e);
            oneDiary = (Diary)(e.Parameter);
            if (oneDiary != null)
            {
                TitleTextBlock1.Text = oneDiary.DiaryDateTime.ToString();
                ContentTextBox.Text = oneDiary.DiaryContent;
                //if (oneDiary.DiaryWeather == "晴")
                //{
                //    WeatherComboBox.SelectedIndex = 0;
                //}
                //else if(oneDiary.DiaryWeather=="多云")
                switch (oneDiary.DiaryWeather)
                {
                    case "晴":
                        WeatherComboBox.SelectedIndex = 0;
                        break;
                    case "多云":
                        WeatherComboBox.SelectedIndex = 1;
                        break;
                    case "阴":
                        WeatherComboBox.SelectedIndex = 2;
                        break;
                    case "雨":
                        WeatherComboBox.SelectedIndex = 3;
                        break;
                }
                
            }
            
            //Canvas.SetZIndex(MyImage1, 0);
            //Canvas.SetZIndex(ContentTextBox, 1);
            //Canvas.SetZIndex(ContentCommandBar, 1);
            //Canvas.SetZIndex(TitleTextBlock1, 1);
            Canvas.SetZIndex(ContentPage.current.RightFrame, 1);
            Canvas.SetZIndex(ContentPage.current.LeftFrame, 0);
        }

        private async void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            
            int i = 0;
            oneDiary.DiaryContent = ContentTextBox.Text;
            Canvas.SetZIndex(ContentPage.current.RightFrame, 0);
            Canvas.SetZIndex(ContentPage.current.LeftFrame, 1);
            foreach (var item in DiaryListVIewPage.current.diaries)
            {
                if (item.DiaryDateTime.Date == Convert.ToDateTime(TitleTextBlock1.Text).Date)
                {
                    i++;
                }
            }

            if (i == 0)
            {
                if (WeatherComboBox.SelectedItem != null)
                {
                    DiaryListVIewPage.current.diaries.Insert(0, new Diary
                    {
                        DiaryDateTime = Convert.ToDateTime(TitleTextBlock1.Text),
                        DiaryContent = ContentTextBox.Text,
                        DiaryWeather = WeatherComboBox.SelectedItem.ToString()
                    });
                }
                else
                {
                    DiaryListVIewPage.current.diaries.Insert(0, new Diary
                    {
                        DiaryDateTime = Convert.ToDateTime(TitleTextBlock1.Text),
                        DiaryContent = ContentTextBox.Text,
                        
                    });
                }
            }
            else if (i == 1)
            {
                foreach (var item in DiaryListVIewPage.current.diaries)
                {
                    if (item.DiaryDateTime.Date == Convert.ToDateTime(TitleTextBlock1.Text).Date)
                    {
                        item.DiaryContent = ContentTextBox.Text;
                        if (WeatherComboBox.SelectedItem != null)
                        {
                            item.DiaryWeather = WeatherComboBox.SelectedItem.ToString();
                        }
                    }
                }
            }
            DiaryListVIewPage.current.TitleTextBlock.Text = DiaryListVIewPage.current.diaries.Count + "篇日记";
            //ContentPage.current.LeftFrame.Navigate(typeof(DiaryListVIewPage));
            await Functions.SaveToXmlFile();

            // If the user hasn't selected a scope then set it to FilesReadAll
            //if (scopes == null)
            //{
            //    scopes = new string[] { MicrosoftGraphScope.FilesReadAll };
            //}

            // Login
            try
            {
                //if (!await OneDriveService.Instance.LoginAsync())
                //{
                //    throw new Exception("Unable to sign in");
                //}
                //else
                //{
                //    throw new Exception("has been login");
                //}
                //if (MainPage.signState == 1)
                //{

                //}
                if ((bool)(ApplicationData.Current.LocalSettings.Containers["signStateContainer"].Values["signState"]))
                {
                    var folder = await OneDriveService.Instance.RootFolderForMeAsync();
                    var folderList = await folder.GetFoldersAsync();
                    foreach (var item in folderList)
                    {
                        if (item.Name == "ApplicationData")
                        {
                            int i1 = 0;
                            foreach (var item1 in await item.GetFoldersAsync())
                            {
                                if (item1.Name == "YourDiary")
                                {
                                    i1++;
                                    var selectedFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/diary.xml"));
                                    if (selectedFile != null)
                                    {
                                        using (var localStream = await selectedFile.OpenReadAsync())
                                        {
                                            var fileCreated = await item1.StorageFolderPlatformService.CreateFileAsync(selectedFile.Name, CreationCollisionOption.ReplaceExisting, localStream);
                                        }
                                    }
                                }
                            }

                            if (i1 == 0)
                            {
                                // Then from there you can play with folders and files
                                // Create Folder
                                string newFolderName = "YourDiary";
                                if (!string.IsNullOrEmpty(newFolderName))
                                {
                                    await item.StorageFolderPlatformService.CreateFolderAsync(newFolderName, CreationCollisionOption.OpenIfExists);
                                }
                                foreach (var item1 in await item.GetFoldersAsync())
                                {
                                    var selectedFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/diary.xml"));
                                    if (selectedFile != null)
                                    {
                                        using (var localStream = await selectedFile.OpenReadAsync())
                                        {
                                            var fileCreated = await item1.StorageFolderPlatformService.CreateFileAsync(selectedFile.Name, CreationCollisionOption.ReplaceExisting, localStream);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                
            }
            catch (Exception w)
            {
                
                var messageDialog = new MessageDialog(w.Message);
                await messageDialog.ShowAsync();
            }

            
            //// Once you have a reference to the Root Folder you can get a list of all items
            //// List the Items from the current folder
            //var OneDriveItems = await folder.GetItemsAsync();
            //do
            //{
            //    // Get the next page of items
            //    OneDriveItems = await folder.NextItemsAsync();
            //}
            //while (OneDriveItems != null);

            // Open the local file or create a local file if brand new

        }


    }
}
