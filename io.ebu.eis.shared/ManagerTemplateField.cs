using System.ComponentModel;
using System.Runtime.Serialization;

namespace io.ebu.eis.shared
{
    [DataContract]
    public class ManagerTemplateField : INotifyPropertyChanged
    {
        public ManagerTemplateField(){}

        public ManagerTemplateField(string title, string value)
        {
            Title = title;
            Value = value;
        }

        [DataMember(Name = "title")]
        private string _title;
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged("Title"); } }

        [DataMember(Name = "value")]
        private string _value;
        public string Value { get { return _value; } set { _value = value; OnPropertyChanged("Value"); } }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }
        #endregion PropertyChanged
    }
}
