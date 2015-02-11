/* 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2011 SwissMediaPartners AG, Mathieu Habegger
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * https://gist.github.com/bb460e2521127020b615.git
 */


using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System;

namespace io.ebu.eis.datastructures.Plain.Collections
{
    public class DispatchedObservableCollection<Titem> : ObservableCollection<Titem>
    {
        DispatchEvent collectionChanged = new DispatchEvent();
        DispatchEvent propertyChanged = new DispatchEvent();

        public DispatchedObservableCollection()
        { }

        public DispatchedObservableCollection(List<Titem> list)
            : base(list)
        { }

        public DispatchedObservableCollection(Dispatcher dispatcher)
        { }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            //base.OnCollectionChanged(e);
            this.collectionChanged.Fire(this, e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            //base.OnPropertyChanged(e);
            this.propertyChanged.Fire(this, e);            
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { this.collectionChanged.Add(value); }
            remove { this.collectionChanged.Remove(value); }
        }

        protected override event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged.Add(value); }
            remove { this.propertyChanged.Remove(value); }
        }        
    }
}
