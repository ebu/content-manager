using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using io.ebu.eis.datastructures.Plain.Collections;
using io.ebu.eis.shared;

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
            int currentIndex;
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
            var currentSlide = Slides.FirstOrDefault(x => x.IsActive);
            var currentIndex = currentSlide != null ? Slides.IndexOf(currentSlide) : 0;
            var nextIndex = (currentIndex + 1) % Slides.Count;

            return Slides[nextIndex];
        }

        public ManagerCart Clone(bool clonePreviewImages)
        {
            var newCart = new ManagerCart(Name);
            foreach (var s in Slides)
            {
                newCart.Slides.Add(s.Clone(clonePreviewImages));
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
