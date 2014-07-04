/* 
 * Copyright (c) 2011, SwissMediaPartners AG, Mathieu Habegger
 * All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
