using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Your_Diary1.MyClasses
{
    [XmlRoot("diary")]
    public class Diary:INotifyPropertyChanged
    {
        private DateTime diaryDateTime;
        [XmlElement("diaryDateTime")]
        public DateTime DiaryDateTime
        { get { return diaryDateTime; }
            set
            {
                if (value != diaryDateTime)
                {
                    diaryDateTime = value;
                    OnPropertyChanged("DiaryDateTime");
                }
            }
        }
        private string diaryWeather;
        [XmlElement("diaryWeather")]
        public string DiaryWeather
        {
            get
            {
                return diaryWeather;
            }
            set
            {
                if (value != diaryWeather)
                {
                    diaryWeather = value;
                    OnPropertyChanged("DiaryWeather");
                }
            }
        }
        private string diaryContent;
        [XmlElement("diaryContent")]
        public string DiaryContent
        {
            get
            {
                return diaryContent;
            }
            set
            {
                if (value != diaryContent)
                {
                    diaryContent = value;
                    OnPropertyChanged("DiaryContent");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
