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
using System.Linq;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;
using System.Reflection;
using System.Windows;
using System.Collections;

namespace io.ebu.eis.datastructures.Plain.Collections
{
    public class DispatchEvent
    {
        #region Data Member

        private List<DispatchHandler> handlerList = new List<DispatchHandler>();

        #endregion

        #region Expose Methods

        public void Add(Delegate handler)
        {
            this.Add(handler, Dispatcher.CurrentDispatcher);
        }

        public void Add(Delegate handler, Dispatcher dispatcher)
        {
            handlerList.Add(new DispatchHandler(handler, dispatcher));
        }

        public void Remove(Delegate handler)
        {
            var rmvHandlers = (from dispatchHandler in handlerList
                               where dispatchHandler.DelegateEquals(handler)
                               select dispatchHandler).ToArray();

            if (rmvHandlers != null && rmvHandlers.Length > 0)
            {
                this.handlerList.Remove(rmvHandlers[0]);
                rmvHandlers[0].Dispose();
            }
        }

        public void Clear()
        {
            foreach (DispatchHandler handler in this.handlerList)
            {
                handler.Dispose();
            }
            this.handlerList.Clear();
        }

        public void Fire(object sender, EventArgs args)
        {
            var disposableHandler = from handler in handlerList
                                    where handler.IsDisposable
                                    select handler;
            foreach (DispatchHandler rmvHandler in disposableHandler.ToArray())
            {
                this.handlerList.Remove(rmvHandler);
                rmvHandler.Dispose();
            }

            foreach (DispatchHandler handler in handlerList)
                handler.Invoke(sender, args);
        }

        #endregion

        #region DispatchHandler Class

        private class DispatchHandler : IDisposable
        {
            private MethodInfo handlerInfo;
            private WeakReference targetRef;
            private WeakReference dispatcherRef;

            public DispatchHandler(Delegate handler, Dispatcher dispatcher)
            {
                this.handlerInfo = handler.Method;
                this.targetRef = new WeakReference(handler.Target);
                this.dispatcherRef = new WeakReference(dispatcher);
            }

            private Dispatcher Dispatcher
            {
                get { return (Dispatcher)this.dispatcherRef.Target; }
            }

            private object Target
            {
                get { return this.targetRef.Target; }
            }

            private bool IsDispatcherThreadAlive
            {
                get { return this.Dispatcher.Thread.IsAlive; }
            }

            public bool IsDisposable
            {
                get
                {
                    object target = this.Target;
                    Dispatcher dispatcher = this.Dispatcher;

                    return (target == null
                            || dispatcher == null
                            || (target is DispatcherObject &&
                               (dispatcher.Thread.ThreadState & (ThreadState.Aborted
                                                                | ThreadState.Stopped
                                                                | ThreadState.StopRequested
                                                                | ThreadState.AbortRequested)) != 0));
                }
            }

            public void Invoke(object arg, params object[] args)
            {
                try
                {
                    object target = this.Target;
                    Dispatcher dispatcher = this.Dispatcher;

                    if (!this.IsDisposable)
                    {
                        if (this.IsDispatcherThreadAlive)
                        {
                            dispatcher.Invoke(DispatcherPriority.Send, new EventHandler(
                                                                                        delegate(object sender, EventArgs e)
                                                                                        {
                                                                                            this.handlerInfo.Invoke(target, new object[] { arg, e });
                                                                                        }), arg, args);
                        }
                        else if (target is DispatcherObject)
                        {
                            dispatcher.BeginInvoke(DispatcherPriority.Send, new EventHandler(
                                                                                            delegate(object sender, EventArgs e)
                                                                                            {
                                                                                                this.handlerInfo.Invoke(target, new object[] { arg, e });
                                                                                            }), arg, args);
                        }
                        else
                        {
                            ArrayList paramList = new ArrayList();
                            paramList.Add(arg);
                            paramList.AddRange(args);
                            this.handlerInfo.Invoke(target, paramList.ToArray());
                        }

                    }
                }
                catch (Exception) { }
            }

            public bool DelegateEquals(Delegate other)
            {
                object target = this.Target;
                return (target != null
                        && object.ReferenceEquals(target, other.Target)
                        && this.handlerInfo.Name == other.Method.Name);
            }

            public void Dispose()
            {
                this.targetRef = null;
                this.handlerInfo = null;
                this.dispatcherRef = null;
            }

        }

        #endregion

    }
}
