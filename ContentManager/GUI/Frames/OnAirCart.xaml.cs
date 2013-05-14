/*
Copyright (C) 2010-2012 European Broadcasting Union
http://www.ebulabs.org
*/
/*
This file is part of ebu-content-manager.
https://code.google.com/p/ebu-content-manager/

EBU-content-manager is free software: you can redistribute it and/or modify
it under the terms of the GNU LESSER GENERAL PUBLIC LICENSE as
published by the Free Software Foundation, version 3.
EBU-content-manager is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU LESSER GENERAL PUBLIC LICENSE for more details.

You should have received a copy of the GNU LESSER GENERAL PUBLIC LICENSE
along with EBU-content-manager.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ContentManager.DataStructure;
using System.Collections;
using System.Windows.Threading;

namespace ContentManager.GUI.Frames
{
    /// <summary>
    /// Interaction logic for OnAirCart.xaml
    /// </summary>
    public partial class OnAirCart : UserControl
    {
        public OnAirCart()
        {
            InitializeComponent();
        }

        void engine_onChangeCart(List<string> slides)
        {
            this.stack.Children.Clear();
            
            foreach (String s in slides)
            {
                Button b = new Button();
                b.Content = s;
                b.Tag = s;
                b.SetResourceReference(BackgroundProperty, "BlackBtn");
                
                b.Foreground = Brushes.White;
                b.Click+=new RoutedEventHandler(Slide_Click);
                b.MouseRightButtonDown += new MouseButtonEventHandler(b_MouseRightButtonDown);

                this.stack.Children.Add(b);

            }
            HighlightSlideOnair();
        }

        void b_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button btn = (Button)sender;
            UIMain.Instance.displayPreviewSlide(btn.Tag.ToString());
        }

        void Slide_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            UIMain.core.broadcast(btn.Tag.ToString());
        }

        public void setEvents()
        {
            UIMain.core.engine.onChangeCart += new BroadcastEngine.ChangeCartEvent(engine_onChangeCart);
            UIMain.core.engine.onBroadcast += new BroadcastEngine.OutputEvent(engine_onBroadcast);
        }

        Button currentOnAirSlide = null;
        void engine_onBroadcast(string filename, string link)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                HighlightSlideOnair();
            }));
        }

        public void HighlightSlideOnair()
        {
            String slideonair = UIMain.core.engine.slideOnAir;
            Button next = null;
            for (int i = 0; i < this.stack.Children.Count; i++)
            {
                Button b = (Button)this.stack.Children[i];
                if (b.Tag.ToString().Equals(slideonair))
                {
                    next = b;
                    break;
                }
            }

            if (next != null)
            {
                if (currentOnAirSlide != null)
                {
                    currentOnAirSlide.Background = next.Background;
                }
                //next.Background = Brushes.Lime;
                next.Foreground = Brushes.White;
                next.Background = this.Resources["LimeBtn"] as LinearGradientBrush;
                currentOnAirSlide = next;
            }
            else
            {
                if (currentOnAirSlide != null)
                {
                    currentOnAirSlide.Background = Brushes.LightGray;
                }
            }
            
        }

        public void refreshSlideCartList()
        {
            this.stackCarts.Children.Clear();
            
            LinkedList<SlideCart> sc = UIMain.Instance.uicartedit.slideCart;
            foreach (SlideCart s in sc)
            {
                Button b = new Button();
                b.Content = s.name;
                b.Tag = s;
                b.Foreground = Brushes.White;
                b.SetResourceReference(BackgroundProperty, "BlackBtn");
                b.Click += new RoutedEventHandler(SlideCart_Click);
                this.stackCarts.Children.Add(b);
            }
        }

        Button currentSlideCartBtn = null;
        void SlideCart_Click(object sender, RoutedEventArgs e)
        {
            Button b = ((Button)sender);
            if (currentSlideCartBtn != null)
            {
                currentSlideCartBtn.Background = b.Background;
            }
            //b.Background = Brushes.Lime;

            b.Background = this.Resources["LimeBtn"] as LinearGradientBrush;
            currentSlideCartBtn = b;
            SlideCart s = (SlideCart)b.Tag;

            List<String> l = new List<string>();
            for (int i = 0; i < s.slides.Count; i++)
            {
                l.Add(s.slides[i].ToString());
            }

            updateList(s.variables);
            UIMain.core.engine.setSlideCart(l);
        }

        private void updateList(Dictionary<string, string> variables)
        {
            foreach(KeyValuePair<String,String> p in variables){
                if (UIMain.core.slidegen.cstlist.ContainsKey(p.Key.ToUpper()))
                    UIMain.core.slidegen.cstlist[p.Key.ToUpper()] = p.Value;
                else
                    UIMain.core.slidegen.cstlist.Add(p.Key.ToUpper(), p.Value);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UIMain.core.engine.broadcastNextInCart();
                
        }
        
      /*  private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           // UIMain.core.engine.onBroadcast += new BroadcastEngine.OutputEvent(engine_onBroadcast);
          //  UIMain.core.engine.onChangeCart += new BroadcastEngine.ChangeCartEvent(engine_onChangeCart);
        }

        void engine_onChangeCart(List<string> slides)
        {
            foreach(String s in slides)
                addNewSlide(s);
        }

        public void addNewSlide(String name)
        {
            Button b = new Button();
            b.Content = name;
            b.Margin = new Thickness(10);
            this.stack.Children.Add(b);
        }
        void engine_onBroadcast(string filename, string link)
        {
            
        }*/
    }
}
