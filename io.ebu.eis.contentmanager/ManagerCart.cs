using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using io.ebu.eis.datastructures;
using io.ebu.eis.datastructures.Plain.Collections;

namespace io.ebu.eis.contentmanager
{
    [DataContract]
    public class ManagerCart : INotifyPropertyChanged
    {

        public ManagerCart(string name)
        {
            Name = name;
            Slides = new DispatchedObservableCollection<ManagerImageReference>();
        }

        private string _name;
        [DataMember(Name = "name")]
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }

        private bool _isActive;
        [DataMember(Name = "isactive")]
        public bool IsActive { get { return _isActive; } set { _isActive = value; OnPropertyChanged("IsActive"); } }

        private bool _showInCartList = true;
        [DataMember(Name = "showincartlist")]
        public bool ShowInCartList { get { return _showInCartList; } set { _showInCartList = value; OnPropertyChanged("ShowInCartList"); } }

        private bool _canBeDeleted = true;
        [DataMember(Name = "canbedeleted")]
        public bool CanBeDeleted { get { return _canBeDeleted; } set { _canBeDeleted = value; OnPropertyChanged("CanBeDeleted"); } }


        private DispatchedObservableCollection<ManagerImageReference> _slides;
        [DataMember(Name = "slides")]
        public DispatchedObservableCollection<ManagerImageReference> Slides { get { return _slides; } set { _slides = value; OnPropertyChanged("Slides"); } }

        public ManagerImageReference GetNextSlide()
        {
            var currentIndex = 0;
            var currentSlide = Slides.FirstOrDefault(x => x.IsActive);
            if (currentSlide != null)
                currentIndex = Slides.IndexOf(currentSlide);
            else
                currentIndex = -1;
            var nextIndex = (currentIndex + 1)%Slides.Count;

            SetAllSlidesInactive();
            var slide = Slides[nextIndex];
            slide.LastUsed = DateTime.Now;
            slide.IsActive = true;
            return slide;
        }

        public void SetAllSlidesInactive()
        {
            foreach (var s in Slides)
            {
                s.IsActive = false;
            }
        }

        public ManagerImageReference PreviewNextSlide()
        {
            var currentIndex = 0;
            var currentSlide = Slides.FirstOrDefault(x => x.IsActive);
            if (currentSlide != null)
                currentIndex = Slides.IndexOf(currentSlide);
            else
                currentIndex = 0;
            var nextIndex = (currentIndex + 1) % Slides.Count;

            return Slides[nextIndex];
        }

        public ManagerCart Clone()
        {
            var newCart = new ManagerCart(this.Name);
            foreach (var s in this.Slides)
            {
                newCart.Slides.Add(s.Clone());
            }
            newCart.SetAllSlidesInactive();
            return newCart;
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
