using Microsoft.Toolkit.Services.OneDrive;
using Microsoft.Toolkit.Services.Services.MicrosoftGraph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
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
            //await Functions.SaveToOneDrive();
            string xmlContent = string.Empty;
            if ((bool)(ApplicationData.Current.LocalSettings.Containers["signStateContainer"].Values["signState"]))
            {
                try
                {
                    var folder1 = await OneDriveService.Instance.RootFolderForMeAsync();
                    var folderList = await folder1.GetFoldersAsync();
                    foreach (var item in folderList)
                    {
                        if (item.Name == "ApplicationData")
                        {
                            foreach (var item1 in await item.GetFoldersAsync())
                            {
                                if (item1.Name == "YourDiary")
                                {
                                    foreach (var item2 in await item1.GetFilesAsync())
                                    {
                                        if (item2.Name == "diary.xml")
                                        {
                                            StorageFolder folder2 = ApplicationData.Current.LocalFolder;
                                            foreach (var item3 in await folder2.GetFilesAsync())
                                            {
                                                if (item3.Name == "diary.xml")
                                                {
                                                    FileInfo fileInfo = new FileInfo(item3.Path);
                                                    if (item2.FileSize <= fileInfo.Length||item2.DateCreated<item3.DateCreated)
                                                    {
                                                        StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/diary.xml"));
                                                        await FileToObservable(file);
                                                    }
                                                    else
                                                    {
                                                        var fileStream = await item2.StorageFilePlatformService.OpenAsync();
                                                        //using (IRandomAccessStream readStream = (IRandomAccessStream)fileStream)
                                                        //{
                                                        //    using (DataReader dataReader = new DataReader(readStream))
                                                        //    {
                                                        //        UInt64 size = readStream.Size;
                                                        //        if (size <= UInt32.MaxValue)
                                                        //        {
                                                        //            UInt32 numBytesLoaded = await dataReader.LoadAsync((UInt32)size);
                                                        //            string fileContent = dataReader.ReadString(numBytesLoaded);
                                                        //            xmlContent = fileContent;
                                                        //        }
                                                        //    }
                                                        //}

                                                        //ObservableCollection<Diary> roamingDiaries = (ObservableCollection<Diary>)Functions.Deserialize(typeof(ObservableCollection<Diary>), xmlContent);
                                                        ObservableCollection<Diary> roamingDiaries = await FileStreamToObservable((IRandomAccessStream)fileStream);
                                                        foreach (var item4 in roamingDiaries)
                                                        {
                                                            diaries.Add(item4);
                                                        }
                                                        TitleTextBlock.Text = diaries.Count + "篇日记";


                                                    }
                                                    break;
                                                    //await Functions.SaveToOneDrive();
                                                }

                                            }

                                        }

                                    }
                                }
                            }
                            break;
                        }
                        break;
                    }
                }
                catch
                {
                    MessageDialog messageDialog = new MessageDialog("网不太好，已加载本地数据");
                    await messageDialog.ShowAsync();
                    StorageFolder folder = ApplicationData.Current.LocalFolder;
                    foreach (var item in await folder.GetFilesAsync())
                    {
                        if (item.Name == "diary.xml")
                        {
                            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/diary.xml"));
                            await FileToObservable(file);
                        }
                    }

                }
                
               

            }
            else
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                foreach (var item in await folder.GetFilesAsync())
                {
                    if (item.Name == "diary.xml")
                    {
                        StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/diary.xml"));
                        await FileToObservable(file);
                    }
                }
            }
            
            
            //if ((await folder.GetFilesAsync()).Count == 1)
            //{
                
            //}
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
                if (!(bool)ApplicationData.Current.LocalSettings.Containers["signStateContainer"].Values["signState"])
                {
                    await OneDriveService.Instance.LoginAsync();
                    //MainPage.signState = 1;
                    //var localSetting = ApplicationData.Current.LocalSettings ;
                    ApplicationData.Current.LocalSettings.CreateContainer("signStateContainer", ApplicationDataCreateDisposition.Always);
                    if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("signStateContainer"))
                    {
                        ApplicationData.Current.LocalSettings.Containers["signStateContainer"].Values["signState"] = true;
                    }
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
                                foreach (var item2 in await item1.GetFilesAsync())
                                {
                                    
                                    if (item2.Name == "diary.xml")
                                    {
                                        var localFolder = ApplicationData.Current.LocalFolder;
                                        var selectedFile2 = await localFolder.TryGetItemAsync("diary.xml");
                                        var selectedFile = (StorageFile)selectedFile2;
                                        if (selectedFile != null)
                                        {
                                            FileInfo fileInfo = new FileInfo(selectedFile.Path);
                                            //var selectedFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/diary.xml"));
                                            if (item2.FileSize <= fileInfo.Length || item2.DateCreated < selectedFile.DateCreated)
                                            {
                                                var oneDriveFileStream = await item2.StorageFilePlatformService.OpenAsync();
                                                //diaries = (await FileStreamToObservable(await selectedFile.OpenAsync(FileAccessMode.Read))).Concat((await FileStreamToObservable((IRandomAccessStream)oneDriveFileStream)));
                                                ObservableCollection<Diary> middleDiaries = await FileStreamToObservable(await selectedFile.OpenAsync(FileAccessMode.Read));
                                                foreach (var item4 in (await FileStreamToObservable((IRandomAccessStream)oneDriveFileStream)))
                                                {
                                                    //if (!(middleDiaries.Contains(item4)))
                                                    //{
                                                    //    middleDiaries.Add(item4);
                                                    //}
                                                    //foreach (var item5 in middleDiaries)
                                                    //{
                                                    //    if (!(item4.DiaryDateTime.Date==item5.DiaryDateTime.Date))
                                                    //        middleDiaries.Add(item4);
                                                    //}
                                                    int i1 = 0;
                                                    foreach (var item6 in middleDiaries)
                                                    {
                                                        if ((item6.DiaryDateTime.Date == item4.DiaryDateTime.Date))
                                                            i1++;
                                                        break;
                                                    }
                                                    if (i1 == 0)
                                                        middleDiaries.Add(item4);

                                                }
                                                for (int i2 = 0; i2 < middleDiaries.Count; i2++)
                                                {
                                                    for (int i3 = i2+1; i3 < middleDiaries.Count; i3++)
                                                    {
                                                        if(middleDiaries[i2].DiaryDateTime == middleDiaries[i3].DiaryDateTime)
                                                        {
                                                            middleDiaries.Remove(middleDiaries[i3]);
                                                        }
                                                    }
                                                }
                                                if (middleDiaries[middleDiaries.Count-1].DiaryDateTime == middleDiaries[middleDiaries.Count - 2].DiaryDateTime)
                                                {
                                                    middleDiaries.Remove(middleDiaries[middleDiaries.Count-1]);
                                                }
                                                diaries = middleDiaries;

                                                Bindings.Update();
                                                TitleTextBlock.Text = diaries.Count + "篇日记";
                                                MessageDialog messageDialog = new MessageDialog("同步完成");
                                                await messageDialog.ShowAsync();
                                                break;
                                            }
                                            else
                                            {
                                                MessageDialog messageDialog = new MessageDialog("不需要同步");
                                                await messageDialog.ShowAsync();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            var oneDriveFileStream = await item2.StorageFilePlatformService.OpenAsync();
                                            diaries = await FileStreamToObservable((IRandomAccessStream)oneDriveFileStream);
                                        }
                                        
                                    }
                                    

                                }
                                
                                await Functions.SaveToXmlFile();
                                var selectedFile1 = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/diary.xml"));
                                if (selectedFile1 != null)
                                {
                                    using (var localStream = await selectedFile1.OpenReadAsync())
                                    {
                                        var fileCreated = await item1.StorageFolderPlatformService.CreateFileAsync(selectedFile1.Name, CreationCollisionOption.ReplaceExisting, localStream);
                                    }
                                }
                                break;

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
                //Functions.SaveToXmlFile();
            }
            catch
            {
                MessageDialog messageDialog = new MessageDialog("登录取消");
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

        private async  void DeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in diaries)
            {
                if (deleteDiary.DiaryDateTime.Date == item.DiaryDateTime.Date)
                {
                    diaries.Remove(item);
                    break;
                }
            }
            await Functions.SaveToOneDrive();
            TitleTextBlock.Text = diaries.Count + "篇日记";
        }

        private async void SignOutAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await OneDriveService.Instance.LogoutAsync();
                MessageDialog messageDialog = new MessageDialog("注销成功");
                ApplicationData.Current.LocalSettings.CreateContainer("signStateContainer", ApplicationDataCreateDisposition.Always);
                if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("signStateContainer"))
                {
                    ApplicationData.Current.LocalSettings.Containers["signStateContainer"].Values["signState"] = false;
                }
                await messageDialog.ShowAsync();
            }
            catch
            {
                MessageDialog messageDialog = new MessageDialog("你没有登录");
                await messageDialog.ShowAsync();
            }
            
        }

        private async Task FileToObservable(StorageFile file)
        {
            string xmlContent = string.Empty;
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
            foreach (var item1 in roamingDiaries)
            {
                diaries.Add(item1);
            }
            TitleTextBlock.Text = diaries.Count + "篇日记";
        }

        private async Task<ObservableCollection<Diary>> FileStreamToObservable(IRandomAccessStream fileStream)
        {
            string xmlContent = string.Empty;
            using (IRandomAccessStream readStream = fileStream)
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
            return roamingDiaries;
            //foreach (var item1 in roamingDiaries)
            //{
            //    diaries.Add(item1);
            //}
            //TitleTextBlock.Text = diaries.Count + "篇日记";
        }

        
    }
}
