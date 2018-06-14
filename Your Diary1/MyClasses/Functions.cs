using Microsoft.Toolkit.Services.OneDrive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Pickers;
using Your_Diary1.MyPages;

namespace Your_Diary1.MyClasses
{
    public class Functions
    {
        public static string Serialize<T>(T t)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializer xz = new XmlSerializer(t.GetType());
                xz.Serialize(sw, t);
                return sw.ToString();
            }
        }

        public static object Deserialize(Type type, string s)
        {
            using (StringReader sr = new StringReader(s))
            {
                XmlSerializer xz = new XmlSerializer(type);
                return xz.Deserialize(sr);
            }
        }

        public async static Task SaveToXmlFile()
        {
            string xmlContent = Functions.Serialize(DiaryListVIewPage.current.diaries);
            //xmlDocument.LoadXml(xmlContent);
            StorageFolder folder = ApplicationData.Current.LocalFolder;

            StorageFile file = await folder.CreateFileAsync("diary.xml", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, xmlContent);
        }

        public async static Task SaveToOneDrive()
        {
            var folder = await OneDriveService.Instance.RootFolderForMeAsync();
            var folderList = await folder.GetFoldersAsync();
            foreach (var item in folderList)
            {
                if (item.Name == "ApplicationData")
                {
                    //int i = 0;
                    foreach (var item1 in await item.GetFoldersAsync())
                    {
                        if (item1.Name == "YourDiary")
                        {
                            await SaveToXmlFile();
                            var selectedFile1 = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/diary.xml"));
                            if (selectedFile1 != null)
                            {
                                using (var localStream = await selectedFile1.OpenReadAsync())
                                {
                                    var fileCreated = await item1.StorageFolderPlatformService.CreateFileAsync(selectedFile1.Name, CreationCollisionOption.ReplaceExisting, localStream);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
