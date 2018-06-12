using Microsoft.Toolkit.Services.OneDrive;
using Microsoft.Toolkit.Services.Services.MicrosoftGraph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
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
    public sealed partial class DiaryListVIewPage : Page
    {
        public Diary deleteDiary = new Diary();
        public static DiaryListVIewPage current;
        public Diary diary = new Diary();
        public ObservableCollection<Diary> diaries = new ObservableCollection<Diary>();
        public DiaryListVIewPage()
        {
            this.InitializeComponent();
            current = this;
            //diaries.Add(new Diary { DiaryDateTime = DateTime.Parse("2018-6-2"), DiaryWeather = "mucn cloud", DiaryContent = "example" });
            //diaries.Insert(0, new Diary { DiaryDateTime = DateTime.Parse("2018-6-1"), DiaryWeather = "rain", DiaryContent = "shahaha" });
            
            //this.DataContextChanged += (s, e) => Bindings.Update();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Using the new converged authentication of the Microsoft Graph we can simply
            // call the Initialize method on the OneDriveService singleton when initializing
            // in UWP applications
            string appClientId = "d9f9e643-c8d6-4eb7-b38e-1878674fc7be";
            string[] scopes = new string[] { MicrosoftGraphScope.FilesReadWriteAll }; ;
            OneDriveService.Instance.Initialize
                (appClientId,
                 scopes,
                 null,
                 null);

            string xmlContent = string.Empty;
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if ((await folder.GetFilesAsync()).Count == 1)
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/diary.xml"));
                using (IRandomAccessStream readStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    using (DataReader dataReader = new DataReader(readStream))
                    {
                        UInt64 size = readStream.Size;
                        if (size <= UInt32.MaxValue)
                        {
                            UInt32 numBytesLoaded = await dataReader.LoadAsync((UInt32)size);
                            string fileContent = dataReader.ReadString(numBytesLoaded);
                            xmlContent = fileContent;
                        }
                    }
                }
                ObservableCollection<Diary> roamingDiaries = (ObservableCollection<Diary>)Functions.Deserialize(typeof(ObservableCollection<Diary>), xmlContent);
                foreach (var item in roamingDiaries)
                {
                    diaries.Add(item);
                }
                TitleTextBlock.Text = diaries.Count + "篇日记";
            }
        }

        private void YourDiaryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ContentPage.current.RightFrame.Navigate(typeof(DiaryContentPage),e.ClickedItem);
            
        }

        

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //Canvas.SetZIndex(MyImage2, 4);
            //Canvas.SetZIndex(YourDiaryListView, 5);
            //Canvas.SetZIndex(TitleTextBlock, 4);
            //Canvas.SetZIndex(ListViewCommandBar, 4);
            
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach (var item in DiaryListVIewPage.current.diaries)
            {
                if (item.DiaryDateTime.Date == DateTime.Now.Date)
                {
                    i++;
                    diary = item;
                }
            }
            if (i == 0)
            {
                ContentPage.current.RightFrame.Navigate(typeof(DiaryContentPage), new Diary { DiaryDateTime = DateTime.Now, DiaryContent = string.Empty });
            }
            else if (i == 1)
            {
                ContentPage.current.RightFrame.Navigate(typeof(DiaryContentPage),diary);
            }
            
            Canvas.SetZIndex(ContentPage.current.RightFrame, 1);
            Canvas.SetZIndex(ContentPage.current.LeftFrame, 0);
            //Bindings.Update();
        }

        private void YourDiaryListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            //foreach (var item in diaries)
            //{
            //    if (item.DiaryDateTime == ((Diary)YourDiaryListView.SelectedItem).DiaryDateTime)
            //        diaries.Remove(item);
            //}
            deleteDiary = (e.OriginalSource as FrameworkElement)?.DataContext as Diary;
            RightTapMenuFlyout.ShowAt(YourDiaryListView, e.GetPosition(this.YourDiaryListView));


        }

        private async void SyncAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Functions.SaveToXmlFile();


            // If the user hasn't selected a scope then set it to FilesReadAll
            //if (scopes == null)
            //{
            //    scopes = new string[] { MicrosoftGraphScope.FilesReadAll };
            //}

            // Login
            try
            {
                if (!await OneDriveService.Instance.LoginAsync())
                {
                    throw new Exception("Unable to sign in");
                }

                var folder = await OneDriveService.Instance.RootFolderForMeAsync();
                var folderList = await folder.GetFoldersAsync();
                foreach (var item in folderList)
                {
                    if (item.Name == "ApplicationData")
                    {
                        int i = 0;
                        foreach (var item1 in await item.GetFoldersAsync())
                        {
                            if (item1.Name == "YourDiary")
                            {
                                i++;
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

                        if (i == 0)
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
            catch(Exception w)
            {
                MessageDialog messageDialog = new MessageDialog(w.Message);
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

        private void DeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in diaries)
            {
                if (deleteDiary.DiaryDateTime.Date == item.DiaryDateTime.Date)
                {
                    diaries.Remove(item);
                    break;
                }
            }
        }

        private async void SignOutAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await OneDriveService.Instance.LogoutAsync();
        }
    }
}
