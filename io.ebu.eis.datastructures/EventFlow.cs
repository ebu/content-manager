using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.ebu.eis.datastructures
{
    public class EventFlow : INotifyPropertyChanged
    {

        private string _name;
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }


        public override string ToString()
        {
            return Name;
        }


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
