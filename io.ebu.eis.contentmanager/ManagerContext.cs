﻿using io.ebu.eis.datastructures;
using io.ebu.eis.datastructures.Plain.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.ebu.eis.contentmanager
{
    public class ManagerContext : INotifyPropertyChanged
    {
        private DispatchedObservableCollection<EventFlow> _runningEvents;
        public DispatchedObservableCollection<EventFlow> RunningEvents { get { return _runningEvents; } set { _runningEvents = value; OnPropertyChanged("RunningEvents"); } }

        private DispatchedObservableCollection<DataFlowItem> _dataFlowItems;
        public DispatchedObservableCollection<DataFlowItem> DataFlowItems { get { return _dataFlowItems; } set { _dataFlowItems = value; OnPropertyChanged("DataFlowItems"); } }

        private DispatchedObservableCollection<DataFlowItem> _imageFlowItems;
        public DispatchedObservableCollection<DataFlowItem> ImageFlowItems { get { return _imageFlowItems; } set { _imageFlowItems = value; OnPropertyChanged("ImageFlowItems"); } }


        public ManagerContext()
        {
            RunningEvents = new DispatchedObservableCollection<EventFlow>();
            DataFlowItems = new DispatchedObservableCollection<DataFlowItem>();
            ImageFlowItems = new DispatchedObservableCollection<DataFlowItem>();
        }

        public void DummyData()
        {
            // Dummy Data
            EventFlow e0 = new EventFlow() { Name = "General" };
            EventFlow e1 = new EventFlow() { Name = "100M Men" };
            EventFlow e2 = new EventFlow() { Name = "High Jump Women" };
            EventFlow e3 = new EventFlow() { Name = "4x100m Relay Women" };
            EventFlow e4 = new EventFlow() { Name = "400m Hurdles Men" };

            RunningEvents.Add(e0);
            RunningEvents.Add(e1);
            RunningEvents.Add(e2);
            RunningEvents.Add(e3);
            RunningEvents.Add(e4);

            DataFlowItem d1 = new DataFlowItem() { Name = "Data 15", Category = "100M", Type = "Results", Short="Short text describing this", Priority=DataFlowPriority.High };
            DataFlowItem d2 = new DataFlowItem() { Name = "Data 25", Category = "ABC", Type = "Results", Short = "Short text describing this", Priority = DataFlowPriority.Medium };
            DataFlowItem d3 = new DataFlowItem() { Name = "Data 35", Category = "DDC", Type = "StartList", Short = "Short text describing this", Priority = DataFlowPriority.Low };
            DataFlowItem d4 = new DataFlowItem() { Name = "Data 45", Category = "DDC", Type = "Results", Short = "Short text describing this", Priority = DataFlowPriority.Neglectable };
            DataFlowItem d5 = new DataFlowItem() { Name = "Data 55", Category = "DDC", Type = "Official Results", Short = "Short text describing this", Priority = DataFlowPriority.Medium };
            DataFlowItem d6 = new DataFlowItem() { Name = "Data 65", Category = "DDC", Type = "Results", Short = "Short text describing this", Priority = DataFlowPriority.Neglectable };

            DataFlowItems.Add(d1);
            DataFlowItems.Add(d2);
            DataFlowItems.Add(d3);
            DataFlowItems.Add(d4);
            DataFlowItems.Add(d5);
            DataFlowItems.Add(d6);


            DataFlowItem i1 = new DataFlowItem() { Name = "Image 1", Category = "Getty", Type = "Image", Short = "Short text describing this", Priority = DataFlowPriority.High };
            i1.LoadImageFromUrl(@"Z:\mh On My Mac\EBU\Assets\GETTY Images Day 1 Braunschweig\450987746.jpg");
            DataFlowItem i2 = new DataFlowItem() { Name = "Image 2", Category = "Getty", Type = "Image", Short = "Short text describing this", Priority = DataFlowPriority.Medium };
            i2.LoadImageFromUrl(@"Z:\mh On My Mac\EBU\Assets\GETTY Images Day 1 Braunschweig\450987748.jpg");
            DataFlowItem i3 = new DataFlowItem() { Name = "Image 3", Category = "Getty", Type = "Image", Short = "Short text describing this", Priority = DataFlowPriority.Low };
            i3.LoadImageFromUrl(@"Z:\mh On My Mac\EBU\Assets\GETTY Images Day 1 Braunschweig\450987750.jpg");

            ImageFlowItems.Add(i1);
            ImageFlowItems.Add(i2);
            ImageFlowItems.Add(i3);
            
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