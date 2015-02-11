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


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace io.ebu.eis.datastructures.Plain.Collections
{
    public class ObservableLinkedList<T> : LinkedList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        DispatchEvent collectionChanged = new DispatchEvent();
        DispatchEvent propertyChanged = new DispatchEvent();

        public ObservableLinkedList() { }
        public ObservableLinkedList(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                base.AddLast(item);
        }

        #region 
        #endregion
        public new virtual void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            base.AddAfter(node, newNode);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newNode));
        }
        public new virtual void AddAfter(LinkedListNode<T> node, T value)
        {
            base.AddAfter(node, value);
            var newNode = node.Next;
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newNode));
        }
        public new virtual void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            base.AddBefore(node, newNode);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newNode));
        }
        public new virtual void AddBefore(LinkedListNode<T> node, T value)
        {
            base.AddBefore(node, value);
            var newNode = node.Previous;
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newNode));
        }
        public new virtual void AddFirst(T value)
        {
            base.AddFirst(value);
            var node = base.First;
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node));
        }
        public new virtual void AddFirst(LinkedListNode<T> node)
        {
            base.AddFirst(node);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node));
        }
        public new virtual void AddLast(T value)
        {
            base.AddLast(value);
            var node = base.Last;
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node));
        }
        public new virtual void AddLast(LinkedListNode<T> node)
        {
            base.AddLast(node);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node));
        }
        public new virtual void Clear()
        {
            base.Clear();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public new virtual void Remove(T value)
        {
            var node = base.Find(value);
            base.Remove(value);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node));
        }
        public new virtual void Remove(LinkedListNode<T> node)
        {
            base.Remove(node);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node));
        }
        public new virtual void RemoveFirst()
        {
            var node = base.First;
            base.RemoveFirst();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node));
        }
        public new virtual void RemoveLast()
        {
            var node = base.Last;
            base.RemoveLast();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node));
        }


        //public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
        //protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        //{
        //    this.RaiseCollectionChanged(e);
        //}

        //protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        //{
        //    this.RaisePropertyChanged(e);
        //}


        //protected virtual event PropertyChangedEventHandler PropertyChanged;
        //private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        //{

        //    if (this.CollectionChanged != null)
        //        this.CollectionChanged(this, e);

        //}

        //private void RaisePropertyChanged(PropertyChangedEventArgs e)
        //{
        //    if (this.PropertyChanged != null)
        //        this.PropertyChanged(this, e);
        //}


        //event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        //{
        //    add { this.PropertyChanged += value; }
        //    remove { this.PropertyChanged -= value; }
        //}

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            //base.OnCollectionChanged(e);
            this.collectionChanged.Fire(this, e);
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            //base.OnPropertyChanged(e);
            this.propertyChanged.Fire(this, e);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { this.collectionChanged.Add(value); }
            remove { this.collectionChanged.Remove(value); }
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { this.propertyChanged.Add(value); }
            remove { this.propertyChanged.Remove(value); }
        }
    }
}
