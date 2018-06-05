using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
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

        public async static void SaveToXmlFile()
        {
            string xmlContent = Functions.Serialize(DiaryListVIewPage.current.diaries);
            //xmlDocument.LoadXml(xmlContent);
            StorageFolder folder = ApplicationData.Current.LocalFolder;

            StorageFile file = await folder.CreateFileAsync("diary.xml", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, xmlContent);
        }


    }
}
