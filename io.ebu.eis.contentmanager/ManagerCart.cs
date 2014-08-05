using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using io.ebu.eis.datastructures;
using io.ebu.eis.datastructures.Plain.Collections;

namespace io.ebu.eis.contentmanager
{
    public class ManagerCart : INotifyPropertyChanged
    {

        public ManagerCart(string name)
        {
            Name = name;
            Slides = new DispatchedObservableCollection<ManagerImageReference>();
        }

        private string _name;
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }


        private DispatchedObservableCollection<ManagerImageReference> _slides;
        public DispatchedObservableCollection<ManagerImageReference> Slides { get { return _slides; } set { _slides = value; OnPropertyChanged("Slides"); } }

        public ManagerImageReference GetNextSlide()
        {
            var slide = Slides.OrderBy(x => x.LastUsed).FirstOrDefault();
            slide.LastUsed = DateTime.Now;
            return slide;
        }

        public ManagerImageReference PreviewNextSlide()
        {
            return Slides.OrderBy(x => x.LastUsed).FirstOrDefault();
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
